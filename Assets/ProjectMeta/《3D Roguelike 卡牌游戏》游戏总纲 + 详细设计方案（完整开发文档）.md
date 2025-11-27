# **《3D Roguelike 卡牌游戏》游戏总纲 + 详细设计方案（完整开发文档）**

# Ⅰ. 游戏总纲（Game Overview）

## 1. 游戏定位

本项目是一款基于 Unity 的 **3D Roguelike 卡牌构筑游戏**，核心玩法借鉴《杀戮尖塔》，采用层级地图推进、回合制战斗、随机事件、多路径选择等机制。

玩家通过不断打牌战斗、构筑卡组、挑选遗物，最终击败 Boss 完成一次 Run。

目标特性：

- **轻量 3D 表现 + 卡牌 UI 驱动战斗**
- **单局 Rogue-like，死亡即结束**
- **深度构筑，轻量剧情，多路线地图**
- **随机性强，但可控度高**
- **模块化系统，可持续扩展**

------

## 2. 核心玩法循环（Core Loop）

1. 主菜单 → 开始游戏
2. 生成一局 Run（地图 + 初始卡组 + 初始属性）
3. 进入地图界面，从起点开始选择路径向上前进
4. 遇到不同类型的节点，根据节点类型进入相应场景：
   - 普通敌人 → 战斗
   - 精英敌人 → 困难战斗
   - 商店 → 购买、移除卡牌
   - 事件 → 随机选择
   - 营火 → 休息/强化卡牌
   - 宝箱 → 获取遗物
5. Boss 战 → 胜利进入结算
6. 玩家死亡 → 失败结算
7. 回到主菜单，开始下一局

------

## 3. 程序架构总览（Architecture Overview）

### 全局单例系统（DontDestroyOnLoad 常驻）

- **GameManager**：游戏总控制器，管理全局状态与单局 Run 的所有数据
- **SceneFlowManager**：场景加载调度器，统一控制场景切换（Additive 加载）
- **EventBus**：系统间事件通信机制
- **UIManager**：统一管理全局 UI（HUD、ESC 菜单、面板切换）
- **AudioManager**：统一管理 BGM、SFX
- **SaveManager**：负责 Run 内地图进度、状态存档/恢复

### 主要场景

- BootScene
- MainMenuScene
- MapScene（地图）
- FightScene（战斗）
- RewardScene
- ShopScene
- EventScene
- RestScene

> 场景间通过 SceneFlowManager 控制，所有流程事件通过 EventBus 通知。

------

## 4. 模块组成（Modules）

### 4.1 地图系统（Layered Map）

采用分层节点系统：

- 每层 1~7 个节点
- 节点类型：普通、精英、商店、事件、营火（休息）、宝箱、Boss
- 无交叉连线算法
- 保证至少一条路径通向 Boss
- 支持 Run 内存档、读档

### 4.2 战斗系统（Battle）

- 回合制
- 手牌系统、抽牌/弃牌堆
- 能量驱动
- 敌人意图（Intent）
- 单体选择或群体卡牌
- 战斗结束奖励

### 4.3 卡牌系统（Cards）

- ScriptableObject 卡牌定义
- 支持卡牌类型：攻击、防御、技能、能力、诅咒
- 支持卡牌升级（Upgraded）
- 支持卡牌效果（伤害 / 护甲 / 抽牌 / Buff）

### 4.4 遗物系统（Relics）

- 被动效果
- Boss 遗物、普通遗物
- 进入战斗/回合开始等触发

### 4.5 事件系统（Events）

- 文本事件
- 多选项 → 执行效果 → 返回地图

### 4.6 商店系统（Shop）

- 购买卡牌
- 购买遗物
- 移除卡牌功能
- 价格机制

### 4.7 营火（Rest）

- 休息（恢复生命）
- 升级卡牌

### 4.8 奖励系统（Reward）

- 卡牌三选一
- 战斗金币奖励
- 精英和 Boss 的额外遗物奖励

------

------

# Ⅱ. 详细设计方案（Technical Design）

以下部分为给你的 Agent 提供行动指导的核心内容。

------

# 1. 全局管理与场景调度

## 1.1 GameManager（游戏总控制器）

GameManager 是项目的“中心控制大脑”，负责：

### （1）管理单局 Run 的所有数据

包括：

- 玩家生命（CurrentHP / MaxHP）
- 金币
- 当前卡组、抽牌堆、弃牌堆
- 当前遗物
- 当前层数
- 当前地图图结构（LayerMapGraph）
- 当前所在节点 ID
- 当前敌人列表（进入战斗时加载）

### （2）对外提供全局属性接口

例如：

```
GameManager.Instance.PlayerHP
GameManager.Instance.CardDeck
GameManager.Instance.CurrentMap
```

### （3）控制游戏流程

例如：

```
StartNewRun()
EnterBattle(node)
EnterReward()
EnterEvent()
EnterShop()
EnterRest()
ReturnToMap()
EndRun(success/fail)
```

------

## 1.2 SceneFlowManager（场景调度器）

### 主要功能

- 所有场景采取 Additive 加载
- 所有 UI、HUD 决定常驻
- 加载新场景 → 设置为 active scene
- 卸载旧场景
- 发布 “OnSceneLoaded” 事件（通过 EventBus）

### 设计方法

```
LoadScene(SceneType type)
UnloadCurrentGameplayScene()
EnsureHUDExists()
```

所有进入战斗、事件、商店都必须走 SceneFlowManager。

------

## 1.3 EventBus（事件总线）

所有模块互相通信必须通过：

```
EventBus.Publish("BattleEnd", result);
EventBus.Subscribe("OnSceneLoaded", OnSceneLoaded);
```

避免硬耦合。

------

# 2. 地图系统（LayerMap）

