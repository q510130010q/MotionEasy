# MotionEasy

MotionEasy 是一个基于 .NET 的运动控制库，用于封装固高（Googol Technology）运动控制器的底层 API（GTN/GLink），提供简洁、易用的 C# 接口以快速构建多轴运动控制系统。

## 项目结构

| 项目 | 说明 |
|------|------|
| **GenMotionEasy** | .NET 8 核心库，封装固高运动控制器 SDK，提供统一轴控接口 |
| **GenDemo** | .NET 10 WPF 演示程序，展示 GenMotionEasy 的基本用法 |
| **GsnMotionEasy** | netstandard 2.0 占位项目，预留用于 Gsn 硬件扩展 |

## GenMotionEasy 功能

### 轴控制器（AxisController）

通过 `MotionControlManager` 管理多轴，每轴对应一个 `AxisController` 实例，支持以下功能模块：

| 模块 | 说明 |
|------|------|
| **Move** | Trap 点位运动（绝对定位、速度模式、S 型加减速） |
| **PT** | 位置-时间（PT）模式运动 |
| **PVT** | 位置-速度-时间（PVT）插值运动 |
| **Follow** | 电子跟随（从轴跟随主轴位置） |
| **FollowEx** | 扩展跟随（支持百分比、速度比、IO 缓冲） |
| **Gear** | 电子齿轮（主从轴比例跟随） |
| **Interp** | 多轴插补（2/3/4 坐标系直线、圆弧、螺旋线插补，支持离线与实时模式） |
| **Home** | 轴回零（支持多种回零方式与探针功能） |

### 基础操作

- 轴使能 / 去使能
- 参数配置文件加载
- 轴重启、报警清除、停止与急停

### EtherCAT 总线

- 总线初始化（EcatLoad）、启动（EcatStart）、状态查询（EcatState）
- 编码器位置 / 速度 / 加速度读取
- 规划器位置与速度读取
- 跟随误差计算

### 状态监控

`StatusInfo` 提供全面的轴状态信息，包括正负限位报警、伺服报警、跟随误差、平滑停止、急停、轴使能、规划器运动状态、当前运动模式，以及规划器与编码器的位置 / 速度 / 加速度数据。

### 错误处理

内置 `GtnErrorHelper` 提供固高卡操作错误码到中文描述的映射，以及 `GtnException` 异常类型，便于快速定位通讯失败、编码器初始化失败、动态库版本不匹配等常见问题。

## 快速开始

### 1. 初始化控制器

```csharp
var motion = new MotionControlManager();
motion.Open(); // 连接运动控制器
```

### 2. 添加并配置轴

```csharp
motion.AddAxis(1, 1); // axisId=1, core=1
var axis = motion.GetAxisController(1);
axis.EnableAxis();
```

### 3. 执行运动

```csharp
// Trap 点位运动
axis.PointMove(pos: 50000, vel: 10000, acc: 5000, dec: 5000);

// 绝对定位
axis.PointAbsMove(pos: 100000, vel: 20000, acc: 10000, dec: 10000);
```

### 4. 状态监控

```csharp
var status = axis.GetEcatStatus();
Console.WriteLine($"轴使能: {status.EnableAxis}, 规划速度: {status.PlannedVel}");
```

## GenDemo

WPF 演示程序，引用 GenMotionEasy 并在启动时创建 `MotionControlManager` 实例，可作为自定义运动控制界面的开发起点。

## 系统要求

- .NET 8.0 SDK 或更高版本（GenMotionEasy）
- 固高运动控制器及配套驱动（GTN / GLink SDK）
- Windows 系统（EtherCAT 实时通信支持）