# 项目待办

## CardEntity World UI 改造

- [ ] 将 `CardEntity` 的卡面整体改造为 World Space UI，不再混用 `SpriteRenderer` 与世界空间文字。
- [ ] 卡面点击、悬停和退出交互改用 UI EventSystem 与 `GraphicRaycaster`，移除摄像机上的 `Physics2DRaycaster` 依赖。
- [ ] 结合dotween的动效

## tooltip

- [ ] 在背包界面，鼠标挪到素材/卡牌上，会显示对应的数据，在战斗界面，鼠标挪到卡牌/遗物UI上，显示对应的数据

## 卡牌升级状态恢复

- [ ] 修正 `CardInfo.Initialize()`：重新进入战斗或重新初始化卡牌时，根据 `CardInfo.isAscended` 恢复全部素材的 `ascendedIniHealth` 与 `ascendedIntents`，避免已升级卡被重置为普通数据。

## 最大生命与治疗上限

- [ ] 将现有“初始生命值”语义统一改为“最大生命值”，战斗开始时当前生命等于最大生命，治疗不得超过最大生命。

## 回合流程重构

- [x] 按 `遗物_ai.md` 重构回合流程与双方护甲清空时机。

## 遗物系统

- [ ] 实现遗物素材数据、玩家/敌方战斗快照和回合内意图结算。
- [ ] 在 `BattleHUDView` 中展示双方遗物。
- [ ] 创建并配置玩家、敌方测试遗物，完成核心流程验证。