你的地图系统已经明确采用：

- LayerMapGenerator
- LayerMapGraph
- MapRenderer
- MapTypes

以下是官方设计化描述（无“你已做过”字样）。

------

## 2.1 LayerMapGenerator（地图生成器）

### 功能

1. 根据参数（层数、每层节点数、节点类型比例）生成节点
2. 应用不交叉连线算法
3. 生成至少一条贯通 Boss 的主路径
4. 输出 LayerMapGraph

### 节点类型规则

- 第一层为普通怪物
- 中间层根据概率生成普通/精英/事件/商店/营火/宝箱
- 最后一层固定为 Boss

------

## 2.2 LayerMapGraph（地图图结构）

包含：

```
List<Layer>
Each Layer: nodes[]
Node:
  nodeId
  layerIndex
  type
  reachable (true/false)
  visited
  connections (next node Ids)
```

------

## 2.3 MapRenderer（地图渲染器）

职责：

- 将 LayerMapGraph 渲染为 UI/3D 节点
- 显示连线
- 管理节点状态变化（visit / reachable）
- 点击后向 GameManager 通知

------

## 2.4 SaveManager（地图存档与恢复）

- 当前节点
- visited 状态
- reachable 状态
- 地图结构

当玩家进入节点场景并返回地图时，自动恢复进度。

------

# 3. 战斗系统（Battle System）

战斗是下一阶段最核心模块。

------

## 3.1 BattleManager（战斗主控制器）

进入战斗时：

1. 从 GameManager 获取敌人数据
2. 创建敌人实例
3. 初始化玩家战斗数据（Block 置 0，抽 N 张牌）
4. 设置当前状态为玩家回合
5. 监听战斗事件（卡牌使用/敌人死亡）

战斗结束：

```
EventBus.Publish("BattleEnd", result);
SceneFlowManager.LoadScene(RewardScene);
```

------

## 3.2 TurnManager（回合管理）

状态机：

- PlayerTurn
- EnemyTurn
- RoundEnd
- BattleEnd

流程：

```
StartPlayerTurn()
  RestoreEnergy()
  DrawCards()

PlayerAction()
  UseCard
  EndTurnButton

StartEnemyTurn()
  AllEnemies.ExecuteIntent()
  CheckPlayerDead()

RoundEnd
  ClearTemporaryBlock
  NextRound
```

------

## 3.3 Combatant（战斗单位）

定义：

```
int MaxHP
int CurrentHP
int Block
StatusContainer Status
```

子类：

- PlayerCombatant
- EnemyCombatant

------

## 3.4 EnemyAIBase（敌人 AI）

每个敌人定义：

```
EnemyIntent[]
Index
```

每回合执行该意图：

- 攻击
- 格挡
- Buff
- Debuff

------

## 3.5 卡牌系统对接

### CardBase

属性：

```
Id
Name
Description
Cost
CardType (Attack / Skill / Power)
TargetType (Single / All / Self)
BaseDamage
BaseBlock
...
```

### CardEffect

卡牌行为：

- 造成伤害
- 提供护甲
- 抽牌
- 添加 Buff
- 增加能量

### Deck 管理

```
drawPile
discardPile
hand
exhaustPile
```

机制：

- 抽牌：Draw(n)
- 弃牌：Discard()
- 洗牌：洗弃牌堆进抽牌堆
- 手牌上限

------

# 4. 奖励系统（Reward）

战斗结束进入 RewardScene。

奖励内容：

1. 金币奖励
2. 随机 3 选 1 的卡牌
3. 精英/ Boss 掉落遗物

流程：

```
Player chooses card → Add to deck
Player presses confirm → ReturnToMap
```

------

# 5. 事件系统（Event）

事件内容：

- 标题
- 文本描述
- 2~3 个选项
- 各选项执行：掉血 / 加金币 / 获得卡牌 / 获得诅咒 / 获得遗物

事件流程：

1. SceneFlowManager.LoadScene(EventScene)
2. EventScene 加载随机事件
3. 玩家选择一个选项
4. 执行效果
5. 返回地图 ReturnToMap()

------

# 6. 商店系统（Shop）

功能：

- 购买卡牌
- 购买遗物
- 移除卡牌（付费）

UI 部分：

- 左侧卡牌列表（点击 → 详情 → 购买）
- 中间遗物列表
- 右侧“移除卡牌”入口

逻辑：

```
Check gold → Deduct → Add item → Update GameManager
```

------

# 7. 营火系统（Rest）

功能：

- 休息：恢复一定百分比生命（例如 30%）
- 升级卡牌：从卡组选择一张 → 变成升级版

流程：

```
Choose action
Execute
Return to map
```

------

# 8. 遗物系统（Relics）

类型：

- 普通遗物（普通战斗）
- 精英遗物
- Boss 遗物

触发机制：

- 开局触发
- 回合开始触发
- 战斗结束触发
- 使用卡牌触发

------

# 9. 数据驱动（Scriptable Objects）

所有静态内容必须 SO 化：

- CardData
- EnemyData
- RelicData
- EventData

GameDatabase 管理访问。

------

# 10. 开发路径（推荐顺序）

1. 战斗基础框架
2. 抽牌/打牌系统
3. 敌人 AI
4. 奖励系统
5. 事件系统
6. 商店系统
7. 营火系统
8. Boss 战
9. 多层地图
10. 音效 / 动画 / 特效

------

# 你如果需要，我可以继续为你生成：

- 更细化的 **代码层级设计（类图 / 方法列表 / 生命周期图）**
- 整份 **ScriptableObject 数据定义文档**
- 整套 **任务拆解（给 Agent 的可执行指令列表）**
- 或 **将此文档转为英文 PRD/Tech Spec**

只需告诉我：
 **你要继续补充哪个部分？**