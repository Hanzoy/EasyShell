# EasyShell

EasyShell 是一个 Windows 托盘工具，用全局快捷键快速打开命令行。

## 功能

- 默认快捷键：`Ctrl+Space`
- 可在托盘菜单中修改快捷键
- 可选择打开 `cmd` 或 `PowerShell`
- 可在设置中开启或关闭开机自启
- 当前焦点在文件资源管理器中时，终端会打开到该资源管理器窗口所在路径
- 当前焦点不在文件资源管理器中时，终端会打开到用户主目录

## 运行

```powershell
dotnet run --project .\EasyShell\EasyShell.csproj
```

运行后，EasyShell 会常驻系统托盘。双击托盘图标或右键选择“设置”可以修改默认终端和快捷键。

## 发布单文件 exe

```powershell
dotnet publish .\EasyShell\EasyShell.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

发布结果位于：

```text
EasyShell\bin\Release\net9.0-windows\win-x64\publish\EasyShell.exe
```

配置文件保存在：

```text
%APPDATA%\EasyShell\settings.json
```
