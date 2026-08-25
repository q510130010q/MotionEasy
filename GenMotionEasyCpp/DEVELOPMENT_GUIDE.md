# GenMotionEasyCpp 开发与集成指南

`GenMotionEasyCpp` 是针对固高（Googol）GTS 系列运动控制卡的 C++ 封装库。它从 C# 版本的 `GenMotionEasy` 移植而来，在保留原有高层控制逻辑的同时，提供 **纯 C API** 以及 **C++ OOP 接口**，旨在方便各类 Windows 桌面开发框架（如 **MFC** 和 **Qt**）进行调用。

---

## 1. 架构与 API 风格

本库导出了两套风格 of API，您可以根据项目偏好进行选择：

### 1.1 纯 C API (通用性好，推荐 Qt 项目)
* **前缀与调用约定**：函数均以 `Gmc_` 开头，采用 `__stdcall` 调用约定，通过 `extern "C"` 导出，完全避免了 C++ 名字粉碎（Name Mangling）问题。
* **文件依赖**：主要引用 [GenMotionEasyCpp.h](file:///D:/Csharp/GenMotionEasy/GenMotionEasyCpp/include/GenMotionEasyCpp/GenMotionEasyCpp.h)。
* **特点**：非常适合通过传统 C 方式隐式链接，或者使用动态加载（`LoadLibrary`）方式加载，对跨编译器或 Qt 环境非常友好。

### 1.2 C++ OOP 接口 (结构化好，推荐 MFC 项目)
* **核心类**：[MotionControlManager](file:///D:/Csharp/GenMotionEasy/GenMotionEasyCpp/include/GenMotionEasyCpp/MotionControlManager.h) (控制器管理器单例式入口) 和 [IAxisController](file:///D:/Csharp/GenMotionEasy/GenMotionEasyCpp/include/GenMotionEasyCpp/IAxisController.h) (轴控制器接口)。
* **设计模式**：通过接口隔离运动模式（如 `Move()` 返回 `IMoveMotion*`，`Jog()` 返回 `IJogMotion*`）。
* **特点**：面向对象设计，能够极大减少轴级状态的管理负担，代码可读性极高。

---

## 2. 准备工作：编译与部署

在集成到其他项目之前，首先需要获取或构建好 `GenMotionEasyCpp` 的二进制产物。

### 2.1 依赖库放置
固高 GTS 控制卡依赖 `gts.lib` 和 `gts.dll`，请将其放置于以下路径：
* **编译期**：将 `gts.lib` 放置在库项目的 `GenMotionEasyCpp/external/` 文件夹中。
* **运行期**：将 `gts.dll` 放置在您最终项目的输出目录（例如 `bin/Release` 或 `Debug`），与您的 `exe` 处于同级目录。

### 2.2 编译库

本项目使用 CMake 生成构建文件，您可以选择使用命令行直接编译，或生成 Visual Studio 解决方案后在 IDE 图形界面中编译。

#### 方法 A：使用 Visual Studio 2022 图形界面编译 (推荐，适合普通开发)
1. 打开命令行（推荐使用 Windows 开始菜单中的 `Developer PowerShell for VS 2022` ），进入项目根目录，生成 VS 解决方案工程：
   ```bash
   cd GenMotionEasyCpp
   mkdir build
   cd build
   # 生成 VS 2022 x64 解决方案工程
   cmake .. -G "Visual Studio 17 2022" -A x64
   ```
2. 进入 `GenMotionEasyCpp/build/` 目录，双击打开新生成的解决方案文件 **`GenMotionEasyCpp.sln`**。
3. 在 Visual Studio 2022 顶部工具栏中，将编译模式由 `Debug` 切换为 **`Release`**，目标平台确认是 **`x64`**（如果第一步编译 32 位生成的是 Win32，则这里选择 `Win32`/`x86`）。
4. 在 VS 右侧“解决方案资源管理器”中，找到核心项目 **`GenMotionEasyCpp`**，鼠标右键点击它并选择 **“生成”**（或者直接在顶部菜单中选择“生成” -> “生成解决方案”）。

#### 方法 B：使用命令行编译
1. 打开命令行，进入项目根目录，生成工程并执行编译：
   ```bash
   cd GenMotionEasyCpp
   mkdir build
   cd build
   cmake .. -G "Visual Studio 17 2022" -A x64
   # 编译 Release 版本
   cmake --build . --config Release
   ```

编译完成后，在 `build/Release/` 目录下会生成：
* `GenMotionEasyCpp.dll`（运行期必须）
* `GenMotionEasyCpp.lib`（隐式链接必须）

---

## 3. MFC 项目集成指南

在 MFC（Microsoft Foundation Classes）项目中，建议根据需求选择 **隐式链接**（最省事）或 **显式链接**（适合无需附带 .lib 的动态加载）。

### 方案 A：隐式链接 (引用 Lib，推荐)

#### 1. 项目属性配置
在您的 MFC vcxproj 项目属性中进行如下设置：
* **附加包含目录**：添加 `GenMotionEasyCpp/include` 所在的路径。
* **附加库目录**：添加包含 `GenMotionEasyCpp.lib` 的路径（如 `build/Release`）。
* **附加依赖项**：在“链接器 -> 输入”中添加 `GenMotionEasyCpp.lib`。

#### 2. 头文件引入与链接
推荐在全局头文件（如 `pch.h` 或 `stdafx.h`）中统一引入：
```cpp
// stdafx.h / pch.h
#pragma once

// 引入 C API 头文件
#include "GenMotionEasyCpp/GenMotionEasyCpp.h"
// 也可以引入 C++ OOP 头文件 (如果使用 OOP 模式)
#include "GenMotionEasyCpp/MotionControlManager.h"

// 自动链接库
#pragma comment(lib, "GenMotionEasyCpp.lib")
```

#### 3. 应用程序生命周期集成
在 MFC 应用程序类（`CWinApp` 派生类）的 `InitInstance` 和 `ExitInstance` 中执行控制卡的开启和关闭。
```cpp
// MyMFCApp.cpp
#include "stdafx.h"
#include "MyMFCApp.h"

BOOL CMyMFCApp::InitInstance() {
    // 1. 初始化并打开控制器 (卡号 0，配置属性 0)
    short ret = Gmc_Open(0, 0);
    if (ret != 0) {
        AfxMessageBox(_T("运动控制卡初始化失败，请检查卡号与驱动连接！"));
        return FALSE;
    }
    
    // 2. 复位并加载总线
    Gmc_Reset(1);      // 复位控制卡
    Gmc_EcatLoad(1);   // 加载 EtherCAT 配置
    Gmc_EcatStart(1);  // 开启 EtherCAT 环路
    
    // 3. 使能轴 0 (示例)
    Gmc_AxisOn(1, 0);

    // 显示主对话框...
    CMyMFCDlg dlg;
    m_pMainWnd = &dlg;
    dlg.DoModal();
    
    return FALSE;
}

int CMyMFCApp::ExitInstance() {
    // 退出时释放控制卡资源，确保安全
    Gmc_AxisOff(1, 0); // 轴去使能
    Gmc_Close();       // 关闭卡
    return CWinApp::ExitInstance();
}
```

---

### 方案 B：显式链接 (免 Lib 动态加载)
如果不想在编译期绑定 `.lib`，可以使用 Windows API 动态加载 DLL，非常适合模块化插件设计。

```cpp
// 定义函数指针类型
typedef short (__stdcall *Gmc_Open_t)(short, short);
typedef short (__stdcall *Gmc_Close_t)();
typedef short (__stdcall *Gmc_AxisOn_t)(short, short);
typedef short (__stdcall *Gmc_SetPos_t)(short, short, long);

void LoadControllerDll() {
    HMODULE hDll = ::LoadLibrary(_T("GenMotionEasyCpp.dll"));
    if (!hDll) {
        AfxMessageBox(_T("无法加载 GenMotionEasyCpp.dll！"));
        return;
    }

    // 获取函数地址
    Gmc_Open_t  Gmc_Open  = (Gmc_Open_t) ::GetProcAddress(hDll, "Gmc_Open");
    Gmc_Close_t Gmc_Close = (Gmc_Close_t)::GetProcAddress(hDll, "Gmc_Close");
    Gmc_AxisOn_t Gmc_AxisOn = (Gmc_AxisOn_t)::GetProcAddress(hDll, "Gmc_AxisOn");
    Gmc_SetPos_t Gmc_SetPos = (Gmc_SetPos_t)::GetProcAddress(hDll, "Gmc_SetPos");

    if (Gmc_Open && Gmc_AxisOn) {
        Gmc_Open(0, 0);
        Gmc_AxisOn(1, 0);
        Gmc_SetPos(1, 0, 50000); // 运动到 50000 脉冲位置
    }
}
```

---

### 方案 C：C++ OOP 接口使用 (面向对象)
如果您倾向于使用类似 C# 的面向对象结构，可以使用高层封装库。
```cpp
// 在您的对话框或业务类中定义成员变量
#include "GenMotionEasyCpp/MotionControlManager.h"
#include "GenMotionEasyCpp/IAxisController.h"

class CMyDlg : public CDialogEx {
private:
    MotionControlManager m_mgr;
    IAxisController*     m_axis0 = nullptr;
    
public:
    void OnInitControlCard() {
        // 开启控制卡管理器
        m_mgr.Open();
        m_mgr.AddAxis(0); // 注册轴 0
        
        m_axis0 = m_mgr.GetAxisController(0);
        m_axis0->EnableAxis(); // 使能轴
    }
    
    void OnMoveButtonClick() {
        if (m_axis0) {
            // 进行绝对运动：目标位置 100000，速度 10000，加速度 1000
            m_axis0->Move()->MoveAbsolute(100000, 10000, 1000);
        }
    }
    
    void OnClose() {
        if (m_axis0) {
            m_axis0->DisableAxis();
        }
        m_mgr.Close();
    }
};
```

> [!TIP]
> **CMake 构建 MFC 项目说明**：
> 如果您习惯使用 CMake 管理 MFC 项目（如本解决方案下的 `DemoMfc`），您需要将 `CMAKE_MFC_FLAG` 设置为 `2` 以启用 MFC 共享库：
> ```cmake
> set(CMAKE_MFC_FLAG 2) # 2 代表在共享 DLL 中使用 MFC
> ```

---

## 4. Qt 项目集成指南

Qt 默认且推荐使用 CMake 作为构建系统，同时也兼容旧的 qmake (`.pro`) 构建。

### 方案 A：CMake 项目集成 (推荐)

在 Qt 的 `CMakeLists.txt` 中，通常将编译好的 `GenMotionEasyCpp` 声明为一个 `IMPORTED`（导入）目标。

#### 1. 配置 `CMakeLists.txt`
```cmake
cmake_minimum_required(VERSION 3.16)
project(MyQtMotionApp LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 寻找 Qt 库
find_package(Qt6 REQUIRED COMPONENTS Widgets Core)

# 定义外部头文件与库文件的相对路径 (根据您的目录调整)
set(MC_INCLUDE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/include")
set(MC_LIB_DIR     "${CMAKE_CURRENT_SOURCE_DIR}/lib")

# 1. 声明并关联 GenMotionEasyCpp 动态库
add_library(GenMotionEasyCpp SHARED IMPORTED)
set_target_properties(GenMotionEasyCpp PROPERTIES
    IMPORTED_IMPLIB   "${MC_LIB_DIR}/GenMotionEasyCpp.lib"
    IMPORTED_LOCATION "${MC_LIB_DIR}/GenMotionEasyCpp.dll"
)
target_include_directories(GenMotionEasyCpp INTERFACE ${MC_INCLUDE_DIR})

# 2. 创建您的可执行程序目标
add_executable(${PROJECT_NAME}
    main.cpp
    mainwindow.cpp
    mainwindow.h
    mainwindow.ui
)

# 3. 链接 Qt 与运动控制库
target_link_libraries(${PROJECT_NAME} PRIVATE
    Qt6::Widgets
    GenMotionEasyCpp
)

# 4. (可选) 自动拷贝 DLL 宏，确保编译后可以直接点击运行
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "${MC_LIB_DIR}/GenMotionEasyCpp.dll"
    $<TARGET_FILE_DIR:${PROJECT_NAME}>
)
# 同样需要把固高的 gts.dll 拷贝过去
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "${MC_LIB_DIR}/gts.dll"
    $<TARGET_FILE_DIR:${PROJECT_NAME}>
)
```

---

### 方案 B：QMake 项目集成 (`.pro`)
如果您的 Qt 项目还在使用经典的 `.pro` 配置文件，可以进行如下配置：

```pro
# 引入运动控制卡头文件目录
INCLUDEPATH += $$PWD/libs/include

# 引入动态链接库的链接导入库 (.lib)
# $$PWD 代表当前 .pro 所在目录
LIBS += -L$$PWD/libs/lib/ -lGenMotionEasyCpp

# 注意：Windows 平台下运行时，需要将 GenMotionEasyCpp.dll 和 gts.dll
# 拷贝至编译生成目录（与生成的 your_app.exe 放在同一个文件夹下）
```

---

### Qt 状态监视与控制最佳实践 (MainWindow 示例)
在运动控制项目中，通常需要在主界面上以较高频率（例如 20ms - 50ms）刷新轴的位置、速度与报警状态。利用 Qt 的 `QTimer` 是最优雅的实现方案。

#### `mainwindow.h` 声明
```cpp
#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <QTimer>

QT_BEGIN_NAMESPACE
namespace Ui { class MainWindow; }
QT_END_NAMESPACE

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

private slots:
    void on_btnOpen_clicked();      // 打开控制卡
    void on_btnAxisOn_clicked();    // 轴使能
    void on_btnMove_clicked();      // 点位运动
    void onRefreshTimerTimeout();   // 定时状态更新槽函数

private:
    Ui::MainWindow *ui;
    QTimer*         m_pRefreshTimer = nullptr;
    bool            m_isOpened = false;
    
    const short     m_cardNo = 1;   // 卡号，通常为 1 或 0
    const short     m_axisId = 0;   // 目标控制轴 ID
};

#endif // MAINWINDOW_H
```

#### `mainwindow.cpp` 实现
```cpp
#include "mainwindow.h"
#include "ui_mainwindow.h"
#include <QMessageBox>
#include <QDebug>

// 引入 C API 头文件
#include "GenMotionEasyCpp/GenMotionEasyCpp.h"

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::MainWindow)
    , m_pRefreshTimer(new QTimer(this))
{
    ui->setupUi(this);

    // 设置定时器 50ms 触发一次
    connect(m_pRefreshTimer, &QTimer::timeout, this, &MainWindow::onRefreshTimerTimeout);
}

MainWindow::~MainWindow()
{
    m_pRefreshTimer->stop();
    if (m_isOpened) {
        Gmc_AxisOff(m_cardNo, m_axisId);
        Gmc_Close();
    }
    delete ui;
}

// 打开控制卡按钮
void MainWindow::on_btnOpen_clicked()
{
    short ret = Gmc_Open(0, 0); // 打开卡
    if (ret != 0) {
        QMessageBox::critical(this, "错误", QString("打开控制卡失败，错误码：%1").arg(ret));
        return;
    }
    
    // 初始化卡配置
    Gmc_Reset(m_cardNo);
    Gmc_EcatLoad(m_cardNo);
    Gmc_EcatStart(m_cardNo);
    
    m_isOpened = true;
    m_pRefreshTimer->start(50); // 启动定时状态刷新
    
    ui->btnOpen->setEnabled(false);
    QMessageBox::information(this, "提示", "运动控制卡加载成功，定时监控已启动。");
}

// 轴使能按钮
void MainWindow::on_btnAxisOn_clicked()
{
    if (!m_isOpened) return;
    Gmc_AxisOn(m_cardNo, m_axisId);
}

// 单轴定位运动
void MainWindow::on_btnMove_clicked()
{
    if (!m_isOpened) return;
    
    double targetPos = ui->spinTargetPos->value(); // 目标位置 (单位：脉冲)
    double speed = 20000;                        // 速度
    double acc = 2000;                           // 加速度
    
    // 使用库内置的绝对运动 API
    short ret = Gmc_MoveAbsolute(m_cardNo, m_axisId, targetPos, speed, acc);
    if (ret != 0) {
        qWarning() << "MoveAbsolute 发送失败，错误码：" << ret;
    }
}

// 周期性状态刷新
void MainWindow::onRefreshTimerTimeout()
{
    if (!m_isOpened) return;
    
    long sts = 0;
    unsigned long clock = 0;
    // 获取轴状态
    Gmc_GetSts(m_cardNo, m_axisId, &sts, 1, &clock);
    
    double prfPos = 0.0;
    // 获取当前规划位置
    Gmc_GetPrfPos(m_cardNo, m_axisId, &prfPos, 1, &clock);
    
    // 更新界面控件数值
    ui->labelPosVal->setText(QString::number(prfPos, 'f', 2));
    
    // 检查伺服使能状态 (固高状态字的第 1 位通常表示使能标志)
    if (sts & 0x200) {
        ui->labelStsVal->setText("已使能 (Axis On)");
        ui->labelStsVal->setStyleSheet("color: green;");
    } else {
        ui->labelStsVal->setText("未使能 (Axis Off)");
        ui->labelStsVal->setStyleSheet("color: red;");
    }
}
```

---

## 5. 部署与常见问题排查 (Deployment & Troubleshooting)

### 5.1 运行时部署清单
打包分发或是在其他电脑部署您的应用程序时，`exe` 同级目录下必须包含以下文件：
1. **`GenMotionEasyCpp.dll`** (本库的构建产物)
2. **`gts.dll`** (固高的核心驱动 DLL)
3. 相关的固高配置文件 (如 `GTS800.cfg`，若您的程序初始化时需要加载特定的配置文件)

### 5.2 常见错误诊断
* **程序启动报错 "找不到 `GenMotionEasyCpp.dll` 或 `gts.dll`"**
  * **解决方案**：请将这两者拷贝至生成的应用程序运行目录（通常为 `Debug/` 或 `Release/` 文件夹），或者将它们的所在路径添加到系统环境变量 `PATH` 中。
* **程序启动时崩溃，错误代码 `0xc000007b`**
  * **原因**：这通常是由于 32位(x86) 与 64位(x64) 冲突所致。固高的 GTS 驱动和您的应用程序必须统一编译平台。
  * **解决方案**：确保您的 MFC/Qt 程序目标架构为 **x64**，且链接的 `gts.lib` / 载入的 `gts.dll` 同样也是 64位 版本。
* **无法打开控制器 (Gmc_Open 返回非 0 报错)**
  * **原因**：常见于没有插控制卡、驱动程序未安装、控制卡号（Card No）选择不匹配。
  * **排查**：确认固高官方工具（如 GTS Demo）是否可以正常连接控制卡。若官方工具亦无法打开，则需检查硬件板卡插槽与驱动连接。
