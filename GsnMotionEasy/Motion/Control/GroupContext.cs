using GTN;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control
{
    public class GroupContext
    {
        public short Core { get; set; } = 1;
        public short Group { get; set; } = 1;
        public short CommandListId { get; set; } = 1;

        /// <summary>用于配置类操作的默认 ListInfo（无段号）</summary>
        public TListInfo CreateListInfo()
        {
            return new TListInfo();
        }

        /// <summary>用于指令流操作的 ListInfo（绑定 listId，segNum 自增）</summary>
        public TListInfo CreateCommandListInfo()
        {
            var list = new TListInfo
            {
                list = CommandListId,
                segNum = 0,
                modal = 1,
            };
            return list;
        }
    }
}
