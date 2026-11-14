// GSN-DMA3D-001 V1.0 / 2026-10-24. Source and scope: JUPIAN/LaserSaw/gsn_dma3d_integration_decision_v1.0.md
namespace GsnMotionEasy.Model;

/// <summary>
/// 加工完成判定的三态结果（DMA 共用）：
/// NotYet = 继续等；Completed = 确认完成；Failed = 必须异常收尾。
/// 定义及成员顺序来自用户 2026-10-24 提供的源码。
/// </summary>
public enum DmaJobCheck { NotYet, Completed, Failed }