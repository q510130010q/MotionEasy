using GTN;
using static GTN.mc;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupStatus 实现。对应 Demo Form1.cs 判断轴状态() 与 button7_Click 中的 GTN_GetGroupStatus / GTN_GetCommandListStatus 轮询逻辑。
    /// </summary>
    public class GroupStatus : IGroupStatus
    {
        private readonly GroupContext _ctx;

        public GroupStatus(GroupContext ctx) => _ctx = ctx;

        public GroupStatusInfo GetStatus()
        {
            TGroupStatus gs;
            short rtn = mc.GTN_GetGroupStatus(_ctx.Core, _ctx.Group, out gs);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GetGroupStatus");

            TCommandListStatus cls;
            rtn = mc.GTN_GetCommandListStatus(_ctx.Core, _ctx.CommandListId, out cls);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GetCommandListStatus");

            int segCount = 0;
            rtn = mc.GTN_GetGroupLookAheadSegCount(_ctx.Core, _ctx.Group, out segCount);
            // 前瞻段数查询失败不阻塞状态返回
            bool enabled = gs.state != mc.GROUP_STATE_DISABLED;

            return new GroupStatusInfo
            {
                IsRunning = gs.run == 1,
                IsEnabled = enabled,
                CommandListExecuting = cls.execute == 1,
                LookAheadSegCount = segCount,
            };
        }

        public bool IsRunning()
        {
            TGroupStatus gs;
            short rtn = mc.GTN_GetGroupStatus(_ctx.Core, _ctx.Group, out gs);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GetGroupStatus");
            return gs.run == 1;
        }

        public bool IsCommandListExecuting()
        {
            TCommandListStatus cls;
            short rtn = mc.GTN_GetCommandListStatus(_ctx.Core, _ctx.CommandListId, out cls);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GetCommandListStatus");
            return cls.execute == 1;
        }
    }
}
