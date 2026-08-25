using System.Runtime.InteropServices;
using GTN;
using static GTN.mc;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupCalibration 实现。对应《五轴标定计算模型参数使用说明.pdf》。
    /// 标准版 GTN_FiveAxisCalibration（gts.cs 已声明）+ 9P 版 GTN_FiveAxisCalibration9P（局部 P/Invoke）。
    /// </summary>
    public class GroupCalibration : IGroupCalibration
    {
        public CalibrationResult Calibrate(CalibrationInput input)
        {
            double[] s0 = PointsToDoubles(input.PointsS0);
            double[] s180 = PointsToDoubles(input.PointsS180);
            double[] p0 = PointsToDoubles(input.PointsP0);

            var prm = new TFiveAxisCalibrationPrm
            {
                type = (short)input.MachineType,
                axisSide = input.AxisSide,
                axisDir = input.AxisDir,
                // 注意：gts.cs 中字段拼写为 ponitS0Count（笔误），按 gts.cs 来
                ponitS0Count = (short)input.PointsS0.Count,
                ponitS180Count = (short)input.PointsS180.Count,
                ponitP0Count = (short)input.PointsP0.Count,
                pPointS0 = IntPtr.Zero,
                pPointS180 = IntPtr.Zero,
                pPointP0 = IntPtr.Zero,
            };

            GCHandle hS0 = default, hS180 = default, hP0 = default;
            try
            {
                if (s0 != null) { hS0 = GCHandle.Alloc(s0, GCHandleType.Pinned); prm.pPointS0 = hS0.AddrOfPinnedObject(); }
                if (s180 != null) { hS180 = GCHandle.Alloc(s180, GCHandleType.Pinned); prm.pPointS180 = hS180.AddrOfPinnedObject(); }
                if (p0 != null) { hP0 = GCHandle.Alloc(p0, GCHandleType.Pinned); prm.pPointP0 = hP0.AddrOfPinnedObject(); }

                TFiveAxisKinematicPrm outKin;
                TFiveAxisCalibrationInfo info;
                short rtn = mc.GTN_FiveAxisCalibration(input.Core, (short)input.Mode, ref prm, out outKin, out info);
                GtnErrorHelper.ThrowIfError(rtn, "GTN_FiveAxisCalibration");

                return BuildResult(outKin, info, input.Mode);
            }
            finally
            {
                if (hS0.IsAllocated) hS0.Free();
                if (hS180.IsAllocated) hS180.Free();
                if (hP0.IsAllocated) hP0.Free();
            }
        }

        public CalibrationResult Calibrate9P(CalibrationInput9P input)
        {
            var prm = new TFiveAxisCalibrationPrm9P
            {
                type = (short)input.MachineType,
                axisSide = input.AxisSide,
                axisDir = input.AxisDir,
                pointS0Count = (short)input.PointsS0.Count,
                pointS180Count = (short)input.PointsS180.Count,
                pointP0Count = (short)input.PointsP0.Count,
                pointS0 = new double[45],
                pointS180 = new double[45],
                pointP0 = new double[45],
            };

            CopyPointsToArray(input.PointsS0, prm.pointS0);
            CopyPointsToArray(input.PointsS180, prm.pointS180);
            CopyPointsToArray(input.PointsP0, prm.pointP0);

            TFiveAxisKinematicPrm outKin;
            TFiveAxisCalibrationInfo info;
            short rtn = GTN_FiveAxisCalibration9P(input.Core, (short)input.Mode, ref prm, out outKin, out info);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_FiveAxisCalibration9P");

            return BuildResult(outKin, info, input.Mode);
        }

        private static double[] PointsToDoubles(System.Collections.Generic.List<CalibrationPoint> points)
        {
            if (points == null || points.Count == 0) return null;
            double[] arr = new double[points.Count * 5];
            for (int i = 0; i < points.Count; i++)
            {
                arr[i * 5 + 0] = points[i].X;
                arr[i * 5 + 1] = points[i].Y;
                arr[i * 5 + 2] = points[i].Z;
                arr[i * 5 + 3] = points[i].P;
                arr[i * 5 + 4] = points[i].S;
            }
            return arr;
        }

        private static void CopyPointsToArray(System.Collections.Generic.List<CalibrationPoint> points, double[] target)
        {
            if (points == null) return;
            for (int i = 0; i < points.Count && i < 9; i++)
            {
                target[i * 5 + 0] = points[i].X;
                target[i * 5 + 1] = points[i].Y;
                target[i * 5 + 2] = points[i].Z;
                target[i * 5 + 3] = points[i].P;
                target[i * 5 + 4] = points[i].S;
            }
        }

        private static CalibrationResult BuildResult(TFiveAxisKinematicPrm kin, TFiveAxisCalibrationInfo info, CalibrationMode mode)
        {
            var result = new CalibrationResult
            {
                MachineType = (MachineType)kin.type,
                PrimaryAxisPoint = kin.primaryAxisPoint,
                SlaveAxisPoint = kin.slaveAxisPoint,
                ToolLocationPoint = kin.toolLocationPoint,
                DirMode = kin.dirMode,
                Dir = kin.dir,
                AxisVectors = kin.axisVector,

                Mode = mode,
                IterationFlag = info.iterationFlag == 1,
                IterationCount = info.iterationCount,
                DirXError = info.dirXError,
                DirXErrorVariance = info.dirXErrorVariance,
                DirYError = info.dirYError,
                DirYErrorVariance = info.dirYErrorVariance,
                DirZError = info.dirZError,
                DirZErrorVariance = info.dirZErrorVariance,
                DirAllError = info.dirAllError,
                DirAllErrorVariance = info.dirAllErrorVariance,
            };
            return result;
        }

        // ── 9P 版 P/Invoke（gts.cs 未声明，按 PDF 局部补齐）──

        [StructLayout(LayoutKind.Sequential)]
        private struct TFiveAxisCalibrationPrm9P
        {
            public short type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public short[] axisSide;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public short[] axisDir;
            public short pointS0Count;
            public short pointS180Count;
            public short pointP0Count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 45)]
            public double[] pointS0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 45)]
            public double[] pointS180;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 45)]
            public double[] pointP0;
        }

        [DllImport("gts.dll")]
        private static extern short GTN_FiveAxisCalibration9P(
            short core, short mode,
            ref TFiveAxisCalibrationPrm9P pCalibrationPrm,
            out TFiveAxisKinematicPrm pOutKinematic,
            out TFiveAxisCalibrationInfo pInfo);
    }
}
