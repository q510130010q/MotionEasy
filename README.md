# MotionEasy

> 你是否厌倦了固高原生 C++ SDK 那晦涩难懂的 P/Invoke 指针传递？你是否在为多轴状态机同步和异常处理头疼？

MotionEasy 是一个基于 .NET 的运动控制库，用于封装**固高（Googol Tech）**运动控制器的底层 SDK（GTN/GLink），提供简洁、易用的 C# 接口以快速构建多轴运动控制系统。

## 功能计划

| 功能 | 状态 | 说明 |
|------|------|------|
| **点位运动 (Trap)** | ✅ 已完成 | Trap 速度曲线相对/绝对定位 |
| **绝对/速度运动** | ✅ 已完成 | 梯形/S 型加减速，定速运动 |
| **Jog 点动** | ✅ 已完成 | 连续点动运动 |
| **PT 运动** | ✅ 已完成 | 位置-时间模式，静态/动态 FIFO |
| **PVT 插值** | ✅ 已完成 | 位置-速度-时间 4 种表模式 |
| **电子齿轮** | ✅ 已完成 | 主从轴比例跟随，多主轴类型 |
| **电子跟随** | ✅ 已完成 | 电子凸轮，梯形轮廓 |
| **扩展跟随** | ✅ 已完成 | S 型曲线，百分比模式，IO 缓冲 |
| **多轴插补** | ✅ 已完成 | 2/3/4D 直线、圆弧、螺旋线插补 |
| **前瞻 (Look-Ahead)** | ✅ 已完成 | 速度前瞻规划，支持 3 轴/多轴/五轴/机器人模式 |
| **回零** | ✅ 已完成 | 8 种回零方式 + 探针，异步等待 |
| **状态监控** | ✅ 已完成 | 轴状态、位置、速度、跟随误差实时监控 |
| **PSO (位置同步输出)** | 🔧 底层 API 就绪 | 位置比较触发、等间距/等时间触发输出，待封装上层接口 |
| **五轴联动** | 🔧 底层 API 就绪 | `FIVE_AXIS`/`FIVE_AXIS_WORK` 机床模式，待封装上层接口 |
| **激光控制** | 🔧 底层 API 就绪 | 激光功率/HSIO 控制变量，待封装上层接口 |
| **振镜控制** | 📋 计划中 | 振镜扫描校正、场畸变补偿、跳转/标记延时优化 |
| **无限视野** | 📋 计划中 | 大图拼接、分块扫描、自动拼接算法 |
| **飞拍** | 📋 计划中 | 高速运动中触发相机采集，位置同步拍照 |
| **视觉定位** | 📋 计划中 | 相机标定、模板匹配、位置补偿 |
| **SCARA/Delta 机器人** | 📋 计划中 | 正逆解算法、轨迹规划、奇异点规避 |
| **CNC G 代码** | 📋 计划中 | G 代码解析、刀补、速度规划 |
| **远程调试 / Web API** | 📋 计划中 | REST API 远程控制、状态推送 |

## 项目结构

