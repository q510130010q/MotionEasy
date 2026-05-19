using GenMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// FollowEx运动模式 - 第6章
    /// FollowEx在Follow模式基础上扩展了S曲线支持、非对称S曲线、
    /// 以及缓冲区DO/DI/延时等辅助功能。
    /// </summary>
    public class AxisMotionFollowEx : IFollowExMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;

        public AxisMotionFollowEx(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        #region 模式设置

        /// <summary>
        /// 设置指定轴为FollowEx运动模式
        /// </summary>
        /// <param name="dir">跟随方式: 0=双向跟随, 1=正向跟随, -1=负向跟随</param>
        public void SetMode(short dir = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PrfFollowEx(_core, _axis, dir), "GTN_PrfFollowEx");
            }
        }

        #endregion

        #region 主轴设置

        /// <summary>
        /// 设置FollowEx运动模式跟随主轴
        /// </summary>
        public void SetMaster(short masterIndex, short masterType = FOLLOW_MASTER_PROFILE, short masterItem = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_SetFollowMasterEx(_core, _axis, masterIndex, masterType, masterItem),
                    "GTN_SetFollowMasterEx");
            }
        }

        /// <summary>
        /// 读取FollowEx运动模式跟随主轴
        /// </summary>
        public void GetMaster(out short masterIndex, out short masterType, out short masterItem)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_GetFollowMasterEx(_core, _axis, out masterIndex, out masterType, out masterItem),
                    "GTN_GetFollowMasterEx");
            }
        }

        #endregion

        #region 启动跟随条件

        /// <summary>
        /// 设置FollowEx运动模式启动跟随条件
        /// </summary>
        /// <param name="followEvent">启动条件: FOLLOW_EVENT_START(1)=立即启动, FOLLOW_EVENT_PASS(2)=穿越后启动</param>
        /// <param name="masterDir">主轴运动方向</param>
        /// <param name="pos">穿越位置(pulse)</param>
        public void SetEvent(short followEvent, short masterDir = 1, int pos = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_SetFollowEventEx(_core, _axis, followEvent, masterDir, pos),
                    "GTN_SetFollowEventEx");
            }
        }

        /// <summary>
        /// 设置为立即启动
        /// </summary>
        public void SetEventStart() => SetEvent(FOLLOW_EVENT_START);

        /// <summary>
        /// 设置为主轴穿越指定位置后启动
        /// </summary>
        public void SetEventPass(int pos, short masterDir = 1) => SetEvent(FOLLOW_EVENT_PASS, masterDir, pos);

        /// <summary>
        /// 读取FollowEx运动模式启动跟随条件
        /// </summary>
        public void GetEvent(out short pEvent, out short pMasterDir, out int pPos)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_GetFollowEventEx(_core, _axis, out pEvent, out pMasterDir, out pPos),
                    "GTN_GetFollowEventEx");
            }
        }

        #endregion

        #region 循环设置

        /// <summary>
        /// 设置FollowEx运动模式循环次数
        /// </summary>
        public void SetLoop(int loop)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetFollowLoopEx(_core, _axis, loop), "GTN_SetFollowLoopEx");
            }
        }

        /// <summary>
        /// 读取FollowEx运动模式已执行循环次数
        /// </summary>
        public int GetLoop()
        {
            lock (_lock)
            {
                int loop;
                GtnErrorHelper.ThrowIfError(GTN_GetFollowLoopEx(_core, _axis, out loop), "GTN_GetFollowLoopEx");
                return loop;
            }
        }

        #endregion

        #region FIFO管理

        /// <summary>
        /// 查询FollowEx模式指定FIFO的剩余空间
        /// </summary>
        public short GetSpace(short fifo = 0)
        {
            lock (_lock)
            {
                short space;
                GtnErrorHelper.ThrowIfError(GTN_FollowSpaceEx(_core, _axis, out space, fifo), "GTN_FollowSpaceEx");
                return space;
            }
        }

        /// <summary>
        /// 清除FollowEx模式指定FIFO中的数据
        /// </summary>
        public void Clear(short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_FollowClearEx(_core, _axis, fifo), "GTN_FollowClearEx");
            }
        }

        #endregion

        #region FIFO缓冲区大小

        /// <summary>
        /// 设置FollowEx运动模式的缓存区大小
        /// </summary>
        /// <param name="memory">0=每个缓存区16段, 1=每个缓存区512段</param>
        public void SetMemory(short memory)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetFollowMemoryEx(_core, _axis, memory), "GTN_SetFollowMemoryEx");
            }
        }

        /// <summary>
        /// 读取FollowEx运动模式的缓存区大小
        /// </summary>
        public short GetMemory()
        {
            lock (_lock)
            {
                short memory;
                GtnErrorHelper.ThrowIfError(GTN_GetFollowMemoryEx(_core, _axis, out memory), "GTN_GetFollowMemoryEx");
                return memory;
            }
        }

        #endregion

        #region S曲线数据段

        /// <summary>
        /// 向FollowEx模式FIFO增加数据，支持S曲线（对称）
        /// </summary>
        /// <param name="masterSegment">主轴位移(pulse)</param>
        /// <param name="slaveSegment">从轴位移(pulse)</param>
        /// <param name="type">数据段类型</param>
        /// <param name="percent">S曲线所占加速时间的百分比[0, 100]</param>
        /// <param name="fifo">FIFO编号</param>
        public void PushDataPercent(double masterSegment, double slaveSegment,
            short type = FOLLOW_SEGMENT_NORMAL, short percent = 0, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_FollowDataPercentEx(_core, _axis, masterSegment, slaveSegment, type, percent, fifo),
                    "GTN_FollowDataPercentEx");
            }
        }

        /// <summary>
        /// 向FollowEx模式FIFO增加数据，支持非对称S曲线
        /// </summary>
        /// <param name="masterSegment">主轴位移(pulse)</param>
        /// <param name="slaveSegment">从轴位移(pulse)</param>
        /// <param name="velBeginRatio">起点从轴和主轴的速度比</param>
        /// <param name="velEndRatio">终点从轴和主轴的速度比</param>
        /// <param name="percent">S曲线所占加速时间的百分比（起点和终点百分比之和）[0, 100]</param>
        /// <param name="percent1">返回起点S曲线的百分比</param>
        /// <param name="fifo">FIFO编号</param>
        public void PushDataPercent2(double masterSegment, double slaveSegment,
            double velBeginRatio, double velEndRatio, out short percent1, short percent = 100,
            short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_FollowDataPercent2Ex(_core, _axis, masterSegment, slaveSegment,
                        velBeginRatio, velEndRatio, percent, out percent1, fifo),
                    "GTN_FollowDataPercent2Ex");
            }
        }

        /// <summary>
        /// 计算非对称S曲线的从轴段长
        /// </summary>
        /// <param name="masterSegment">主轴位移(pulse)</param>
        /// <param name="velBeginRatio">起点速度比</param>
        /// <param name="velEndRatio">终点速度比</param>
        /// <param name="percent">S曲线百分比之和</param>
        /// <param name="percent1">起点S曲线百分比</param>
        /// <returns>从轴位移(pulse)</returns>
        public double CalculateSlavePos(double masterSegment, double velBeginRatio,
            double velEndRatio, short percent, short percent1)
        {
            lock (_lock)
            {
                double slavePos;
                GtnErrorHelper.ThrowIfError(
                    GTN_GetFollowDataPercent2Ex(_core, masterSegment, velBeginRatio,
                        velEndRatio, percent, percent1, out slavePos),
                    "GTN_GetFollowDataPercent2Ex");
                return slavePos;
            }
        }

        #endregion

        #region 切换

        /// <summary>
        /// 切换FollowEx运动所使用的FIFO
        /// </summary>
        public void Switch()
        {
            lock (_lock)
            {
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_FollowSwitchEx(_core, mask), "GTN_FollowSwitchEx");
            }
        }

        /// <summary>
        /// FollowEx模式下立即切换运动段或运动FIFO
        /// </summary>
        /// <param name="method">切换方式: FOLLOW_SWITCH_SEGMENT(1)=切换到下一段, FOLLOW_SWITCH_TABLE(2)=切换到另一个FIFO</param>
        /// <param name="buffer">执行方式: 0=立即执行, 1=放入缓冲区</param>
        /// <param name="fifo">FIFO编号</param>
        public void SwitchNow(short method, short buffer = 0, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_FollowSwitchNowEx(_core, _axis, method, buffer, fifo),
                    "GTN_FollowSwitchNowEx");
            }
        }

        #endregion

        #region 缓冲区辅助指令

        /// <summary>
        /// 向FollowEx缓冲区增加DO输出数据
        /// 可在Follow数据段之前或之后调用。
        /// 在数据段之前调用=进入该段立即执行DO输出；
        /// 在数据段之后调用=该段执行完毕时执行DO输出。
        /// </summary>
        /// <param name="doType">输出类型: MC_ENABLE(10)=使能, MC_CLEAR(11)=报警清除, MC_GPO(12)=通用输出</param>
        /// <param name="index">输出IO索引</param>
        /// <param name="value">输出值: 1=高电平, 0=低电平</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufferDoBit(short doType, short index, short value, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_FollowDoBitEx(_core, _axis, doType, index, value, fifo),
                    "GTN_FollowDoBitEx");
            }
        }

        /// <summary>
        /// 向FollowEx缓冲区增加延时数据
        /// 该指令会阻塞Follow缓冲区的执行（但不影响Follow运动本身）
        /// </summary>
        /// <param name="delayTime">延时时间(ms)</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufferDelay(uint delayTime, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_FollowDelayEx(_core, _axis, delayTime, fifo),
                    "GTN_FollowDelayEx");
            }
        }

        /// <summary>
        /// 向FollowEx缓冲区增加等待DI输入数据
        /// 该指令会阻塞Follow缓冲区的执行（但不影响Follow运动本身）
        /// </summary>
        /// <param name="diType">DI类型: MC_LIMIT_POSITIVE(0)=正限位, MC_LIMIT_NEGATIVE(1)=负限位, MC_ALARM(2)=驱动报警, MC_HOME(3)=原点开关, MC_GPI(4)=通用输入</param>
        /// <param name="index">输入IO索引</param>
        /// <param name="value">期望电平: 1=高电平, 0=低电平</param>
        /// <param name="time">DI等待超时时间(ms)，0表示一直等待</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufferDiBit(short diType, short index, short value, uint time = 0, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_FollowDiBitEx(_core, _axis, diType, index, value, time, fifo),
                    "GTN_FollowDiBitEx");
            }
        }

        #endregion

        #region 运动控制

        /// <summary>
        /// 启动FollowEx运动
        /// </summary>
        /// <param name="option">按位指示所使用的FIFO</param>
        public void Start(int option = 0)
        {
            lock (_lock)
            {
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_FollowStartEx(_core, mask, option), "GTN_FollowStartEx");
            }
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 一键配置并启动FollowEx运动（支持S曲线）
        /// </summary>
        /// <param name="masterAxis">主轴轴号</param>
        /// <param name="segments">数据段数组: [masterPos, slavePos, type, percent]每组4个值</param>
        /// <param name="loop">循环次数</param>
        public void QuickStart(short masterAxis, double[,] segments, int loop = 0)
        {
            lock (_lock)
            {
                SetMode();
                Clear();
                SetMaster(masterAxis);
                SetEventStart();

                int segCount = segments.GetLength(0);
                for (int i = 0; i < segCount; i++)
                {
                    double masterPos = segments[i, 0];
                    double slavePos = segments[i, 1];
                    short type = (short)segments[i, 2];
                    short percent = (short)segments[i, 3];
                    PushDataPercent(masterPos, slavePos, type, percent);
                }

                SetLoop(loop);
                Start();
            }
        }

        #endregion
    }
}