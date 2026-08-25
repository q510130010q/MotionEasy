namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>PCS/TCS/ACS 坐标变换。对应 Demo button3_Click 中 GTN_SetGroupCoordinateTransformPrm / GTN_SetGroupCartesianTransform / GTN_SetGroupAcsKinematicOffset / GTN_SetGroupCommandPosDefine / GTN_SetGroupCommandVelDefine / GTN_SetGroupProfileCoordinateSystem。</summary>
    public interface IGroupCoordTransform
    {
        /// <summary>设置 PCS（工件坐标系）偏移</summary>
        void SetPcsOffset(double x, double y, double z, double p = 0, double s = 0);

        /// <summary>设置 TCS（刀具坐标系）偏移</summary>
        void SetTcsOffset(double x, double y, double z,
                          double rot1 = 0, double rot2 = 0, double rot3 = 0);

        /// <summary>设置 ACS（轴坐标系）运动学偏移</summary>
        void SetAcsOffset(double[] offsets);

        /// <summary>设置位置描述坐标系和姿态模式</summary>
        void SetCommandPosDefine(short coordSystem, short orientationMode, short configIndex = 0);

        /// <summary>设置速度描述模式</summary>
        void SetCommandVelDefine(short type = 1, short mode = 0);

        /// <summary>设置规划坐标系</summary>
        void SetProfileCoordinateSystem(short coordSystem);

        /// <summary>设置 Group 姿态轴约束（对应 GTN_SetGroupOrientationConstraint）</summary>
        void SetGroupOrientationConstraint(double oriVelMax, double oriAccMax, double oriDecMax, double oriJerkMax);

        /// <summary>设置 Group 速度规划模式（对应 GTN_SetGroupVelProfileMode）</summary>
        void SetGroupVelProfileMode(short mode, double accTime, double k);
    }
}