```text
MotionEasy.slnx
├── GenMotionEasy/              # .NET 8 核心库
│   ├── Model/                  # 数据模型
│   │   └── StatusInfo.cs       # 轴状态信息模型
│   ├── Motion/
│   │   ├── MotionControlManager.cs  # 控制器管理器（入口）
│   │   ├── Control/
│   │   │   ├── AxisController.cs    # 轴控制器门面
│   │   │   ├── IAxisController.cs   # 轴控制器接口
│   │   │   └── Details/             # 运动模式实现
│   │   │       ├── AxisHome.cs          # 回零
│   │   │       ├── AxisMotionPoint.cs   # Trap 点位运动
│   │   │       ├── AxisMotionMove.cs    # 绝对/速度运动
│   │   │       ├── AxisMotionJog.cs     # Jog 运动
│   │   │       ├── AxisMotionPT.cs      # PT 运动
│   │   │       ├── AxisMotionPVT.cs     # PVT 插值运动
│   │   │       ├── AxisMotionFollow.cs  # 电子跟随
│   │   │       ├── AxisMotionFollowEx.cs# 扩展跟随
│   │   │       ├── AxisMotionGear.cs    # 电子齿轮
│   │   │       ├── AxisMotionInterp.cs  # 多轴插补
│   │   │       ├── AxisAlarm.cs         # 报警处理（预留）
│   │   │       └── AxisSetup.cs         # 轴设置（预留）
│   │   └── Interfaces/           # 运动模式接口
│   │       ├── IAxisHome.cs
│   │       ├── IPointMotion.cs
│   │       ├── IMoveMotion.cs
│   │       ├── IJogMotion.cs
│   │       ├── IPTMotion.cs
│   │       ├── IPVTMotion.cs
│   │       ├── IFollowMotion.cs
│   │       ├── IFollowExMotion.cs
│   │       ├── IGearMotion.cs
│   │       └── IInterpMotion.cs
│   ├── Source/                  # P/Invoke 原生 API 封装
│   │   ├── gts.cs               # GTN.mc         - 运动控制核心 API
│   │   ├── gtgl500.cs           # GTN.glink      - GLink（EtherCAT）总线 API
│   │   ├── config.cs            # GTN.mc_cfg     - 配置结构体
│   │   └── LookAheadEx.cs       # GXN.mc_la      - 前瞻插补扩展 API
│   └── Tool/
│       └── GtnErrorHelper.cs    # 错误码工具 + GtnException
│
├── GenDemo/                     # .NET 10 WPF 演示程序
│   ├── App.xaml(.cs)
│   ├── MainWindow.xaml(.cs)     # 9 个标签页的完整演示 UI
│   └── GenDemo.csproj
│
└── GsnMotionEasy/               # netstandard 2.0 占位项目（预留 Gsn 硬件扩展）
    └── GsnMotionEasy.csproj
```

### 项目说明

| 项目 | 目标框架 | 说明 |
|------|----------|------|
| **GenMotionEasy** | `net8.0` | 核心库，封装固高 GTN/GLink SDK，提供统一轴控接口 |
| **GenDemo** | `net10.0-windows` (x86) | WPF 演示程序，展示所有运动模式用法 |
| **GsnMotionEasy** | `netstandard2.0` | 空项目，预留用于 Gsn 系列硬件扩展 |

> **注意**：GenDemo 强制 `x86` 平台，因固高原生 `gts.dll` 为 32 位。

## 架构设计

### 设计模式

- **门面模式（Facade）**：`AxisController` 统一暴露 10 个子模块属性，隐藏原生 API 复杂性
- **接口分离（Interface Segregation）**：每种运动模式有独立接口（`IPointMotion`、`IPTMotion` 等），按需依赖
- **管理器 + 注册表（Manager + Registry）**：`MotionControlManager` 负责控制器生命周期和轴实例管理
- **P/Invoke 防腐层（Anti-Corruption Layer）**：`Source/` 目录封装原生 `gts.dll` 调用，上层代码不直接接触原生 API
- **线程安全**：所有运动操作共享同一 `_lock` 对象，保证单轴操作的原子性

### 调用层次

```
用户代码
    ↓
MotionControlManager   - 控制器管理（Open/Close/AddAxis）
    ↓
AxisController          - 轴控制门面（使能/状态/报警）
    ↓ (10 个子模块)
AxisHome / AxisMotion*  - 各运动模式实现（GtnErrorHelper.ThrowIfError 统一异常处理）
    ↓
Source/*.cs             - P/Invoke 静态 extern 声明（GTN/gts.dll）
    ↓
gts.dll (固高 SDK)      - 原生 32 位运动控制 DLL
```

## GenMotionEasy API 参考

### MotionControlManager（控制器管理器）

