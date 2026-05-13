# EasyShell

EasyShell 是一个 Windows 托盘工具，用全局快捷键快速打开命令行。

## 功能

- 默认快捷键：`Ctrl+Space`
- 默认管理员快捷键：`Ctrl+Shift+Space`
- 可在托盘菜单中修改快捷键
- 可选择打开 `cmd` 或 `PowerShell`
- 自动检测 Git Bash，并加入默认终端选项
- 自动扫描 Windows Terminal profiles，可选择 Windows PowerShell、命令提示符、Ubuntu、VS Developer Prompt 等 profile
- 可开启“Git 目录使用额外终端”，在资源管理器路径位于 Git 工作区内时自动切换到指定终端
- 可在设置中开启或关闭开机自启；开启后，新版本 exe 启动时会自动刷新自启路径
- 当前焦点在文件资源管理器中时，终端会打开到该资源管理器窗口所在路径
- 当前焦点不在文件资源管理器中时，终端会打开到用户主目录

## 运行

```powershell
dotnet run --project .\EasyShell\EasyShell.csproj
```

运行后，EasyShell 会常驻系统托盘。双击托盘图标或右键选择“设置”可以修改默认终端和快捷键。

## 下载

发布版本位于 [GitHub Releases](https://github.com/Hanzoy/EasyShell/releases)。

- `EasyShell-vx.x.x-runtime-required.exe`：体积较小，需要目标电脑已安装 .NET 9 Windows Desktop Runtime。
- `EasyShell-vx.x.x-standalone.exe`：体积较大，已包含 .NET 运行时，下载后可直接运行。

## 发布单文件 exe

如果目标电脑已经安装 .NET 9 Windows Desktop Runtime，可以发布较小的框架依赖版本：

```powershell
dotnet publish .\EasyShell\EasyShell.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

发布结果位于：

```text
EasyShell\bin\Release\net9.0-windows\win-x64\publish\EasyShell.exe
```

如果要发给没有安装 .NET 的电脑，发布自包含版本：

```powershell
dotnet publish .\EasyShell\EasyShell.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

这个版本会把 .NET 运行时一起打包进 exe，文件更大，但可以直接运行。

仓库里提供了统一发布脚本，会清理 `dist\` 后同时生成两个版本：

```powershell
.\publish-release.ps1
```

- `dist\runtime-required\EasyShell-v1.1.3-runtime-required.exe`：不带 .NET 运行时，文件较小，目标电脑需要安装 .NET 9 Windows Desktop Runtime
- `dist\standalone\EasyShell-v1.1.3-standalone.exe`：自包含版本，文件较大，目标电脑不需要安装 .NET

配置文件保存在：

```text
%APPDATA%\EasyShell\settings.json
```
