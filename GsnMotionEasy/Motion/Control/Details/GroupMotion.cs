using System.Runtime.InteropServices;
using GTN;
using static GTN.mc;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupMotion 实现。对应 Demo Form1.cs button6_Click 行 924-1190。
    ///
    /// RTCP 实现按《新架构功能编程手册》10.4 章节：
    /// Group + 运动学变换 + 规划坐标系(PCS) + 笛卡尔坐标轴限位 配置完成后自动生效。
    /// gts.cs 未声明 GTN_SetGroupCartesianCoordinateAxisLimit，此处局部 P/Invoke 补齐。
    /// </summary>
    public class GroupMotion : IGroupMotion
    {
        private readonly GroupContext _ctx;

        public GroupMotion(GroupContext ctx) => _ctx = ctx;

        public void MoveLinearAbsolute(double[] pos, double vel, double acc, double dec, short modal = 1)
        {
            var list = _ctx.CreateCommandListInfo();
            list.modal = modal;
            list.segNum++;

            var movePrm = new TGroupMoveParameter
            {
                velocity = vel,
                acceleration = acc,
                deceleration = dec,
                reserve1 = new double[3],
                reserve2 = new short[3],
                reserve3 = new long[3],
            };

            // pos 与 dir 数组按固高要求长度 8
            double[] posArr = new double[8];
            for (int i = 0; i < 8; i++) posArr[i] = i < pos.Length ? pos[i] : 0;
            short[] dir = new short[8];

            short rtn = mc.GTN_MoveLinearAbsolute(_ctx.Core, _ctx.Group, ref posArr[0], ref dir[0], ref movePrm, ref list);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_MoveLinearAbsolute");
        }

        public void MoveCircularAbsolute(double[] endPos, double[] auxPoint, double vel, double acc, double dec, short modal = 1)
        {
            var list = _ctx.CreateCommandListInfo();
            list.modal = modal;
            list.segNum++;

            var movePrm = new TGroupMoveParameter
            {
                velocity = vel,
                acceleration = acc,
                deceleration = dec,
                reserve1 = new double[3],
                reserve2 = new short[3],
                reserve3 = new long[3],
            };

            double[] endArr = new double[8];
            for (int i = 0; i < 8; i++) endArr[i] = i < endPos.Length ? endPos[i] : 0;

            var circPrm = new TCircularParameter
            {
                arcMode = mc.CIRCULAR_MODE_SPACE_BORDER,
                pad = new short[3],
            };
            circPrm.data.spaceBorder.endPointMode = mc.CIRCULAR_END_POINT_MODE_END_POINT;
            circPrm.data.spaceBorder.auxPoint1 = auxPoint.Length > 0 ? auxPoint[0] : 0;
            circPrm.data.spaceBorder.auxPoint2 = auxPoint.Length > 1 ? auxPoint[1] : 0;
            circPrm.data.spaceBorder.auxPoint3 = auxPoint.Length > 2 ? auxPoint[2] : 0;
            circPrm.data.spaceBorder.auxPoint4 = auxPoint.Length > 3 ? auxPoint[3] : 0;
            circPrm.data.spaceBorder.auxPoint5 = auxPoint.Length > 4 ? auxPoint[4] : 0;
            circPrm.data.spaceBorder.auxPoint6 = 0;
            circPrm.data.spaceBorder.auxPoint7 = 0;
            circPrm.data.spaceBorder.auxPoint8 = 0;

            short rtn = mc.GTN_MoveCircularAbsolute(_ctx.Core, _ctx.Group, ref endArr[0], ref circPrm, ref movePrm, ref list);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_MoveCircularAbsolute");
        }

        public void EnableRtcpMode()
        {
            // 1. 规划坐标系设为 PCS（若未设置）
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetGroupProfileCoordinateSystem(_ctx.Core, _ctx.Group, mc.COORD_SYSTEM_PCS, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupProfileCoordinateSystem(PCS for RTCP)");

            // 2. 启用 Group 笛卡尔坐标轴限位（手册 10.4 章节关键调用）
            rtn = GTN_SetGroupCartesianCoordinateAxisLimit(_ctx.Core, _ctx.Group, 1, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupCartesianCoordinateAxisLimit(enable)");
        }

        public void DisableRtcpMode()
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = GTN_SetGroupCartesianCoordinateAxisLimit(_ctx.Core, _ctx.Group, 0, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupCartesianCoordinateAxisLimit(disable)");
        }

        [DllImport("gts.dll")]
        private static extern short GTN_SetGroupCartesianCoordinateAxisLimit(
            short core, short group, short enable, ref TListInfo pListInfo);
    }
}