控制器生命周期入口，位于 `GenMotionEasy.Motion` 命名空间。

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `Open()` | `bool` | 打开控制器（`GTN_Open(5,1)`），执行 `GTN_Reset` |
| `Close()` | `void` | 停止所有轴（`GTN_Stop`） |
| `EcatLoad()` | `short` | 终止并重新初始化 EtherCAT 通信 |
| `EcatState(out short state)` | `short` | 查询 EtherCAT 总线就绪状态 |
| `EcatStart()` | `short` | 启动 EtherCAT 通信 |
| `AddAxis(short axisId, short core)` | `void` | 注册新轴，创建 `AxisController` 实例 |
| `GetAxisController(int axisId)` | `AxisController` | 获取已注册的轴控制器（不存在则抛异常） |
| `GetAxisCount()` | `int` | 返回已注册轴数量 |

```csharp
var motion = new MotionControlManager();
motion.Open();                          // 连接控制器
motion.AddAxis(1, 1);                   // 注册轴 1（core=1）
motion.AddAxis(2, 1);                   // 注册轴 2
var axis = motion.GetAxisController(1); // 获取轴控制器
```

### AxisController（轴控制器门面）

位于 `GenMotionEasy.Motion.Control`，实现 `IAxisController` 接口。

#### 基本操作

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `EnableAxis()` | `short` | 轴使能（`GTN_AxisOn`） |
| `DisableAxis()` | `short` | 轴去使能（`GTN_AxisOff`） |
| `Restart()` | `short` | 控制器复位（`GTN_Reset`） |
| `LoadConfig(string path)` | `short` | 加载配置文件（`GTN_LoadConfig`） |
| `ClearAlarm(short count = 1)` | `short` | 清除轴报警（`GTN_ClrSts`） |
| `StopAxis()` | `void` | 平滑停止轴（`GTN_Stop`） |
| `EcatLoad()` | `short` | 加载 EtherCAT 总线 |
| `EcatState()` | `short` | 查询 EtherCAT 状态 |
| `EcatStart()` | `short` | 启动 EtherCAT |
| `GetEcatStatus()` | `StatusInfo?` | 获取轴完整状态快照 |
| `GetRemainingDistance(int targetPos)` | `int` | 计算到目标位置的剩余距离 |
| `WaitAxisStop(...)` | `Task<bool>` | 异步等待轴到达目标位置并停止 |

#### 运动模式子模块（属性）

每个属性返回对应的接口实例，按需调用：

| 属性 | 接口 | 说明 |
|------|------|------|
| `AxisHome` | `IAxisHome` | 轴回零（支持 8 种回零方式 + 探针） |
| `Point` | `IPointMotion` | Trap 点位运动（相对定位、绝对定位） |
| `Move` | `IMoveMotion` | 绝对/速度运动（梯形/S 型加/减速） |
| `Jog` | `IJogMotion` | 点动/Jog 运动 |
| `PT` | `IPTMotion` | 位置-时间（PT）模式运动 |
| `PVT` | `IPVTMotion` | 位置-速度-时间（PVT）插值运动 |
| `Follow` | `IFollowMotion` | 电子跟随（从轴跟随主轴） |
| `FollowEx` | `IFollowExMotion` | 扩展跟随（百分比曲线、IO 缓冲） |
| `Gear` | `IGearMotion` | 电子齿轮（主从轴比例跟随） |
| `Interp` | `IInterpMotion` | 多轴插补（2/3/4D 直线、圆弧、螺旋线） |

### 运动模式详解

#### 1. 点位运动（IPointMotion）

Trap 速度曲线点位运动，自动使能未激活的轴。

```csharp
// 相对定位（从当前位置移动指定距离）
axis.Point.PointMove(pos: 50000, vel: 10000, acc: 5000, dec: 5000);

// 绝对定位（移动到绝对坐标位置）
axis.Point.PointAbsMove(pos: 100000, vel: 20000, acc: 10000, dec: 10000);
```

#### 2. 绝对/速度运动（IMoveMotion）

支持梯形和 S 型加减速的绝对定位和定速运动。

```csharp
// 绝对定位（梯形加减速）
axis.Move.MoveAbsolute(pos: 100000, vel: 20000, acc: 5000, dec: 5000);

// 绝对定位（S 型加减速，可指定起止速度和加加速度百分比）
axis.Move.MoveAbsoluteEx(pos: 100000, vel: 20000, acc: 5000, dec: 5000,
    velStart: 0, velEnd: 0, accStartPercent: 0.5, decEndPercent: 0.5);

// 正方向定速运动
axis.Move.MoveVelocityPositive(vel: 10000, acc: 5000, dec: 5000);

// 负方向定速运动
axis.Move.MoveVelocityNegative(vel: 10000, acc: 5000, dec: 5000);
```

