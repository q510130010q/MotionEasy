namespace GsnMotionEasy.Model
{
    /// <summary>
    /// Group 状态信息。从固高 TGroupStatus 提取关键字段。
    /// </summary>
    public class GroupStatusInfo
    {
        /// <summary>Group 是否运动中（TGroupStatus.run）</summary>
        public bool IsRunning { get; set; }

        /// <summary>Group 是否已使能</summary>
        public bool IsEnabled { get; set; }

        /// <summary>指令流是否执行中</summary>
        public bool CommandListExecuting { get; set; }

        /// <summary>前瞻缓冲段数</summary>
        public int LookAheadSegCount { get; set; }

        public static GroupStatusInfo FromGroupStatus(bool run, bool enabled, int segCount = 0)
        {
            return new GroupStatusInfo
            {
                IsRunning = run,
                IsEnabled = enabled,
                LookAheadSegCount = segCount,
            };
        }
    }
}
