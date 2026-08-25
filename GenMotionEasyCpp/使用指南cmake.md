# GenMotionEasyCpp 使用与 Qt CMake 集成指南

本指南旨在指导工业用户如何使用本项目编译生成 32位 (x86) 或 64位 (x64) 的 C++ 动态链接库（DLL），并在 **Qt 5.15.1 (Widgets + CMake)** 项目中一步步完成控制卡配置与“开卡”运行。

---

## 第一步：准备固高官方依赖库

在编译本项目之前，需要先准备好固高控制卡官方提供的 SDK 依赖库。
> [!IMPORTANT]
> **关于动态库名称**：固高官方提供的核心动态库名字为 `gts.dll` 和 `gts.lib`。

1. 获取固高官方提供的 `gts.lib` 文件（区分 32位 和 64位 版本）。
2. 将 `gts.lib` 复制到本项目目录下的 `GenMotionEasyCpp/external/` 文件夹中。
   * **若您需要生成 64位的 DLL**：请放入 64位的 `gts.lib`。
   * **若您需要生成 32位的 DLL**：请放入 32位的 `gts.lib`。

---

## 第二步：编译生成 C++ 动态库 (DLL)

本项目使用 CMake 管理构建。用户可以通过命令行工具和 Visual Studio (2019/2022) 快速编译出对应位数的 DLL。

1. **打开专用命令行**：
   点击 Windows 开始菜单，搜索并打开 **`Developer PowerShell for VS 2022`** (或 `Developer Command Prompt for VS 2022`)，进入本项目的 `GenMotionEasyCpp` 目录：
   ```cmd
   cd D:\Csharp\GenMotionEasy\GenMotionEasyCpp
   ```
2. **创建并进入构建目录**：
   ```cmd
   mkdir build
   cd build
   ```
   *(注：如果提示文件夹已存在，请直接执行 `cd build` 进入。若您想彻底清理旧的编译缓存，可先手动删除已有的 `build` 文件夹再重新创建。)*
3. **根据目标位数，使用 CMake 生成对应的 Visual Studio 解决方案**：
   * **生成 64位 (x64) 动态库解决方案**：
     ```cmd
     cmake .. -G "Visual Studio 17 2022" -A x64
     ```
     *(注：如果您使用的是 VS 2019，请将构建生成器指定为 `"Visual Studio 16 2019"`)*
   * **生成 32位 (x86) 动态库解决方案**：
     ```cmd
     cmake .. -G "Visual Studio 17 2022" -A Win32
     ```
4. **编译项目生成 DLL（提供两种方案）**：

   #### 方法 A：使用 Visual Studio 2022 界面编译 (推荐，最直观)
   1. 进入 `GenMotionEasyCpp/build/` 目录，双击打开生成的解决方案文件 **`GenMotionEasyCpp.sln`**。
   2. 在 Visual Studio 2022 顶部工具栏中，将编译配置从 `Debug` 切换为 **`Release`**，右侧的编译平台确认是 **`x64`**（如果是 32 位则是 `Win32` 或 `x86`）。
   3. 在右侧“解决方案资源管理器”中，找到核心项目 **`GenMotionEasyCpp`**，鼠标右键点击它并选择 **“生成”**（或者直接在顶部菜单栏点击“生成” -> “生成解决方案”）。

   #### 方法 B：使用命令行编译
   在刚刚的命令行中直接运行以下编译构建命令：
   ```cmd
   cmake --build . --config Release
   ```
5. **获取编译产物**：
   编译成功后，在 `GenMotionEasyCpp/build/Release/` 目录下会生成：
   * `GenMotionEasyCpp.dll`（运行期必须的动态库）
   * `GenMotionEasyCpp.lib`（编译链接期需要的静态导入库）

---

## 第三步：搭建 Qt 5.15.1 (CMake) 项目

在 Qt 5.15.1 中开发运动控制界面，首要保证的是**编译器位数与控制卡 DLL 位数必须完全对应**。

> [!WARNING]
> **编译器套件选择避坑**：
> 固高官方提供的 `gts.lib` 以及我们刚刚生成的 `GenMotionEasyCpp.lib` 均是由微软的 MSVC 编译器编译的。
> **在 Qt Creator 中创建项目时，必须选择 MSVC 编译套件（如 Desktop Qt 5.15.1 MSVC2019 64bit 或 32bit），切勿选择 MinGW 套件**。如果误用了 MinGW 套件，会导致编译阶段报大量“未定义的引用 (undefined reference)”链接错误。

1. 打开 Qt Creator，点击 **New Project** 新建项目。
2. 依次选择 **Application (Qt)** -> **Qt Widgets Application**。
3. 命名项目名称为 `QtGenMotionDemo`，并选择存储路径。
4. **构建系统 (Build System) 选择 CMake**（工业项目推荐使用 CMake 代替 qmake）。
5. 在 **Kit Selection（构建套件选择）** 页面：
   * **如果您在第二步编译了 64位 DLL**：请勾选 `Desktop Qt 5.15.1 MSVC2019 64bit`。
   * **如果您在第二步编译了 32位 DLL**：请勾选 `Desktop Qt 5.15.1 MSVC2019 32bit`。
6. 完成项目创建。系统会默认生成 `CMakeLists.txt`、`main.cpp`、`mainwindow.cpp`、`mainwindow.h` 和 `mainwindow.ui`。

---

## 第四步：在 Qt 项目中引入链接库与配置 CMakeLists.txt

