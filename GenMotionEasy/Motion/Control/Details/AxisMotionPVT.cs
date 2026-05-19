using GenMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// PVT运动模式（Position-Velocity-Time）- 第4章
    /// PVT模式使用"位置、速度、时间"数据点描述运动规律。
    /// 支持PVT、Complete、Percent、Continuous四种描述方式。
    /// 控制器提供32个数据表，每个数据表1024个存储空间。
    /// </summary>
    public class AxisMotionPVT : IPVTMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;

        public AxisMotionPVT(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        #region 模式设置

        /// <summary>
        /// 设置指定轴为PVT运动模式
        /// </summary>
        public void SetMode()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PrfPvt(_core, _axis), "GTN_PrfPvt");
            }
        }

        #endregion

        #region 循环设置

        /// <summary>
        /// 设置PVT运动模式循环次数
        /// </summary>
        /// <param name="loop">循环次数，0表示无限循环</param>
        public void SetLoop(int loop)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetPvtLoop(_core, _axis, loop), "GTN_SetPvtLoop");
            }
        }

        /// <summary>
        /// 查询PVT运动模式循环次数
        /// </summary>
        /// <param name="loopCount">已循环次数</param>
        /// <param name="loop">总循环次数</param>
        public void GetLoop(out int loopCount, out int loop)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_GetPvtLoop(_core, _axis, out loopCount, out loop), "GTN_GetPvtLoop");
            }
        }

        #endregion

        #region 数据表选择

        /// <summary>
        /// 选择PVT运动模式数据表
        /// 可以在运动状态下切换数据表，当前表执行完毕后切换到新表
        /// </summary>
        /// <param name="tableId">数据表ID，取值范围[1, 32]</param>
        public void SelectTable(short tableId)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PvtTableSelect(_core, _axis, tableId), "GTN_PvtTableSelect");
            }
        }

        #endregion

        #region PVT描述方式

        /// <summary>
        /// 向数据表传送数据，采用PVT描述方式
        /// 直接定义各数据点的"位置、速度、时间"
        /// </summary>
        /// <param name="tableId">数据表ID</param>
        /// <param name="count">数据点个数（≤1024）</param>
        /// <param name="time">时间数组(ms)</param>
        /// <param name="pos">位置数组(pulse)</param>
        /// <param name="vel">速度数组(pulse/ms)</param>
        public void SendPvtTable(short tableId, int count, double[] time, double[] pos, double[] vel)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_PvtTable(_core, tableId, count, ref time[0], ref pos[0], ref vel[0]),
                    "GTN_PvtTable");
            }
        }

        #endregion

        #region Complete描述方式

        /// <summary>
        /// 向数据表传送数据，采用Complete描述方式
        /// 定义各数据点的"位置、时间"及起终点速度，控制器自动计算中间点速度
        /// 适合描述光滑的速度曲线（如三角函数）
        /// </summary>
        /// <param name="tableId">数据表ID</param>
        /// <param name="count">数据点个数（≤1024）</param>
        /// <param name="time">时间数组(ms)</param>
        /// <param name="pos">位置数组(pulse)</param>
        /// <param name="a">工作数组A（用户不必赋值）</param>
        /// <param name="b">工作数组B（用户不必赋值）</param>
        /// <param name="c">工作数组C（用户不必赋值）</param>
        /// <param name="velBegin">起点速度(pulse/ms)</param>
        /// <param name="velEnd">终点速度(pulse/ms)</param>
        public void SendCompleteTable(short tableId, int count, double[] time, double[] pos,
            double[] a, double[] b, double[] c, double velBegin, double velEnd)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_PvtTableComplete(_core, tableId, count, ref time[0], ref pos[0],
                        ref a[0], ref b[0], ref c[0], velBegin, velEnd),
                    "GTN_PvtTableComplete");
            }
        }

        #endregion

        #region Percent描述方式

        /// <summary>
        /// 向数据表传送数据，采用Percent描述方式
        /// 定义各数据点的"位置、时间、百分比"及起点速度
        /// 可以精确控制加减速曲线的光滑程度
        /// </summary>
        /// <param name="tableId">数据表ID</param>
        /// <param name="count">数据点个数</param>
        /// <param name="time">时间数组(ms)</param>
        /// <param name="pos">位置数组(pulse)</param>
        /// <param name="percent">百分比数组[0, 100]</param>
        /// <param name="velBegin">起点速度(pulse/ms)</param>
        public void SendPercentTable(short tableId, int count, double[] time, double[] pos,
            double[] percent, double velBegin)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_PvtTablePercent(_core, tableId, count, ref time[0], ref pos[0],
                        ref percent[0], velBegin),
                    "GTN_PvtTablePercent");
            }
        }

        /// <summary>
        /// 计算PVT运动模式Percent描述方式下各数据点的速度
        /// 该指令仅计算速度，不会将数据点下载到运动控制器
        /// </summary>
        /// <param name="count">数据点个数</param>
        /// <param name="time">时间数组(ms)</param>
        /// <param name="pos">位置数组(pulse)</param>
        /// <param name="percent">百分比数组</param>
        /// <param name="velBegin">起点速度(pulse/ms)</param>
        /// <param name="vel">返回速度数组(pulse/ms)</param>
        public void CalculatePercentVel(int count, double[] time, double[] pos,
            double[] percent, double velBegin, double[] vel)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_PvtPercentCalculate(_core, count, ref time[0], ref pos[0],
                        ref percent[0], velBegin, ref vel[0]),
                    "GTN_PvtPercentCalculate");
            }
        }

        #endregion

        #region Continuous描述方式

        /// <summary>
        /// 向数据表传送数据，采用Continuous描述方式
        /// 定义各数据点的"位置、速度、最大速度、加速度、减速度、百分比"
        /// 控制器自动拆分加速段、匀速段、减速段
        /// </summary>
        /// <param name="tableId">数据表ID</param>
        /// <param name="count">数据点个数</param>
        /// <param name="pos">位置数组(pulse)</param>
        /// <param name="vel">速度数组(pulse/ms)</param>
        /// <param name="percent">百分比数组[0, 100]</param>
        /// <param name="velMax">最大速度数组(pulse/ms)</param>
        /// <param name="acc">加速度数组(pulse/ms²)</param>
        /// <param name="dec">减速度数组(pulse/ms²)</param>
        /// <param name="timeBegin">起点时间(ms)</param>
        public void SendContinuousTable(short tableId, int count, double[] pos, double[] vel,
            double[] percent, double[] velMax, double[] acc, double[] dec, double timeBegin)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_PvtTableContinuous(_core, tableId, count, ref pos[0], ref vel[0],
                        ref percent[0], ref velMax[0], ref acc[0], ref dec[0], timeBegin),
                    "GTN_PvtTableContinuous");
            }
        }

        /// <summary>
        /// 计算PVT运动模式Continuous描述方式下各数据点的时间
        /// 该指令仅计算时间，不会将数据点下载到运动控制器
        /// </summary>
        /// <param name="count">数据点个数</param>
        /// <param name="pos">位置数组(pulse)</param>
        /// <param name="vel">速度数组(pulse/ms)</param>
        /// <param name="percent">百分比数组</param>
        /// <param name="velMax">最大速度数组(pulse/ms)</param>
        /// <param name="acc">加速度数组(pulse/ms²)</param>
        /// <param name="dec">减速度数组(pulse/ms²)</param>
        /// <param name="time">返回时间数组(ms)</param>
        public void CalculateContinuousTime(int count, double[] pos, double[] vel,
            double[] percent, double[] velMax, double[] acc, double[] dec, double[] time)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_PvtContinuousCalculate(_core, count, ref pos[0], ref vel[0],
                        ref percent[0], ref velMax[0], ref acc[0], ref dec[0], ref time[0]),
                    "GTN_PvtContinuousCalculate");
            }
        }

        #endregion

        #region 运动控制

        /// <summary>
        /// 启动PVT运动
        /// 启动前需调用SelectTable选择数据表（默认使用数据表1）
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_PvtStart(_core, mask), "GTN_PvtStart");
            }
        }

        /// <summary>
        /// 读取PVT运动状态
        /// </summary>
        /// <param name="tableId">当前正在使用的数据表ID</param>
        /// <param name="time">当前轴已运动的时间(ms)</param>
        public void GetStatus(out short tableId, out double time)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_PvtStatus(_core, _axis, out tableId, out time, 1), "GTN_PvtStatus");
            }
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 一键配置并启动PVT运动（PVT描述方式）
        /// </summary>
        /// <param name="tableId">数据表ID</param>
        /// <param name="count">数据点个数</param>
        /// <param name="time">时间数组</param>
        /// <param name="pos">位置数组</param>
        /// <param name="vel">速度数组</param>
        /// <param name="loop">循环次数（0=无限循环）</param>
        public void QuickStart(short tableId, int count, double[] time, double[] pos,
            double[] vel, int loop = 0)
        {
            lock (_lock)
            {
                SetMode();
                SendPvtTable(tableId, count, time, pos, vel);
                SelectTable(tableId);
                if (loop != 1)
                    SetLoop(loop);
                Start();
            }
        }

        /// <summary>
        /// 配置并启动PVT运动（Complete描述方式）
        /// </summary>
        public void QuickStartComplete(short tableId, int count, double[] time, double[] pos,
            double[] a, double[] b, double[] c, double velBegin, double velEnd, int loop = 0)
        {
            lock (_lock)
            {
                SetMode();
                SendCompleteTable(tableId, count, time, pos, a, b, c, velBegin, velEnd);
                SelectTable(tableId);
                if (loop != 1)
                    SetLoop(loop);
                Start();
            }
        }

        /// <summary>
        /// 配置并启动PVT运动（Percent描述方式）
        /// </summary>
        public void QuickStartPercent(short tableId, int count, double[] time, double[] pos,
            double[] percent, double velBegin, int loop = 0)
        {
            lock (_lock)
            {
                SetMode();
                SendPercentTable(tableId, count, time, pos, percent, velBegin);
                SelectTable(tableId);
                if (loop != 1)
                    SetLoop(loop);
                Start();
            }
        }

        /// <summary>
        /// 配置并启动PVT运动（Continuous描述方式）
        /// </summary>
        public void QuickStartContinuous(short tableId, int count, double[] pos, double[] vel,
            double[] percent, double[] velMax, double[] acc, double[] dec,
            double timeBegin, int loop = 0)
        {
            lock (_lock)
            {
                SetMode();
                SendContinuousTable(tableId, count, pos, vel, percent, velMax, acc, dec, timeBegin);
                SelectTable(tableId);
                if (loop != 1)
                    SetLoop(loop);
                Start();
            }
        }

        #endregion
    }
}