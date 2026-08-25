# GenMotionEasyCpp

Googol GTS 运动控制卡 C++ DLL 封装库，从 C# GenMotionEasy 移植，同时提供 C API 和 C++ OOP 接口，可供 Qt / MFC 等 Windows 框架调用。

---

## 项目结构

```
GenMotionEasyCpp/
├── CMakeLists.txt                    # CMake 构建配置
├── build/                            # 构建输出 (CMake 生成的 .sln/.vcxproj)
├── external/
│   └── gts.lib                       # Googol GTS 导入库 (需自行放置)
├── include/GenMotionEasyCpp/
│   ├── GenMotionEasyCpp.h            # C API 头文件 (所有 Gmc_ 函数声明)
│   ├── GtnTypes.h                    # 结构体定义 + GTN_ 配置函数声明
│   ├── StatusInfo.h                  # 轴状态信息结构体
│   ├── GtnError.h                    # 异常类 GtnException
│   ├── MotionControlManager.h        # 高层管理器 (单例式入口)
│   ├── AxisController.h              # 轴控制器 (组合了所有运动模式)
│   ├── EcatIO.h                      # EtherCAT IO
│   ├── ExpansionIO.h                 # GLink 扩展 IO
│   ├── IAxisController.h             # 轴控制器接口 (纯虚)
│   ├── IAxisHome.h                   # 回零接口
│   ├── IMoveMotion.h                 # MoveAbsolute/MoveVelocity 接口
│   ├── IJogMotion.h                  # Jog 接口
│   ├── IPTMotion.h                   # PT 点动模式接口
│   ├── IPVTMotion.h                  # PVT 模式接口
│   ├── IGearMotion.h                 # 电子齿轮接口
│   ├── IFollowMotion.h               # 电子凸轮接口
│   ├── IFollowExMotion.h             # 扩展电子凸轮接口
│   ├── IInterpMotion.h               # 插补运动接口
│   └── IPointMotion.h                # 点位运动接口
└── src/
    ├── GenMotionEasyCpp_export.cpp   # C API 实现 (薄封装, 直接调 GTN_)
    ├── AxisController.cpp            # AxisController 实现
    ├── EcatIO.cpp                    # EtherCAT IO 实现
    ├── ExpansionIO.cpp               # GLink 扩展 IO 实现
    ├── MotionControlManager.cpp      # 管理器实现
    └── GtnError.cpp                  # 异常实现
```

### 构建产物

| 文件 | 路径 |
|------|------|
| `GenMotionEasyCpp.dll` | `build/Release/` |
| `GenMotionEasyCpp.lib` | `build/Release/` |
| `GenMotionEasyCpp.exp` | `build/Release/` |

依赖: `gts.dll` (运行时), `gts.lib` (链接时)

---

## API 分层

该库提供两层 API:

### 1. C API (推荐 Qt 调用)

纯 C 函数, `__stdcall` 调用约定, `extern "C"` 导出, 前缀 `Gmc_`。

```c
// 引用头文件
#include "GenMotionEasyCpp/GenMotionEasyCpp.h"

// 链接 lib
#pragma comment(lib, "GenMotionEasyCpp.lib")

// 打开控制器
short ret = Gmc_Open(0, 0);

// 轴控
Gmc_AxisOn(1, 0);
Gmc_SetTrapPrm(1, 0, &prm);
Gmc_PrfTrap(1, 0);
Gmc_SetPos(1, 0, 10000);
```

C API 函数覆盖以下功能域:

| 域 | 函数前缀 | 示例 |
|----|---------|------|
| 控制器生命周期 | `Gmc_Open/Close/Reset` |
| EtherCAT 总线 | `Gmc_EcatLoad/Start/Stop` |
| 轴控 | `Gmc_AxisOn/Off/Stop` |
| 状态查询 | `Gmc_GetSts/GetPos/GetVel` |
| Trap 点位 | `Gmc_PrfTrap/SetTrapPrm` |
| Move | `Gmc_MoveAbsolute/MoveVelocity` |
| Jog | `Gmc_PrfJog/SetJogPrm` |
| PT | `Gmc_PrfPt/PtData/PtStart` |
| PVT | `Gmc_PrfPvt/PvtTable/PvtStart` |
| Gear | `Gmc_PrfGear/SetGearRatio` |
| Follow | `Gmc_PrfFollow/FollowData/FollowStart` |
| 插补 | `Gmc_LnXY/ArcXYR/CrdStart` |
| 回零 | `Gmc_SetHomingMode/StartEcatHoming` |
| PID | `Gmc_SetPid/GetPid` |
| 软限位 | `Gmc_SetSoftLimit/GetSoftLimit` |
| 龙门 | `Gmc_SetGantryMode` |
| EcatIO | `Gmc_EcatIOReadInput/WriteOutput` |
| GLink | `Gmc_GLinkInit/SetGLinkDo/GetGLinkDi` |
| 配置 | `Gmc_SetDiConfig/SetDoConfig/SetAxisConfig` |
| 编码器 | `Gmc_EncOn/EncOff/SetEncoderScale` |

