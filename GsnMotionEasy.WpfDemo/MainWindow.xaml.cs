using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion;
using GsnMotionEasy.Motion.Control;

namespace GsnMotionEasy.WpfDemo
{
    public partial class MainWindow : Window
    {
        private MotionControlManager? _motionManager;
        private readonly FiveAxisManager _fiveAxisManager = new();
        private FiveAxisConfig? _config;

        private readonly DispatcherTimer _statusTimer;
        private readonly DispatcherTimer _rtTimer;
        private double _rtX, _rtY;

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 1; i <= 8; i++) CboAxisCount.Items.Add(i.ToString());
            CboAxisCount.SelectedIndex = 3;

            CboInterpDim.Items.Add("2D");
            CboInterpDim.Items.Add("3D");
            CboInterpDim.Items.Add("4D");
            CboInterpDim.SelectedIndex = 0;

            CboInterpBufMode.Items.Add("动态默认");
            CboInterpBufMode.Items.Add("动态保持");
            CboInterpBufMode.SelectedIndex = 0;

            CbbMachineType.ItemsSource = Enum.GetValues(typeof(MachineType));
            CbbMachineType.SelectedItem = MachineType.RW_C_ON_A;
            CbbCalibMode.ItemsSource = Enum.GetValues(typeof(CalibrationMode));
            CbbCalibMode.SelectedItem = CalibrationMode.Iteration;

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _statusTimer.Tick += StatusTimer_Tick;

