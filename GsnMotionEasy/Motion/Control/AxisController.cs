using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Control.Details;
using GsnMotionEasy.Motion.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control
{
    /// <summary>
    /// 运动控制器轴（Gsn / Glink-II 卡）。
    /// 与 GenMotionEasy.AxisController 结构一致，但：
    ///  - 不含 EtherCAT 总线成员（EcatLoad/EcatState/EcatStart）；
    ///  - 状态查询使用非 Ecat 函数（GTN_GetEncPos / GTN_GetAxisEncVel 等）。
    /// </summary>
    public class AxisController : IAxisController
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock = new object();
        private bool _homingTag = false;

        public IAxisHome AxisHome { get; }
        public IPTMotion PT { get; }
        public IFollowMotion Follow { get; }
        public IPVTMotion PVT { get; }
        public IMoveMotion Move { get; }
        public IFollowExMotion FollowEx { get; }
        public IGearMotion Gear { get; }
        public IInterpMotion Interp { get; }
        public IPointMotion Point { get; }
        public IJogMotion Jog { get; }


        public AxisController(short core, short axis, short crd = 1)
        {
            _core = core;
            _axis = axis;
            AxisHome = new AxisHome(_core, axis, _lock);
            PT = new AxisMotionPT(_core, axis, _lock);
            Follow = new AxisMotionFollow(_core, axis, _lock);
            PVT = new AxisMotionPVT(_core, axis, _lock);
            Move = new AxisMotionMove(_core, axis, _lock);
            FollowEx = new AxisMotionFollowEx(_core, axis, _lock);
            Gear = new AxisMotionGear(_core, axis, _lock);
            Interp = new AxisMotionInterp(_core, crd, _lock);
            Point = new AxisMotionPoint(_core, axis, _lock, () => ClearAlarm(), () => GetStatus(), () => EnableAxis());
            Jog = new AxisMotionJog(_core, axis, _lock);
        }


        public short Restart()
        {
            lock (_lock)
            {
                return GTN_Reset(_core);
            }
        }

        public short LoadConfig(string path)
        {
            lock (_lock)
            {
                return GTN_LoadConfig(_core, path);
            }
        }


        public short EnableAxis()
        {
            lock (_lock)
            {
                return GTN_AxisOn(_core, _axis);
            }
        }


        public short DisableAxis()
        {
            lock (_lock)
            {
                return GTN_AxisOff(_core, _axis);
            }
        }


        public short ClearAlarm(short count = 1)
        {
            lock (_lock)
            {
                return GTN_ClrSts(_core, _axis, count);
            }
        }


        /// <summary>
        /// 查询轴状态（使用非 Ecat 函数：GTN_GetEncPos / GTN_GetAxisEncVel 等）。
        /// </summary>
        public StatusInfo? GetStatus()
        {
            StatusInfo status = new StatusInfo();
            short rtn;
            int tag;
            uint p;
            rtn = GTN_GetSts(_core, _axis, out tag, 1, out p);
            if (rtn != 0) { return null; }
            //伺服报警
            if ((tag & 0x2) != 0)
            {
                status.AxisAlarm = true;
                if (status.AxisAlarm)
                {
                    Console.WriteLine($"{_axis}轴报警触发");
                }
            }
            else
            {
                status.AxisAlarm = false;
            }
            // 跟随误差越限标志
            if ((tag & 0x10) != 0)
            {
                status.FollowAlarm = true;
            }
            else
            {
                status.FollowAlarm = false;
            }

            //平滑停止触发
            if ((tag & 0x80) != 0)
            {
                //触发
                status.SmoothStopAlarm = true;
            }
            else
            {
                status.SmoothStopAlarm = false;
            }
            //急停触发
            if ((tag & 0x100) != 0)
            {
                status.Scram = true;
            }
            else
            {
                status.Scram = false;
            }
            //伺服使能标志
            if ((tag & 0x200) != 0)
            {
                //使能
                status.EnableAxis = true;
            }
            else
            {
                status.EnableAxis = false;
            }
            //规划器正在运动标志
            if ((tag & 0x400) != 0)
            {
                //正在运动
                status.Planning = true;
            }
            else
            {
                status.Planning = false;
            }
            //正限位
            if ((tag & 0x20) != 0)
            {
                status.PlusLimitAlarm = true;
            }
            else
            {
                status.PlusLimitAlarm = false;
            }
            //负限位
            if ((tag & 0x40) != 0)
            {
                status.MinusLimitAlarm = true;
            }
            else
            {
                status.MinusLimitAlarm = false;
            }
            int pValue;
            uint pClock;
            GTN_GetPrfMode(_core, _axis, out pValue, 1, out pClock);
            switch (pValue)
            {
                case 0:
                    status.MotionType = "Trap";
                    break;
                case 1:
                    status.MotionType = "Jog";
                    break;
                case 2:
                    status.MotionType = "PT";
                    break;
                case 3:
                    status.MotionType = "Gear";
                    break;
                case 4:
                    status.MotionType = "Follow";
                    break;
                case 5:
                    status.MotionType = "Interpolation";
                    break;
                case 6:
                    status.MotionType = "PVT";
                    break;
                default:
                    status.MotionType = "未知";
                    break;
            }
            double encValue;
            uint encClock;
            double prfValue;
            uint prfClock;
            GTN_GetEncPos(_core, _axis, out encValue, 1, out encClock);//编码器位置
            GTN_GetPrfPos(_core, _axis, out prfValue, 1, out prfClock);//规划位置
            status.DriveLocation = encValue;
            status.PlannedLocation = prfValue;
            status.FollowErr = prfValue - encValue;
            double encVelValue;
            uint encVelClock;
            GTN_GetAxisEncVel(_core, _axis, out encVelValue, 1, out encVelClock);
            status.DriveVel = encVelValue;
            double encAccValue;
            uint encAccClock;
            GTN_GetAxisEncAcc(_core, _axis, out encAccValue, 1, out encAccClock);
            status.DriveAccVel = encAccValue;
            double prfVelValue;
            uint prfVelClock;
            GTN_GetAxisPrfVel(_core, _axis, out prfVelValue, 1, out prfVelClock);
            status.PlannedVel = prfVelValue;
            double prfVelAccValue;
            uint prfVelAccClock;
            GTN_GetAxisPrfAcc(_core, _axis, out prfVelAccValue, 1, out prfVelAccClock);
            status.PlannedAccVel = prfVelAccValue;
            return status;
        }


        /// <summary>
        /// 查询剩余距离：目标位置 - 编码器位置
        /// </summary>
        public int GetRemainingDistance(int targetPos)
        {
            double encPos;
            uint clock;
            GTN_GetEncPos(_core, _axis, out encPos, 1, out clock);
            return targetPos - (int)encPos;
        }

        /// <summary>
        /// 等待轴到达目标位置并停止运动。
        /// 根据速度与距离估算运动时间，再加上 extraSeconds 秒的缓冲。
        /// </summary>
        /// <param name="targetPos">目标位置</param>
        /// <param name="vel">运动速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <param name="tolerance">规划位置与编码器位置允许的偏差</param>
        /// <param name="extraSeconds">额外等待秒数（在估算时间上增加）</param>
        /// <returns>是否在超时前到达</returns>
        public async Task<bool> WaitAxisStop(int targetPos, double vel, double acc, double dec,
            double tolerance = 10, int extraSeconds = 5)
        {
            double encPos;
            uint encClock;
            GTN_GetEncPos(_core, _axis, out encPos, 1, out encClock);
            double distance = Math.Abs(targetPos - encPos);

            double safeVel = Math.Max(vel, 1);
            double safeAcc = Math.Max(acc, 1);
            double safeDec = Math.Max(dec, 1);
            double estimatedTime = distance / safeVel + safeVel / safeAcc + safeVel / safeDec;
            int waitMs = (int)(estimatedTime * 1000) + extraSeconds * 1000;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < waitMs)
            {
                double prfPos;
                uint prfClock;
                GTN_GetPrfPos(_core, _axis, out prfPos, 1, out prfClock);

                double encPosNow;
                uint encClockNow;
                GTN_GetEncPos(_core, _axis, out encPosNow, 1, out encClockNow);

                if (Math.Abs(prfPos - encPosNow) <= tolerance && Math.Abs(prfPos - targetPos) <= tolerance)
                {
                    return true;
                }

                await Task.Delay(50);
            }

            return false;
        }


        public void StopAxis()
        {
            lock (_lock)
            {
                GTN_Stop(_core, 1 << (_axis - 1), 1 << (_axis - 1));
            }
        }

    }
}
