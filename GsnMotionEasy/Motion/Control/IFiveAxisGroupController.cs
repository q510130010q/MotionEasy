using System.Collections.Generic;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Interfaces;

namespace GsnMotionEasy.Motion.Control
{
    /// <summary>
    /// 五轴 Group 门面接口。聚合 7 个子接口，提供 3 个高层组合方法。
    /// </summary>
    public interface IFiveAxisGroupController
    {
        // ── 子模块访问（供需要细粒度控制的用户使用）──
        IGroupSetup Setup { get; }
        IGroupKinematics Kinematics { get; }
        IGroupCoordTransform CoordTransform { get; }
        IGroupMotion Motion { get; }
        IGroupLookAhead LookAhead { get; }
        IGroupCommandList CommandList { get; }
        IGroupStatus Status { get; }
        IGroupCalibration Calibration { get; }

        /// <summary>一键五轴初始化（对应 Demo button3_Click 的全部 Group 建立流程）</summary>
        void InitFiveAxis(FiveAxisConfig config);

        /// <summary>配置平滑约束 + 前瞻（对应 Demo button4_Click）</summary>
        void ConfigMotionConstraints(MotionConstraintConfig config);

        /// <summary>执行轨迹段序列（对应 Demo button6_Click + button8_Click + button7_Click）</summary>
        void ExecuteTrajectory(IEnumerable<TrajectorySegment> segments);
    }
}
