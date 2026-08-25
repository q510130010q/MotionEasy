using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// Follow运动模式 - 第3章
    /// 用于两轴或多轴之间的位置同步和速度同步。
    /// 被跟随的轴叫主轴，跟随的轴叫从轴。
    /// </summary>
    public class AxisMotionFollow : IFollowMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;

        public AxisMotionFollow(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        #region 模式设置

        /// <summary>
        /// 设置指定轴为Follow运动模式
        /// </summary>
        /// <param name="dir">跟随方式: 0=双向跟随, 1=正向跟随, -1=负向跟随</param>
        public void SetMode(short dir = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PrfFollow(_core, _axis, dir), "GTN_PrfFollow");
            }
        }

        #endregion

        #region 主轴设置

        /// <summary>
        /// 设置Follow运动模式跟随主轴
        /// </summary>
        /// <param name="masterIndex">主轴索引（最好小于从轴轴号以减少跟随滞后）</param>
        /// <param name="masterType">主轴类型: FOLLOW_MASTER_PROFILE(2)=规划位置, FOLLOW_MASTER_ENCODER(1)=编码器, FOLLOW_MASTER_AXIS(3)=轴</param>
        /// <param name="masterItem">合成轴类型，仅masterType=FOLLOW_MASTER_AXIS时有效: 0=规划位置, 1=编码器位置</param>
        public void SetMaster(short masterIndex, short masterType = FOLLOW_MASTER_PROFILE, short masterItem = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_SetFollowMaster(_core, _axis, masterIndex, masterType, masterItem),
                    "GTN_SetFollowMaster");
            }
        }

        /// <summary>
        /// 读取Follow运动模式跟随主轴
        /// </summary>
        public void GetMaster(out short masterIndex, out short masterType, out short masterItem)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_GetFollowMaster(_core, _axis, out masterIndex, out masterType, out masterItem),
                    "GTN_GetFollowMaster");
            }
        }

        #endregion

        #region 启动跟随条件

        /// <summary>
        /// 设置Follow运动模式启动跟随条件
        /// </summary>
        /// <param name="followEvent">启动条件: FOLLOW_EVENT_START(1)=立即启动, FOLLOW_EVENT_PASS(2)=主轴穿越设定位置后启动</param>
        /// <param name="masterDir">主轴运动方向（穿越启动时有效）: 1=正向, -1=负向</param>
        /// <param name="pos">穿越位置（仅FOLLOW_EVENT_PASS时有效），单位pulse</param>
        public void SetEvent(short followEvent, short masterDir = 1, int pos = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_SetFollowEvent(_core, _axis, followEvent, masterDir, pos),
                    "GTN_SetFollowEvent");
            }
        }

        /// <summary>
        /// 设置为立即启动
        /// </summary>
        public void SetEventStart() => SetEvent(FOLLOW_EVENT_START);

        /// <summary>
        /// 设置为主轴穿越指定位置后启动
        /// </summary>
        /// <param name="pos">穿越位置(pulse)</param>
        /// <param name="masterDir">主轴方向: 1=正向, -1=负向</param>
        public void SetEventPass(int pos, short masterDir = 1) => SetEvent(FOLLOW_EVENT_PASS, masterDir, pos);

        /// <summary>
        /// 读取Follow运动模式启动跟随条件
        /// </summary>
        public void GetEvent(out short pEvent, out short pMasterDir, out int pPos)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_GetFollowEvent(_core, _axis, out pEvent, out pMasterDir, out pPos),
                    "GTN_GetFollowEvent");
            }
        }

        #endregion

        #region 循环设置

        /// <summary>
        /// 设置Follow运动模式循环次数
        /// </summary>
        /// <param name="loop">循环次数，小于1表示无限循环</param>
        public void SetLoop(int loop)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetFollowLoop(_core, _axis, loop), "GTN_SetFollowLoop");
            }
        }

        /// <summary>
        /// 读取Follow运动模式已执行完成的循环次数
        /// </summary>
        public int GetLoop()
        {
            lock (_lock)
            {
                int loop;
                GtnErrorHelper.ThrowIfError(GTN_GetFollowLoop(_core, _axis, out loop), "GTN_GetFollowLoop");
                return loop;
            }
        }

        #endregion

        #region FIFO管理

        /// <summary>
        /// 查询Follow模式指定FIFO的剩余空间
        /// </summary>
        /// <param name="fifo">FIFO编号(0或1)</param>
        /// <returns>剩余空间（段数）</returns>
        public short GetSpace(short fifo = 0)
        {
            lock (_lock)
            {
                short space;
                GtnErrorHelper.ThrowIfError(GTN_FollowSpace(_core, _axis, out space, fifo), "GTN_FollowSpace");
                return space;
            }
        }

        /// <summary>
        /// 向Follow模式指定FIFO增加数据段
        /// </summary>
        /// <param name="masterSegment">主轴位移（相对于数据段起点），单位pulse</param>
        /// <param name="slaveSegment">从轴位移（相对于数据段起点），单位pulse</param>
        /// <param name="type">数据段类型: FOLLOW_SEGMENT_NORMAL(0)=普通段, FOLLOW_SEGMENT_EVEN(1)=匀速段, FOLLOW_SEGMENT_STOP(2)=停止段, FOLLOW_SEGMENT_CONTINUE(3)=连续段（保持FIFO间速度连续）</param>
        /// <param name="fifo">FIFO编号</param>
        public void PushData(int masterSegment, double slaveSegment, short type = FOLLOW_SEGMENT_NORMAL, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_FollowData(_core, _axis, masterSegment, slaveSegment, type, fifo),
                    "GTN_FollowData");
            }
        }

        /// <summary>
        /// 向FIFO增加普通段数据
        /// </summary>
        public void PushNormalSegment(int masterPos, double slavePos, short fifo = 0)
            => PushData(masterPos, slavePos, FOLLOW_SEGMENT_NORMAL, fifo);

        /// <summary>
        /// 向FIFO增加匀速段数据
        /// </summary>
        public void PushEvenSegment(int masterPos, double slavePos, short fifo = 0)
            => PushData(masterPos, slavePos, FOLLOW_SEGMENT_EVEN, fifo);

        /// <summary>
        /// 向FIFO增加停止段数据
        /// </summary>
        public void PushStopSegment(int masterPos, double slavePos, short fifo = 0)
            => PushData(masterPos, slavePos, FOLLOW_SEGMENT_STOP, fifo);

        /// <summary>
        /// 向FIFO增加连续段数据（用于FIFO切换时保持速度连续）
        /// 换FIFO后第一段使用此类型，起点速度比率等于上个FIFO的终点速度比率
        /// </summary>
        public void PushContinueSegment(int masterPos, double slavePos, short fifo = 0)
            => PushData(masterPos, slavePos, FOLLOW_SEGMENT_CONTINUE, fifo);

        /// <summary>
        /// 清除Follow模式指定FIFO中的数据
        /// 运动状态下该指令无效
        /// </summary>
        /// <param name="fifo">FIFO编号</param>
        public void Clear(short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_FollowClear(_core, _axis, fifo), "GTN_FollowClear");
            }
        }

        #endregion

        #region FIFO缓冲区大小

        /// <summary>
        /// 设置Follow运动模式的缓存区大小
        /// </summary>
        /// <param name="memory">0=每个缓存区有16段空间, 1=每个缓存区有512段空间</param>
        public void SetMemory(short memory)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetFollowMemory(_core, _axis, memory), "GTN_SetFollowMemory");
            }
        }

        /// <summary>
        /// 设置为16段FIFO
        /// </summary>
        public void SetMemorySmall() => SetMemory(0);

        /// <summary>
        /// 设置为512段FIFO
        /// </summary>
        public void SetMemoryLarge() => SetMemory(1);

        /// <summary>
        /// 读取Follow运动模式的缓存区大小
        /// </summary>
        public short GetMemory()
        {
            lock (_lock)
            {
                short memory;
                GtnErrorHelper.ThrowIfError(GTN_GetFollowMemory(_core, _axis, out memory), "GTN_GetFollowMemory");
                return memory;
            }
        }

        #endregion

        #region 运动控制

        /// <summary>
        /// 启动Follow运动
        /// </summary>
        /// <param name="option">按位指示所使用的FIFO: bit=0用FIFO0, bit=1用FIFO1</param>
        public void Start(int option = 0)
        {
            lock (_lock)
            {
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_FollowStart(_core, mask, option), "GTN_FollowStart");
            }
        }

        /// <summary>
        /// 切换Follow运动模式所使用的FIFO
        /// 当前工作FIFO中的数据遍历完后才会切换
        /// </summary>
        public void Switch()
        {
            lock (_lock)
            {
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_FollowSwitch(_core, mask), "GTN_FollowSwitch");
            }
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 一键配置并启动Follow运动（梯形跟随曲线）
        /// </summary>
        /// <param name="masterAxis">主轴轴号</param>
        /// <param name="accelMasterPos">加速段主轴位移</param>
        /// <param name="accelSlavePos">加速段从轴位移</param>
        /// <param name="evenMasterPos">匀速段主轴位移</param>
        /// <param name="evenSlavePos">匀速段从轴位移</param>
        /// <param name="decelMasterPos">减速段主轴位移</param>
        /// <param name="decelSlavePos">减速段从轴位移</param>
        /// <param name="loop">循环次数，小于1表示无限循环</param>
        /// <param name="dir">跟随方向: 0=双向, 1=正向, -1=负向</param>
        public void QuickStartTrapezoid(short masterAxis,
            int accelMasterPos, double accelSlavePos,
            int evenMasterPos, double evenSlavePos,
            int decelMasterPos, double decelSlavePos,
            int loop = 0, short dir = 0)
        {
            lock (_lock)
            {
                SetMode(dir);
                Clear();
                SetMaster(masterAxis);

                PushData(accelMasterPos, accelSlavePos, FOLLOW_SEGMENT_NORMAL);
                PushData(accelMasterPos + evenMasterPos, accelSlavePos + evenSlavePos, FOLLOW_SEGMENT_NORMAL);
                PushData(accelMasterPos + evenMasterPos + decelMasterPos,
                         accelSlavePos + evenSlavePos + decelSlavePos, FOLLOW_SEGMENT_NORMAL);

                SetLoop(loop);
                SetEventStart();
                Start();
            }
        }

        /// <summary>
        /// 配置FIFO1的数据并准备切换（用于双FIFO切换场景）
        /// 将过渡段数据放入另一个FIFO，设置连续段保持速度连续，然后切换
        /// </summary>
        public void PrepareSwitchFifo(short targetFifo, int[] masterPositions, double[] slavePositions)
        {
            Clear(targetFifo);
            // 第一段使用CONTINUE类型保持速度连续
            PushData(masterPositions[0], slavePositions[0], FOLLOW_SEGMENT_CONTINUE, targetFifo);
            for (int i = 1; i < masterPositions.Length; i++)
            {
                PushData(masterPositions[i], slavePositions[i], FOLLOW_SEGMENT_NORMAL, targetFifo);
            }
            Switch();
        }

        #endregion
    }
}
