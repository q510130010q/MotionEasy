using GsnMotionEasy.Model;

namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>状态查询。对应 Demo 中 GTN_GetGroupStatus / GTN_GetCommandListStatus / GTN_GetGroupLookAheadSegCount 的轮询逻辑。</summary>
    public interface IGroupStatus
    {
        /// <summary>获取 Group 综合状态</summary>
        GroupStatusInfo GetStatus();

        /// <summary>Group 是否运动中（阻塞等待场景用）</summary>
        bool IsRunning();

        /// <summary>指令流是否执行中</summary>
        bool IsCommandListExecuting();
    }
}
