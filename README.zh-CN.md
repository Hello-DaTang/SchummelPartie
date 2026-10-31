# Schummel Partie 1.0.5

本次只修改 SchummelPartie，不需要替换 UrGUI 或 MelonLoader。

## 使用方法

- **F8**：暂停所有辅助功能，再按恢复之前配置。GUI 顶部也有暂停开关和按钮。
- **Insert / 右 Shift**：显示或隐藏 GUI；隐藏界面不会暂停功能。
- **F11**：开启或退出独立本地自由视角，不再调用会停用其他组件的游戏内置自由相机。无需打开 Debug。
- 自由视角：**WASD** 移动，**Q / E** 下降 / 上升，**右键**转向，**Shift** 加速。游戏和玩家输入仍照常运行。
- 暂停全部功能、切换场景或源相机失效时，会退出自由视角。
- **Force Present** 开启后，只把自己已有礼盒的开盒结果固定为好礼盒（传送到终点）；不会生成或发放礼盒，也不会替其他玩家和 AI 改结果。旧配置中的 Action ID 会忽略。

暂停时保留各模块的设置，并还原本次启用期间保存的速度、跳跃次数、炸弹参数、射速、散弹数量、镜头抖动等运行参数。暂停不会回滚已经发生的伤害、得分、奖励或已经放出的炸弹，正在执行的游戏动作会按游戏自身逻辑结束。

Debug 是另一个独立功能；其游戏内置快捷键仍会生效，例如主键盘 `-` 会给当前玩家钥匙和随机道具。只需要自由视角和好礼盒时，可以关闭 Debug。

## 安装

退出游戏后，将 `artifacts\SchummelPartie.dll` 复制到游戏的 `Mods` 目录，替换旧版同名 DLL。保留现有 `UserLibs\UrGUI.dll`、MelonLoader 和 `settings.json` 即可。不要同时放入两个不同文件名的 SchummelPartie DLL。

当前机器的安装目录为：

```text
C:\SteamLibrary\steamapps\common\Pummel Party\Mods
```

## 编译与验证

需要 .NET 8 SDK，以及已经安装 MelonLoader 和 UrGUI 的游戏目录。

```powershell
.\build.ps1 -GameDirectory 'C:\SteamLibrary\steamapps\common\Pummel Party'
```

脚本编译 .NET Framework 4.8 的 Release DLL，执行使用实际生产源文件的生命周期 / 暂停 / 状态还原 / 礼盒权限检查，并核对安装游戏中的 Harmony 补丁目标和输出 DLL。

自动检查不依赖 Unity 原生运行环境，不能代替游戏内的相机画面和多人联机测试。测试依据的本机版本为 Pummel Party 1.14.1k、Unity 2021.3.45f2、MelonLoader 0.7.3。
