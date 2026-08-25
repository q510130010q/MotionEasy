namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>
    /// 五轴运动。
    /// 对应 Demo button6_Click 中的 GTN_MoveLinearAbsolute / GTN_MoveCircularAbsolute。
    ///
    /// RTCP 实现按《新架构功能编程手册》10.4 章节：Group 模式 + 运动学变换 + 规划坐标系(PCS) +
    /// 笛卡尔坐标轴限位(GTN_SetGroupCartesianCoordinateAxisLimit) 配置完成后自动生效，
    /// 没有显式的 On/Off 开关函数。
    /// </summary>
    public interface IGroupMotion
    {
        /// <summary>五轴直线插补运动（绝对坐标）</summary>
        /// <param name="pos">目标位置 [X,Y,Z,P,S,0,0,0]，长度 8</param>
        /// <param name="vel">合成速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <param name="modal">阻塞模式，1=阻塞 0=非阻塞</param>
        void MoveLinearAbsolute(double[] pos, double vel, double acc, double dec, short modal = 1);

        /// <summary>五轴空间圆弧插补运动（绝对坐标）</summary>
        /// <param name="endPos">终点位置 [X,Y,Z,P,S,0,0,0]</param>
        /// <param name="auxPoint">辅助点 [x,y,z,p,s]，空间圆弧用</param>
        /// <param name="vel">合成速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <param name="modal">阻塞模式</param>
        void MoveCircularAbsolute(double[] endPos, double[] auxPoint, double vel, double acc, double dec, short modal = 1);

        /// <summary>
        /// 启用 RTCP 模式。按手册 10.4：设置规划坐标系为 PCS + 启用 Group 笛卡尔坐标轴限位。
        /// 调用前需已完成运动学变换配置（IGroupKinematics.SetModel）和 PCS 偏移（IGroupCoordTransform.SetPcsOffset）。
        /// </summary>
        void EnableRtcpMode();

        /// <summary>关闭 RTCP 模式（关闭 Group 笛卡尔坐标轴限位）</summary>
        void DisableRtcpMode();
    }
}
