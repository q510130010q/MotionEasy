// GSN-DMA3D-001 V1.0 / 2026-10-24. Source and scope: JUPIAN/LaserSaw/gsn_dma3d_integration_decision_v1.0.md
using GSN = GTN.mc;
using System;

namespace GsnMotionEasy.Motion.Control
{
    /// <summary>
    /// 插补指令的唯一出口（生产代码，不含任何测试开关）。
    /// 2D/3D 的 DMA 封装类都通过这里调用固高 GSN(gts.dll) 的插补指令，
    /// 便于统一加日志/排查；本身不做任何状态处理，参数原样透传。
    /// </summary>
    public static class CrdSdk
    {
        public static short CrdHsOn(short core, short crd, short fifo, short link, short threshold, short lookAheadInMc)
        { return GSN.GTN_CrdHsOn(core, crd, fifo, link, threshold, lookAheadInMc); }

        public static short CrdHsOff(short core, short crd, short fifo)
        { return GSN.GTN_CrdHsOff(core, crd, fifo); }

        public static short CrdData(short core, short crd, IntPtr pCrdData, short fifo)
        { return GSN.GTN_CrdData(core, crd, pCrdData, fifo); }

        public static short CrdSpace(short core, short crd, out int pSpace, short fifo)
        { return GSN.GTN_CrdSpace(core, crd, out pSpace, fifo); }

        public static short CrdStatus(short core, short crd, out short pRun, out int pSegment, short fifo)
        { return GSN.GTN_CrdStatus(core, crd, out pRun, out pSegment, fifo); }

        public static short CrdStart(short core, short mask, short option)
        { return GSN.GTN_CrdStart(core, mask, option); }

        public static short Stop(short core, int mask, int option)
        { return GSN.GTN_Stop(core, mask, option); }

        public static short LnXY(short core, short crd, int x, int y, double synVel, double synAcc, double velEnd, short fifo)
        { return GSN.GTN_LnXY(core, crd, x, y, synVel, synAcc, velEnd, fifo); }

        public static short LnXYZ(short core, short crd, int x, int y, int z, double synVel, double synAcc, double velEnd, short fifo)
        { return GSN.GTN_LnXYZ(core, crd, x, y, z, synVel, synAcc, velEnd, fifo); }

        public static short BufIO(short core, short crd, short doType, ushort doMask, ushort doValue, short fifo)
        { return GSN.GTN_BufIO(core, crd, doType, doMask, doValue, fifo); }

        public static short CrdClear(short crd, short fifo)
        { return GSN.GT_CrdClear(crd, fifo); }

        // GSN-PSO-001 V1.0: ordinary interpolation buffer; do not mix with the Ex pipeline.
        public static short BufPosCompareStart(short core, short crd, short fifo, short index)
        { return GSN.GTN_BufPosCompareStart(core, crd, fifo, index); }

        public static short BufPosCompareStop(short core, short crd, short fifo, short index)
        { return GSN.GTN_BufPosCompareStop(core, crd, fifo, index); }

        public static short CrdClearForCore(short core, short crd, short fifo)
        { return GSN.GTN_CrdClear(core, crd, fifo); }
        public static short SetCrdPrm(short core, short crd, ref GSN.TCrdPrm pCrdPrm)
        { return GSN.GTN_SetCrdPrm(core, crd, ref pCrdPrm); }
    }
}
