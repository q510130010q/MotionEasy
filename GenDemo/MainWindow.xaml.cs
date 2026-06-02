// 引入运动控制相关的模型类（比如轴状态、运动参数等数据结构）
using GenMotionEasy.Model;
// 引入运动控制核心库（运动控制器、轴管理等）
using GenMotionEasy.Motion;
// 引入运动控制中的具体控制功能（点动、插补、齿轮等）
using GenMotionEasy.Motion.Control;
// 引入工具类（比如错误码解析、辅助函数等）
using GenMotionEasy.Tool;
// 引入Windows文件对话框（用于打开配置文件）
using Microsoft.Win32;
// 引入调试相关功能（这里用于输出调试信息到"输出"窗口）
using System.Diagnostics;
// 引入线程支持（用于延时等待EtherCAT总线就绪）
using System.Threading;
// 引入WPF核心命名空间（Window、控件基类等）
using System.Windows;
// 引入WPF控件命名空间（Button、ComboBox、TextBox等）
using System.Windows.Controls;
// 引入WPF界面线程调度器（DispatcherTimer定时器用）
using System.Windows.Threading;

// 当前代码属于"GenDemo"这个命名空间（可以理解为项目的"文件夹"）
namespace GenDemo
{
    // MainWindow是主窗口类，继承自WPF的Window类
    // "partial"表示这个类的代码分散在多个文件中（另一个是XAML）
    public partial class MainWindow : Window
    {
        // 运动控制管理器对象（负责管理所有轴、连接等核心操作）
        // "?"表示这个变量可以为null（还没初始化）
        private MotionControlManager? _motionManager;
        // 定时器：每隔一段时间刷新轴的状态显示（使能、限位等）
        private DispatcherTimer? _statusTimer;
        // 定时器：每隔一段时间刷新监控面板（位置、速度、误差等）
        private DispatcherTimer? _monitorTimer;
        // 当前初始化的轴数量
        private int _axisCount;
        // 当前是否已连接到控制器
        private bool _isConnected;

        // 扩展模块定时器（自动刷新DI/AI状态）
        private DispatcherTimer? _extTimer;
        // EcatIO模块定时器（自动刷新DI显示）
        private DispatcherTimer? _ecatIOTimer;
        // 扩展模块的16个DO按钮
        private Button[] _extDoBtns = new Button[16];
        // 扩展模块的16个DI标签
        private Label[] _extDiLabels = new Label[16];
        // 扩展模块的6个AI标签
        private Label[] _extAiLabels = new Label[6];
        // 扩展模块的6个AO输入框
        private TextBox[] _extAoTbs = new TextBox[6];
        // EcatIO模块的16个DO按钮
        private Button[] _ecatDoBtns = new Button[16];
        // EcatIO模块的16个DI标签
        private Label[] _ecatDiLabels = new Label[16];

        // 构造函数：窗口创建时自动调用
        public MainWindow()
        {
            // 调用系统方法，加载XAML中定义的界面布局和控件
            InitializeComponent();
            // 给窗口的"加载完成"事件挂载处理方法
            // 等价于：窗口完全显示出来后，执行MainWindow_Loaded方法
            Loaded += MainWindow_Loaded;
        }

        // 窗口加载完成后执行的方法（相当于"启动后的初始化"）
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建一个状态刷新定时器：每300毫秒（0.3秒）触发一次
            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            // 给定时器的"触发"事件绑定处理方法StatusTimer_Tick
            _statusTimer.Tick += StatusTimer_Tick;

            // 创建一个监控刷新定时器：每200毫秒（0.2秒）触发一次
            _monitorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            // 给定时器的"触发"事件绑定处理方法MonitorTimer_Tick
            _monitorTimer.Tick += MonitorTimer_Tick;

            // 把8个轴的名字（"轴1"到"轴8"）填充到所有下拉选择框中
            PopulateAxisComboBoxes();

            // 创建扩展模块的IO控件（DO按钮、DI标签、AI标签、AO输入框）
            CreateExpansionIOControls();
            // 创建EcatIO模块的IO控件（DO按钮、DI标签）
            CreateEcatIOControls();
        }

        // 把所有下拉框都填上"轴1"~"轴8"的选项
        private void PopulateAxisComboBoxes()
        {
            // 准备8个轴的中文名称数组
            var items = new[] { "轴1", "轴2", "轴3", "轴4", "轴5", "轴6", "轴7", "轴8" };
            // 遍历第一组下拉框（轴控制、回零、点位运动、PT、PVT、齿轮从轴、跟随、监控）
            foreach (var cbo in new[] { CboAxisCtrl, CboHomeAxis, CboMoveAxis, CboPtAxis, CboPvtAxis, CboGearSlave, CboFollowAxis, CboMonitorAxis })
            {
                // 把轴名数组设置为下拉框的选项列表
                cbo.ItemsSource = items;
                // 默认选中第一个（轴1）
                cbo.SelectedIndex = 0;
            }
            // 遍历第二组下拉框（插补的4个轴选择）
            foreach (var cbo in new[] { CboInterpAxis1, CboInterpAxis2, CboInterpAxis3, CboInterpAxis4 })
            {
                // 同样填充轴名选项，但不设置默认选中项
                cbo.ItemsSource = items;
            }
        }

        // 日志输出方法：在界面上显示一条带时间戳的消息
        private void Log(string msg)
        {
            // 获取当前系统时间，格式化为"时:分:秒.毫秒"
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            // 在日志文本框（TxtLog）末尾追加一条日志（带时间戳和换行）
            TxtLog.AppendText($"[{time}] {msg}\n");
            // 自动滚动日志文本框到底部，保证最新日志可见
            TxtLog.ScrollToEnd();
            // 在界面底部的状态标签（LblStatus）上显示当前操作的消息
            LblStatus.Text = msg;
        }

        // 设置界面上的连接状态（已连接/未连接），更新按钮、文字、颜色
        private void SetConnectedState(bool connected)
        {
            // 保存连接状态到成员变量
            _isConnected = connected;
            // 已连接时禁用"打开"按钮（不能再点），断开时启用
            BtnOpen.IsEnabled = !connected;
            // 已连接时启用"关闭"按钮，断开时禁用
            BtnClose.IsEnabled = connected;
            // 已连接时启用"初始化轴"按钮
            BtnInitAxes.IsEnabled = connected;
            // 已连接时禁用轴数量下拉框（初始化后不能再改数量）
            CboAxisCount.IsEnabled = !connected;
            // 在界面上显示"已连接"或"未连接"的文字
            LblCardStatus.Content = connected ? "已连接" : "未连接";
            // 根据连接状态设置文字颜色：绿色=已连接，红色=未连接
            LblCardStatus.Foreground = connected
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        }

