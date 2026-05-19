using GenMotionEasy.Model;
using GenMotionEasy.Motion.Control.Details;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GTN.glink;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control
{
    public class AxisController : IAxisController
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock = new object();
        private bool _homingTag = false;
        private int _pos;
        private double _vel;
        private double _acc;
        private double _dec;

        public IAxisHome AxisHome { get; }
        public IPTMotion PT { get; }
        public IFollowMotion Follow { get; }
        public IPVTMotion PVT { get; }
        public IMoveMotion Move { get; }
        public IFollowExMotion FollowEx { get; }
        public IGearMotion Gear { get; }
        public IInterpMotion Interp { get; }


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


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public short EcatLoad()
        {
            GTN_TerminateEcatComm(_core);
            lock (_lock)
            {
                return GTN_InitEcatComm(_core);
            }
        }


        //查询总线状态
        public short EcatState()
        {
            lock (_lock)
            {
                short state;
                GTN_IsEcatReady(_core, out state);
                return state;
            }
        }


        //启动Ecat通信
        public short EcatStart()
        {
            GT_GLinkInitEx(0, 0);
            lock (_lock)
            {
                return GTN_StartEcatComm(_core);
            }
        }

        public short ClearAlarm(short count = 1)
        {
            //首先看一下是否轴报警

            lock (_lock)
            {
                return GTN_ClrSts(_core, _axis, count);
            }
        }

        public StatusInfo GetEcatStatus()
        {
            StatusInfo status = new StatusInfo();
            short rtn;
            int tag;
            uint p;
            //internal static extern short GTN_GetSts(short core, short axis, out Int32 pSts, short count, out UInt32 pClock);
            rtn = GTN_GetSts(1, _axis, out tag, 1, out p);
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
            GTN_GetPrfMode(1, _axis, out pValue, 1, out pClock);
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
            int encValue;
            uint encClock;
            double prfValue;
            uint prfClock;
            GTN_GetEcatEncPos(1, _axis, out encValue);//编码器位置
            GTN_GetPrfPos(1, _axis, out prfValue, 1, out prfClock);//规划位置
            status.DriveLocation = encValue;
            status.PlannedLocation = prfValue;
            status.FollowErr = prfValue - encValue;
            int encVelValue;
            uint encVelClock;
            //GTN_GetAxisEncVel(1, _axis, out encVelValue, 1, out encVelClock);
            GTN_GetEcatEncVel(1, _axis, out encVelValue);
            status.DriveVel = encVelValue;
            double encAccValue;
            uint encAccClock;
            GTN_GetAxisEncAcc(1, _axis, out encAccValue, 1, out encAccClock);
            status.DriveAccVel = encAccValue;
            double prfVelValue;
            uint prfVelClock;
            GTN_GetAxisPrfVel(1, _axis, out prfVelValue, 1, out prfVelClock);
            status.PlannedVel = prfVelValue;
            double prfVelAccValue;
            uint prfVelAccClock;
            GTN_GetAxisPrfAcc(1, _axis, out prfVelAccValue, 1, out prfVelAccClock);
            status.PlannedAccVel = prfVelAccValue;
            return status;
        }



        //点位运动
        public void PointMove(int pos, double vel, double acc, double dec)
        {
            this._acc = acc;
            this._vel = vel;
            this._pos = pos;
            this._dec = dec;
            lock (_lock)
            {
                ClearAlarm();
                //查询是否使能
                StatusInfo status = GetEcatStatus();
                if (status.EnableAxis == false)
                {
                    EnableAxis();
                    Thread.Sleep(100);
                }
                TTrapPrm trap;
                short rtn = GTN_PrfTrap(_core, _axis);
                rtn = GTN_GetTrapPrm(_core, _axis, out trap);
                trap.acc = acc;
                trap.dec = dec;
                trap.smoothTime = 50;
                rtn = GTN_SetTrapPrm(_core, _axis, ref trap);
                double prfValue;
                int encValue;
                uint encClock, prfClock;
                GTN_GetEcatEncPos(1, _axis, out encValue);//编码器位置
                GTN_GetPrfPos(1, _axis, out prfValue, 1, out prfClock);//规划位置
                rtn = GTN_SetVel(_core, _axis, vel);
                Console.WriteLine("axis:" + _axis + ",pos:" + pos + ",acc:" + acc + ",dec:" + dec + ",vel:" + vel);
                rtn = GTN_SetPos(_core, _axis, pos);
                rtn = GTN_Update(_core, 1 << (_axis - 1));
            }
        }


        public void PointAbsMove(int pos, double vel, double acc, double dec)
        {
            lock (_lock)
            {
                TMoveAbsolutePrmEx move = new TMoveAbsolutePrmEx();
                move.acc = acc;
                move.dec = dec;
                move.pos = pos;
                move.vel = vel;
                GTN_MoveAbsoluteEx(_core, _axis, ref move);
            }
        }

        public void StopAxis()
        {
            lock (_lock)
            {
                GTN_Stop(_core, 1, 1);
            }
        }



    }
}