            _rtTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _rtTimer.Tick += RtTimer_Tick;
        }

        private void Log(string msg)
        {
            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
            TxtLog.ScrollToEnd();
        }

        private void LogError(string prefix, Exception ex) => Log($"{prefix} 失败: {ex.Message}");

        private static short SelectedAxis(ComboBox cbo) => (short)(cbo.SelectedIndex + 1);

        private AxisController GetAxis(ComboBox cbo) => _motionManager!.GetAxisController(SelectedAxis(cbo));

        private void RefreshAxisCombos(int count)
        {
            foreach (var cbo in new[] { CboAxisCtrl, CboHomeAxis, CboMoveAxis, CboJogAxis,
                                        CboInterpAxis1, CboInterpAxis2, CboInterpAxis3 })
            {
                cbo.Items.Clear();
                for (int i = 1; i <= count; i++) cbo.Items.Add(i.ToString());
                if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
            }
        }

        // ── 工具栏 ──

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _motionManager = new MotionControlManager();
                if (!_motionManager.Open())
                {
                    Log("打开卡失败：GTN_Open 返回非 0");
                    return;
                }
                LblCardStatus.Text = "已连接";
                LblCardStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4E, 0xC9, 0xB0));
                Log("打开卡成功（GTN_Open(5,2) + Reset）");
            }
            catch (Exception ex) { LogError("打开卡", ex); }
        }

        private void BtnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            if (_motionManager == null) { Log("请先打开卡"); return; }
            try
            {
                short rtn = _motionManager.LoadConfig("gtn_core1.cfg");
                Log(rtn == 0 ? "加载配置成功" : $"加载配置失败，错误码 {rtn}");
            }
            catch (Exception ex) { LogError("加载配置", ex); }
        }

        private void BtnInitAxes_Click(object sender, RoutedEventArgs e)
        {
            if (_motionManager == null) { Log("请先打开卡"); return; }
            try
            {
                int count = CboAxisCount.SelectedIndex + 1;
                for (int i = 1; i <= count; i++) _motionManager.AddAxis((short)i, 1);
                RefreshAxisCombos(count);
                _statusTimer.Start();
                Log($"已初始化 {count} 个轴");
            }
            catch (Exception ex) { LogError("初始化轴", ex); }
        }

        // ── 轴控制 ──

        private void CboAxis_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void BtnEnable_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboAxisCtrl).EnableAxis(); } catch (Exception ex) { LogError("使能", ex); } }

        private void BtnDisable_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboAxisCtrl).DisableAxis(); } catch (Exception ex) { LogError("禁用", ex); } }

        private void BtnClearAlarm_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboAxisCtrl).ClearAlarm(); } catch (Exception ex) { LogError("清报警", ex); } }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboAxisCtrl).Restart(); } catch (Exception ex) { LogError("复位", ex); } }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboAxisCtrl).StopAxis(); } catch (Exception ex) { LogError("停止", ex); } }

        // ── 回零 ──

        private async void BtnStartHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboHomeAxis);
                short method = short.Parse(TxtHomeMode.Text);
                double switchVel = double.Parse(TxtHomeSwitchVel.Text);
                double indexVel = double.Parse(TxtHomeIndexVel.Text);
                double acc = double.Parse(TxtHomeAcc.Text);
                int offset = int.Parse(TxtHomeOffset.Text);

                TxtHomeStatus.Text = "回零中...";
                Log($"轴{SelectedAxis(CboHomeAxis)} 开始回零 (mode={method})");
                await axis.AxisHome.Home(switchVel, indexVel, acc, method, offset, 0, true, 10);
                bool ok = axis.AxisHome.GetHomeStatus();
                TxtHomeStatus.Text = ok ? "回零成功" : "回零失败/超时";
                Log(TxtHomeStatus.Text);
            }
            catch (Exception ex) { LogError("回零", ex); }
        }

        private void BtnResetHome_Click(object sender, RoutedEventArgs e)
        {
            try { GetAxis(CboHomeAxis).AxisHome.ResetHomeStatus(); TxtHomeStatus.Text = "已重置"; }
            catch (Exception ex) { LogError("重置回零", ex); }
        }

        // ── 点位运动 ──

        private async void BtnPointMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboMoveAxis);
                int pos = int.Parse(TxtMovePos.Text);
                double vel = double.Parse(TxtMoveVel.Text);
                double acc = double.Parse(TxtMoveAcc.Text);
                double dec = double.Parse(TxtMoveDec.Text);
                await Task.Run(() => axis.Point.PointMove(pos, vel, acc, dec));
                Log($"轴{SelectedAxis(CboMoveAxis)} 点位运动 -> {pos}");
            }
            catch (Exception ex) { LogError("点位运动", ex); }
        }

        private void BtnAbsMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboMoveAxis);
                int pos = int.Parse(TxtMovePos.Text);
                double vel = double.Parse(TxtMoveVel.Text);
                double acc = double.Parse(TxtMoveAcc.Text);
                double dec = double.Parse(TxtMoveDec.Text);
                axis.Point.PointAbsMove(pos, vel, acc, dec);
                Log($"轴{SelectedAxis(CboMoveAxis)} 绝对定位 -> {pos}");
            }
            catch (Exception ex) { LogError("绝对定位", ex); }
        }

        private void BtnMoveStop_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboMoveAxis).StopAxis(); } catch (Exception ex) { LogError("停止", ex); } }

        // ── Jog ──

        private void BtnJog_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboJogAxis);
                double vel = double.Parse(TxtJogVel.Text);
                double acc = double.Parse(TxtJogAcc.Text);
                double dec = double.Parse(TxtJogDec.Text);
                axis.Jog.SetJogMode(acc, dec);
                axis.Jog.JogMove(vel);
            }
            catch (Exception ex) { LogError("Jog", ex); }
        }

        private void BtnJog_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try { GetAxis(CboJogAxis).Jog.Stop(); }
            catch { }
        }

        // ── 插补 ──

        private void BtnInterpSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboInterpAxis1);
                int dim = CboInterpDim.SelectedIndex + 2;
                double synVel = double.Parse(TxtInterpSynVel.Text);
                double synAcc = double.Parse(TxtInterpSynAcc.Text);
                short a1 = SelectedAxis(CboInterpAxis1);
                short a2 = SelectedAxis(CboInterpAxis2);
                short a3 = SelectedAxis(CboInterpAxis3);

                if (dim == 2) axis.Interp.SetupCrd2D(a1, a2, synVel, synAcc);
                else if (dim == 3) axis.Interp.SetupCrd3D(a1, a2, a3, synVel, synAcc);
                else axis.Interp.SetupCrd4D(a1, a2, a3, a2, synVel, synAcc);
                Log($"建立 {dim}D 坐标系 (轴{a1},{a2}{(dim >= 3 ? "," + a3 : "")})");
            }
            catch (Exception ex) { LogError("建立坐标系", ex); }
        }

        private void BtnInterpClear_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboInterpAxis1).Interp.OfflineClear(); Log("已清空插补缓冲"); } catch (Exception ex) { LogError("清空缓冲", ex); } }

        private void BtnInterpAddLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboInterpAxis1);
                int x = int.Parse(TxtInterpX.Text);
                int y = int.Parse(TxtInterpY.Text);
                double synVel = double.Parse(TxtInterpSynVel.Text);
                double synAcc = double.Parse(TxtInterpSynAcc.Text);
                double velEnd = double.Parse(TxtInterpVelEnd.Text);
                axis.Interp.LnXY(x, y, synVel, synAcc, velEnd);
                Log($"添加直线 XY -> ({x},{y})");
            }
            catch (Exception ex) { LogError("添加直线", ex); }
        }

        private void BtnInterpSquare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboInterpAxis1);
                double synVel = double.Parse(TxtInterpSynVel.Text);
                double synAcc = double.Parse(TxtInterpSynAcc.Text);
                axis.Interp.OfflineClear();
                axis.Interp.LnXY(0, 0, synVel, synAcc, 0);
                axis.Interp.LnXY(0, 50000, synVel, synAcc, 0);
                axis.Interp.LnXY(50000, 50000, synVel, synAcc, 0);
                axis.Interp.LnXY(50000, 0, synVel, synAcc, 0);
                axis.Interp.LnXY(0, 0, synVel, synAcc, 0);
                Log("已生成示例方形轨迹（5 段直线），点击“离线启动”运行");
            }
            catch (Exception ex) { LogError("画方形", ex); }
        }

        private void BtnInterpOfflineStart_Click(object sender, RoutedEventArgs e)
        { try { GetAxis(CboInterpAxis1).Interp.OfflineStart(); Log("离线插补启动"); } catch (Exception ex) { LogError("离线启动", ex); } }

        private void BtnInterpRtStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var axis = GetAxis(CboInterpAxis1);
                if (CboInterpBufMode.SelectedIndex == 0) axis.Interp.SetDynamicBufferMode();
                else axis.Interp.SetDynamicKeepBufferMode();

                if (ChkInterpLookAhead.IsChecked == true) axis.Interp.EnableLookAhead();

                double ov = double.Parse(TxtInterpOverride.Text);
                axis.Interp.SetOverride(ov);

                _rtX = 0; _rtY = 0;
                axis.Interp.Start();
                _rtTimer.Interval = TimeSpan.FromMilliseconds(double.Parse(TxtInterpRtInterval.Text));
                _rtTimer.Start();
                Log("实时插补启动");
            }
            catch (Exception ex) { LogError("实时启动", ex); }
        }

        private void RtTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var axis = GetAxis(CboInterpAxis1);
                double step = double.Parse(TxtInterpRtStep.Text);
                double synVel = double.Parse(TxtInterpSynVel.Text);
                double synAcc = double.Parse(TxtInterpSynAcc.Text);
                _rtX += step;
                axis.Interp.LnXY((int)_rtX, (int)_rtY, synVel, synAcc, 0);
            }
            catch { }
        }

        private void BtnInterpRtStop_Click(object sender, RoutedEventArgs e)
        {
            _rtTimer.Stop();
            try { GetAxis(CboInterpAxis1).Interp.SmoothStop(); Log("实时插补已停止"); }
            catch (Exception ex) { LogError("停止实时", ex); }
        }

        private void BtnInterpSmoothStop_Click(object sender, RoutedEventArgs e)
        { _rtTimer.Stop(); try { GetAxis(CboInterpAxis1).Interp.SmoothStop(); } catch (Exception ex) { LogError("平滑停止", ex); } }

        private void BtnInterpEStop_Click(object sender, RoutedEventArgs e)
        { _rtTimer.Stop(); try { GetAxis(CboInterpAxis1).Interp.EmergencyStop(); } catch (Exception ex) { LogError("急停", ex); } }

        // ── 状态刷新 ──

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            if (_motionManager == null) return;
            try { RefreshAxisStatus(); } catch { }
            try { RefreshMoveStatus(); } catch { }
            try { RefreshInterpStatus(); } catch { }
        }

        private void RefreshAxisStatus()
        {
            if (CboAxisCtrl.SelectedIndex < 0) return;
            var s = GetAxis(CboAxisCtrl).GetStatus();
            if (s == null) return;
            TxtMotionType.Text = s.MotionType;
            TxtEnableState.Text = s.EnableAxis ? "是" : "否";
            TxtPlanning.Text = s.Planning ? "是" : "否";
            TxtPlusLimit.Text = s.PlusLimitAlarm ? "触发" : "正常";
            TxtMinusLimit.Text = s.MinusLimitAlarm ? "触发" : "正常";
            TxtAxisAlarm.Text = s.AxisAlarm ? "报警" : "正常";
            TxtPlanPos.Text = s.PlannedLocation.ToString("F2");
            TxtEncPos.Text = s.DriveLocation.ToString("F2");
            TxtPlanVel.Text = s.PlannedVel.ToString("F2");
            TxtFollowErr.Text = s.FollowErr.ToString("F2");
        }

        private void RefreshMoveStatus()
        {
            if (CboMoveAxis.SelectedIndex < 0) return;
            var s = GetAxis(CboMoveAxis).GetStatus();
            if (s == null) return;
            TxtMoveCurPos.Text = s.PlannedLocation.ToString("F2");
            TxtMoveEnc.Text = s.DriveLocation.ToString("F2");
            TxtMoveVelDisp.Text = s.PlannedVel.ToString("F2");
        }

        private void RefreshInterpStatus()
        {
            if (CboInterpAxis1.SelectedIndex < 0) return;
            var interp = GetAxis(CboInterpAxis1).Interp;
            TxtInterpRunning.Text = interp.IsRunning() ? "是" : "否";
            TxtInterpSpace.Text = interp.GetCrdSpace().ToString();
            TxtInterpPos.Text = interp.GetCrdPos().ToString("F2");
            TxtInterpVel.Text = interp.GetCrdVel().ToString("F2");
        }

        // ── 五轴门面 ──

        private void BtnBrowseIni_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            { Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*", Title = "选择运动学配置 ini" };
            if (dlg.ShowDialog() == true) TxtIniPath.Text = dlg.FileName;
        }

        private async void BtnInitFiveAxis_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtIniPath.Text) || !File.Exists(TxtIniPath.Text))
            { Log("请先选择有效的 ini 文件"); return; }
            try
            {
                _config = await Task.Run(() => FiveAxisConfig.LoadFromIni(TxtIniPath.Text));
                _config.Core = short.Parse(TxtCore.Text);
                _config.Group = short.Parse(TxtGroup.Text);
                await Task.Run(() => _fiveAxisManager.EnterFiveAxis(_config));
                Log($"五轴初始化成功，机床类型={_config.MachineType}");
            }
            catch (Exception ex) { LogError("五轴初始化", ex); }
        }

        private async void BtnEnableRtcp_Click(object sender, RoutedEventArgs e)
        {
            try { await Task.Run(() => _fiveAxisManager.Controller.Motion.EnableRtcpMode()); Log("RTCP 模式已启用"); }
            catch (Exception ex) { LogError("启用 RTCP", ex); }
        }

        private async void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double vel = double.Parse(TxtVel.Text);
                double acc = double.Parse(TxtAcc.Text);
                var segments = new List<TrajectorySegment>
                {
                    TrajectorySegment.Linear(new[] { 229.127, -620.022, -113.714, 0, -90, 0, 0, 0 }, vel, acc),
                    TrajectorySegment.Linear(new[] { 243.189, -620.327, -113.841, 0, -90, 0, 0, 0 }, vel, acc),
                    TrajectorySegment.Circular(
                        new[] { 253.890, -631.946, -114.294, 0, 0, 0, 0, 0 },
                        new[] { 250.659, -623.417, -113.901, 0, -45 }, vel * 0.33, acc),
                    TrajectorySegment.Linear(new[] { 252.671, -776.320, -114.224, 0, 0, 0, 0, 0 }, vel, acc),
                    TrajectorySegment.Circular(
                        new[] { 241.687, -787.408, -113.724, 0, 90, 0, 0, 0 },
                        new[] { 249.186, -784.479, -113.809, 0, 45 }, vel * 0.33, acc),
                };
                Log($"开始执行五轴轨迹，共 {segments.Count} 段");
                await Task.Run(() => _fiveAxisManager.Controller.ExecuteTrajectory(segments));
                Log("五轴轨迹执行完成");
            }
            catch (Exception ex) { LogError("执行轨迹", ex); }
        }

        private async void BtnExitFiveAxis_Click(object sender, RoutedEventArgs e)
        {
            try { await Task.Run(() => _fiveAxisManager.ExitFiveAxis()); Log("已退出五轴模式"); }
            catch (Exception ex) { LogError("退出五轴", ex); }
        }

        // ── 五轴标定 ──

        private async void BtnCalibrate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var machineType = (MachineType)CbbMachineType.SelectedItem;
                var mode = (CalibrationMode)CbbCalibMode.SelectedItem;
                short core = short.Parse(TxtCore.Text);
                bool use9P = Chk9P.IsChecked == true;

                var s0 = ParsePoints(TxtS0.Text);
                var s180 = ParsePoints(TxtS180.Text);
                var p0 = ParsePoints(TxtP0.Text);

                Log($"开始标定：模式={mode}, 9P={use9P}, S0={s0.Count}, S180={s180.Count}, P0={p0.Count}");

                CalibrationResult? result = null;
                if (use9P)
                {
                    var input = new CalibrationInput9P
                    {
                        Core = core,
                        MachineType = machineType,
                        Mode = mode,
                        AxisSide = new short[] { 0, 1, 0 },
                        AxisDir = new short[] { 0, 1, 0 },
                        PointsS0 = s0,
                        PointsS180 = s180,
                        PointsP0 = p0,
                    };
                    result = await Task.Run(() => _fiveAxisManager.Controller.Calibration.Calibrate9P(input));
                }
                else
                {
                    var input = new CalibrationInput
                    {
                        Core = core,
                        MachineType = machineType,
                        Mode = mode,
                        AxisSide = new short[] { 0, 1, 0 },
                        AxisDir = new short[] { 0, 1, 0 },
                        PointsS0 = s0,
                        PointsS180 = s180,
                        PointsP0 = p0,
                    };
                    result = await Task.Run(() => _fiveAxisManager.Controller.Calibration.Calibrate(input));
                }

                Log($"标定完成：迭代次数={result.IterationCount}, 总误差={result.DirAllError:F6}");
                Log($"  主轴点=[{result.PrimaryAxisPoint[0]:F3}, {result.PrimaryAxisPoint[1]:F3}, {result.PrimaryAxisPoint[2]:F3}]");
                Log($"  从轴点=[{result.SlaveAxisPoint[0]:F3}, {result.SlaveAxisPoint[1]:F3}, {result.SlaveAxisPoint[2]:F3}]");

                if (_config != null)
                {
                    result.ApplyTo(_config);
                    Log("已将标定结果应用到当前 FiveAxisConfig");
                }
            }
            catch (Exception ex) { LogError("标定", ex); }
        }

        private static List<CalibrationPoint> ParsePoints(string text)
        {
            var list = new List<CalibrationPoint>();
            if (string.IsNullOrWhiteSpace(text)) return list;
            foreach (var line in text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                list.Add(CalibrationPoint.Parse(trimmed));
            }
            return list;
        }

        private async void BtnRefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var status = await Task.Run(() => _fiveAxisManager.Controller.Status.GetStatus());
                TxtStatus.Text = $"使能={status.IsEnabled}, 运动中={status.IsRunning}, 指令流执行中={status.CommandListExecuting}, 前瞻段数={status.LookAheadSegCount}";
                Log($"五轴状态: {TxtStatus.Text}");
            }
            catch (Exception ex) { LogError("查询状态", ex); }
        }
    }
}