#### 3. Jog 运动（IJogMotion）

```csharp
axis.Jog.SetJogMode(acc: 5000, dec: 5000);
axis.Jog.JogMove(vel: 10000);
axis.Jog.Stop();
```

#### 4. 回零（IAxisHome）

支持多回零方式，异步等待完成.

```csharp
// 参数：(switchSpeed, indexSpeed, acc, method, offset, probeFunction)
await axis.AxisHome.Home(
    switchSpeed: 10000,  // 高速找开关速度
    indexSpeed: 1000,    // 低速找 Z 相速度
    acc: 5000,           // 加速度
    method: 0,           // 回零方式（0/1/2/6/7 等）
    offset: 0);          // 回零偏移量

bool done = await axis.AxisHome.HomeDone(timeoutMinutes: 10);
```

#### 5. PT 运动（IPTMotion）

位置-时间模式，支持静态/动态 FIFO 模式，可配置梯形轮廓加速段。

```csharp
// 静态模式
axis.PT.SetStaticMode();
axis.PT.SetupTrapezoidProfile(accelPos: 1000, evenPos: 5000, decelPos: 1000, timePerSegment: 10);
axis.PT.SetLoop(1);
axis.PT.Start();
```

#### 6. PVT 插值运动（IPVTMotion）

位置-速度-时间模式，支持 4 种表模式（PVT/Complete/Percent/Continuous），最多 32 张表 × 1024 点。

```csharp
// 快速启动 PVT 模式
axis.PVT.QuickStart(
    tableId: 0,
    count: 3,
    time: new[] { 0.0, 500.0, 1000.0 },
    pos: new[] { 0.0, 50000.0, 100000.0 },
    vel: new[] { 0.0, 100.0, 0.0 },
    loop: 0);
```

#### 7. 电子跟随（IFollowMotion）

从轴跟随主轴位置的电子凸轮功能，支持梯形轮廓快速启动。

```csharp
// 梯形轮廓快速启动
axis.Follow.QuickStartTrapezoid(
    masterAxis: 1,
    accelMasterPos: 2000,   accelSlavePos: 1000,
    evenMasterPos: 10000,   evenSlavePos: 5000,
    decelMasterPos: 2000,   decelSlavePos: 1000,
    loop: 1);
```

#### 8. 扩展跟随（IFollowExMotion）

支持 S 型曲线加减速、百分比模式、缓冲区 DO/DI/延时控制。

```csharp
axis.FollowEx.SetMode();
axis.FollowEx.SetMaster(masterIndex: 1, masterType: 0);
axis.FollowEx.PushDataPercent(masterSegment: 1000, slaveSegment: 500,
    percent: 100, fifo: 0);
axis.FollowEx.BufferDoBit(doType: 1, index: 1, value: 1);
axis.FollowEx.BufferDelay(delayTime: 100);
axis.FollowEx.Start();
```

#### 9. 电子齿轮（IGearMotion）

主从轴比例跟随，支持编码器/规划器/轴三种主轴类型，带离合器区。

```csharp
// 1:1 齿轮比
axis.Gear.QuickStart(masterAxis: 1, masterEven: 1, slaveEven: 1);

// 2:1 减速比
axis.Gear.SetReductionRatio(2);
axis.Gear.SetMasterAsAxis(masterIndex: 1);
axis.Gear.Start();
```

#### 10. 多轴插补（IInterpMotion）

2/3/4 坐标系直线、圆弧、螺旋线插补，支持离线与实时模式，集成了前瞻（Look-Ahead）功能。

