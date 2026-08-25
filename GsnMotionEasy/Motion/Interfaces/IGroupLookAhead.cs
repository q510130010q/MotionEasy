namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>前瞻管理。对应 Demo button4_Click 中的 GTN_GroupLookAheadDisable / GTN_GroupLookAheadEnable / GTN_SetGroupLookAheadParameter。</summary>
    public interface IGroupLookAhead
    {
        /// <summary>使能前瞻</summary>
        void Enable(int lookAheadNum, double time, double radiusRatio);

        /// <summary>关闭前瞻</summary>
        void Disable();

        /// <summary>获取前瞻缓冲段数</summary>
        int GetSegCount();
    }
}