完整函数列表见 `GenMotionEasyCpp.h`。

### 2. C++ OOP 封装 (推荐 MFC 调用)

```cpp
#include "GenMotionEasyCpp/MotionControlManager.h"
#include "GenMotionEasyCpp/IAxisController.h"
#include "GenMotionEasyCpp/IMoveMotion.h"

MotionControlManager mgr;
mgr.Open();
mgr.AddAxis(0);

IAxisController* ctrl = mgr.GetAxisController(0);
ctrl->EnableAxis();
ctrl->Move()->MoveAbsolute(50000, 100000, 10000);
```

---

## MFC 调用方法

### 方式 A: 隐式链接 (推荐)

1. 将 `GenMotionEasyCpp.dll` 放到 exe 目录或系统 PATH 中。
2. 将 `GenMotionEasyCpp.lib` 配置到链接器输入。
3. 在 `stdafx.h` 或需要使用的位置 `#include` 头文件。

```
// stdafx.h 中加入:
#include "GenMotionEasyCpp/GenMotionEasyCpp.h"
#pragma comment(lib, "GenMotionEasyCpp.lib")
```

4. 在 CWinApp 派生类中初始化 / 销毁:

```cpp
// CMFCApp.h
class CMFCApp : public CWinApp {
    virtual BOOL InitInstance();
    virtual int ExitInstance();
};

// CMFCApp.cpp
#include "GenMotionEasyCpp/GenMotionEasyCpp.h"

BOOL CMFCApp::InitInstance() {
    // 打开控制器
    if (Gmc_Open(0, 0) != 0) {
        AfxMessageBox(_T("打开控制器失败"));
        return FALSE;
    }
    Gmc_Reset(1);
    Gmc_EcatLoad(1);
    Gmc_EcatStart(1);
    Gmc_AxisOn(1, 0);
    return CWinApp::InitInstance();
}

int CMFCApp::ExitInstance() {
    Gmc_Close();
    return CWinApp::ExitInstance();
}
```

### 方式 B: 显式链接 (无需 .lib)

```cpp
typedef short (__stdcall *Gmc_Open_t)(short, short);
typedef short (__stdcall *Gmc_Close_t)();
typedef short (__stdcall *Gmc_AxisOn_t)(short, short);
typedef short (__stdcall *Gmc_SetPos_t)(short, short, long);
typedef short (__stdcall *Gmc_PrfTrap_t)(short, short);

HMODULE hDll = LoadLibrary(_T("GenMotionEasyCpp.dll"));
Gmc_Open_t    Gmc_Open    = (Gmc_Open_t)   GetProcAddress(hDll, "Gmc_Open");
Gmc_Close_t   Gmc_Close   = (Gmc_Close_t)  GetProcAddress(hDll, "Gmc_Close");
Gmc_AxisOn_t  Gmc_AxisOn  = (Gmc_AxisOn_t) GetProcAddress(hDll, "Gmc_AxisOn");
Gmc_SetPos_t  Gmc_SetPos  = (Gmc_SetPos_t) GetProcAddress(hDll, "Gmc_SetPos");
Gmc_PrfTrap_t Gmc_PrfTrap = (Gmc_PrfTrap_t)GetProcAddress(hDll, "Gmc_PrfTrap");

Gmc_Open(0, 0);
Gmc_AxisOn(1, 0);
Gmc_SetPos(1, 0, 50000);
Gmc_PrfTrap(1, 0);
```

### 方式 C: C++ OOP 封装

将 `GenMotionEasyCpp.lib` 加入链接依赖，项目 `附加包含目录` 指向 `GenMotionEasyCpp/include`，然后:

```cpp
#include "GenMotionEasyCpp/MotionControlManager.h"

MotionControlManager mgr;
mgr.Open();
mgr.AddAxis(0);

IAxisController* axis = mgr.GetAxisController(0);
axis->EnableAxis();
axis->Move()->MoveAbsolute(50000, 100000, 10000);

// 查询状态
StatusInfo st = axis->GetEcatStatus();
```