```csharp
// 建立 2D 坐标系
axis.Interp.SetupCrd2D(profile1: 1, profile2: 2,
    synVelMax: 50000, synAccMax: 10000);

// 离线模式下添加线段
axis.Interp.OfflineClear();
axis.Interp.LnXY(x: 10000, y: 5000, synVel: 10000, synAcc: 5000, velEnd: 0);
axis.Interp.ArcXYR(x: 20000, y: 10000, radius: 5000,
    circleDir: 1, synVel: 10000, synAcc: 5000, velEnd: 0);
axis.Interp.LnXY(x: 30000, y: 15000, synVel: 10000, synAcc: 5000, velEnd: 0);
axis.Interp.OfflineStart();

// 实时插补（动态模式）
axis.Interp.SetDynamicBufferMode();
axis.Interp.LnXY(x: 1000, y: 500, synVel: 10000, synAcc: 5000, velEnd: 0);
axis.Interp.LnXY(x: 2000, y: 1000, synVel: 10000, synAcc: 5000, velEnd: 0);
axis.Interp.Start();

// 前瞻（Look-Ahead）
axis.Interp.EnableLookAhead(threshold: 50);
axis.Interp.SetOverride(synVelRatio: 0.8);  // 速度倍率 80%
```

### 状态监控（StatusInfo）

`GetEcatStatus()` 返回 `StatusInfo` 对象，包含完整轴状态：

| 属性 | 类型 | 说明 |
|------|------|------|
| `PlusLimitAlarm` | `bool` | 正限位触发 |
| `MinusLimitAlarm` | `bool` | 负限位触发 |
| `AxisAlarm` | `bool` | 伺服报警 |
| `FollowAlarm` | `bool` | 跟随误差越限 |
| `SmoothStopAlarm` | `bool` | 平滑停止触发 |
| `Scram` | `bool` | 急停触发 |
| `EnableAxis` | `bool` | 轴使能状态 |
| `Planning` | `bool` | 规划器运动中 |
| `MotionType` | `string` | 当前运动模式（Trap/Jog/PT/Gear/Follow/Interp/PVT） |
| `PlannedLocation` | `double` | 规划器位置 |
| `PlannedVel` | `double` | 规划器速度 |
| `PlannedAccVel` | `double` | 规划器加速度 |
| `DriveLocation` | `double` | 编码器/驱动器位置 |
| `DriveVel` | `double` | 编码器/驱动器速度 |
| `DriveAccVel` | `double` | 编码器/驱动器加速度 |
| `FollowErr` | `double` | 跟随误差（规划位置 - 编码器位置） |

```csharp
var status = axis.GetEcatStatus();
Console.WriteLine($"轴使能: {status.EnableAxis}");
Console.WriteLine($"规划位置: {status.PlannedLocation}, 编码器位置: {status.DriveLocation}");
Console.WriteLine($"跟随误差: {status.FollowErr}");
Console.WriteLine($"运动模式: {status.MotionType}");
```

### 异步等待轴停止

```csharp
bool reached = await axis.WaitAxisStop(
    targetPos: 100000,   // 目标位置
    vel: 20000,          // 运动速度
    acc: 5000,           // 加速度
    dec: 5000,           // 减速度
    tolerance: 10,       // 允许偏差
    extraSeconds: 5);    // 额外缓冲秒数
```

### 错误处理

所有底层操作失败时会抛出 `GtnException`，携带错误码和中文描述。

```csharp
try
{
    axis.EnableAxis();
}
catch (GtnException ex)
{
    Console.WriteLine($"错误码 {ex.ErrorCode}: {ex.Message}");
}
// 输出示例: "固高卡操作失败 [EnableAxis] : 主机和运动控制器通讯失败"
```

常见错误码对应：

| 错误码 | 含义 |
|--------|------|
| 0 | 指令执行成功 |
| -1 | 主机和运动控制器通讯失败 |
| -6 | 打开控制器失败 |
| -7 | 运动控制器没有响应 |
| -13 / -14 | 编码器初始化失败 |
| -15 / 15 | 动态库版本不匹配 |

## 快速开始

### 1. 初始化控制器

```csharp
var motion = new MotionControlManager();
if (!motion.Open())
{
    Console.WriteLine("打开控制器失败");
    return;
}
```

### 2. 初始化 EtherCAT 总线

