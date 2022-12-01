# DesktopTools

![](https://img.shields.io/badge/language-WPF-red.svg)
![](https://img.shields.io/badge/license-MIT-green.svg)
![](https://img.shields.io/badge/version-V1.0.1.1.bate-blue.svg)

# 重要事项


## V1.0.0.8 - V1.0.1.0版本存在严重问题

键盘输入过程中存在未释放资源将导致界面崩溃，该文件在`V1.0.1.1.bate`版本后修复。

# 说明

## 1、窗口切换

在平常需要开启多个窗口，并在多个窗口之间需要来回切换的场景中，频繁的<kbd>ALT</kbd>+<kbd>TAB</kbd>带来的体验并不良好，且经常有切换过头要再按下<kbd>SHIFT</kbd>向前选择的情况发生。

为解决上述问题，简单开发了这款软件。

其功能的核心就是，使用快捷键绑定和在不同的窗口之间进行快速、准确的切换。默认情况下，用户可以按下<kbd>CTRL</kbd>(*支持通过设置变更键位*)加任意数字键完成操作。

数字键部分使用了右侧小键盘区域和顶部区域，两个区域各自独立，即通过右侧小键盘<kbd>CTRL</kbd>+<kbd>1</kbd>绑定的窗口是不能被顶部<kbd>CTRL</kbd>+<kbd>1</kbd>唤出的。

## 2、锁屏禁用

受企业版策略影响一般都会有自动锁屏机制，且用户无法自主修改，为应对某些场景下不让系统自动锁屏，提供了禁止自动锁屏功能。

# 环境依赖

![](https://img.shields.io/badge/DoNet-6.X-pink.svg)

# 兼容性

![](https://img.shields.io/badge/win-10/11-blue.svg)

> `Net6`多次更新中存在不兼容性升级，建议直接安装最新版使用，搞不清也可以选独立版本。
>
> 独立版本内置了依赖，体积较大，但可以独立运行

# 功能和操作

> 内部集成必应壁纸、软件切换、下班提醒、紧急避险等模式

## 1. 切换壁纸

<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>B</kbd>+<kbd>N</kbd>

当设置中开启时：

随机获取必应壁纸，每间隔一小时自动切换

也可按下快捷键进行切换

## 2. 窗体绑定快捷键

<kbd>Ctrl</kbd>+<kbd>Dn</kbd>

<kbd>Ctrl</kbd>+<kbd>Numn</kbd>

通过默认或指定快捷键+数字键绑定当前前置状态的窗体

按下时不同窗体状态对应的不同效果为：

1、当窗体处于前置且激活时，最小化窗体

2、当窗体处于非前置或非激活状态时，前置或激活窗体

3、当窗体处于最小化状态时，显示并前置窗体

4、当窗体被关闭（如聊天软件等最小化到托盘）时，打开并前置窗体

5、当窗体被关闭（进程结束）时，尝试重新绑定到当前处于前置状态的窗体

## 3. 强制绑定快捷键

<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Dn</kbd>

<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Numn</kbd>

使用强制绑定时会替换快捷键原本绑定的窗体

## 4. 解绑所有快捷键

<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Back</kbd>

将当前处于前置状态的窗体所绑定的所有快捷键移除

## 5. 紧急避险

<kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>Space</kbd>

奇奇怪怪的东西

## 6. 开关禁止锁屏（默认开启）

<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Space</kbd>

快捷键用于开关禁止锁屏模式，该模式下将模拟用户鼠标行为，以此保障系统不进入锁屏状态

## 7. 下班提醒

到达指定时间后，根据模式不同给出不同提醒，炸街模式慎用。

