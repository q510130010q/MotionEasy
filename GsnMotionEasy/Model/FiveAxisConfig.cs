using System.Collections.Generic;
using System.IO;

namespace GsnMotionEasy.Model
{
    /// <summary>
    /// 五轴初始化配置。对应 Demo Form1.cs button3_Click 的所有参数。
    /// ini 文件字段与固高 TKinematicTransform 结构体对齐。
    /// </summary>
    public class FiveAxisConfig
    {
        // ── 卡/Group/轴号 ──
        public short Core { get; set; } = 1;
        public short Group { get; set; } = 1;
        public short AxisX { get; set; } = 1;
        public short AxisY { get; set; } = 2;
        public short AxisZ { get; set; } = 3;
        public short AxisP { get; set; } = 4;
        public short AxisS { get; set; } = 5;
        public short CommandListId { get; set; } = 1;

        // ── 脉冲当量（五个轴共用）──
        public double PulseAlpha { get; set; } = 1;
        public double PulseBeta { get; set; } = 2000;

        // ── 运动学模型 ──
        public MachineType MachineType { get; set; } = MachineType.RW_C_ON_A;

        // 主旋转轴中心相对 MCS（[x,y,z]）
        public double[] PrimaryAxisPoint { get; set; } = new double[3];
        // 从旋转轴中心相对 MCS
        public double[] SlaveAxisPoint { get; set; } = new double[3];
        // 刀具中心相对 MCS（默认 [0,0,0]）
        public double[] ToolLocationPoint { get; set; } = new double[3] { 0, 0, 0 };

        // 方向：dirMode=1 按轴设置；dir[0..2] 对应 X/Y/Z，1=工件侧 0=刀具侧；dir[3..4] 必须为 0
        public short DirMode { get; set; } = 1;
        public short[] AxisDir { get; set; } = new short[5];
        // 轴方向向量 [15]：X[3]+Y[3]+Z[3]+P[3]+S[3]
        public double[] AxisVectors { get; set; } = new double[15];

        // ── 坐标系偏移 ──
        public double[] PcsOffset { get; set; } = new double[5]; // x,y,z + 旋转 2
        public double[] TcsOffset { get; set; } = new double[3]; // x,y,z
        public double[] AcsOffset { get; set; } = new double[3]; // x,y,z（一般激光/点胶用不上）

        // ── 默认坐标描述 ──
        public short CoordSystem { get; set; } = 0;     // COORD_SYSTEM_PCS
        public short OrientationMode { get; set; } = 4; // ORI_MODE_ROTATE_AXIS_POS
        public short VelDefineType { get; set; } = 1;
        public short VelDefineMode { get; set; } = 0;
        public short ProfileCoordSystem { get; set; } = 0; // COORD_SYSTEM_PCS

        /// <summary>
        /// 从 ini 文件加载运动学参数（与 Demo ReadIniFile 一致的格式：每行 Key=value）。
        /// 仅加载运动学相关字段，其他字段保留默认值。
        /// </summary>
        public static FiveAxisConfig LoadFromIni(string iniFilePath)
        {
            var cfg = new FiveAxisConfig();
            var values = new Dictionary<string, double>();
            foreach (var line in File.ReadLines(iniFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#")) continue;
                var parts = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && double.TryParse(parts[1].Trim(), out double v))
                    values[parts[0].Trim()] = v;
            }

            cfg.MachineType = (MachineType)(short)values["Type"];

            cfg.PrimaryAxisPoint[0] = values["cpx"];
            cfg.PrimaryAxisPoint[1] = values["cpy"];
            cfg.PrimaryAxisPoint[2] = values["cpz"];

            cfg.SlaveAxisPoint[0] = values["csx"];
            cfg.SlaveAxisPoint[1] = values["csy"];
            cfg.SlaveAxisPoint[2] = values["csz"];

            cfg.AxisDir[0] = (short)values["x"];
            cfg.AxisDir[1] = (short)values["y"];
            cfg.AxisDir[2] = (short)values["z"];
            cfg.AxisDir[3] = 0;
            cfg.AxisDir[4] = 0;

            cfg.AxisVectors[0] = values["vxx"];
            cfg.AxisVectors[1] = values["vxy"];
            cfg.AxisVectors[2] = values["vxz"];
            cfg.AxisVectors[3] = values["vyx"];
            cfg.AxisVectors[4] = values["vyy"];
            cfg.AxisVectors[5] = values["vyz"];
            cfg.AxisVectors[6] = values["vzx"];
            cfg.AxisVectors[7] = values["vzy"];
            cfg.AxisVectors[8] = values["vzz"];
            cfg.AxisVectors[9] = values["vpx"];
            cfg.AxisVectors[10] = values["vpy"];
            cfg.AxisVectors[11] = values["vpz"];
            cfg.AxisVectors[12] = values["vsx"];
            cfg.AxisVectors[13] = values["vsy"];
            cfg.AxisVectors[14] = values["vsz"];

            return cfg;
        }
    }
}
