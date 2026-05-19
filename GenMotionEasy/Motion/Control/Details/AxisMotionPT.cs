using GenMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// PT运动模式（Position-Time）- 第2章
    /// PT模式使用一系列"位置、时间"数据点描述速度规划，
    /// 支持静态FIFO和动态FIFO两种模式。
    /// </summary>
    public class AxisMotionPT : IPTMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;

        public AxisMotionPT(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        #region 模式设置

        /// <summary>
        /// 设置指定轴为PT运动模式
        /// </summary>
        /// <param name="mode">FIFO使用模式: PT_MODE_STATIC(0)=静态模式, PT_MODE_DYNAMIC(1)=动态模式</param>
        public void SetMode(short mode = PT_MODE_STATIC)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PrfPt(_core, _axis, mode), "GTN_PrfPt");
            }
        }

        /// <summary>
        /// 设置为PT静态FIFO模式（默认32段）
        /// </summary>
        public void SetStaticMode() => SetMode(PT_MODE_STATIC);

        /// <summary>
        /// 设置为PT动态FIFO模式
        /// 动态模式下两个FIFO交替使用，一个用完自动清空并切换到另一个
        /// </summary>
        public void SetDynamicMode() => SetMode(PT_MODE_DYNAMIC);

        #endregion

        #region FIFO管理

        /// <summary>
        /// 查询PT模式指定FIFO的剩余空间
        /// </summary>
        /// <param name="fifo">FIFO编号(0或1)，动态模式下该参数无效</param>
        /// <returns>剩余空间（段数）</returns>
        public short GetSpace(short fifo = 0)
        {
            lock (_lock)
            {
                short space;
                GtnErrorHelper.ThrowIfError(GTN_PtSpace(_core, _axis, out space, fifo), "GTN_PtSpace");
                return space;
            }
        }

        /// <summary>
        /// 向PT模式指定FIFO增加数据段
        /// </summary>
        /// <param name="pos">段末位置（相对于第一段起点的绝对值），单位pulse</param>
        /// <param name="time">段末时间，单位ms</param>
        /// <param name="type">数据段类型: PT_SEGMENT_NORMAL(0)=普通段, PT_SEGMENT_EVEN(1)=匀速段, PT_SEGMENT_STOP(2)=停止段</param>
        /// <param name="fifo">FIFO编号，动态模式下该参数无效</param>
        public void PushData(double pos, int time, short type = PT_SEGMENT_NORMAL, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PtData(_core, _axis, pos, time, type, fifo), "GTN_PtData");
            }
        }

        /// <summary>
        /// 向PT模式FIFO增加普通段数据
        /// </summary>
        public void PushNormalSegment(double pos, int time, short fifo = 0)
            => PushData(pos, time, PT_SEGMENT_NORMAL, fifo);

        /// <summary>
        /// 向PT模式FIFO增加匀速段数据
        /// </summary>
        public void PushEvenSegment(double pos, int time, short fifo = 0)
            => PushData(pos, time, PT_SEGMENT_EVEN, fifo);

        /// <summary>
        /// 向PT模式FIFO增加停止段数据
        /// </summary>
        public void PushStopSegment(double pos, int time, short fifo = 0)
            => PushData(pos, time, PT_SEGMENT_STOP, fifo);

        /// <summary>
        /// 清除PT模式指定FIFO中的数据
        /// 运动状态下该指令无效，动态模式下该指令无效
        /// </summary>
        /// <param name="fifo">FIFO编号</param>
        public void Clear(short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PtClear(_core, _axis, fifo), "GTN_PtClear");
            }
        }

        #endregion

        #region FIFO缓冲区大小

        /// <summary>
        /// 设置PT运动模式的缓存区大小
        /// </summary>
        /// <param name="memory">0=每个FIFO有32段空间, 1=每个FIFO有1024段空间</param>
        public void SetMemory(short memory)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetPtMemory(_core, _axis, memory), "GTN_SetPtMemory");
            }
        }

        /// <summary>
        /// 设置为32段FIFO
        /// </summary>
        public void SetMemorySmall() => SetMemory(0);

        /// <summary>
        /// 设置为1024段FIFO
        /// </summary>
        public void SetMemoryLarge() => SetMemory(1);

        /// <summary>
        /// 读取PT运动模式的缓存区大小
        /// </summary>
        /// <returns>0=32段, 1=1024段</returns>
        public short GetMemory()
        {
            lock (_lock)
            {
                short memory;
                GtnErrorHelper.ThrowIfError(GTN_GetPtMemory(_core, _axis, out memory), "GTN_GetPtMemory");
                return memory;
            }
        }

        #endregion

        #region 循环设置

        /// <summary>
        /// 设置PT运动模式循环执行的次数
        /// 动态模式下该指令无效
        /// </summary>
        /// <param name="loop">循环次数，0表示无限循环</param>
        public void SetLoop(int loop)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetPtLoop(_core, _axis, loop), "GTN_SetPtLoop");
            }
        }

        /// <summary>
        /// 查询PT运动模式循环已执行完成的次数
        /// 动态模式下该指令无效
        /// </summary>
        /// <returns>循环已执行完成的次数</returns>
        public int GetLoop()
        {
            lock (_lock)
            {
                int loop;
                GtnErrorHelper.ThrowIfError(GTN_GetPtLoop(_core, _axis, out loop), "GTN_GetPtLoop");
                return loop;
            }
        }

        #endregion

        #region 运动控制

        /// <summary>
        /// 启动PT运动
        /// </summary>
        /// <param name="option">
        /// 按位指示所使用的FIFO:
        /// bit位为0=使用FIFO0, bit位为1=使用FIFO1
        /// 动态模式下该参数无效
        /// </param>
        public void Start(int option = 0)
        {
            lock (_lock)
            {
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_PtStart(_core, mask, option), "GTN_PtStart");
            }
        }

        /// <summary>
        /// 获取PT运动状态信息
        /// </summary>
        /// <returns>PT运动状态信息结构体</returns>
        public TPtInfo GetInfo()
        {
            lock (_lock)
            {
                TPtInfo info;
                GtnErrorHelper.ThrowIfError(GTN_GetPtInfo(_core, _axis, out info), "GTN_GetPtInfo");
                return info;
            }
        }

        #endregion

        #region PT缓冲区辅助指令

        /// <summary>
        /// PT缓冲区DO输出
        /// </summary>
        /// <param name="doType">输出类型: MC_GPO=通用输出</param>
        /// <param name="index">输出IO索引</param>
        /// <param name="value">输出值: 1=高电平, 0=低电平</param>
        /// <param name="fifo">FIFO编号</param>
        public void DoBit(short doType, short index, short value, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PtDoBit(_core, _axis, doType, index, value, fifo), "GTN_PtDoBit");
            }
        }

        /// <summary>
        /// PT缓冲区AO输出
        /// </summary>
        /// <param name="aoType">输出类型</param>
        /// <param name="index">输出索引</param>
        /// <param name="value">输出值</param>
        /// <param name="fifo">FIFO编号</param>
        public void Ao(short aoType, short index, double value, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PtAo(_core, _axis, aoType, index, value, fifo), "GTN_PtAo");
            }
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 梯形曲线速度规划（三段：加速、匀速、减速）
        /// </summary>
        /// <param name="accelPos">加速段位移</param>
        /// <param name="evenPos">匀速段位移</param>
        /// <param name="decelPos">减速段位移</param>
        /// <param name="timePerSegment">每段的时间(ms)</param>
        public void SetupTrapezoidProfile(double accelPos, double evenPos, double decelPos, int timePerSegment)
        {
            int time = timePerSegment;
            PushData(accelPos, time, PT_SEGMENT_NORMAL);
            time += timePerSegment;
            PushData(accelPos + evenPos, time, PT_SEGMENT_NORMAL);
            time += timePerSegment;
            PushData(accelPos + evenPos + decelPos, time, PT_SEGMENT_NORMAL);
        }

        #endregion
    }
}