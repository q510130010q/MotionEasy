using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    public interface IMoveMotion
    {
        void MoveAbsolute(int pos, double vel, double acc, double dec, short percent = 0);
        TMoveAbsolutePrm GetMoveAbsolute();

        void MoveAbsoluteEx(int pos, double vel, double acc, double dec, short percent = 0,
            double velStart = 0, double velEnd = 0, double accStartPercent = 1, double decEndPercent = 1);
        TMoveAbsolutePrmEx GetMoveAbsoluteEx();

        void MoveVelocity(double vel, double acc, double dec, short direction,
            double jerkBegin = 0, double jerkEnd = 0);
        void MoveVelocityPositive(double vel, double acc, double dec,
            double jerkBegin = 0, double jerkEnd = 0);
        void MoveVelocityNegative(double vel, double acc, double dec,
            double jerkBegin = 0, double jerkEnd = 0);
        TMoveVelocityPrm GetMoveVelocity();
    }
}