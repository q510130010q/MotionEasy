using GenMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// Move运动模式 - 第5章
    /// 包含MoveAbsolute、MoveAbsoluteEx、MoveVelocity三种运动模式。
    /// 可在任何运动模式下调用并立即执行。
    /// </summary>
    public class AxisMotionMove : IMoveMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;

        public AxisMotionMove(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        #region MoveAbsolute

        /// <summary>
        /// 执行MoveAbsolute运动
        /// 从当前位置和当前速度开始，按设定的最大速度和加速度运动到目标位置后停止。
        /// 可以在运动状态下调用，无论当前何种运动模式都立即执行。
        /// 调用后模式字为20。
        /// </summary>
        /// <param name="pos">目标位置(pulse)</param>
        /// <param name="vel">最大速度(pulse/ms)</param>
        /// <param name="acc">加速度(pulse/ms²)</param>
        /// <param name="dec">减速度(pulse/ms²)</param>
        /// <param name="percent">S曲线百分比[0, 100]，0=梯形曲线，100=无匀加速段</param>
        public void MoveAbsolute(int pos, double vel, double acc, double dec, short percent = 0)
        {
            lock (_lock)
            {
                TMoveAbsolutePrm prm = new TMoveAbsolutePrm
                {
                    pos = pos,
                    vel = vel,
                    acc = acc,
                    dec = dec,
                    percent = percent
                };
                GtnErrorHelper.ThrowIfError(GTN_MoveAbsolute(_core, _axis, ref prm), "GTN_MoveAbsolute");
            }
        }

        /// <summary>
        /// 读取MoveAbsolute运动参数
        /// </summary>
        public TMoveAbsolutePrm GetMoveAbsolute()
        {
            lock (_lock)
            {
                TMoveAbsolutePrm prm;
                GtnErrorHelper.ThrowIfError(GTN_GetMoveAbsolute(_core, _axis, out prm), "GTN_GetMoveAbsolute");
                return prm;
            }
        }

        #endregion

        #region MoveAbsoluteEx

        /// <summary>
        /// 执行MoveAbsoluteEx运动（扩展版）
        /// 在MoveAbsolute基础上增加了起点速度、起点加速度、终点速度、终点加速度设置。
        /// 可以在运动状态下调用。
        /// 调用后模式字为20。
        /// </summary>
        /// <param name="pos">目标位置(pulse)</param>
        /// <param name="vel">最大速度(pulse/ms)</param>
        /// <param name="acc">加速度(pulse/ms²)</param>
        /// <param name="dec">减速度(pulse/ms²)</param>
        /// <param name="percent">S曲线百分比[0, 100]</param>
        /// <param name="velStart">起点速度(pulse/ms)，仅静止状态调用时可设置</param>
        /// <param name="velEnd">终点速度(pulse/ms)</param>
        /// <param name="accStartPercent">加速段起始加速度百分比，计算公式: 起点加速度 = accStartPercent × acc</param>
        /// <param name="decEndPercent">减速段终点加速度百分比，计算公式: 终点加速度 = decEndPercent × dec</param>
        public void MoveAbsoluteEx(int pos, double vel, double acc, double dec, short percent = 0,
            double velStart = 0, double velEnd = 0, double accStartPercent = 1, double decEndPercent = 1)
        {
            lock (_lock)
            {
                TMoveAbsolutePrmEx prm = new TMoveAbsolutePrmEx
                {
                    pos = pos,
                    vel = vel,
                    acc = acc,
                    dec = dec,
                    percent = percent,
                    velStart = velStart,
                    velEnd = velEnd,
                    accStartPercent = accStartPercent,
                    decEndPercent = decEndPercent
                };
                GtnErrorHelper.ThrowIfError(GTN_MoveAbsoluteEx(_core, _axis, ref prm), "GTN_MoveAbsoluteEx");
            }
        }

        /// <summary>
        /// 读取MoveAbsoluteEx运动参数
        /// </summary>
        public TMoveAbsolutePrmEx GetMoveAbsoluteEx()
        {
            lock (_lock)
            {
                TMoveAbsolutePrmEx prm;
                GtnErrorHelper.ThrowIfError(GTN_GetMoveAbsoluteEx(_core, _axis, out prm), "GTN_GetMoveAbsoluteEx");
                return prm;
            }
        }

        #endregion

        #region MoveVelocity

        /// <summary>
        /// 执行MoveVelocity运动
        /// 从当前速度按设定的加速度和加加速度运动到目标速度，之后一直保持目标速度。
        /// 可以在运动状态下调用。
        /// 调用后模式字为30。
        /// </summary>
        /// <param name="vel">目标速度(pulse/ms)</param>
        /// <param name="acc">加速度(pulse/ms²)</param>
        /// <param name="dec">减速度(pulse/ms²)</param>
        /// <param name="direction">运动方向: MC_POSITIVE_DIRECTION=正向, MC_NEGATIVE_DIRECTION=负向, MC_CURRENT_DIRECTION=保持当前方向</param>
        /// <param name="jerkBegin">起始加加速度(pulse/ms³)，加速度从0变化到最大加速度</param>
        /// <param name="jerkEnd">到达目标速度时的加加速度(pulse/ms³)，加速度从最大变化到0</param>
        public void MoveVelocity(double vel, double acc, double dec, short direction,
            double jerkBegin = 0, double jerkEnd = 0)
        {
            lock (_lock)
            {
                TMoveVelocityPrm prm = new TMoveVelocityPrm
                {
                    vel = vel,
                    acc = acc,
                    dec = dec,
                    direction = direction,
                    jerkBegin = jerkBegin,
                    jerkEnd = jerkEnd
                };
                GtnErrorHelper.ThrowIfError(GTN_MoveVelocity(_core, _axis, ref prm), "GTN_MoveVelocity");
            }
        }

        /// <summary>
        /// 正向MoveVelocity运动
        /// </summary>
        public void MoveVelocityPositive(double vel, double acc, double dec,
            double jerkBegin = 0, double jerkEnd = 0)
        {
            MoveVelocity(vel, acc, dec, 0, jerkBegin, jerkEnd);
        }

        /// <summary>
        /// 负向MoveVelocity运动
        /// </summary>
        public void MoveVelocityNegative(double vel, double acc, double dec,
            double jerkBegin = 0, double jerkEnd = 0)
        {
            MoveVelocity(vel, acc, dec, 1, jerkBegin, jerkEnd);
        }

        /// <summary>
        /// 读取MoveVelocity运动参数
        /// </summary>
        public TMoveVelocityPrm GetMoveVelocity()
        {
            lock (_lock)
            {
                TMoveVelocityPrm prm;
                GtnErrorHelper.ThrowIfError(GTN_GetMoveVelocity(_core, _axis, out prm), "GTN_GetMoveVelocity");
                return prm;
            }
        }

        #endregion
    }
}