---

## Qt 调用方法

### 方式 A: CMake + target_link_libraries (推荐)

在 Qt 项目的 `CMakeLists.txt` 中:

```cmake
cmake_minimum_required(VERSION 3.16)
project(MyQtApp LANGUAGES CXX)

find_package(Qt6 REQUIRED COMPONENTS Widgets Core)

# GenMotionEasyCpp 库
add_library(GenMotionEasyCpp SHARED IMPORTED)
set_target_properties(GenMotionEasyCpp PROPERTIES
    IMPORTED_IMPLIB   "${CMAKE_CURRENT_SOURCE_DIR}/lib/GenMotionEasyCpp.lib"
    IMPORTED_LOCATION "${CMAKE_CURRENT_SOURCE_DIR}/lib/GenMotionEasyCpp.dll"
)
target_include_directories(GenMotionEasyCpp INTERFACE
    "${CMAKE_CURRENT_SOURCE_DIR}/include"
)

# 可执行程序
add_executable(${PROJECT_NAME}
    main.cpp
    MainWindow.cpp
    MainWindow.h
)

target_link_libraries(${PROJECT_NAME} PRIVATE
    Qt6::Widgets
    GenMotionEasyCpp
)
```

**请将 `GenMotionEasyCpp.dll` 放到 exe 运行目录或在 PATH 中。**

### 方式 B: 手动配置 (qmake .pro)

```pro
# .pro 文件
INCLUDEPATH += $$PWD/../GenMotionEasyCpp/include
LIBS += $$PWD/../GenMotionEasyCpp/lib/GenMotionEasyCpp.lib

# 运行时需将 GenMotionEasyCpp.dll 放在 exe 目录
```

### Qt 使用示例

```cpp
// mainwindow.h
#include <QMainWindow>
#include <QTimer>

class MainWindow : public QMainWindow {
    Q_OBJECT
public:
    MainWindow();
    ~MainWindow();

private slots:
    void onUpdateStatus();

private:
    QTimer* _timer;
};

// mainwindow.cpp
#include "mainwindow.h"
#include "GenMotionEasyCpp/GenMotionEasyCpp.h"

MainWindow::MainWindow()
    : _timer(new QTimer(this))
{
    // 打开控制器
    short ret = Gmc_Open(0, 0);
    if (ret != 0) {
        qFatal("Gmc_Open failed: %d", ret);
    }
    Gmc_Reset(1);
    Gmc_EcatLoad(1);
    Gmc_EcatStart(1);
    Gmc_AxisOn(1, 0);

    // 定时查询状态
    connect(_timer, &QTimer::timeout, this, &MainWindow::onUpdateStatus);
    _timer->start(50); // 50ms 刷新
}

MainWindow::~MainWindow() {
    _timer->stop();
    Gmc_Close();
}

void MainWindow::onUpdateStatus() {
    long sts = 0;
    unsigned long clock = 0;
    Gmc_GetSts(1, 0, &sts, 1, &clock);

    double pos = 0;
    Gmc_GetPrfPos(1, 0, &pos, 1, &clock);

    // 更新 UI
    // ui->labelPos->setText(QString("位置: %1").arg(pos));
}
```

### Qt + OOP 封装

```cpp
#include "GenMotionEasyCpp/MotionControlManager.h"
#include "GenMotionEasyCpp/IAxisController.h"
#include "GenMotionEasyCpp/IMoveMotion.h"

// C++ 封装需要 GenMotionEasyCpp.lib 链接
// 并且运行时需要 gts.dll + GenMotionEasyCpp.dll

MotionControlManager mgr;
mgr.Open();
mgr.AddAxis(0);

IAxisController* axis = mgr.GetAxisController(0);
axis->EnableAxis();
axis->Move()->MoveAbsolute(50000, 100000, 10000);
```

---

## 部署清单

运行时需将以下文件放在 exe 目录或系统 PATH 中:

| 文件 | 来源 |
|------|------|
| `GenMotionEasyCpp.dll` | 本库构建产出 |
| `gts.dll` | Googol GTS SDK (`Gen_Gantry1.6/Gen_Gantry/`) |
| 其他 Googol 运行时 | 按控制器型号可能需要 `gtsmc.dll` 等 |

---

## 构建说明

```bash
cd GenMotionEasyCpp
mkdir build && cd build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release
```

要求:
- Visual Studio 2019 / 2022 (x64)
- CMake >= 3.16
- `external/gts.lib` (从 Googol SDK 复制)
- Googol GTS 运动控制卡硬件及驱动
