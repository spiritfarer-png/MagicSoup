# 遗物系统

## 当前目标与范围

遗物是一种特殊素材。它留在素材背包时作为独立遗物参与战斗；进入合成栏并被合成进卡牌后，只作为卡牌素材参与名称、生命、汤色、素材图层和卡牌意图计算。两种身份由素材当前所在位置决定，不额外保存启用状态。

当前已经完成遗物数据、背包身份切换、玩家与敌方战斗快照，以及三个战斗触发节点的核心逻辑。战斗 HUD 遗物展示和遗物动效不属于当前闭环，继续保留为后续工作。

## 数据结构

- 每种遗物建立一个继承 `CardMaterialData` 并实现 `IRelic` 的 ScriptableObject 类，例如 `PorkLegRelic` 和 `SeaweedClumpRelic`。
- 遗物继承的 `materialID`、名称、图标、颜色、生命和 `normalIntents / ascendedIntents` 表示它作为卡牌素材时的数据。
- 遗物留在背包时的效果由对应类实现，不建立通用 `relicIntents` 数组，也不建立 `RelicMaterialData` 或 `RelicMaterialInfo`。
- `IRelic.MaterialData` 返回遗物自身的 `CardMaterialData` 引用。
- `IRelic` 提供 `OnBattleStart`、`OnRoundStart` 和 `OnCardAction` 三个战斗回调，以及供 Tooltip 使用的 `GetRelicInfo`。
- 回调接收当前 `BattleManager` 和阵营参数 `isEnemy`；`OnCardAction` 额外接收正在行动的 `CardEntity`。
- 遗物 SO 不保存所属阵营、`BattleManager` 或其他单场战斗临时状态，避免玩家和敌方引用同一资产时互相污染。
- 回调返回值表示该遗物是否实际触发了效果；当前 `BattleManager` 不依赖返回值控制流程。

## 背包、合成与升级

- 遗物和普通素材共用 `InventoryManager.materialSlots`、`craftingSlots`、`MaterialSlotData`、`MaterialSlotUI` 和拖拽换位逻辑，不建立独立遗物背包。
- `MaterialSlotData.isRelic` 通过 `materialData is IRelic` 判断当前素材是否为遗物。
- 遗物不堆叠；同一遗物可以存在多份，每份分别占用一个素材槽位。
- 位于 `materialSlots` 中的遗物会进入下一场战斗的玩家遗物快照。
- 位于 `craftingSlots` 中但尚未合成的遗物不会进入快照；拖回素材背包后恢复独立遗物身份。
- 遗物可以单独合成卡牌，也可以和普通素材或其他遗物混合合成。
- 合成后遗物占用卡牌最多三个素材位置中的一个，并按放入顺序参与卡面和卡牌意图计算。
- 合成进卡牌后，战斗系统只通过 `CardMaterialInfo` 使用遗物继承的卡牌数据，不会因为该素材实现了 `IRelic` 而触发独立遗物回调。
- 卡牌升级时，遗物素材和普通素材一样切换到 `ascendedIniHealth / ascendedIntents`；独立遗物效果不随卡牌升级。
- 当前不实现拆解和遗物自身升级。

## 战斗快照与敌人配置

- `InventoryManager.CreateRelicSnapshot()` 按 `materialSlots` 的槽位顺序扫描，只收集实现 `IRelic` 的素材，忽略空槽、普通素材和合成栏。
- 玩家快照是紧凑数组，只保留遗物之间的相对顺序；相同资产不会去重，因此多份遗物会分别触发。
- `BattleManager.InitializeBattle()` 在双方卡牌生成后建立玩家和敌方遗物快照。本场战斗后续只使用该快照。
- 玩家遗物来自 `InventoryManager.CreateRelicSnapshot()`。
- `EnemyConfig.enemyRelics` 使用 `ScriptableObject[]` 保存 Inspector 可序列化的敌方遗物资产；初始化时逐个转换为运行时 `IRelic[]`，不能直接进行数组强制转换。
- 敌方配置由 `EnemyConfigPool` 按战斗类型抽取；当前测试战斗使用普通敌人池。
- 遗物不是场上单位，不参与存活数量和目标槽位计算。

## 触发节点与回合顺序

### 战斗开始

- 第一回合推进时钟并清空玩家护甲后，双方遗物按各自快照顺序执行一次 `OnBattleStart`。
- `OnBattleStart` 整场战斗只执行一次，不会在后续回合重复触发。
- 战斗开始效果可以读取并修改已经初始化完成的双方 `CardEntity` 和卡牌运行时意图。

### 回合开始

- 每回合玩家决策前，玩家遗物按快照顺序执行一次 `OnRoundStart`。
- 玩家卡牌完成正常行动并清空敌方护甲后，敌方遗物按快照顺序执行一次 `OnRoundStart`，随后敌方卡牌行动。
- 当前回合顺序为：推进时钟 → 清空玩家护甲 → 首回合双方 `OnBattleStart` → 玩家 `OnRoundStart` → 玩家决策与行动 → 清空敌方护甲 → 敌方 `OnRoundStart` → 敌方行动 → 下一回合。

### 卡牌行动

- 每张卡开始一次完整行动时，所属阵营的遗物按快照顺序执行一次 `OnCardAction`，之后才结算该卡的意图。
- 玩家额外行动和双方正常行动都会触发 `OnCardAction`。
- 一张卡拥有多条意图时，`OnCardAction` 仍只触发一次，不会按满足条件的意图数量重复触发。

## 胜负与中断

- 每个遗物回调结束后，`BattleManager` 立即调用 `IsBattleOver()`。
- 遗物效果结束战斗时立即进入 `BattleOver`，停止剩余遗物、卡牌意图和本回合后续流程。
- `OnCardAction` 在卡牌意图之前执行；如果遗物在该节点结束战斗，原卡牌不再继续行动。
- 遗物实现自行决定目标和效果，`BattleManager` 不为遗物增加统一目标枚举或额外合法性检查。

## 当前遗物

### 精品猪后腿肉

- 类型：`PorkLegRelic`。
- 节点：`OnBattleStart`。
- 效果：从所属阵营随机选择一张存活卡牌，获得配置数值的护甲；不存在可选卡牌时不触发。

### 海藻团块

- 类型：`SeaweedClumpRelic`。
- 节点：`OnBattleStart`。
- 效果：遍历所属阵营全部存活卡牌，将其当前运行时素材意图中的每条 `Attack` 数值增加配置值。
- 修改目标是 `CardMaterialInfo.Intents` 的战斗运行时副本，不修改素材 SO，因此不会永久污染后续战斗。

## 尚未完成

- 在 `BattleHUDView` 中展示双方遗物图标并接入统一 Tooltip。
- 为敌方测试配置实际添加遗物，并验证敌方三个回调节点、重复遗物顺序和遗物结束战斗的中断行为。
- 遗物触发与卡牌行动动效。
- 将 `GetRelicInfo` 中的数值描述改为读取 SO 配置，避免 Tooltip 文案与实际数值脱节。
