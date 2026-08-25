using GTN;
using static GTN.mc;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupCoordTransform 实现。对应 Demo Form1.cs button3_Click 行 582-634。
    /// </summary>
    public class GroupCoordTransform : IGroupCoordTransform
    {
        private readonly GroupContext _ctx;

        public GroupCoordTransform(GroupContext ctx) => _ctx = ctx;

        public void SetPcsOffset(double x, double y, double z, double p = 0, double s = 0)
        {
            var pPrm = new TCoordinateTransformUnionReloadOffset
            {
                mode = mc.COORD_TRANS_TYPE_OFFSET,
                reserve = new short[3],
                offset =
                {
                    prm = new double[8] { x, y, z, 0, 0, 0, 0, 0 },
                    reserve = new double[12],
                }
            };
            // Demo 行 586-590: prm[0..2] = x/y/z, prm[3..4] = 0
            pPrm.offset.prm[0] = x;
            pPrm.offset.prm[1] = y;
            pPrm.offset.prm[2] = z;

            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupCoordinateTransformPrm(
                _ctx.Core, _ctx.Group, mc.COORD_SYSTEM_PCS, 1, ref pPrm, 1, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupCoordinateTransformPrm(PCS)");
        }

        public void SetTcsOffset(double x, double y, double z,
                                 double rot1 = 0, double rot2 = 0, double rot3 = 0)
        {
            var cartPrm = new TCartesianParameter
            {
                transX = x,
                transY = y,
                transZ = z,
                rotAngle1 = rot1,
                rotAngle2 = rot2,
                rotAngle3 = rot3,
            };

            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupCartesianTransform(
                _ctx.Core, _ctx.Group, mc.COORD_SYSTEM_TCS, 1, ref cartPrm, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupCartesianTransform(TCS)");
        }

        public void SetAcsOffset(double[] offsets)
        {
            // Demo 行 606-614: kinOffset[8]，索引 0/1/2 对应 acsy/acsx/acsz（注意 Demo 顺序）
            var kinOffset = new double[8];
            for (int i = 0; i < 8; i++)
                kinOffset[i] = i < offsets.Length ? offsets[i] : 0;

            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupAcsKinematicOffset(_ctx.Core, _ctx.Group, ref kinOffset[0], ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupAcsKinematicOffset");
        }

        public void SetCommandPosDefine(short coordSystem, short orientationMode, short configIndex = 0)
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupCommandPosDefine(
                _ctx.Core, _ctx.Group, coordSystem, orientationMode, configIndex, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupCommandPosDefine");
        }

        public void SetCommandVelDefine(short type = 1, short mode = 0)
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupCommandVelDefine(_ctx.Core, _ctx.Group, type, mode, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupCommandVelDefine");
        }

        public void SetProfileCoordinateSystem(short coordSystem)
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupProfileCoordinateSystem(_ctx.Core, _ctx.Group, coordSystem, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupProfileCoordinateSystem");
        }

        public void SetGroupOrientationConstraint(double oriVelMax, double oriAccMax, double oriDecMax, double oriJerkMax)
        {
            var prm = new TGroupOrientationConstraint
            {
                oriVelMax = oriVelMax,
                oriAccMax = oriAccMax,
                oriDecMax = oriDecMax,
                oriJerkMax = oriJerkMax,
            };
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupOrientationConstraint(_ctx.Core, _ctx.Group, ref prm, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupOrientationConstraint");
        }

        public void SetGroupVelProfileMode(short mode, double accTime, double k)
        {
            var prm = new TVelProfileMode
            {
                mode = mode,
                parameter = new TVelProfileModeSmooth
                {
                    accTime = accTime,
                    k = k,
                    reserve1 = new double[18],
                },
            };
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupVelProfileMode(_ctx.Core, _ctx.Group, ref prm, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupVelProfileMode");
        }
    }
}
