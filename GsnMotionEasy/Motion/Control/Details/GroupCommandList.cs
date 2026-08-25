using GTN;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupCommandList 实现。对应 Demo Form1.cs button3_Click/button6_Click/button7_Click/button8_Click
    /// 中的 GTN_StopCommandList / GTN_ClearCommandListData / GTN_CommandListDataEnd / GTN_StartCommandList。
    /// </summary>
    public class GroupCommandList : IGroupCommandList
    {
        private readonly GroupContext _ctx;
        private int _segNum;

        public GroupCommandList(GroupContext ctx) => _ctx = ctx;

        public int SegmentNumber
        {
            get => _segNum;
            set => _segNum = value;
        }

        /// <summary>停止指令流并清空数据，准备写入新指令</summary>
        public void Begin()
        {
            _segNum = 0;
            var list0 = _ctx.CreateListInfo();

            short rtn = mc.GTN_StopCommandList(_ctx.Core, _ctx.Group, 0, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_StopCommandList");

            rtn = mc.GTN_ClearCommandListData(_ctx.Core, _ctx.CommandListId, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_ClearCommandListData");
        }

        /// <summary>指令流数据结束（压入缓存）。Demo button8_Click 中循环调用直到 rtn==0。</summary>
        public void End()
        {
            short rtn;
            do
            {
                rtn = mc.GTN_CommandListDataEnd(_ctx.Core, _ctx.CommandListId);
                if (rtn != 0 && rtn != 11700)
                    GtnErrorHelper.ThrowIfError(rtn, "GTN_CommandListDataEnd");
            } while (rtn != 0);
        }

        public void Start()
        {
            var listInfo = _ctx.CreateListInfo();
            short rtn = mc.GTN_StartCommandList(_ctx.Core, _ctx.CommandListId, ref listInfo);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_StartCommandList");
        }

        public void Stop()
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_StopCommandList(_ctx.Core, _ctx.Group, 0, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_StopCommandList");
        }

        public void Clear()
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_ClearCommandListData(_ctx.Core, _ctx.CommandListId, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_ClearCommandListData");
        }
    }
}