        // 获取指定轴号的轴控制器对象
        // axisId: 轴的编号（从0开始），默认-1表示使用0号轴
        private AxisController GetAxis(int axisId = -1)
        {
            // 如果运动管理器还没创建（没打开控制器），就抛出错误
            if (_motionManager == null)
                throw new InvalidOperationException("控制器未打开");
            // 如果没传轴号或传了负数，默认用0号轴
            if (axisId < 0) axisId = 0;
            // 从管理器获取并返回指定编号的轴控制器
            return _motionManager.GetAxisController(axisId);
        }

        // 安全版的获取轴控制器（额外检查轴是否已初始化）
        private AxisController GetAxisSafe(int axisId = -1)
        {
            // 如果管理器为空，或者轴数量为0（还没初始化），就报错
            if (_motionManager == null || _motionManager.GetAxisCount() == 0)
                throw new InvalidOperationException("控制器未打开或轴未初始化");
            // 默认使用0号轴
            if (axisId < 0) axisId = 0;
            // 返回轴控制器
            return _motionManager.GetAxisController(axisId);
        }

        // 获取下拉框中当前选中的是第几个轴（从1开始，因为用户看到的是"轴1"~"轴8"）
        private int SelectedAxis(ComboBox cbo)
        {
            // SelectedIndex从0开始，所以要+1
            return cbo.SelectedIndex + 1;
        }

        #region 连接与初始化（折叠区域，方便代码折叠）

