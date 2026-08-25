namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>指令流管理。对应 Demo button3_Click/button6_Click/button7_Click/button8_Click 中的 GTN_StopCommandList / GTN_ClearCommandListData / GTN_CommandListDataEnd / GTN_StartCommandList。</summary>
    public interface IGroupCommandList
    {
        /// <summary>停止指令流并清空数据，准备写入新指令</summary>
        void Begin();

        /// <summary>指令流数据结束（压入缓存）</summary>
        void End();

        /// <summary>启动指令流执行</summary>
        void Start();

        /// <summary>停止指令流</summary>
        void Stop();

        /// <summary>清空指令流数据</summary>
        void Clear();

        /// <summary>当前指令流段号（自增，用于追踪）</summary>
        int SegmentNumber { get; set; }
    }
}
