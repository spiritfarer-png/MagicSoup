# 项目待办

## 游戏主流程

- [x] 完成标题场景、地图进入战斗、战利品结算、死亡重开和 Boss 胜利界面。
- [ ] 重构并接入问号事件 UI，使随机事件节点完成后能够继续推进地图。
- [ ] 实现药水并接入玩家回合决策。

## CardEntity World UI 改造

- [x] 将 `CardEntity` 的卡面整体改造为 World Space UI，不再混用 `SpriteRenderer` 与世界空间文字。
- [ ] 卡面点击、悬停和退出交互改用 UI EventSystem 与 `GraphicRaycaster`，移除摄像机上的 `Physics2DRaycaster` 依赖。
- [ ] 结合dotween的动效

## tooltip

- [ ] 在背包界面，鼠标挪到素材/卡牌上，会显示对应的数据，在战斗界面，鼠标挪到卡牌/遗物UI上，显示对应的数据

## 卡牌升级状态恢复

- [x] 修正 `CardInfo.Initialize()`：重新进入战斗或重新初始化卡牌时，根据 `CardInfo.isAscended` 恢复全部素材的 `ascendedMaxHealth` 与 `ascendedIntents`，避免已升级卡被重置为普通数据。

## 最大生命与治疗上限

- [x] 将现有“初始生命值”语义统一改为“最大生命值”，战斗开始时当前生命等于最大生命，治疗不得超过最大生命。

## 回合流程重构

- [x] 按 `遗物_ai.md` 重构回合流程与双方护甲清空时机。

## 遗物系统

- [x] 实现 `CardMaterialData + IRelic` 遗物数据、背包身份切换、玩家/敌方战斗快照和三个战斗触发节点。
- [ ] 在 `BattleHUDView` 中展示双方遗物。
- [ ] 为敌方配置测试遗物，验证双方触发顺序、重复遗物和遗物结束战斗的中断行为。
