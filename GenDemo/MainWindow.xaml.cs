using GenMotionEasy.Model;
using GenMotionEasy.Motion;
using GenMotionEasy.Motion.Control;
using GenMotionEasy.Tool;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GenDemo
{
    public partial class MainWindow : Window
    {
        private MotionControlManager? _motionManager;
        private DispatcherTimer? _statusTimer;
        private DispatcherTimer? _monitorTimer;
        private int _axisCount;
        private bool _isConnected;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _statusTimer.Tick += StatusTimer_Tick;

            _monitorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _monitorTimer.Tick += MonitorTimer_Tick;

            PopulateAxisComboBoxes();
        }

        private void PopulateAxisComboBoxes()
        {
            var items = new[] { "轴1", "轴2", "轴3", "轴4", "轴5", "轴6", "轴7", "轴8" };
            foreach (var cbo in new[] { CboAxisCtrl, CboHomeAxis, CboMoveAxis, CboPtAxis, CboPvtAxis, CboGearSlave, CboFollowAxis, CboMonitorAxis })
            {
                cbo.ItemsSource = items;
                cbo.SelectedIndex = 0;
            }
            foreach (var cbo in new[] { CboInterpAxis1, CboInterpAxis2, CboInterpAxis3, CboInterpAxis4 })
            {
                cbo.ItemsSource = items;
            }
        }

        private void Log(string msg)
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            TxtLog.AppendText($"[{time}] {msg}\n");
            TxtLog.ScrollToEnd();
            LblStatus.Text = msg;
        }

        private void SetConnectedState(bool connected)
        {
            _isConnected = connected;
            BtnOpen.IsEnabled = !connected;
            BtnClose.IsEnabled = connected;
            BtnInitAxes.IsEnabled = connected;
            CboAxisCount.IsEnabled = !connected;
            LblCardStatus.Content = connected ? "已连接" : "未连接";
            LblCardStatus.Foreground = connected
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        }

        private AxisController GetAxis(int axisId = -1)
        {
            if (_motionManager == null)
                throw new InvalidOperationException("控制器未打开");
            if (axisId < 0) axisId = 0;
            return _motionManager.GetAxisController(axisId);
        }

        private AxisController GetAxisSafe(int axisId = -1)
        {
            if (_motionManager == null || _motionManager.GetAxisCount() == 0)
                throw new InvalidOperationException("控制器未打开或轴未初始化");
            if (axisId < 0) axisId = 0;
            return _motionManager.GetAxisController(axisId);
        }

        private int SelectedAxis(ComboBox cbo)
        {
            return cbo.SelectedIndex + 1;
        }

        #region 连接与初始化

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _motionManager = new MotionControlManager();
                if (!_motionManager.Open())
                {
                    Log("打开控制器失败");
                    return;
                }
                _motionManager.AddAxis(1, 1);
                _motionManager.AddAxis(2, 1);
                _motionManager.AddAxis(3, 1);
                _motionManager.EcatLoad();
                short state = 0;
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(500);
                    _motionManager.EcatState(out state);
                    if (state == 1) break;
                }
                if (state != 1)
                {
                    Log("EtherCAT总线未就绪");
                    return;
                }

                _motionManager.EcatStart();
                SetConnectedState(true);
                Log("控制器打开成功");
            }
            catch (Exception ex)
            {
                Log($"打开控制器异常: {ex.Message}");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _statusTimer?.Stop();
                _monitorTimer?.Stop();
                _motionManager = null;
                SetConnectedState(false);
                Log("控制器已关闭");
            }
            catch (Exception ex)
            {
                Log($"关闭控制器异常: {ex.Message}");
            }
        }

        private void BtnInitAxes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _axisCount = CboAxisCount.SelectedIndex + 1;
                for (int i = 0; i < _axisCount; i++)
                {
                    _motionManager!.AddAxis((short)(i + 1), 1);
                }
                Log($"已初始化 {_axisCount} 个轴");

                _statusTimer?.Start();
                _monitorTimer?.Start();
            }
            catch (Exception ex)
            {
                Log($"初始化轴异常: {ex.Message}");
            }
        }

        #endregion

        #region 轴控制

        private void BtnEnable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var rtn = axis.EnableAxis();
                GtnErrorHelper.ThrowIfError(rtn, "使能");
                Log($"轴{CboAxisCtrl.SelectedIndex + 1} 使能成功");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

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

        private void BtnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog { Filter = "配置文件|*.cfg;*.ini;*.txt|所有文件|*.*" };
                if (dialog.ShowDialog() == true)
                {
                    var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                    var rtn = axis.LoadConfig(dialog.FileName);
                    GtnErrorHelper.ThrowIfError(rtn, "加载配置");
                    Log($"配置文件加载成功: {dialog.FileName}");
                }
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (!_isConnected || _motionManager == null || _motionManager.GetAxisCount() == 0)
                    return;

                var axis = GetAxis(SelectedAxis(CboAxisCtrl));
                var status = axis.GetEcatStatus();
                if (status == null) return;

                TxtMotionType.Text = status.MotionType;
                TxtEnableState.Text = status.EnableAxis ? "已使能" : "未使能";
                TxtPlanning.Text = status.Planning ? "运动中" : "停止";
                TxtPlusLimit.Text = status.PlusLimitAlarm ? "触发" : "正常";
                TxtMinusLimit.Text = status.MinusLimitAlarm ? "触发" : "正常";
                TxtAxisAlarm.Text = status.AxisAlarm ? "报警" : "正常";
                TxtFollowAlarm.Text = status.FollowAlarm ? "报警" : "正常";
                TxtSmoothStop.Text = status.SmoothStopAlarm ? "触发" : "正常";
                TxtScram.Text = status.Scram ? "触发" : "正常";
            }
            catch { }
        }

        #endregion

        #region 回零

        private async void BtnStartHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboHomeAxis));
                var tag = ((ComboBoxItem)CboHomeMethod.SelectedItem).Tag as string ?? "0";
                var method = short.Parse(tag);
                var switchSpeed = double.Parse(TxtHomeSwitchSpeed.Text);
                var indexSpeed = double.Parse(TxtHomeIndexSpeed.Text);
                var acc = double.Parse(TxtHomeAcc.Text);
                var offset = int.Parse(TxtHomeOffset.Text);

                TxtHomeStatus.Text = "回零进行中...";
                TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                BtnStartHome.IsEnabled = false;

                await axis.AxisHome.Home(switchSpeed, indexSpeed, acc, method, offset);
                var done = await axis.AxisHome.HomeDone(10);

                if (done)
                {
                    TxtHomeStatus.Text = "回零完成!";
                    TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
                    Log($"轴{CboHomeAxis.SelectedIndex + 1} 回零完成");
                }
                else
                {
                    TxtHomeStatus.Text = "回零超时或失败";
                    TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    Log($"轴{CboHomeAxis.SelectedIndex + 1} 回零失败");
                }
            }
            catch (Exception ex)
            {
                TxtHomeStatus.Text = $"回零异常: {ex.Message}";
                TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                Log(ex.Message);
            }
            finally
            {
                BtnStartHome.IsEnabled = true;
            }
        }

        private void BtnResetHomeStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboHomeAxis));
                axis.AxisHome.ResetHomeStatus();
                TxtHomeStatus.Text = "回零状态已重置";
                TxtHomeStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 点位运动

        private void BtnPointMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboMoveAxis));
                var pos = int.Parse(TxtMovePos.Text);
                var vel = double.Parse(TxtMoveVel.Text);
                var acc = double.Parse(TxtMoveAcc.Text);
                var dec = double.Parse(TxtMoveDec.Text);

                axis.Point.PointMove(pos, vel, acc, dec);
                Log($"轴{CboMoveAxis.SelectedIndex + 1} Trap点位运动: pos={pos}, vel={vel}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnAbsMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboMoveAxis));
                var pos = int.Parse(TxtMovePos.Text);
                var vel = double.Parse(TxtMoveVel.Text);
                var acc = double.Parse(TxtMoveAcc.Text);
                var dec = double.Parse(TxtMoveDec.Text);

                axis.Point.PointAbsMove(pos, vel, acc, dec);
                Log($"轴{CboMoveAxis.SelectedIndex + 1} 绝对定位: pos={pos}, vel={vel}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

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

        private bool _ptModeSet;
        private bool _ptDataPushed;
        private DispatcherTimer? _ptRtTimer;
        private double _ptRtPos, _ptRtVel, _ptRtTime;
        private bool _ptRtStarted;

        private void SetupPtMode()
        {
            var axis = GetAxis(SelectedAxis(CboPtAxis));

            if (RdoPtStatic.IsChecked == true)
                axis.PT.SetStaticMode();
            else
                axis.PT.SetDynamicMode();

            if (RdoPtMemSmall.IsChecked == true)
                axis.PT.SetMemorySmall();
            else
                axis.PT.SetMemoryLarge();

            var loop = int.Parse(TxtPtLoop.Text);
            axis.PT.SetLoop(loop);

            axis.PT.Clear();

            _ptModeSet = true;
            _ptDataPushed = false;
            TxtPtStatus.Text = "PT模式已设置";
            TxtPtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
            Log("PT模式设置完成");
        }

        private void RdoPtMode_Checked(object sender, RoutedEventArgs e)
        {
            _ptModeSet = false;
        }

        private void BtnPtPush_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_ptModeSet) SetupPtMode();

                var axis = GetAxis(SelectedAxis(CboPtAxis));
                var pos = double.Parse(TxtPtPos.Text);
                var time = int.Parse(TxtPtTime.Text);
                var segType = (short)CboPtSegType.SelectedIndex;

                if (segType == 1 && !_ptDataPushed)
                {
                    Log("EVEN段需要前一段定义速度，已自动转为NORMAL");
                    segType = 0;
                }

                axis.PT.PushData(pos, time, segType);
                _ptDataPushed = true;

                var space = axis.PT.GetSpace();
                TxtPtSpace.Text = $"剩余空间: {space}";
                Log($"PT推送数据: pos={pos}, time={time}ms, type={CboPtSegType.Text}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnPtClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPtAxis));
                axis.PT.Clear();
                _ptDataPushed = false;
                TxtPtSpace.Text = "剩余空间: -";
                Log("PT数据已清除");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnPtSpace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_ptModeSet) SetupPtMode();

                var axis = GetAxis(SelectedAxis(CboPtAxis));
                var space = axis.PT.GetSpace();
                TxtPtSpace.Text = $"剩余空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnPtStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RdoPtDynamic.IsChecked == true)
                {
                    StartDynamicDemo();
                    return;
                }

                if (!_ptModeSet) { SetupPtMode(); return; }

                if (!_ptDataPushed)
                {
                    Log("请先推送数据再启动运动");
                    return;
                }

                var axis = GetAxis(SelectedAxis(CboPtAxis));
                axis.PT.Start();
                _ptModeSet = false;
                _ptDataPushed = false;
                TxtPtStatus.Text = "PT运动进行中...";
                TxtPtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                Log($"轴{CboPtAxis.SelectedIndex + 1} PT运动启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void StartDynamicDemo()
        {
            if (_ptRtTimer != null)
            {
                Log("动态演示已在运行");
                return;
            }

            var axis = GetAxis(SelectedAxis(CboPtAxis));
            axis.PT.SetDynamicMode();
            axis.PT.SetMemoryLarge();
            axis.PT.Clear();

            _ptRtPos = 0;
            _ptRtVel = 0;
            _ptRtTime = 0;
            _ptRtStarted = false;

            var interval = 50;
            _ptRtTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(interval) };
            _ptRtTimer.Tick += PtRtTimer_Tick;
            _ptRtTimer.Start();

            _ptModeSet = true;
            TxtPtStatus.Text = "动态演示运行中...";
            TxtPtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
            Log("动态PT演示启动");
        }

        private void PtRtTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPtAxis));
                var space = axis.PT.GetSpace();

                if (space > 0)
                {
                    var dt = 0.05;
                    var a = 50000;
                    var freq = 2 * Math.PI * 0.5;
                    _ptRtTime += dt;
                    var vel = a * Math.Sin(freq * _ptRtTime);
                    _ptRtPos += (vel + _ptRtVel) * dt / 2;
                    _ptRtVel = vel;

                    axis.PT.PushData(_ptRtPos, (int)(_ptRtTime * 1000), 0);
                    _ptDataPushed = true;
                }

                if (space <= 0 && !_ptRtStarted)
                {
                    axis.PT.Start();
                    _ptRtStarted = true;
                }

                TxtPtSpace.Text = $"剩余空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnPtStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ptRtTimer?.Stop();
                _ptRtTimer = null;
                _ptRtStarted = false;

                var axis = GetAxis(SelectedAxis(CboPtAxis));
                axis.StopAxis();
                axis.PT.Clear();
                _ptModeSet = false;
                _ptDataPushed = false;
                TxtPtStatus.Text = "PT运动已停止";
                TxtPtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                Log("PT运动停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region PVT运动

        private void BtnPvtStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboPvtAxis));
                var tableId = (short)(CboPvtTable.SelectedIndex + 1);
                var loop = int.Parse(TxtPvtLoop.Text);
                var time = double.Parse(TxtPvtTime.Text) / 1000.0;

                var pos1 = double.Parse(TxtPvtPos1.Text);
                var pos2 = double.Parse(TxtPvtPos2.Text);
                var pos3 = double.Parse(TxtPvtPos3.Text);
                var vel1 = double.Parse(TxtPvtVel1.Text);
                var vel2 = double.Parse(TxtPvtVel2.Text);
                var vel3 = double.Parse(TxtPvtVel3.Text);

                switch (CboPvtMode.SelectedIndex)
                {
                    case 0:
                        axis.PVT.QuickStart(tableId, 3,
                            new[] { 0.0, time, time * 2 },
                            new[] { 0.0, pos1, pos2 },
                            new[] { 0.0, vel1, vel2 }, loop);
                        break;
                    case 1:
                        axis.PVT.QuickStartComplete(tableId, 3,
                            new[] { 0.0, time, time * 2 },
                            new[] { 0.0, pos1, pos2 },
                            new[] { 0.0, 0.5, 0.0 },
                            new[] { 0.5, 0.5, 0.5 },
                            new[] { 0.0, 0.5, 1.0 },
                            0, 0, loop);
                        break;
                    case 2:
                        axis.PVT.QuickStartPercent(tableId, 2,
                            new[] { 0.0, time },
                            new[] { 0.0, pos1 },
                            new double[] { 0, 50 },
                            0, loop);
                        break;
                    case 3:
                        axis.PVT.QuickStartContinuous(tableId, 3,
                            new[] { 0.0, pos1, pos2 },
                            new[] { 0.0, vel1, vel2 },
                            new double[] { 0, 0, 0 },
                            new[] { 50000.0, 50000.0, 50000.0 },
                            new[] { 1000000.0, 1000000.0, 1000000.0 },
                            new[] { 1000000.0, 1000000.0, 1000000.0 },
                            0.0, loop);
                        break;
                }

                TxtPvtStatus.Text = "PVT运动进行中...";
                Log($"轴{CboPvtAxis.SelectedIndex + 1} PVT运动启动, 模式={CboPvtMode.Text}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

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

        private void BtnGearStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboGearSlave));
                var masterIdx = short.Parse(TxtGearMasterIdx.Text);
                short masterType = (short)CboGearMasterType.SelectedIndex;
                var masterEven = int.Parse(TxtGearMasterEven.Text);
                var slaveEven = int.Parse(TxtGearSlaveEven.Text);
                var slope = int.Parse(TxtGearSlope.Text);

                axis.Gear.SetGearMode();
                axis.Gear.SetMaster(masterIdx, masterType);
                axis.Gear.SetRatio(masterEven, slaveEven, slope);
                axis.Gear.Start();

                TxtGearStatus.Text = $"齿轮运行中: {masterEven}:{slaveEven}";
                Log($"轴{CboGearSlave.SelectedIndex + 1} 电子齿轮启动, 比例={masterEven}:{slaveEven}");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

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

        private void BtnGearOneToOne_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboGearSlave));
                var masterIdx = short.Parse(TxtGearMasterIdx.Text);
                axis.Gear.QuickStart(masterIdx, 1, 1);
                TxtGearStatus.Text = "齿轮1:1运行中";
                Log("电子齿轮 1:1 快速启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnGearReduction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboGearSlave));
                var masterIdx = short.Parse(TxtGearMasterIdx.Text);
                axis.Gear.QuickStart(masterIdx, 2, 1);
                TxtGearStatus.Text = "齿轮减速比2:1运行中";
                Log("电子齿轮 2:1 (减速) 快速启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 跟随

        private void BtnFollowStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(SelectedAxis(CboFollowAxis));
                var masterAxis = short.Parse(TxtFollowMaster.Text);
                var loop = int.Parse(TxtFollowLoop.Text);

                if (RdoFollowMemSmall.IsChecked == true)
                    axis.Follow.SetMemorySmall();
                else
                    axis.Follow.SetMemoryLarge();

                var accelM = int.Parse(TxtFollowAccelM.Text);
                var accelS = double.Parse(TxtFollowAccelS.Text);
                var evenM = int.Parse(TxtFollowEvenM.Text);
                var evenS = double.Parse(TxtFollowEvenS.Text);
                var decelM = int.Parse(TxtFollowDecelM.Text);
                var decelS = double.Parse(TxtFollowDecelS.Text);

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

        private bool _interpSetup;

        private void BtnInterpSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var a1 = (short)(CboInterpAxis1.SelectedIndex + 1);
                var a2 = (short)(CboInterpAxis2.SelectedIndex + 1);
                var synVel = double.Parse(TxtInterpSynVel.Text);
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                var dim = CboInterpDim.SelectedIndex;

                var axis1 = GetAxis(a1);

                switch (dim)
                {
                    case 0:
                        axis1.Interp.SetupCrd2D(a1, a2, synVel, synAcc);
                        Log($"建立2D坐标系: 轴{a1}, 轴{a2}");
                        break;
                    case 1:
                        var a3 = (short)(CboInterpAxis3.SelectedIndex + 1);
                        axis1.Interp.SetupCrd3D(a1, a2, a3, synVel, synAcc);
                        Log($"建立3D坐标系: 轴{a1}, 轴{a2}, 轴{a3}");
                        break;
                    case 2:
                        var a3b = (short)(CboInterpAxis3.SelectedIndex + 1);
                        var a4 = (short)(CboInterpAxis4.SelectedIndex + 1);
                        axis1.Interp.SetupCrd4D(a1, a2, a3b, a4, synVel, synAcc);
                        Log($"建立4D坐标系: 轴{a1}, 轴{a2}, 轴{a3b}, 轴{a4}");
                        break;
                }

                _interpSetup = true;
                TxtInterpStatus.Text = "坐标系已建立";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnInterpClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) return;
                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                axis.Interp.OfflineClear();
                Log("插补轨迹已清除");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnInterpAddLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) { Log("请先建立坐标系"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                var synVel = double.Parse(TxtInterpSynVel.Text);
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                var velEnd = double.Parse(TxtInterpVelEnd.Text);
                var dim = CboInterpDim.SelectedIndex;

                var x = int.Parse(TxtInterpX.Text);
                var y = int.Parse(TxtInterpY.Text);
                var z = int.Parse(TxtInterpZ.Text);

                switch (dim)
                {
                    case 0:
                        axis.Interp.LnXY(x, y, synVel, synAcc, velEnd);
                        Log($"添加直线段: X={x}, Y={y}");
                        break;
                    case 1:
                        axis.Interp.LnXYZ(x, y, z, synVel, synAcc, velEnd);
                        Log($"添加直线段: X={x}, Y={y}, Z={z}");
                        break;
                    case 2:
                        var a = int.Parse(TxtInterpA.Text);
                        axis.Interp.LnXYZA(x, y, z, a, synVel, synAcc, velEnd);
                        Log($"添加直线段: X={x}, Y={y}, Z={z}, A={a}");
                        break;
                }

                var space = axis.Interp.GetCrdSpace();
                TxtInterpSpace.Text = $"缓存空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnInterpAddArc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) { Log("请先建立坐标系"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                var synVel = double.Parse(TxtInterpSynVel.Text);
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                var velEnd = double.Parse(TxtInterpVelEnd.Text);
                var dir = (short)CboInterpDir.SelectedIndex;

                var x = int.Parse(TxtInterpX.Text);
                var y = int.Parse(TxtInterpY.Text);

                axis.Interp.ArcXYR(x, y, 50000, dir, synVel, synAcc, velEnd);
                Log($"添加圆弧段: X={x}, Y={y}, 半径=50000, 方向={(dir == 0 ? "CW" : "CCW")}");

                var space = axis.Interp.GetCrdSpace();
                TxtInterpSpace.Text = $"缓存空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnInterpStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) { Log("请先建立坐标系"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                axis.Interp.OfflineStart();
                TxtInterpStatus.Text = "插补运行中...";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                Log("插补运动启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

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

        private DispatcherTimer? _interpRtTimer;
        private double _rtPosX, _rtPosY, _rtPosZ;
        private bool _interpRtRunning;

        private void BtnInterpRtStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpSetup) { Log("请先建立坐标系"); return; }
                if (_interpRtRunning) { Log("实时插补已在运行"); return; }

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);

                switch (CboInterpBufMode.SelectedIndex)
                {
                    case 0: axis.Interp.SetDynamicBufferMode(); break;
                    case 1: axis.Interp.SetDynamicKeepBufferMode(); break;
                }

                if (ChkInterpLookAhead.IsChecked == true)
                    axis.Interp.EnableLookAhead();
                else
                    axis.Interp.DisableLookAhead();

                var ratio = double.Parse(TxtInterpOverride.Text);
                axis.Interp.SetOverride(ratio);

                _rtPosX = 0;
                _rtPosY = 0;
                _rtPosZ = 0;
                _interpRtRunning = true;

                var interval = int.Parse(TxtInterpRtInterval.Text);
                _interpRtTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(interval) };
                _interpRtTimer.Tick += InterpRtTimer_Tick;
                _interpRtTimer.Start();

                axis.Interp.Start();

                TxtInterpStatus.Text = "实时插补运行中...";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                Log("实时插补启动");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void InterpRtTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                var step = int.Parse(TxtInterpRtStep.Text);
                var synVel = double.Parse(TxtInterpSynVel.Text);
                var synAcc = double.Parse(TxtInterpSynAcc.Text);
                var velEnd = double.Parse(TxtInterpVelEnd.Text);

                _rtPosX += step;
                _rtPosY += step;

                var dim = CboInterpDim.SelectedIndex;
                switch (dim)
                {
                    case 0:
                        axis.Interp.LnXY((int)_rtPosX, (int)_rtPosY, synVel, synAcc, velEnd);
                        break;
                    case 1:
                        _rtPosZ += step;
                        axis.Interp.LnXYZ((int)_rtPosX, (int)_rtPosY, (int)_rtPosZ, synVel, synAcc, velEnd);
                        break;
                    case 2:
                        _rtPosZ += step;
                        var a = int.Parse(TxtInterpA.Text);
                        axis.Interp.LnXYZA((int)_rtPosX, (int)_rtPosY, (int)_rtPosZ, a, synVel, synAcc, velEnd);
                        break;
                }

                var space = axis.Interp.GetCrdSpace();
                TxtInterpSpace.Text = $"缓存空间: {space}";
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        private void BtnInterpRtStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_interpRtRunning) return;
                _interpRtRunning = false;
                _interpRtTimer?.Stop();

                var axis = GetAxis(CboInterpAxis1.SelectedIndex + 1);
                axis.Interp.SmoothStop();

                TxtInterpStatus.Text = "实时插补已停止";
                TxtInterpStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                Log("实时插补停止");
            }
            catch (Exception ex) { Log(ex.Message); }
        }

        #endregion

        #region 状态监控

        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (!ChkAutoRefresh.IsChecked == true) return;
            RefreshMonitor();
        }

        private void BtnMonitorRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshMonitor();
        }

        private void RefreshMonitor()
        {
            try
            {
                if (!_isConnected || _motionManager == null || _motionManager.GetAxisCount() == 0)
                    return;

                var axis = GetAxis(SelectedAxis(CboMonitorAxis));
                var status = axis.GetEcatStatus();
                if (status == null) return;

                SetMonitorValue(MonPlusLimitVal, status.PlusLimitAlarm ? "触发" : "正常", status.PlusLimitAlarm);
                SetMonitorValue(MonMinusLimitVal, status.MinusLimitAlarm ? "触发" : "正常", status.MinusLimitAlarm);
                SetMonitorValue(MonAxisAlarmVal, status.AxisAlarm ? "报警" : "正常", status.AxisAlarm);
                SetMonitorValue(MonFollowAlarmVal, status.FollowAlarm ? "报警" : "正常", status.FollowAlarm);
                SetMonitorValue(MonSmoothStopVal, status.SmoothStopAlarm ? "触发" : "正常", status.SmoothStopAlarm);
                SetMonitorValue(MonScramVal, status.Scram ? "触发" : "正常", status.Scram);
                SetMonitorValue(MonEnabledVal, status.EnableAxis ? "已使能" : "未使能", status.EnableAxis);
                SetMonitorValue(MonPlanningVal, status.Planning ? "运动中" : "停止", status.Planning);
                MonMotionTypeVal.Text = status.MotionType;

                MonPlanPosVal.Text = status.PlannedLocation.ToString("F2");
                MonDrivePosVal.Text = status.DriveLocation.ToString("F2");
                MonFollowErrVal.Text = status.FollowErr.ToString("F2");

                MonPlanVelVal.Text = status.PlannedVel.ToString("F2");
                MonDriveVelVal.Text = status.DriveVel.ToString("F2");
                MonPlanAccVal.Text = status.PlannedAccVel.ToString("F2");
                MonDriveAccVal.Text = status.DriveAccVel.ToString("F2");

                TxtMoveCurrentPos.Text = $"规划位置: {status.PlannedLocation:F2}";
                TxtMoveCurrentEnc.Text = $"编码器位置: {status.DriveLocation:F2}";
                TxtMoveCurrentVel.Text = $"速度: {status.DriveVel:F2}";
                TxtMoveFollowErr.Text = $"跟随误差: {status.FollowErr:F2}";
            }
            catch { }
        }

        private void SetMonitorValue(System.Windows.Documents.Run valText, string text, bool isAlarm)
        {
            valText.Text = text;
            valText.Foreground = isAlarm
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
        }

        #endregion
    }
}
