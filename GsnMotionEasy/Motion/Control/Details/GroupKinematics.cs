using System.IO;
using System.Collections.Generic;
using GTN;
using static GTN.mc;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupKinematics 实现。对应 Demo Form1.cs button3_Click 行 521-578 的 TKinematicTransform 填充与 GTN_SetGroupKinematicTransform 调用。
    /// </summary>
    public class GroupKinematics : IGroupKinematics
    {
        private readonly GroupContext _ctx;

        public GroupKinematics(GroupContext ctx) => _ctx = ctx;

        public void SetModel(MachineType machineType,
                             double[] primaryAxisPoint, double[] slaveAxisPoint,
                             double[] toolLocationPoint,
                             short dirMode, short[] axisDir, double[] axisVectors)
        {
            var kt = new TKinematicTransform
            {
                type = mc.KIN_TYPE_FIVE_AXIS,
                fiveAxis =
                {
                    type = (short)machineType,
                    dirMode = dirMode,
                    dir = new short[5],
                    axisVector = new double[15],
                    reserve1 = new short[3],
                    reserve2 = new short[2],
                    primaryAxisPoint = new double[3],
                    slaveAxisPoint = new double[3],
                    toolLocationPoint = new double[3],
                    reserve3 = new int[10],
                    reserve4 = new double[16],
                }
            };

            for (int i = 0; i < 3; i++)
            {
                kt.fiveAxis.primaryAxisPoint[i] = primaryAxisPoint[i];
                kt.fiveAxis.slaveAxisPoint[i] = slaveAxisPoint[i];
                kt.fiveAxis.toolLocationPoint[i] = toolLocationPoint != null && i < toolLocationPoint.Length
                    ? toolLocationPoint[i] : 0;
            }

            for (int i = 0; i < 5; i++)
                kt.fiveAxis.dir[i] = axisDir != null && i < axisDir.Length ? axisDir[i] : (short)0;

            for (int i = 0; i < 15; i++)
                kt.fiveAxis.axisVector[i] = axisVectors != null && i < axisVectors.Length ? axisVectors[i] : 0;

            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupKinematicTransform(_ctx.Core, _ctx.Group, ref kt, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupKinematicTransform");
        }

        public void LoadFromIni(string iniFilePath)
        {
            var values = ReadIni(iniFilePath);
            var cfg = FiveAxisConfig.LoadFromIni(iniFilePath);

            SetModel(
                cfg.MachineType,
                cfg.PrimaryAxisPoint,
                cfg.SlaveAxisPoint,
                cfg.ToolLocationPoint,
                cfg.DirMode,
                cfg.AxisDir,
                cfg.AxisVectors);
        }

        private static Dictionary<string, double> ReadIni(string filePath)
        {
            var values = new Dictionary<string, double>();
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#")) continue;
                var parts = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && double.TryParse(parts[1].Trim(), out double v))
                    values[parts[0].Trim()] = v;
            }
            return values;
        }
    }
}
