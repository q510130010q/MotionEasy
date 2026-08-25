using static GTN.mc;

namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>
    /// Move运动模式接口。
    /// Gsn 卡仅支持基础 MoveAbsolute（GTN_MoveAbsolute），不支持 MoveAbsoluteEx / MoveVelocity。
    /// </summary>
    public interface IMoveMotion
    {
        void MoveAbsolute(int pos, double vel, double acc, double dec, short percent = 0);
        TMoveAbsolutePrm GetMoveAbsolute();
    }
}
