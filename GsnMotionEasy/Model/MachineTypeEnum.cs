namespace GsnMotionEasy.Model
{
    public enum MachineType : short
    {
        // ── 双转台（Rotary Workpiece）──
        RW_C_ON_B = 0,
        RW_B_ON_A = 1,
        RW_A_ON_B = 2,
        RW_C_ON_A = 3,

        // ── 双摆头（Dual Tilt）──
        DT_B_ON_A = 4,
        DT_A_ON_B = 5,
        DT_A_ON_C = 6,
        DT_B_ON_C = 7,

        // ── 转台+摆头混合（Table+Wrist）──
        T_A_W_B = 8,
        T_B_W_A = 9,
        T_A_W_C = 10,
        T_B_W_C = 11,
    }
}
