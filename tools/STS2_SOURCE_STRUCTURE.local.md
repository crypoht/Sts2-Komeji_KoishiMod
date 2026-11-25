# 杀戮尖塔 2 源码目录说明

更新时间：2026-09-08

本文记录当前电脑上用于恋恋 Mod 开发的杀戮尖塔 2 源码、依赖库源码和参考 Mod 源码位置。

## 总入口

```text
D:\UINya\sts2\
├─ stable\                  稳定版官方源码镜像
├─ sts2\                    Beta 版官方源码镜像
├─ BaseLib-StS2-master\     BaseLib 源码
├─ STS2-RitsuLib-main\      RitsuLib 源码
├─ HakureiReimu\            博丽灵梦 Mod 源码参考
├─ llo\                     其他角色 Mod 源码参考
├─ SimpleMap2\              地图 Mod 源码参考
├─ other_music\             音乐替换 Mod 源码参考
└─ pck\                     PCK 相关工具或临时内容
```

## 官方稳定版源码

根目录：

```text
D:\UINya\sts2\stable\sts2\
└─ MegaCrit\sts2\
   ├─ Core\                 游戏主要逻辑
   ├─ GameInfo\             游戏版本、构建和元数据
   ├─ SourceGeneration\     源码生成相关逻辑
   └─ addons\               Godot 插件与附加内容
```

最常用的官方稳定版源码目录：

```text
D:\UINya\sts2\stable\sts2\MegaCrit\sts2\Core\
```

## 官方 Beta 源码

根目录：

```text
D:\UINya\sts2\sts2\
├─ MegaCrit\sts2\
│  ├─ Core\                 Beta 版游戏主要逻辑
│  ├─ GameInfo\             Beta 版版本与构建信息
│  ├─ SourceGeneration\     Beta 版源码生成相关逻辑
│  └─ addons\               Godot 插件与附加内容
├─ GodotPlugins\            Godot 插件工程
├─ Properties\              工程属性
├─ RiderTestRunner\         Rider 测试运行器
├─ System\                  系统兼容源码
└─ sts2\                    工程内部目录
```

最常用的官方 Beta 源码目录：

```text
D:\UINya\sts2\sts2\MegaCrit\sts2\Core\
```

## Core 一级目录

稳定版与 Beta 版的 `Core` 一级结构目前一致：

```text
Core\
├─ Achievements             成就
├─ Animation                动画
├─ Assets                   资源加载、资源路径与预加载
├─ Audio                    音频、BGM、音效
├─ AutoSlay                 自动战斗相关
├─ Bindings                 游戏绑定层
├─ CardSelection            选牌界面和选牌命令
├─ Combat                   战斗流程、回合、伤害与 CombatManager
├─ Commands                 官方异步命令，例如 CardPileCmd、CardCmd、PowerCmd
├─ Context                  本地、多人、运行上下文
├─ ControllerInput          手柄输入
├─ Daily                    每日挑战
├─ Debug                    调试逻辑
├─ DevConsole               游戏控制台与官方控制台命令
├─ Entities                 运行时实体状态
├─ Events                   生命周期事件
├─ Exceptions               自定义异常
├─ Extensions               扩展方法
├─ Factories                模型、节点和对象工厂
├─ GameActions              游戏动作，含多人同步动作
├─ Helpers                  常用辅助工具
├─ Hooks                    游戏钩子
├─ HoverTips                悬浮说明
├─ Leaderboard              排行榜
├─ Localization             本地化与动态变量
├─ Logging                  日志
├─ Map                      地图和节点
├─ Modding                  Mod 加载、注册和内容池
├─ Models                   卡牌、能力、遗物、事件等模型定义
├─ MonsterMoves             怪物行动
├─ Multiplayer              联机大厅、同步和网络状态
├─ Nodes                    Godot 显示节点与 UI
├─ Odds                     概率与权重
├─ Platform                 Steam 等平台接口
├─ Random                   RNG 与随机流
├─ Rewards                  奖励
├─ RichTextTags             富文本标签渲染
├─ Rooms                    房间、战斗房间、商店、宝箱等
├─ Runs                     局内运行状态
├─ Saves                    存档与序列化
├─ Settings                 游戏设置
├─ TestSupport              官方测试辅助
├─ TextEffects              文本特效
├─ Timeline                 时间线
├─ Unlocks                  解锁内容
└─ ValueProps               数值属性与计算
```

## 最常查的官方位置