### 1. 整理第三方库文件目录
为了保持项目整洁，我们在新建的 Qt 项目根目录下（与 `CMakeLists.txt` 处于同级目录）建立如下文件夹结构：
```
QtGenMotionDemo/
├── 3rdparty/
│   └── GenMotionEasy/
│       ├── include/                 # 存放头文件
│       │   └── GenMotionEasyCpp/    # 拷贝库中整个头文件夹
│       └── lib/                     # 存放 DLL/LIB 库文件
│           ├── GenMotionEasyCpp.lib
│           ├── GenMotionEasyCpp.dll
│           └── gts.dll              # 固高官方核心运行库
```
* 将 `GenMotionEasyCpp/include/` 下的整个 `GenMotionEasyCpp` 文件夹拷贝到 `3rdparty/GenMotionEasy/include/` 中。
* 将刚刚编译生成的 `GenMotionEasyCpp.lib` 和 `GenMotionEasyCpp.dll` 以及固高的 `gts.dll` 拷贝到 `3rdparty/GenMotionEasy/lib/` 中。

### 2. 配置 `CMakeLists.txt`
打开您 Qt 项目中的 `CMakeLists.txt`，在文件尾部追加以下配置代码。
这段配置不仅关联了头文件和导入库，还包含了一条 **POST_BUILD（编译后自动拷贝）** 指令。这样，每次编译完成后，系统会自动把 DLL 复制到 EXE 同级目录，避免了手动拷贝易遗忘的问题。

```cmake
# ==================== 固高运动控制封装库集成配置 ====================

# 1. 定义第三方库路径的变量
set(GEN_MOTION_DIR "${CMAKE_CURRENT_SOURCE_DIR}/3rdparty/GenMotionEasy")

# 2. 将头文件搜索目录包含进来
include_directories("${GEN_MOTION_DIR}/include")

# 3. 声明库并链接到您的可执行程序 (以项目名 QtGenMotionDemo 为例)
# 如果您的项目名称不同，请将 ${PROJECT_NAME} 替换为您的可执行目标名称
target_link_libraries(${PROJECT_NAME} PRIVATE
    "${GEN_MOTION_DIR}/lib/GenMotionEasyCpp.lib"
)

# 4. 自动拷贝动态链接库 (DLL) 到生成的 EXE 运行目录
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "${GEN_MOTION_DIR}/lib/GenMotionEasyCpp.dll"
    "${GEN_MOTION_DIR}/lib/gts.dll"
    $<TARGET_FILE_DIR:${PROJECT_NAME}>
    COMMENT "Auto copying dynamic libraries (GenMotionEasyCpp.dll & gts.dll) to output directory..."
)
```
保存 `CMakeLists.txt`。在 Qt Creator 底部弹出的提示中选择 **保存并执行 CMake 重新配置**。

---

## 第五步：编写极简开卡代码 (Qt Widgets)

本节将指导用户在 UI 界面上放置一个按钮，点击该按钮即可执行“开卡”初始化。

### 1. 设计 UI 界面
1. 双击打开 `mainwindow.ui` 进入图形化界面设计器。
2. 从左侧控件栏拖入一个 **Push Button**（按钮）到主界面上。
3. 双击该按钮，将显示文字改为 `初始化控制卡`。
4. 右键点击该按钮，选择 **转到槽...** (Go to slot...)。
5. 在弹出的对话框中选择 **clicked()** 并点击确定。此时 Qt Creator 会自动在 `mainwindow.cpp` 中生成按钮的点击事件响应函数 `on_pushButton_clicked()`。

### 2. 编写逻辑代码
1. 打开 **`mainwindow.cpp`**，在文件顶部引入必要的头文件：
   ```cpp
   #include "mainwindow.h"
   #include "./ui_mainwindow.h"
   #include <QMessageBox> // 引入弹出提示框类

   // 引入 GenMotionEasyCpp 导出的 C API 头文件
   #include "GenMotionEasyCpp/GenMotionEasyCpp.h"
   ```
2. 在生成的点击事件响应槽函数中，编写开卡逻辑：
   ```cpp
   void MainWindow::on_pushButton_clicked()
   {
       // 1. 打开控制器 (卡号 0，配置参数 0)
       short ret = Gmc_Open(0, 0);
       if (ret != 0) {
           QMessageBox::critical(this, "开卡失败", QString("无法打开控制卡，错误码: %1\n请检查驱动安装或硬件连接！").arg(ret));
           return;
       }

       // 2. 复位控制卡核心 (通常开卡后需执行一次)
       Gmc_Reset(1);

       // 3. 启动 EtherCAT 总线 (以核心 1 为例)
       Gmc_EcatLoad(1);
       Gmc_EcatStart(1);

       QMessageBox::information(this, "开卡成功", "固高控制卡初始化成功！已成功打开控制器并启动 EtherCAT 总线。");
   }
   ```

---

## 第六步：编译与运行

1. 在 Qt Creator 左下角，确认构建模式（例如 **Debug**）。
2. 点击左下角的 **运行** 按钮（绿色三角图标）。
3. **为什么不会闪退？**：因为我们在 `CMakeLists.txt` 中配置了 `POST_BUILD` 自动拷贝指令，编译生成 EXE 的同时，`GenMotionEasyCpp.dll` 和 `gts.dll` 已经静默复制到了您的程序输出目录下，直接运行即可成功。
4. 运行后，点击界面上的 `初始化控制卡` 按钮，如果硬件无异常，即可看到“开卡成功”的弹窗反馈！