```csharp
motion.EcatLoad();
short state;
for (int i = 0; i < 20; i++)
{
    motion.EcatState(out state);
    if (state == 1) break;
    Thread.Sleep(500);
}
motion.EcatStart();
```

### 3. 添加并配置轴

```csharp
motion.AddAxis(1, 1);
var axis = motion.GetAxisController(1);
axis.EnableAxis();
```

### 4. 执行运动

```csharp
// Trap 点位运动
axis.Point.PointAbsMove(pos: 100000, vel: 20000, acc: 5000, dec: 5000);

// 电子齿轮（从轴跟随主轴）
axis.Gear.QuickStart(masterAxis: 1, masterEven: 2, slaveEven: 1);

// 等待完成
bool done = await axis.WaitAxisStop(100000, 20000, 5000, 5000);
```

### 5. 清理

```csharp
motion.Close();
```

## GenDemo 演示程序

GenDemo 是一个完整的 WPF 应用程序，位于 `GenDemo/` 目录，包含 **9 个功能标签页**：

| 标签页 | 功能 |
|--------|------|
| **轴控制** | 连接/断开控制器、轴使能/去使能、报警清除、急停、EtherCAT 总线控制、加载配置文件 |
| **回零** | 选择回零方式（0/1/2/6/7）、设置速度/加速度/偏移量、异步回零并显示状态 |
| **点位运动** | Trap 点动和绝对定位、显示规划器/编码器位置和跟随误差 |
| **PT 运动** | 静态/动态模式切换、FIFO 操作（小/大存储）、梯形轮廓、正弦速度演示 |
| **PVT 运动** | 4 种表模式切换、选表（0-3）、3 点数据发送与启动 |
| **电子齿轮** | 主轴类型选择（编码器/规划器/轴）、齿轮比设定、离合区、一键 1:1/2:1 |
| **电子跟随** | 梯形轮廓跟随、FIFO 存储切换、加/匀速/减速段参数配置 |
| **插补** | 2D/3D/4D 坐标系、离线线段/圆弧拼接、实时插补、前瞻使能、速度倍率、平滑/急停 |
| **状态监控** | 自动刷新（200ms）或手动刷新、状态标志颜色指示、位置/速度/加速度数据、日志面板 |

UI 使用 `DispatcherTimer` 定时轮询轴状态（轴控制页 300ms，监控页 200ms）。

## 系统要求

- **.NET 8.0 SDK** 或更高版本（GenMotionEasy）
- **.NET 10.0 SDK**（GenDemo，如需编译运行演示）
- **固高运动控制器**及配套驱动和 `gts.dll`
- **Windows 操作系统**（EtherCAT 实时通信）
- **Visual Studio 2022** 或更高版本（推荐）

## 编译与运行

```bash
# 还原解决方案
dotnet restore

# 编译核心库
dotnet build GenMotionEasy/GenMotionEasy.csproj

# 编译并运行 WPF 演示（需管理员权限）
dotnet run --project GenDemo/GenDemo.csproj
```

> **注意**：运行 GenDemo 前须确保已安装固高运动控制器驱动，且 `gts.dll` 在系统路径或程序目录中。

## 常见问题

**Q: `MotionControlManager.Open()` 返回 false？**
A: 检查运动控制器硬件连接，确保已安装固高驱动，确认 `gts.dll` 版本匹配。

**Q: 编译报错 "未能找到 gts.dll"？**
A: GenDemo 强制 `x86` 平台，须确保引用的 `gts.dll` 为 32 位版本，且位于输出目录。

**Q: 轴使能后无法运动？**
A: 检查 EtherCAT 总线状态（`EcatState()`），确认总线就绪（state=1）后再执行运动指令。

**Q: 如何添加轴配置文件（.cfg）？**
A: 使用固高 ConfigTool 软件导出 `.cfg` 文件后，通过 `axis.LoadConfig("path/to/config.cfg")` 加载。

## 许可证

本项目基于 [MIT License](LICENSE) 开源，你可以自由使用、修改、分发本软件，包括用于商业用途。详见 [LICENSE](LICENSE) 文件。
