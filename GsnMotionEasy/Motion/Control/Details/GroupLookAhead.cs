using GTN;
using static GTN.mc;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupLookAhead 实现。对应 Demo Form1.cs button4_Click 行 1737-1752。
    /// </summary>
    public class GroupLookAhead : IGroupLookAhead
    {
        private readonly GroupContext _ctx;

        public GroupLookAhead(GroupContext ctx) => _ctx = ctx;

        public void Enable(int lookAheadNum, double time, double radiusRatio)
        {
            var list0 = _ctx.CreateListInfo();
            short rtn;

            rtn = mc.GTN_GroupLookAheadDisable(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GroupLookAheadDisable");

            rtn = mc.GTN_GroupLookAheadEnable(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GroupLookAheadEnable");

            var prm = new TGroupLookAheadParameter
            {
                lookAheadNum = lookAheadNum,
                time = time,
                radiusRatio = radiusRatio,
                reserve1 = new short[6],
                reserve2 = new int[4],
                reserve3 = new double[7],
            };

            rtn = mc.GTN_SetGroupLookAheadParameter(_ctx.Core, _ctx.Group, ref prm, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_SetGroupLookAheadParameter");
        }

        public void Disable()
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_GroupLookAheadDisable(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GroupLookAheadDisable");
        }

        public int GetSegCount()
        {
            int segCount;
            short rtn = mc.GTN_GetGroupLookAheadSegCount(_ctx.Core, _ctx.Group, out segCount);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GetGroupLookAheadSegCount");
            return segCount;
        }
    }
}
