![](https://img.shields.io/badge/language-WPF-red.svg)
![](https://img.shields.io/badge/license-MIT-green.svg)
![](https://img.shields.io/badge/version-V1.0.0.6.bate1-blue.svg)

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

到达指定时间后，飘窗抽风

## 8. 暴力模式

慎用就好