        // 点击"打开控制器"按钮时执行
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            // try-catch: 尝试执行代码，如果出错就捕获异常，防止程序崩溃
            try
            {
                // 创建一个新的运动控制管理器实例
                _motionManager = new MotionControlManager();
                // 尝试打开控制器（返回true成功，false失败）
                if (!_motionManager.Open())
                {
                    // 打开失败，在日志中显示错误
                    Log("打开控制器失败");
                    // 不再继续往下执行
                    return;
                }
                // 添加1号轴（参数1=轴ID，参数1=类型）
                _motionManager.AddAxis(1, 1);
                // 添加2号轴
                _motionManager.AddAxis(2, 1);
                // 添加3号轴
                _motionManager.AddAxis(3, 1);
                // 加载EtherCAT总线配置（让控制器识别总线上有哪些设备）
                _motionManager.EcatLoad();
                // 定义一个变量记录总线状态（0=未就绪，1=就绪）
                short state = 0;
                // 循环最多20次（每次等500ms，总共最多等10秒）
                for (int i = 0; i < 20; i++)
                {
                    // 等待500毫秒，给总线一点时间初始化
                    Thread.Sleep(500);
                    // 查询当前EtherCAT总线状态，结果写到state变量中
                    _motionManager.EcatState(out state);
                    // 如果状态变为1（就绪），提前跳出循环，不用等了
                    if (state == 1) break;
                }
                // 循环结束后检查：如果总线还没就绪
                if (state != 1)
                {
                    // 提示用户总线没准备好
                    Log("EtherCAT总线未就绪");
                    // 停止执行
                    return;
                }

                // 总线就绪了，启动EtherCAT总线通信（开始正常收发数据）
                _motionManager.EcatStart();
                // 更新界面状态为"已连接"
                SetConnectedState(true);
                // 日志显示打开成功
                Log("控制器打开成功");
            }
            catch (Exception ex)
            {
                // 如果上面任何一步出错，把异常信息显示到日志
                Log($"打开控制器异常: {ex.Message}");
            }
        }

        // 点击"关闭控制器"按钮时执行
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 如果状态定时器不为空，就停止它
                _statusTimer?.Stop();
                // 如果监控定时器不为空，就停止它
                _monitorTimer?.Stop();
                // 把运动管理器设为null（断开连接，释放资源）
                _motionManager = null;
                // 更新界面状态为"未连接"
                SetConnectedState(false);
                // 日志显示已关闭
                Log("控制器已关闭");
            }
            catch (Exception ex)
            {
                Log($"关闭控制器异常: {ex.Message}");
            }
        }

        // 点击"初始化轴"按钮时执行
        private void BtnInitAxes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 从下拉框中获取用户选择的轴数量（SelectedIndex=0表示1个轴，+1后=1）
                _axisCount = CboAxisCount.SelectedIndex + 1;
                // 循环创建指定数量的轴
                for (int i = 0; i < _axisCount; i++)
                {
                    // 添加轴，轴号从1开始递增，类型固定为1
                    _motionManager!.AddAxis((short)(i + 1), 1);
                }
                // 日志显示成功初始化了几个轴
                Log($"已初始化 {_axisCount} 个轴");

                // 启动状态刷新定时器（开始每隔300ms刷新一次轴状态）
                _statusTimer?.Start();
                // 启动监控刷新定时器（开始每隔200ms刷新一次监控数据）
                _monitorTimer?.Start();
            }
            catch (Exception ex)
            {
                Log($"初始化轴异常: {ex.Message}");
            }
        }

        #endregion

        #region 轴控制

        // 点击"使能"按钮：给轴通电，让电机进入可运动状态
        private void BtnEnable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取当前选中的轴控制器
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                // 调用使能命令，返回错误码
                var rtn = axis.EnableAxis();
                // 检查错误码，如果有错误就抛出异常（ThrowIfError是工具方法）
                GtnErrorHelper.ThrowIfError(rtn, "使能");
                // 使成功，记录日志
                Log($"轴{CboAxisCtrl.SelectedIndex + 1} 使能成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"关闭使能"按钮：断开电机电源，轴失去保持力
        private void BtnDisable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var rtn = axis.DisableAxis();
                GtnErrorHelper.ThrowIfError(rtn, "关闭使能");
                Log($"轴{CboAxisCtrl.SelectedIndex + 1} 关闭使能成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"复位"按钮：复位轴的错误状态
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var rtn = axis.Restart();
                GtnErrorHelper.ThrowIfError(rtn, "复位");
                Log($"轴{CboAxisCtrl.SelectedIndex + 1} 复位成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"清除报警"按钮：清除轴的报警标志
        private void BtnClearAlarm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var rtn = axis.ClearAlarm();
                GtnErrorHelper.ThrowIfError(rtn, "清除报警");
                Log($"轴{CboAxisCtrl.SelectedIndex + 1} 清除报警成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"紧急停止"按钮：立即停止轴运动
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                axis.StopAxis();
                Log($"轴{CboAxisCtrl.SelectedIndex + 1} 紧急停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"加载EtherCAT"按钮：重新加载EtherCAT总线配置
        private void BtnEcatLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var rtn = axis.EcatLoad();
                GtnErrorHelper.ThrowIfError(rtn, "加载EtherCAT总线");
                Log("EtherCAT总线加载成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"启动EtherCAT"按钮：启动EtherCAT总线通信
        private void BtnEcatStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var rtn = axis.EcatStart();
                GtnErrorHelper.ThrowIfError(rtn, "启动EtherCAT总线");
                Log("EtherCAT总线启动成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"查询EtherCAT状态"按钮：查看当前总线状态
        private void BtnEcatState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var state = axis.EcatState();
                Log($"EtherCAT总线状态: {state}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"加载配置"按钮：从文件加载轴的配置文件
        private void BtnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建一个打开文件对话框，过滤显示配置文件（.cfg/.ini/.txt）
                var dialog = new OpenFileDialog { Filter = "配置文件|*.cfg;*.ini;*.txt|所有文件|*.*" };
                // 如果用户点击了"打开"按钮（不是取消）
                if (dialog.ShowDialog() == true)
                {
                    var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                    // 调用轴控制器的LoadConfig方法，传入文件路径
                    var rtn = axis.LoadConfig(dialog.FileName);
                    GtnErrorHelper.ThrowIfError(rtn, "加载配置");
                    Log($"配置文件加载成功: {dialog.FileName}");
                }
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 状态定时器触发时执行：刷新界面上的轴状态显示
        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // 如果没连接、管理器为空、或者轴数量为0，就跳过本次刷新
                if (!_isConnected || _motionManager == null || _motionManager.GetAxisCount() == 0)
                    return;

                // 获取当前轴控制区域选中的轴
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                // 获取轴的EtherCAT状态信息（运动类型、使能、限位等）
                var status = axis.GetEcatStatus();
                // 如果获取不到状态信息，跳过
                if (status == null) return;

                // 显示当前运动类型（如：停止、点位、JOG等）
                TxtMotionType.Text = status.MotionType;
                // 显示使能状态：已使能 / 未使能
                TxtEnableState.Text = status.EnableAxis ? "已使能" : "未使能";
                // 显示规划状态：运动中 / 停止
                TxtPlanning.Text = status.Planning ? "运动中" : "停止";
                // 显示正限位报警：触发 / 正常
                TxtPlusLimit.Text = status.PlusLimitAlarm ? "触发" : "正常";
                // 显示负限位报警：触发 / 正常
                TxtMinusLimit.Text = status.MinusLimitAlarm ? "触发" : "正常";
                // 显示轴报警：报警 / 正常
                TxtAxisAlarm.Text = status.AxisAlarm ? "报警" : "正常";
                // 显示跟随误差报警：报警 / 正常
                TxtFollowAlarm.Text = status.FollowAlarm ? "报警" : "正常";
                // 显示平滑停止报警：触发 / 正常
                TxtSmoothStop.Text = status.SmoothStopAlarm ? "触发" : "正常";
                // 显示急停状态：触发 / 正常
                TxtScram.Text = status.Scram ? "触发" : "正常";
            }
            catch { }
        }

        #endregion

        #region 回零

        // 点击"开始回零"按钮：让轴自动寻找原点位置
        // "async"表示这个方法里用了"await"（异步等待，不会卡住界面）
        private async void BtnStartHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取选中的轴控制器
                var axis = GetAxis(SelectedAxis(CboHomeAxis));
                // 获取用户选择的回零模式（从下拉框的Tag属性中读取）
                var tag = ((ComboBoxItem)CboHomeMethod.SelectedItem).Tag as string ?? "0";
                // 把回零模式从字符串转为数字
                var method = short.Parse(tag);
                // 读取用户输入的"高速"（接近原点时的速度）
                var switchSpeed = double.Parse(TxtHomeSwitchSpeed.Text);
                // 读取用户输入的"低速"（找Z相信号时的速度）
                var indexSpeed = double.Parse(TxtHomeIndexSpeed.Text);
                // 读取用户输入的加速度
                var acc = double.Parse(TxtHomeAcc.Text);
                // 读取用户输入的偏移量（找到原点后再偏移多少）
                var offset = int.Parse(TxtHomeOffset.Text);

                // 在界面上显示"回零进行中..."（橙色文字）
                TxtHomeStatus.Text = "回零进行中...";
                TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                // 禁用按钮，防止重复点击
                BtnStartHome.IsEnabled = false;

                // 异步执行回零操作（await让界面在回零过程中不卡死）
                await axis.AxisHome.Home(switchSpeed, indexSpeed, acc, method, offset);
                // 异步等待回零完成（最多等10秒），返回true=成功，false=超时/失败
                var done = await axis.AxisHome.HomeDone(10);

                // 如果回零成功
                if (done)
                {
                    // 显示"回零完成!"（绿色文字）
                    TxtHomeStatus.Text = "回零完成!";
                    TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
                    Log($"轴{CboHomeAxis.SelectedIndex + 1} 回零完成");
                }
                else
                {
                    // 回零失败或超时，显示红色错误
                    TxtHomeStatus.Text = "回零超时或失败";
                    TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    Log($"轴{CboHomeAxis.SelectedIndex + 1} 回零失败");
                }
            }
            catch (Exception ex)
            {
                // 如果回零过程发生异常，显示异常信息（红色）
                TxtHomeStatus.Text = $"回零异常: {ex.Message}";
                TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                Log(ex.Message);
            }
            finally
            {
                // 不管成功还是失败，最终都要重新启用按钮，让用户可以再次点击
                BtnStartHome.IsEnabled = true;
            }
        }

        // 点击"重置回零状态"按钮：清除回零完成标志
        private void BtnResetHomeStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboHomeAxis));
                // 调用重置回零状态的方法
                axis.AxisHome.ResetHomeStatus();
                TxtHomeStatus.Text = "回零状态已重置";
                TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 点位运动

        // 点击"点位运动"按钮：以梯形速度曲线运动到指定位置
        private void BtnPointMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboMoveAxis));
                // 读取用户输入的目标位置（脉冲数）
                var pos = int.Parse(TxtMovePos.Text);
                // 读取用户输入的目标速度
                var vel = double.Parse(TxtMoveVel.Text);
                // 读取用户输入的加速度
                var acc = double.Parse(TxtMoveAcc.Text);
                // 读取用户输入的减速度
                var dec = double.Parse(TxtMoveDec.Text);

                // 执行点位运动（相对运动，从当前位置移动pos个脉冲）
                axis.Point.PointMove(pos, vel, acc, dec);
                Log($"轴{CboMoveAxis.SelectedIndex + 1} Trap点位运动: pos={pos}, vel={vel}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"绝对定位"按钮：运动到坐标系中的绝对位置
        private void BtnAbsMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboMoveAxis));
                var pos = int.Parse(TxtMovePos.Text);
                var vel = double.Parse(TxtMoveVel.Text);
                var acc = double.Parse(TxtMoveAcc.Text);
                var dec = double.Parse(TxtMoveDec.Text);

                // 执行绝对定位运动（以原点为参考，运动到pos位置）
                axis.Point.PointAbsMove(pos, vel, acc, dec);
                Log($"轴{CboMoveAxis.SelectedIndex + 1} 绝对定位: pos={pos}, vel={vel}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"停止"按钮：立即停止点位运动
        private void BtnMoveStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboMoveAxis));
                axis.StopAxis();
                Log($"轴{CboMoveAxis.SelectedIndex + 1} 停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region PT运动

        // 模式选择变化时的占位事件（这里什么也不做）
        private void RdoPtMode_Checked(object sender, RoutedEventArgs e)
        {
        }

        // 点击"推送数据"按钮：向PT缓冲区添加一个位置-时间数据点
        private void BtnPtPush_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPtAxis));
                // 读取用户输入的位置值
                var pos = double.Parse(TxtPtPos.Text);
                // 读取用户输入的时间值（毫秒）
                var time = int.Parse(TxtPtTime.Text);
                // 获取用户选择的段类型（如：直线段、圆弧段等）
                short segType = (short)CboPtSegType.SelectedIndex;

                // 把位置、时间、段类型推送到PT缓冲区
                axis.PT.PushData(pos, time, segType);
                // 查询PT缓冲区还剩多少空间
                var space = axis.PT.GetSpace();
                // 显示剩余空间
                TxtPtSpace.Text = $"剩余空间: {space}";
                Log($"PT推送数据: pos={pos}, time={time}ms, type={CboPtSegType.Text}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"清除数据"按钮：清空PT缓冲区中的所有数据
        private void BtnPtClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPtAxis));
                axis.PT.Clear();
                Log("PT数据已清除");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"查询空间"按钮：查看PT缓冲区还有多少空间可以推送数据
        private void BtnPtSpace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPtAxis));
                var space = axis.PT.GetSpace();
                TxtPtSpace.Text = $"剩余空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"启动"按钮：开始执行PT运动
        private void BtnPtStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPtAxis));

                // 如果用户选了"静态模式"
                if (RdoPtStatic.IsChecked == true)
                    axis.PT.SetStaticMode();   // 静态模式：数据全部推完后再开始运动
                else
                    axis.PT.SetDynamicMode();   // 动态模式：边推数据边运动

                // 如果用户选了"小内存"模式
                if (RdoPtMemSmall.IsChecked == true)
                    axis.PT.SetMemorySmall();   // 使用较小的缓冲区
                else
                    axis.PT.SetMemoryLarge();   // 使用较大的缓冲区

                // 读取用户设置的循环次数
                var loop = int.Parse(TxtPtLoop.Text);
                axis.PT.SetLoop(loop);

                // 启动PT运动
                axis.PT.Start();
                TxtPtStatus.Text = "PT运动进行中...";
                Log($"轴{CboPtAxis.SelectedIndex + 1} PT运动启动, 循环={loop}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"停止"按钮：停止PT运动
        private void BtnPtStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPtAxis));
                axis.StopAxis();
                TxtPtStatus.Text = "PT运动已停止";
                Log("PT运动停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region PVT运动

        // 点击"启动"按钮：开始执行PVT运动（位置-速度-时间插值运动）
        private void BtnPvtStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPvtAxis));
                // 获取用户选择的数据表ID
                var tableId = (short)CboPvtTable.SelectedIndex;
                // 读取循环次数
                var loop = int.Parse(TxtPvtLoop.Text);
                // 读取总时间（毫秒），转成秒
                var time = double.Parse(TxtPvtTime.Text) / 1000.0;

                // 读取3个位置点
                var pos1 = double.Parse(TxtPvtPos1.Text);
                var pos2 = double.Parse(TxtPvtPos2.Text);
                var pos3 = double.Parse(TxtPvtPos3.Text);
                // 读取3个速度点
                var vel1 = double.Parse(TxtPvtVel1.Text);
                var vel2 = double.Parse(TxtPvtVel2.Text);
                var vel3 = double.Parse(TxtPvtVel3.Text);

                // 设置PVT模式
                axis.PVT.SetMode();
                // 设置循环次数
                axis.PVT.SetLoop(loop);
                // 选择使用哪个数据表
                axis.PVT.SelectTable(tableId);

                // 根据用户选的PVT模式执行不同的启动方式
                switch (CboPvtMode.SelectedIndex)
                {
                    case 0: // 快速启动模式
                        axis.PVT.QuickStart(tableId, 3,              // 表ID，3个数据点
                            new[] { 0.0, time, time * 2 },           // 时间数组
                            new[] { 0.0, pos1, pos2 },               // 位置数组
                            new[] { 0.0, vel1, vel2 }, loop);        // 速度数组
                        break;
                    case 1: // 快速启动（完整参数模式）
                        axis.PVT.QuickStartComplete(tableId, 3,
                            new[] { 0.0, time, time * 2 },
                            new[] { 0.0, pos1, pos2 },
                            new[] { 0.0, 0.5, 0.0 },                 // 位置比例
                            new[] { 0.5, 0.5, 0.5 },                 // 速度比例
                            new[] { 0.0, 0.5, 1.0 },                 // 时间比例
                            0, 0, loop);
                        break;
                    case 2: // 百分比模式
                        axis.PVT.QuickStartPercent(tableId, 2,
                            new[] { 0.0, time },
                            new[] { 0.0, pos1 },
                            new double[] { 0, 50 },                   // 速度百分比
                            0, loop);
                        break;
                    case 3: // 连续模式
                        axis.PVT.QuickStartContinuous(tableId, 3,
                            new[] { 0.0, pos1, pos2 },
                            new[] { 0.0, vel1, vel2 },
                            new double[] { 0, 0, 0 },                 // 加速度
                            new[] { 50000.0, 50000.0, 50000.0 },      // 最大速度限制
                            new[] { 1000000.0, 1000000.0, 1000000.0 },// 最大加速度限制
                            new[] { 1000000.0, 1000000.0, 1000000.0 },// 最大减速度限制
                            0.0, loop);
                        break;
                }

                TxtPvtStatus.Text = "PVT运动进行中...";
                Log($"轴{CboPvtAxis.SelectedIndex + 1} PVT运动启动, 模式={CboPvtMode.Text}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"停止"按钮：停止PVT运动
        private void BtnPvtStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPvtAxis));
                axis.StopAxis();
                TxtPvtStatus.Text = "PVT运动已停止";
                Log("PVT运动停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 电子齿轮

        // 点击"启动"按钮：启动电子齿轮模式（从轴跟随主轴运动，按比例）
        private void BtnGearStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboGearSlave));
                // 读取主轴编号
                var masterIdx = short.Parse(TxtGearMasterIdx.Text);
                // 读取主轴类型（如：实轴、虚拟轴等）
                short masterType = (short)CboGearMasterType.SelectedIndex;
                // 读取电子齿轮的分子（主轴脉冲数）
                var masterEven = int.Parse(TxtGearMasterEven.Text);
                // 读取电子齿轮的分母（从轴脉冲数）
                var slaveEven = int.Parse(TxtGearSlaveEven.Text);
                // 读取啮合斜率（从轴加速跟上主轴的时间）
                var slope = int.Parse(TxtGearSlope.Text);

                // 设置为齿轮模式
                axis.Gear.SetGearMode();
                // 设置主轴（哪个轴作为主轴，以及主轴类型）
                axis.Gear.SetMaster(masterIdx, masterType);
                // 设置齿轮比（主轴走masterEven，从轴走slaveEven）
                axis.Gear.SetRatio(masterEven, slaveEven, slope);
                // 启动齿轮啮合
                axis.Gear.Start();

                TxtGearStatus.Text = $"齿轮运行中: {masterEven}:{slaveEven}";
                Log($"轴{CboGearSlave.SelectedIndex + 1} 电子齿轮启动, 比例={masterEven}:{slaveEven}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"停止"按钮：停止电子齿轮模式
        private void BtnGearStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboGearSlave));
                axis.Gear.Stop();
                TxtGearStatus.Text = "齿轮已停止";
                Log("电子齿轮停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"1:1快速"按钮：快速启动主轴从轴1:1跟随
        private void BtnGearOneToOne_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboGearSlave));
                var masterIdx = short.Parse(TxtGearMasterIdx.Text);
                // 快速启动齿轮，比例1:1（主轴走1，从轴也走1）
                axis.Gear.QuickStart(masterIdx, 1, 1);
                TxtGearStatus.Text = "齿轮1:1运行中";
                Log("电子齿轮 1:1 快速启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"2:1减速"按钮：快速启动主轴从轴2:1跟随（减速）
        private void BtnGearReduction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboGearSlave));
                var masterIdx = short.Parse(TxtGearMasterIdx.Text);
                // 快速启动齿轮，比例2:1（主轴走2，从轴走1）
                axis.Gear.QuickStart(masterIdx, 2, 1);
                TxtGearStatus.Text = "齿轮减速比2:1运行中";
                Log("电子齿轮 2:1 (减速) 快速启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 跟随

        // 点击"启动"按钮：启动跟随运动（从轴按梯形曲线跟随主轴）
        private void BtnFollowStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboFollowAxis));
                // 读取主轴编号
                var masterAxis = short.Parse(TxtFollowMaster.Text);
                // 读取循环次数
                var loop = int.Parse(TxtFollowLoop.Text);

                // 根据用户选择设置缓冲区大小
                if (RdoFollowMemSmall.IsChecked == true)
                    axis.Follow.SetMemorySmall();
                else
                    axis.Follow.SetMemoryLarge();

                // 读取加速段的主轴脉冲数
                var accelM = int.Parse(TxtFollowAccelM.Text);
                // 读取加速段的从轴脉冲数
                var accelS = double.Parse(TxtFollowAccelS.Text);
                // 读取匀速段的主轴脉冲数
                var evenM = int.Parse(TxtFollowEvenM.Text);
                // 读取匀速段的从轴脉冲数
                var evenS = double.Parse(TxtFollowEvenS.Text);
                // 读取减速段的主轴脉冲数
                var decelM = int.Parse(TxtFollowDecelM.Text);
                // 读取减速段的从轴脉冲数
                var decelS = double.Parse(TxtFollowDecelS.Text);

                // 用梯形曲线快速启动跟随运动
                axis.Follow.QuickStartTrapezoid(masterAxis,
                    accelM, accelS,
                    evenM, evenS,
                    decelM, decelS,
                    loop);

                TxtFollowStatus.Text = "跟随运行中...";
                Log($"轴{CboFollowAxis.SelectedIndex + 1} 跟随启动, 主轴={masterAxis}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"停止"按钮：停止跟随运动
        private void BtnFollowStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboFollowAxis));
                axis.StopAxis();
                TxtFollowStatus.Text = "跟随已停止";
                Log("跟随运动停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 插补

        // 标记坐标系是否已经建立
        private bool _interpSetup;

        // 点击"建立坐标系"按钮：创建2D/3D/4D坐标系
        private void BtnInterpSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 读取用户选择的第1个轴编号
                var a1 = (short)(CboInterpAxis1.SelectedIndex + 1);
                // 读取用户选择的第2个轴编号
                var a2 = (short)(CboInterpAxis2.SelectedIndex + 1);
                // 读取用户设置的合成速度
                var synVel = double.Parse(TxtInterpSynVel.Text);
                // 读取用户设置的合成加速度
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                // 读取用户选择的维度（0=2D, 1=3D, 2=4D）
                var dim = CboInterpDim.SelectedIndex;

                // 获取第1个轴的控制器（用它来建立坐标系）
                var axis1 = GetAxis(a1);

                // 根据用户选择的维度分别处理
                switch (dim)
                {
                    case 0: // 2D坐标系（两根轴）
                        axis1.Interp.SetupCrd2D(a1, a2, synVel, synAcc);
                        Log($"建立2D坐标系: 轴{a1}, 轴{a2}");
                        break;
                    case 1: // 3D坐标系（三根轴）
                        var a3 = (short)(CboInterpAxis3.SelectedIndex + 1);
                        axis1.Interp.SetupCrd3D(a1, a2, a3, synVel, synAcc);
                        Log($"建立3D坐标系: 轴{a1}, 轴{a2}, 轴{a3}");
                        break;
                    case 2: // 4D坐标系（四根轴）
                        var a3b = (short)(CboInterpAxis3.SelectedIndex + 1);
                        var a4 = (short)(CboInterpAxis4.SelectedIndex + 1);
                        axis1.Interp.SetupCrd4D(a1, a2, a3b, a4, synVel, synAcc);
                        Log($"建立4D坐标系: 轴{a1}, 轴{a2}, 轴{a3b}, 轴{a4}");
                        break;
                }

                // 标记坐标系已建立
                _interpSetup = true;
                TxtInterpStatus.Text = "坐标系已建立";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"清除轨迹"按钮：清除离线插补的所有轨迹数据
        private void BtnInterpClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 如果坐标系还没建立，不执行操作
                if (!_interpSetup) return;
                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                axis.Interp.OfflineClear();
                Log("插补轨迹已清除");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"添加直线"按钮：在插补轨迹中添加一条直线段
        private void BtnInterpAddLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 如果坐标系没建立，提示用户
                if (!_interpSetup) { Log("请先建立坐标系"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                var synVel = double.Parse(TxtInterpSynVel.Text);
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                var velEnd = double.Parse(TxtInterpVelEnd.Text);
                var dim = CboInterpDim.SelectedIndex;

                // 读取目标点坐标
                var x = int.Parse(TxtInterpX.Text);
                var y = int.Parse(TxtInterpY.Text);
                var z = int.Parse(TxtInterpZ.Text);

                // 根据维度添加不同的直线插补指令
                switch (dim)
                {
                    case 0: // 2D直线（XY平面）
                        axis.Interp.LnXY(x, y, synVel, synAcc, velEnd);
                        Log($"添加直线段: X={x}, Y={y}");
                        break;
                    case 1: // 3D直线（XYZ空间）
                        axis.Interp.LnXYZ(x, y, z, synVel, synAcc, velEnd);
                        Log($"添加直线段: X={x}, Y={y}, Z={z}");
                        break;
                    case 2: // 4D直线（XYZA空间）
                        var a = int.Parse(TxtInterpA.Text);
                        axis.Interp.LnXYZA(x, y, z, a, synVel, synAcc, velEnd);
                        Log($"添加直线段: X={x}, Y={y}, Z={z}, A={a}");
                        break;
                }

                // 查询并显示插补缓存剩余空间
                var space = axis.Interp.GetCrdSpace();
                TxtInterpSpace.Text = $"缓存空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"添加圆弧"按钮：在插补轨迹中添加一条圆弧段
        private void BtnInterpAddArc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) { Log("请先建立坐标系"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                var synVel = double.Parse(TxtInterpSynVel.Text);
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                var velEnd = double.Parse(TxtInterpVelEnd.Text);
                // 读取圆弧方向（0=顺时针CW，1=逆时针CCW）
                var dir = (short)CboInterpDir.SelectedIndex;

                var x = int.Parse(TxtInterpX.Text);
                var y = int.Parse(TxtInterpY.Text);

                // 添加圆弧段（终点x,y，半径50000，方向dir）
                axis.Interp.ArcXYR(x, y, 50000, dir, synVel, synAcc, velEnd);
                Log($"添加圆弧段: X={x}, Y={y}, 半径=50000, 方向={(dir == 0 ? "CW" : "CCW")}");

                var space = axis.Interp.GetCrdSpace();
                TxtInterpSpace.Text = $"缓存空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"开始插补"按钮：启动离线插补运动
        private void BtnInterpStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) { Log("请先建立坐标系"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                // 启动离线插补（按照之前添加的直线/圆弧轨迹运动）
                axis.Interp.OfflineStart();
                TxtInterpStatus.Text = "插补运行中...";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                Log("插补运动启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"平滑停止"按钮：让插补运动平稳减速停止
        private void BtnInterpSmoothStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) return;
                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                axis.Interp.SmoothStop();
                TxtInterpStatus.Text = "平滑停止";
                Log("插补平滑停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"紧急停止"按钮：立即停止插补运动（可能产生冲击）
        private void BtnInterpStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) return;
                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                axis.Interp.EmergencyStop();
                TxtInterpStatus.Text = "紧急停止";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                Log("插补紧急停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 实时插补

        // 实时插补的定时器（按固定间隔往缓冲区送数据）
        private DispatcherTimer? _interpRtTimer;
        // 实时插补的当前位置坐标（X, Y, Z）
        private double _rtPosX, _rtPosY, _rtPosZ;
        // 标记实时插补是否正在运行
        private bool _interpRtRunning;

        // 点击"启动"按钮：启动实时插补（边计算边运动，而非提前准备全部轨迹）
        private void BtnInterpRtStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) { Log("请先建立坐标系"); return; }
                if (_interpRtRunning) { Log("实时插补已在运行"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);

                // 根据用户选择设置缓冲区模式
                switch (CboInterpBufMode.SelectedIndex)
                {
                    case 0: axis.Interp.SetDynamicBufferMode(); break;       // 动态模式
                    case 1: axis.Interp.SetDynamicKeepBufferMode(); break;   // 动态保持模式
                }

                // 如果用户勾选了"预读"功能
                if (ChkInterpLookAhead.IsChecked == true)
                    axis.Interp.EnableLookAhead();      // 启用预读（提前规划速度）
                else
                    axis.Interp.DisableLookAhead();     // 禁用预读

                // 读取速度倍率（百分比，如100表示100%速度）
                var ratio = double.Parse(TxtInterpOverride.Text);
                axis.Interp.SetOverride(ratio);

                // 把当前坐标重置为0
                _rtPosX = 0;
                _rtPosY = 0;
                _rtPosZ = 0;
                // 标记实时插补已启动
                _interpRtRunning = true;

                // 读取用户设定的定时器间隔（多久往缓冲区加一个点）
                var interval = int.Parse(TxtInterpRtInterval.Text);
                // 创建定时器，按用户设定的间隔触发
                _interpRtTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(interval) };
                // 绑定定时器触发事件
                _interpRtTimer.Tick += InterpRtTimer_Tick;
                // 启动定时器
                _interpRtTimer.Start();

                // 启动插补运动（开始执行）
                axis.Interp.Start();

                TxtInterpStatus.Text = "实时插补运行中...";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                Log("实时插补启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 实时插补定时器触发时执行：往插补缓冲区添加下一个目标点
        private void InterpRtTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                // 读取步长（每次增加的距离）
                var step = int.Parse(TxtInterpRtStep.Text);
                var synVel = double.Parse(TxtInterpSynVel.Text);
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                var velEnd = double.Parse(TxtInterpVelEnd.Text);

                // 当前位置增加一个步长
                _rtPosX += step;
                _rtPosY += step;

                var dim = CboInterpDim.SelectedIndex;
                // 根据维度向缓冲区添加不同的直线插补指令
                switch (dim)
                {
                    case 0: // 2D
                        axis.Interp.LnXY((int)_rtPosX, (int)_rtPosY, synVel, synAcc, velEnd);
                        break;
                    case 1: // 3D
                        _rtPosZ += step;
                        axis.Interp.LnXYZ((int)_rtPosX, (int)_rtPosY, (int)_rtPosZ, synVel, synAcc, velEnd);
                        break;
                    case 2: // 4D
                        _rtPosZ += step;
                        var a = int.Parse(TxtInterpA.Text);
                        axis.Interp.LnXYZA((int)_rtPosX, (int)_rtPosY, (int)_rtPosZ, a, synVel, synAcc, velEnd);
                        break;
                }

                // 显示缓存剩余空间
                var space = axis.Interp.GetCrdSpace();
                TxtInterpSpace.Text = $"缓存空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        // 点击"停止"按钮：停止实时插补
        private void BtnInterpRtStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpRtRunning) return;
                // 标记已停止
                _interpRtRunning = false;
                // 停止定时器
                _interpRtTimer?.Stop();

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                // 平滑停止（减速停下，不是急停）
                axis.Interp.SmoothStop();

                TxtInterpStatus.Text = "实时插补已停止";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                Log("实时插补停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 状态监控

        // 监控定时器触发时执行：如果开启了自动刷新，就刷新监控面板
        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            // 如果用户没有勾选"自动刷新"复选框，就不刷新
            if (!ChkAutoRefresh.IsChecked == true) return;
            RefreshMonitor();
        }

        // 点击"刷新"按钮：手动刷新一次监控面板
        private void BtnMonitorRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshMonitor();
        }

        // 刷新监控面板：读取轴状态并显示到界面上
        private void RefreshMonitor()
        {
            try
            {
                // 如果没连接、管理器为空、或者轴数量为0，跳过
                if (!_isConnected || _motionManager == null || _motionManager.GetAxisCount() == 0)
                    return;

                var axis = GetAxis(SelectedAxis(CboMonitorAxis));
                var status = axis.GetEcatStatus();
                if (status == null) return;

                // 更新各个状态指示器（红色=报警/触发，绿色=正常）
                SetMonitorValue(MonPlusLimitVal, status.PlusLimitAlarm ? "触发" : "正常", status.PlusLimitAlarm);
                SetMonitorValue(MonMinusLimitVal, status.MinusLimitAlarm ? "触发" : "正常", status.MinusLimitAlarm);
                SetMonitorValue(MonAxisAlarmVal, status.AxisAlarm ? "报警" : "正常", status.AxisAlarm);
                SetMonitorValue(MonFollowAlarmVal, status.FollowAlarm ? "报警" : "正常", status.FollowAlarm);
                SetMonitorValue(MonSmoothStopVal, status.SmoothStopAlarm ? "触发" : "正常", status.SmoothStopAlarm);
                SetMonitorValue(MonScramVal, status.Scram ? "触发" : "正常", status.Scram);
                SetMonitorValue(MonEnabledVal, status.EnableAxis ? "已使能" : "未使能", status.EnableAxis);
                SetMonitorValue(MonPlanningVal, status.Planning ? "运动中" : "停止", status.Planning);
                MonMotionTypeVal.Text = status.MotionType;

                // 显示位置数据（保留2位小数）
                MonPlanPosVal.Text = status.PlannedLocation.ToString("F2");  // 规划位置
                MonDrivePosVal.Text = status.DriveLocation.ToString("F2");   // 驱动位置（编码器反馈）
                MonFollowErrVal.Text = status.FollowErr.ToString("F2");      // 跟随误差

                // 显示速度数据
                MonPlanVelVal.Text = status.PlannedVel.ToString("F2");       // 规划速度
                MonDriveVelVal.Text = status.DriveVel.ToString("F2");        // 实际速度
                // 显示加速度数据
                MonPlanAccVal.Text = status.PlannedAccVel.ToString("F2");    // 规划加速度
                MonDriveAccVal.Text = status.DriveAccVel.ToString("F2");     // 实际加速度

                // 在点位运动区域显示当前位置、编码器位置、速度、跟随误差
                TxtMoveCurrentPos.Text = $"规划位置: {status.PlannedLocation:F2}";
                TxtMoveCurrentEnc.Text = $"编码器位置: {status.DriveLocation:F2}";
                TxtMoveCurrentVel.Text = $"速度: {status.DriveVel:F2}";
                TxtMoveFollowErr.Text = $"跟随误差: {status.FollowErr:F2}";
            }
            catch { }
        }

        // 设置监控面板中某个指示器的文字和颜色
        // valText: 要更新的文字控件, text: 要显示的文字, isAlarm: 是否为报警状态
        private void SetMonitorValue(System.Windows.Documents.Run valText, string text, bool isAlarm)
        {
            valText.Text = text;
            // 报警状态显示红色，正常显示绿色
            valText.Foreground = isAlarm
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
        }

        #endregion

        #region 扩展模块 (gL500)

        private void CreateExpansionIOControls()
        {
            var green = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
            var gray = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);

            // 创建16个DO按钮（灰色=关闭，绿色=打开）
            for (int i = 0; i < 16; i++)
            {
                var btn = new Button
                {
                    Content = $"DO{i}",
                    Width = 55,
                    Height = 28,
                    Margin = new System.Windows.Thickness(2),
                    Background = gray,
                    Foreground = System.Windows.Media.Brushes.White,
                    Tag = i
                };
                btn.Click += BtnExtDo_Click;
                _extDoBtns[i] = btn;
                WpExtDo.Children.Add(btn);
            }

            // 创建16个DI标签（灰色=0，绿色=1）
            for (int i = 0; i < 16; i++)
            {
                var lbl = new Label
                {
                    Content = $"DI{i}: 0",
                    Width = 65,
                    Margin = new System.Windows.Thickness(2),
                    Foreground = gray
                };
                _extDiLabels[i] = lbl;
                WpExtDi.Children.Add(lbl);
            }

            // 创建6个AI显示标签
            for (int i = 0; i < 6; i++)
            {
                var lbl = new Label
                {
                    Content = $"AI{i}: -",
                    Width = 100,
                    Margin = new System.Windows.Thickness(2)
                };
                _extAiLabels[i] = lbl;
                WpExtAi.Children.Add(lbl);
            }

            // 创建6个AO输入框+设置按钮
            for (int i = 0; i < 6; i++)
            {
                var idx = i;
                var tb = new TextBox { Text = "0", Width = 70, Margin = new System.Windows.Thickness(2) };
                var btn = new Button
                {
                    Content = $"AO{idx} 设置",
                    Width = 70,
                    Height = 24,
                    Margin = new System.Windows.Thickness(2),
                    Tag = idx
                };
                btn.Click += BtnExtAoSet_Click;
                _extAoTbs[idx] = tb;
                WpExtAo.Children.Add(new Label { Content = $"CH{idx}:", Width = 35 });
                WpExtAo.Children.Add(tb);
                WpExtAo.Children.Add(btn);
            }

            // 创建扩展模块定时器
            _extTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _extTimer.Tick += ExtTimer_Tick;
        }

        private async void BtnExtInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) { Log("请先打开控制器"); return; }
                var rtn = _motionManager.Expansion.Init(0);
                if (rtn != 0) { Log($"扩展模块初始化失败: {rtn}"); return; }
                // 查询在线从站数量
                var count = _motionManager.Expansion.GetOnlineSlaveCount();
                TxtExtSlaveCount.Text = $"在线从站数: {count}";
                BtnExtInit.IsEnabled = false;
                BtnExtDeInit.IsEnabled = true;
                // 启动定时器自动刷新
                _extTimer?.Start();
                Log("扩展模块初始化成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnExtDeInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _extTimer?.Stop();
                _motionManager?.Expansion.DeInit();
                TxtExtSlaveCount.Text = "在线从站数: -";
                BtnExtInit.IsEnabled = true;
                BtnExtDeInit.IsEnabled = false;
                Log("扩展模块已去初始化");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnExtDo_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) return;
                var btn = (Button)sender!;
                var idx = (int)btn.Tag;
                // 切换当前状态（取反：原来0变1，原来1变0）
                var current = btn.Background.ToString() == "#FF00FF00" ? (byte)1 : (byte)0;
                var newVal = current == 0 ? (byte)1 : (byte)0;
                _motionManager.Expansion.SetDoBit(0, (short)idx, newVal);
                btn.Background = newVal == 1
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                Log($"扩展DO{idx} -> {newVal}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnExtAoSet_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) return;
                var btn = (Button)sender!;
                var idx = (int)btn.Tag;
                var val = short.Parse(_extAoTbs[idx].Text);
                _motionManager.Expansion.WriteAo(0, (ushort)idx, new short[] { val });
                Log($"扩展AO{idx} 已设置: {val}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void ExtTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (_motionManager == null || !ChkExtAutoRefresh.IsChecked == true) return;
                // 读取DI（2个字节=16位）
                var diData = _motionManager.Expansion.ReadDi(0, 0, 2);
                if (diData.Length >= 2)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        var bit = (diData[i / 8] >> (i % 8)) & 1;
                        _extDiLabels[i].Content = $"DI{i}: {bit}";
                        _extDiLabels[i].Foreground = bit == 1
                            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
                            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                    }
                }
                // 读取AI（6个通道）
                for (int i = 0; i < 6; i++)
                {
                    var aiVal = _motionManager.Expansion.ReadAi(0, (ushort)i, 1);
                    _extAiLabels[i].Content = $"AI{i}: {aiVal[0]}";
                }
            }
            catch { }
        }

        #endregion

        #region EcatIO模块

        private void CreateEcatIOControls()
        {
            var gray = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);

            // 创建16个DO按钮
            for (int i = 0; i < 16; i++)
            {
                var btn = new Button
                {
                    Content = $"DO{i}",
                    Width = 55,
                    Height = 28,
                    Margin = new System.Windows.Thickness(2),
                    Background = gray,
                    Foreground = System.Windows.Media.Brushes.White,
                    Tag = i
                };
                btn.Click += BtnEcatDo_Click;
                _ecatDoBtns[i] = btn;
                WpEcatIODo.Children.Add(btn);
            }

            // 创建16个DI标签
            for (int i = 0; i < 16; i++)
            {
                var lbl = new Label
                {
                    Content = $"DI{i}: 0",
                    Width = 65,
                    Margin = new System.Windows.Thickness(2),
                    Foreground = gray
                };
                _ecatDiLabels[i] = lbl;
                WpEcatIODi.Children.Add(lbl);
            }

            // 创建EcatIO定时器
            _ecatIOTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _ecatIOTimer.Tick += EcatIOTimer_Tick;
        }

        private void BtnEcatIOReadInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) { Log("请先打开控制器"); return; }
                var slaveNo = ushort.Parse(TxtEcatIOSlaveNo.Text);
                var data = _motionManager.EcatIO.ReadInput(slaveNo, 0, 2);
                TxtEcatIOData.Text = string.Join(" ", data.Select(b => b.ToString("X2")));
                UpdateEcatIODiDisplay(data);
                Log("EcatIO 读取输入成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnEcatIOReadOutput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) { Log("请先打开控制器"); return; }
                var slaveNo = ushort.Parse(TxtEcatIOSlaveNo.Text);
                var data = _motionManager.EcatIO.ReadOutput(slaveNo, 0, 2);
                TxtEcatIOData.Text = string.Join(" ", data.Select(b => b.ToString("X2")));
                UpdateEcatIODoDisplay(data);
                Log("EcatIO 读取输出成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnEcatIOWrite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) { Log("请先打开控制器"); return; }
                var slaveNo = ushort.Parse(TxtEcatIOSlaveNo.Text);
                var hexStr = TxtEcatIOData.Text.Trim();
                var bytes = hexStr.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                _motionManager.EcatIO.WriteOutput(slaveNo, 0, bytes);
                Log("EcatIO 写入输出成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnEcatDo_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) return;
                var btn = (Button)sender!;
                var idx = (int)btn.Tag;
                var slaveNo = ushort.Parse(TxtEcatIOSlaveNo.Text);
                var current = btn.Background.ToString() == "#FF00FF00" ? (byte)1 : (byte)0;
                var newVal = current == 0 ? (byte)1 : (byte)0;
                _motionManager.EcatIO.WriteOutputBit(slaveNo, 0, (ushort)idx, newVal);
                btn.Background = newVal == 1
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                Log($"EcatIO DO{idx} -> {newVal}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnEcatIOBitRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) { Log("请先打开控制器"); return; }
                var slaveNo = ushort.Parse(TxtEcatIOSlaveNo.Text);
                var idx = ushort.Parse(TxtEcatIOBitIdx.Text);
                var val = _motionManager.EcatIO.ReadInputBit(slaveNo, 0, idx);
                Log($"EcatIO 输入Bit[{idx}] = {val}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnEcatIOBitWrite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionManager == null) { Log("请先打开控制器"); return; }
                var slaveNo = ushort.Parse(TxtEcatIOSlaveNo.Text);
                var idx = ushort.Parse(TxtEcatIOBitIdx.Text);
                var val = byte.Parse(TxtEcatIOBitVal.Text);
                _motionManager.EcatIO.WriteOutputBit(slaveNo, 0, idx, val);
                Log($"EcatIO 输出Bit[{idx}] = {val}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void EcatIOTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (_motionManager == null || !ChkEcatIOAutoRefresh.IsChecked == true) return;
                var slaveNo = ushort.Parse(TxtEcatIOSlaveNo.Text);
                var diData = _motionManager.EcatIO.ReadInput(slaveNo, 0, 2);
                UpdateEcatIODiDisplay(diData);
                var doData = _motionManager.EcatIO.ReadOutput(slaveNo, 0, 2);
                UpdateEcatIODoDisplay(doData);
            }
            catch { }
        }

        private void UpdateEcatIODiDisplay(byte[] data)
        {
            if (data.Length < 2) return;
            for (int i = 0; i < 16; i++)
            {
                var bit = (data[i / 8] >> (i % 8)) & 1;
                _ecatDiLabels[i].Content = $"DI{i}: {bit}";
                _ecatDiLabels[i].Foreground = bit == 1
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void UpdateEcatIODoDisplay(byte[] data)
        {
            if (data.Length < 2) return;
            for (int i = 0; i < 16; i++)
            {
                var bit = (data[i / 8] >> (i % 8)) & 1;
                _ecatDoBtns[i].Background = bit == 1
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        #endregion
    }
}
