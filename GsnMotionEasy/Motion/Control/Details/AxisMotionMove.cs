using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// Move运动模式 - 第5章
    /// Gsn 卡仅支持基础 MoveAbsolute（GTN_MoveAbsolute / GTN_GetMoveAbsolute）。
    /// 不支持 MoveAbsoluteEx / MoveVelocity（Gsn gts.cs 无 GTN_MoveAbsoluteEx / GTN_MoveVelocity）。
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
    }
}