```text
Core\Commands\
├─ CardPileCmd.cs           加入、移除、移动卡牌
├─ CardCmd.cs               打出、消耗、弃置、升级卡牌
├─ CreatureCmd.cs           生物受伤、死亡、生命值操作
├─ PlayerCmd.cs             玩家能量、星星等操作
└─ PowerCmd.cs              给予、移除能力

Core\Models\
├─ CardModel.cs             卡牌通用生命周期、结算、去向
├─ Cards\                   官方卡牌
├─ Powers\                  官方能力
├─ Relics\                  官方遗物
├─ Events\                  官方事件
├─ Monsters\                官方怪物
├─ Enchantments\            官方附魔
├─ Orbs\                    官方充能球
├─ Characters\              官方角色
├─ CardPools\               卡池
└─ RelicPools\              遗物池

Core\Entities\
├─ Cards\                   CardPile、CardPlay 等运行时卡牌实体
├─ Creatures\               Creature、伤害与生命实体
├─ Players\                 Player、玩家战斗状态
├─ Powers\                  Power 实体
├─ Relics\                  Relic 实体
├─ Orbs\                    Orb 实体
├─ Potions\                 药水实体
└─ Multiplayer\             多人实体

Core\Nodes\
├─ Combat\                  战斗场景中的卡牌、生物、战斗 UI
├─ Vfx\                     官方特效节点
├─ Screens\                 主菜单、角色选择等界面
├─ Rooms\                   房间节点
├─ Relics\                  遗物显示
├─ Rewards\                 奖励显示
├─ Orbs\                    充能球显示
├─ CommonUi\                通用 UI
└─ Multiplayer\             联机 UI

Core\Multiplayer\
└─ Game\                    游戏状态、同步器和联机流程

Core\DevConsole\
└─ ConsoleCommands\         官方控制台命令，例如 win、godmode、die
```

## 详细子结构

`Core\Models\`：

```text
Achievements  Acts          Afflictions  Badges       CardPools
Cards         Characters    Enchantments Encounters   Events
Exceptions    Modifiers     Monsters     Orbs         PotionPools
Potions       Powers        RelicPools   Relics       Singleton
```

`Core\Entities\`：

```text
Actions       Ancients      Ascension    CardRewardAlternatives
Cards         Characters    Creatures    Enchantments
Encounters    Gold          Intents      Merchant
Models        Multiplayer   Orbs         Players
Potions       Powers        Relics       RestSite
Rewards       Rngs          Text         TreasureRelicPicking
UI
```

`Core\Nodes\`：

```text
Animation     Audio         Cards        Combat
CommonUi      Debug         Events       Ftue
GodotExtensions  HoverTips  Multiplayer  Orbs
Pooling       Potions       Reaction     Relics
RestSite      Rewards       Rooms        Screens
TopBar        TreasureRooms Vfx
```

## BaseLib 源码

根目录：

```text
D:\UINya\sts2\BaseLib-StS2-master\
```

主要目录：

```text
Abstracts      自定义卡牌、遗物、能力等基类
Cards          BaseLib 卡牌辅助
Commands       BaseLib 的扩展命令
Common         通用能力、奖励、池等
Config         Mod 设置 UI
ConsoleCommands 控制台扩展
Hooks          BaseLib 钩子
Patches        BaseLib Harmony Patch
BaseLibScenes  BaseLib 自带 Godot 场景
Extensions     扩展方法
Utils          工具类
docs           文档
```

## RitsuLib 源码

根目录：

```text
D:\UINya\sts2\STS2-RitsuLib-main\
```

主要源码在：

```text
D:\UINya\sts2\STS2-RitsuLib-main\src\
```

常用内容包括：

```text
src\Compat\          兼容性检查
src\Diagnostics\     诊断与开发工具
src\Updates\         创意工坊更新检测
resources\           本地化和资源
schemas\             配置、数据格式定义
docs\                使用说明
```

## 参考 Mod 源码

```text
D:\UINya\sts2\HakureiReimu\HakureiReimuMod\
    博丽灵梦 Mod，卡牌、角色与事件参考。

D:\UINya\sts2\llo\Zephyr\
    也文摄辉 Mod，角色、对话与联机参考。

D:\UINya\sts2\SimpleMap2\
    地图系统参考。

D:\UINya\sts2\other_music\
    音乐替换 Mod 参考。
```

## 查找顺序建议

1. 先查当前目标版本的官方源码：稳定版查 `stable\sts2`，Beta 查 `sts2`。
2. 查不到或需要 Mod 接口时，再查 `BaseLib-StS2-master`。
3. 需要控制台补全、动态注册、开发工具或 RitsuLib 专属功能时，查 `STS2-RitsuLib-main`。
4. 需要已有 Mod 的具体实践时，再查对应参考 Mod。

> 注意：稳定版和 Beta 版的接口可能不同。修改恋恋 Mod 前，应优先对照当前正在运行的游戏分支对应的官方源码。
