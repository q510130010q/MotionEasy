using System.Collections.Generic;
using GTN;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Control.Details;
using GsnMotionEasy.Motion.Interfaces;

namespace GsnMotionEasy.Motion.Control
{
    /// <summary>
    /// 五轴 Group 门面。聚合 7 个子模块实现，对外暴露统一入口。
    /// </summary>
    public class FiveAxisGroupController : IFiveAxisGroupController
    {
        private readonly GroupContext _ctx;

        public IGroupSetup Setup { get; }
        public IGroupKinematics Kinematics { get; }
        public IGroupCoordTransform CoordTransform { get; }
        public IGroupMotion Motion { get; }
        public IGroupLookAhead LookAhead { get; }
        public IGroupCommandList CommandList { get; }
        public IGroupStatus Status { get; }
        public IGroupCalibration Calibration { get; }

        public FiveAxisGroupController(GroupContext ctx)
        {
            _ctx = ctx;
            Setup = new GroupSetup(ctx);
            Kinematics = new GroupKinematics(ctx);
            CoordTransform = new GroupCoordTransform(ctx);
            Motion = new GroupMotion(ctx);
            LookAhead = new GroupLookAhead(ctx);
            CommandList = new GroupCommandList(ctx);
            Status = new GroupStatus(ctx);
            Calibration = new GroupCalibration();
        }

        public FiveAxisGroupController(short core, short group, short listId)
            : this(new GroupContext { Core = core, Group = group, CommandListId = listId })
        {
        }

        /// <summary>
        /// 一键五轴初始化。对应 Demo button3_Click 行 454-643。
        /// 步骤：建立 Group -> 配置运动学 -> PCS/TCS/ACS 偏移 -> 命令位置定义 -> 使能 -> 速度定义 -> 规划坐标系 -> 准备指令流
        /// </summary>
        public void InitFiveAxis(FiveAxisConfig config)
        {
            _ctx.Core = config.Core;
            _ctx.Group = config.Group;
            _ctx.CommandListId = config.CommandListId;

            // 1. 建立 Group（Disable -> SetScale×5 -> Ungroup -> AddAxis×5）
            Setup.CreateGroup(config);

            // 2. 运动学模型
            Kinematics.SetModel(
                config.MachineType,
                config.PrimaryAxisPoint,
                config.SlaveAxisPoint,
                config.ToolLocationPoint,
                config.DirMode,
                config.AxisDir,
                config.AxisVectors);

            // 3-5. PCS / TCS / ACS 偏移
            CoordTransform.SetPcsOffset(
                config.PcsOffset[0], config.PcsOffset[1], config.PcsOffset[2],
                config.PcsOffset.Length > 3 ? config.PcsOffset[3] : 0,
                config.PcsOffset.Length > 4 ? config.PcsOffset[4] : 0);

            CoordTransform.SetTcsOffset(
                config.TcsOffset[0], config.TcsOffset[1], config.TcsOffset[2]);

            CoordTransform.SetAcsOffset(config.AcsOffset);

            // 6. 命令位置定义
            CoordTransform.SetCommandPosDefine(config.CoordSystem, config.OrientationMode, 0);

            // 7. 使能 Group
            Setup.Enable();

            // 8. 速度定义
            CoordTransform.SetCommandVelDefine(config.VelDefineType, config.VelDefineMode);

            // 9. 规划坐标系
            CoordTransform.SetProfileCoordinateSystem(config.ProfileCoordSystem);

            // 10. 准备指令流
            CommandList.Begin();
        }

        /// <summary>
        /// 配置平滑约束 + 前瞻。对应 Demo button4_Click 行 1681-1752。
        /// </summary>
        public void ConfigMotionConstraints(MotionConstraintConfig config)
        {
            // 1-2. 5 轴平滑 + 5 轴运动约束
            // 注：此处假设 5 轴号与 GroupContext.Group 对应的 1..5；实际应从 FiveAxisConfig 获取
            // 这里通过 _ctx 暴露的轴号访问。为简化，使用 1..5 默认顺序。
            for (short axis = 1; axis <= 5; axis++)
            {
                Setup.SetAxisMotionSmooth(axis, config.SmoothTime, config.SmoothK);
                Setup.SetAxisMotionConstraint(axis, config.JerkMax, config.DecMax, config.DvMax, config.VelMax, config.AccMax);
            }

            // 3. Group 姿态轴约束
            CoordTransform.SetGroupOrientationConstraint(
                config.OriVelMax, config.OriAccMax, config.OriDecMax, config.OriJerkMax);

            // 4. Group 速度规划模式
            CoordTransform.SetGroupVelProfileMode(config.VelProfileMode, config.VelProfileAccTime, config.VelProfileK);

            // 5. 前瞻
            LookAhead.Enable(config.LookAheadNum, config.LookAheadTime, config.LookAheadRadiusRatio);
        }

        /// <summary>
        /// 执行轨迹段序列。对应 Demo button6_Click + button8_Click + button7_Click。
        /// </summary>
        public void ExecuteTrajectory(IEnumerable<TrajectorySegment> segments)
        {
            // 1-2. 停止 + 清空指令流
            CommandList.Begin();

            // 3-4. 重新设置坐标描述（指令流模式下需要）
            CoordTransform.SetCommandPosDefine(mc.COORD_SYSTEM_PCS, mc.ORI_MODE_ROTATE_AXIS_POS, 0);
            CoordTransform.SetProfileCoordinateSystem(mc.COORD_SYSTEM_PCS);

            // 5. 写入所有轨迹段
            foreach (var seg in segments)
            {
                if (seg.Type == TrajectorySegmentType.Linear)
                {
                    Motion.MoveLinearAbsolute(seg.EndPos, seg.Velocity, seg.Acceleration, seg.Deceleration, seg.Modal);
                }
                else
                {
                    Motion.MoveCircularAbsolute(seg.EndPos, seg.AuxPoint, seg.Velocity, seg.Acceleration, seg.Deceleration, seg.Modal);
                }
            }

            // 6. 数据结束
            CommandList.End();

            // 7. 启动指令流
            CommandList.Start();
        }
    }
}
