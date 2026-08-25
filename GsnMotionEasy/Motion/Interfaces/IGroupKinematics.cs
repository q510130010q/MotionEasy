using GsnMotionEasy.Model;

namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>运动学模型配置。对应 Demo button3_Click 中 TKinematicTransform 的填充与 GTN_SetGroupKinematicTransform 调用。</summary>
    public interface IGroupKinematics
    {
        /// <summary>
        /// 设置五轴运动学模型
        /// </summary>
        void SetModel(MachineType machineType,
                      double[] primaryAxisPoint, double[] slaveAxisPoint,
                      double[] toolLocationPoint,
                      short dirMode, short[] axisDir, double[] axisVectors);

        /// <summary>从 ini 配置文件加载运动学参数</summary>
        void LoadFromIni(string iniFilePath);
    }
}
