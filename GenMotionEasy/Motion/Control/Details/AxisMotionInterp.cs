using GenMotionEasy.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// 插补运动 - 7.5章
    /// 包含离线插补和实时插补两种模式
    /// </summary>
    public class AxisMotionInterp : IInterpMotion
    {
        private readonly short _core;
        private readonly short _crd;
        private readonly object _lock;

        public AxisMotionInterp(short core, short crd, object lockObj)
        {
            _core = core;
            _crd = crd;
            _lock = lockObj;
        }

        #region 坐标系建立

        /// <summary>
        /// 建立坐标系（2维）
        /// </summary>
        /// <param name="profile1">X轴profile编号</param>
        /// <param name="profile2">Y轴profile编号</param>
        /// <param name="synVelMax">最大合成速度</param>
        /// <param name="synAccMax">最大合成加速度</param>
        /// <param name="evenTime">最小插补时间(ms)</param>
        /// <param name="setOriginFlag">设置原点标志,0:默认当前规划位为原点;1:用户指定原点</param>
        /// <param name="originPos1">原点位置1</param>
        /// <param name="originPos2">原点位置2</param>
        public void SetupCrd2D(short profile1, short profile2,
            double synVelMax, double synAccMax, short evenTime = 10,
            short setOriginFlag = 1, int originPos1 = 0, int originPos2 = 0)
        {
            lock (_lock)
            {
                TCrdPrm crdPrm = new TCrdPrm();
                crdPrm.dimension = 2;
                crdPrm.profile1 = profile1;
                crdPrm.profile2 = profile2;
                crdPrm.profile3 = 0;
                crdPrm.profile4 = 0;
                crdPrm.profile5 = 0;
                crdPrm.profile6 = 0;
                crdPrm.profile7 = 0;
                crdPrm.profile8 = 0;
                crdPrm.synVelMax = synVelMax;
                crdPrm.synAccMax = synAccMax;
                crdPrm.evenTime = evenTime;
                crdPrm.setOriginFlag = setOriginFlag;
                crdPrm.originPos1 = originPos1;
                crdPrm.originPos2 = originPos2;
                crdPrm.originPos3 = 0;
                crdPrm.originPos4 = 0;
                crdPrm.originPos5 = 0;
                crdPrm.originPos6 = 0;
                crdPrm.originPos7 = 0;
                crdPrm.originPos8 = 0;
                GtnErrorHelper.ThrowIfError(GTN_SetCrdPrm(_core, _crd, ref crdPrm), "GTN_SetCrdPrm");
            }
        }

        /// <summary>
        /// 建立坐标系（3维）
        /// </summary>
        public void SetupCrd3D(short profile1, short profile2, short profile3,
            double synVelMax, double synAccMax, short evenTime = 10,
            short setOriginFlag = 1, int originPos1 = 0, int originPos2 = 0, int originPos3 = 0)
        {
            lock (_lock)
            {
                TCrdPrm crdPrm = new TCrdPrm();
                crdPrm.dimension = 3;
                crdPrm.profile1 = profile1;
                crdPrm.profile2 = profile2;
                crdPrm.profile3 = profile3;
                crdPrm.profile4 = 0;
                crdPrm.profile5 = 0;
                crdPrm.profile6 = 0;
                crdPrm.profile7 = 0;
                crdPrm.profile8 = 0;
                crdPrm.synVelMax = synVelMax;
                crdPrm.synAccMax = synAccMax;
                crdPrm.evenTime = evenTime;
                crdPrm.setOriginFlag = setOriginFlag;
                crdPrm.originPos1 = originPos1;
                crdPrm.originPos2 = originPos2;
                crdPrm.originPos3 = originPos3;
                crdPrm.originPos4 = 0;
                crdPrm.originPos5 = 0;
                crdPrm.originPos6 = 0;
                crdPrm.originPos7 = 0;
                crdPrm.originPos8 = 0;

                GtnErrorHelper.ThrowIfError(GTN_SetCrdPrm(_core, _crd, ref crdPrm), "GTN_SetCrdPrm");
            }
        }

        /// <summary>
        /// 建立坐标系（4维）
        /// </summary>
        public void SetupCrd4D(short profile1, short profile2, short profile3, short profile4,
            double synVelMax, double synAccMax, short evenTime = 10,
            short setOriginFlag = 1, int originPos1 = 0, int originPos2 = 0, int originPos3 = 0, int originPos4 = 0)
        {
            lock (_lock)
            {
                TCrdPrm crdPrm = new TCrdPrm();
                crdPrm.dimension = 4;
                crdPrm.profile1 = profile1;
                crdPrm.profile2 = profile2;
                crdPrm.profile3 = profile3;
                crdPrm.profile4 = profile4;
                crdPrm.profile5 = 0;
                crdPrm.profile6 = 0;
                crdPrm.profile7 = 0;
                crdPrm.profile8 = 0;
                crdPrm.synVelMax = synVelMax;
                crdPrm.synAccMax = synAccMax;
                crdPrm.evenTime = evenTime;
                crdPrm.setOriginFlag = setOriginFlag;
                crdPrm.originPos1 = originPos1;
                crdPrm.originPos2 = originPos2;
                crdPrm.originPos3 = originPos3;
                crdPrm.originPos4 = originPos4;
                crdPrm.originPos5 = 0;
                crdPrm.originPos6 = 0;
                crdPrm.originPos7 = 0;
                crdPrm.originPos8 = 0;

                GtnErrorHelper.ThrowIfError(GTN_SetCrdPrm(_core, _crd, ref crdPrm), "GTN_SetCrdPrm");
            }
        }

        #endregion

        #region 离线插补

        /// <summary>
        /// 离线插补 - 清除坐标系FIFO缓冲区
        /// </summary>
        /// <param name="fifo">FIFO编号(0或1)</param>
        public void OfflineClear(short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_CrdClear(_core, _crd, fifo), "GTN_CrdClear");
            }
        }

        /// <summary>
        /// 离线插补 - 查询缓冲区剩余空间
        /// </summary>
        public int GetCrdSpace(short fifo = 0)
        {
            lock (_lock)
            {
                int space;
                GtnErrorHelper.ThrowIfError(GTN_CrdSpace(_core, _crd, out space, fifo), "GTN_CrdSpace");
                return space;
            }
        }

        /// <summary>
        /// 离线插补 - 直线插补 XY
        /// </summary>
        public void LnXY(int x, int y, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXY(_core, _crd, x, y, synVel, synAcc, velEnd, fifo), "GTN_LnXY");
            }
        }

        /// <summary>
        /// 离线插补 - 直线插补 XY（带段号）
        /// </summary>
        public void LnXYWN(int x, int y, double synVel, double synAcc, double velEnd, int segNum, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXYWN(_core, _crd, x, y, synVel, synAcc, velEnd, segNum, fifo), "GTN_LnXYWN");
            }
        }

        /// <summary>
        /// 离线插补 - 快速定位 XY (G0)
        /// </summary>
        public void LnXYG0(int x, int y, double synVel, double synAcc, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXYG0(_core, _crd, x, y, synVel, synAcc, fifo), "GTN_LnXYG0");
            }
        }

        /// <summary>
        /// 离线插补 - 直线插补 XYZ
        /// </summary>
        public void LnXYZ(int x, int y, int z, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXYZ(_core, _crd, x, y, z, synVel, synAcc, velEnd, fifo), "GTN_LnXYZ");
            }
        }

        /// <summary>
        /// 离线插补 - 直线插补 XYZ（带段号）
        /// </summary>
        public void LnXYZWN(int x, int y, int z, double synVel, double synAcc, double velEnd, int segNum, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXYZWN(_core, _crd, x, y, z, synVel, synAcc, velEnd, segNum, fifo), "GTN_LnXYZWN");
            }
        }

        /// <summary>
        /// 离线插补 - 快速定位 XYZ (G0)
        /// </summary>
        public void LnXYZG0(int x, int y, int z, double synVel, double synAcc, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXYZG0(_core, _crd, x, y, z, synVel, synAcc, fifo), "GTN_LnXYZG0");
            }
        }

        /// <summary>
        /// 离线插补 - 直线插补 XYZA（4轴）
        /// </summary>
        public void LnXYZA(int x, int y, int z, int a, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXYZA(_core, _crd, x, y, z, a, synVel, synAcc, velEnd, fifo), "GTN_LnXYZA");
            }
        }

        /// <summary>
        /// 离线插补 - 快速定位 XYZA (G0)
        /// </summary>
        public void LnXYZAG0(int x, int y, int z, int a, double synVel, double synAcc, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_LnXYZAG0(_core, _crd, x, y, z, a, synVel, synAcc, fifo), "GTN_LnXYZAG0");
            }
        }

        /// <summary>
        /// 离线插补 - 圆弧插补 XY（半径方式）
        /// </summary>
        public void ArcXYR(int x, int y, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_ArcXYR(_core, _crd, x, y, radius, circleDir, synVel, synAcc, velEnd, fifo), "GTN_ArcXYR");
            }
        }

        /// <summary>
        /// 离线插补 - 圆弧插补 XY（圆心方式）
        /// </summary>
        public void ArcXYC(int x, int y, double xCenter, double yCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_ArcXYC(_core, _crd, x, y, xCenter, yCenter, circleDir, synVel, synAcc, velEnd, fifo), "GTN_ArcXYC");
            }
        }

        /// <summary>
        /// 离线插补 - 圆弧插补 YZ（半径方式）
        /// </summary>
        public void ArcYZR(int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_ArcYZR(_core, _crd, y, z, radius, circleDir, synVel, synAcc, velEnd, fifo), "GTN_ArcYZR");
            }
        }

        /// <summary>
        /// 离线插补 - 圆弧插补 YZ（圆心方式）
        /// </summary>
        public void ArcYZC(int y, int z, double yCenter, double zCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_ArcYZC(_core, _crd, y, z, yCenter, zCenter, circleDir, synVel, synAcc, velEnd, fifo), "GTN_ArcYZC");
            }
        }

        /// <summary>
        /// 离线插补 - 圆弧插补 ZX（半径方式）
        /// </summary>
        public void ArcZXR(int z, int x, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_ArcZXR(_core, _crd, z, x, radius, circleDir, synVel, synAcc, velEnd, fifo), "GTN_ArcZXR");
            }
        }

        /// <summary>
        /// 离线插补 - 圆弧插补 ZX（圆心方式）
        /// </summary>
        public void ArcZXC(int z, int x, double zCenter, double xCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_ArcZXC(_core, _crd, z, x, zCenter, xCenter, circleDir, synVel, synAcc, velEnd, fifo), "GTN_ArcZXC");
            }
        }

        /// <summary>
        /// 离线插补 - 螺旋线插补 XYRZ（半径方式）
        /// </summary>
        public void HelixXYRZ(int x, int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_HelixXYRZ(_core, _crd, x, y, z, radius, circleDir, synVel, synAcc, velEnd, fifo), "GTN_HelixXYRZ");
            }
        }

        /// <summary>
        /// 离线插补 - 螺旋线插补 XYCZ（圆心方式）
        /// </summary>
        public void HelixXYCZ(int x, int y, int z, double xCenter, double yCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_HelixXYCZ(_core, _crd, x, y, z, xCenter, yCenter, circleDir, synVel, synAcc, velEnd, fifo), "GTN_HelixXYCZ");
            }
        }

        /// <summary>
        /// 离线插补 - 螺旋线插补 YZRX（半径方式）
        /// </summary>
        public void HelixYZRX(int x, int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_HelixYZRX(_core, _crd, x, y, z, radius, circleDir, synVel, synAcc, velEnd, fifo), "GTN_HelixYZRX");
            }
        }

        /// <summary>
        /// 离线插补 - 螺旋线插补 YZCX（圆心方式）
        /// </summary>
        public void HelixYZCX(int x, int y, int z, double yCenter, double zCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_HelixYZCX(_core, _crd, x, y, z, yCenter, zCenter, circleDir, synVel, synAcc, velEnd, fifo), "GTN_HelixYZCX");
            }
        }

        /// <summary>
        /// 离线插补 - 螺旋线插补 ZXRY（半径方式）
        /// </summary>
        public void HelixZXRY(int x, int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_HelixZXRY(_core, _crd, x, y, z, radius, circleDir, synVel, synAcc, velEnd, fifo), "GTN_HelixZXRY");
            }
        }

        /// <summary>
        /// 离线插补 - 螺旋线插补 ZXCY（圆心方式）
        /// </summary>
        public void HelixZXCY(int x, int y, int z, double zCenter, double xCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_HelixZXCY(_core, _crd, x, y, z, zCenter, xCenter, circleDir, synVel, synAcc, velEnd, fifo), "GTN_HelixZXCY");
            }
        }

        /// <summary>
        /// 离线插补 - 启动坐标系运动
        /// </summary>
        /// <param name="option">0:普通启动,1:覆盖速度启动</param>
        public void OfflineStart(short option = 0)
        {
            lock (_lock)
            {
                short mask = (short)(1 << (_crd - 1));
                GtnErrorHelper.ThrowIfError(GTN_CrdStart(_core, mask, option), "GTN_CrdStart");
            }
        }

        /// <summary>
        /// 离线插补 - 单步启动坐标系运动
        /// </summary>
        /// <param name="option">0:普通启动,1:覆盖速度启动</param>
        public void OfflineStartStep(short option = 0)
        {
            lock (_lock)
            {
                short mask = (short)(1 << (_crd - 1));
                GtnErrorHelper.ThrowIfError(GTN_CrdStartStep(_core, mask, option), "GTN_CrdStartStep");
            }
        }

        #endregion

        #region 离线插补 - 缓存区辅助指令

        /// <summary>
        /// 缓存区数字量IO输出设置
        /// </summary>
        /// <param name="doType">输出类型: MC_GPO=通用输出</param>
        /// <param name="doMask">输出掩码</param>
        /// <param name="doValue">输出值</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufIO(short doType, ushort doMask, ushort doValue, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufIO(_core, _crd, doType, doMask, doValue, fifo), "GTN_BufIO");
            }
        }

        /// <summary>
        /// 缓存区延时设置
        /// </summary>
        /// <param name="delayTime">延时时间(ms)</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufDelay(ushort delayTime, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufDelay(_core, _crd, delayTime, fifo), "GTN_BufDelay");
            }
        }

        /// <summary>
        /// 缓存区DA输出
        /// </summary>
        /// <param name="chn">DA通道号</param>
        /// <param name="daValue">DA值</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufDA(short chn, short daValue, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufDA(_core, _crd, chn, daValue, fifo), "GTN_BufDA");
            }
        }

        /// <summary>
        /// 缓存区使能限位开关
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="limitType">限位类型</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufLmtsOn(short axis, short limitType, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufLmtsOn(_core, _crd, axis, limitType, fifo), "GTN_BufLmtsOn");
            }
        }

        /// <summary>
        /// 缓存区禁用限位开关
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="limitType">限位类型</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufLmtsOff(short axis, short limitType, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufLmtsOff(_core, _crd, axis, limitType, fifo), "GTN_BufLmtsOff");
            }
        }

        /// <summary>
        /// 缓存区设置停止IO
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="stopType">停止类型</param>
        /// <param name="inputType">输入类型</param>
        /// <param name="inputIndex">输入索引</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufSetStopIo(short axis, short stopType, short inputType, short inputIndex, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufSetStopIo(_core, _crd, axis, stopType, inputType, inputIndex, fifo), "GTN_BufSetStopIo");
            }
        }

        /// <summary>
        /// 缓存区启动轴点位运动（刀向跟随）
        /// </summary>
        /// <param name="moveAxis">运动轴号</param>
        /// <param name="pos">目标位置</param>
        /// <param name="vel">速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="modal">模态: 0=增量, 1=绝对</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufMove(short moveAxis, int pos, double vel, double acc, short modal, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufMove(_core, _crd, moveAxis, pos, vel, acc, modal, fifo), "GTN_BufMove");
            }
        }

        /// <summary>
        /// 缓存区启动轴跟随运动（刀向跟随）
        /// </summary>
        /// <param name="gearAxis">跟随轴号</param>
        /// <param name="pos">位置</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufGear(short gearAxis, int pos, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufGear(_core, _crd, gearAxis, pos, fifo), "GTN_BufGear");
            }
        }

        /// <summary>
        /// 缓存区启动轴跟随运动（带加减速百分比）
        /// </summary>
        /// <param name="gearAxis">跟随轴号</param>
        /// <param name="pos">位置</param>
        /// <param name="accPercent">加速百分比</param>
        /// <param name="decPercent">减速百分比</param>
        /// <param name="fifo">FIFO编号</param>
        public void BufGearPercent(short gearAxis, int pos, short accPercent, short decPercent, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_BufGearPercent(_core, _crd, gearAxis, pos, accPercent, decPercent, fifo), "GTN_BufGearPercent");
            }
        }

        #endregion

        #region 实时插补

        /// <summary>
        /// 实时插补 - 设置动态缓冲模式
        /// </summary>
        /// <param name="bufferMode">缓冲模式: 0=动态默认, 1=动态保持, 11=静态输入, 12=静态就绪, 13=静态启动</param>
        /// <param name="fifo">FIFO编号</param>
        public void SetBufferMode(short bufferMode, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetCrdBufferMode(_core, _crd, bufferMode, fifo), "GTN_SetCrdBufferMode");
            }
        }

        /// <summary>
        /// 实时插补 - 设置动态默认缓冲模式
        /// </summary>
        public void SetDynamicBufferMode(short fifo = 0)
        {
            SetBufferMode(CRD_BUFFER_MODE_DYNAMIC_DEFAULT, fifo);
        }

        /// <summary>
        /// 实时插补 - 设置动态保持缓冲模式
        /// </summary>
        public void SetDynamicKeepBufferMode(short fifo = 0)
        {
            SetBufferMode(CRD_BUFFER_MODE_DYNAMIC_KEEP, fifo);
        }

        /// <summary>
        /// 实时插补 - 切换到静态输入模式
        /// </summary>
        public void SetStaticInputBufferMode(short fifo = 0)
        {
            SetBufferMode(CRD_BUFFER_MODE_STATIC_INPUT, fifo);
        }

        /// <summary>
        /// 实时插补 - 切换到静态就绪模式
        /// </summary>
        public void SetStaticReadyBufferMode(short fifo = 0)
        {
            SetBufferMode(CRD_BUFFER_MODE_STATIC_READY, fifo);
        }

        /// <summary>
        /// 实时插补 - 切换到静态启动模式
        /// </summary>
        public void SetStaticStartBufferMode(short fifo = 0)
        {
            SetBufferMode(CRD_BUFFER_MODE_STATIC_START, fifo);
        }

        /// <summary>
        /// 实时插补 - 开启前瞻功能
        /// </summary>
        /// <param name="fifo">FIFO编号</param>
        /// <param name="link">前瞻连接方式</param>
        /// <param name="threshold">前瞻阈值</param>
        /// <param name="lookaheadInMc">控制器内前瞻使能</param>
        public void EnableLookAhead(short fifo = 0, short link = 1, ushort threshold = 50, short lookaheadInMc = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_CrdHsOn(_core, _crd, fifo, link, threshold, lookaheadInMc), "GTN_CrdHsOn");
            }
        }

        /// <summary>
        /// 实时插补 - 关闭前瞻功能
        /// </summary>
        public void DisableLookAhead(short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_CrdHsOff(_core, _crd, fifo), "GTN_CrdHsOff");
            }
        }

        /// <summary>
        /// 实时插补 - 获取前瞻缓冲区剩余空间
        /// </summary>
        public int GetLookAheadSpace(short fifo = 0)
        {
            lock (_lock)
            {
                int space;
                GtnErrorHelper.ThrowIfError(GTN_GetLookAheadSpace(_core, _crd, out space, fifo), "GTN_GetLookAheadSpace");
                return space;
            }
        }

        /// <summary>
        /// 实时插补 - 获取前瞻段数
        /// </summary>
        public int GetLookAheadSegCount(short fifo = 0)
        {
            lock (_lock)
            {
                int segCount;
                GtnErrorHelper.ThrowIfError(GTN_GetLookAheadSegCount(_core, _crd, out segCount, fifo), "GTN_GetLookAheadSegCount");
                return segCount;
            }
        }

        /// <summary>
        /// 实时插补 - 速度倍率设置
        /// </summary>
        /// <param name="synVelRatio">速度倍率(0~1)</param>
        public void SetOverride(double synVelRatio)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetOverride(_core, _crd, synVelRatio), "GTN_SetOverride");
            }
        }

        /// <summary>
        /// 实时插补 - 速度倍率设置(扩展)
        /// </summary>
        public void SetOverride2(double synVelRatio)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetOverride2(_core, _crd, synVelRatio), "GTN_SetOverride2");
            }
        }

        /// <summary>
        /// 实时插补 - 启动坐标系运动
        /// </summary>
        /// <param name="option">0:普通启动,1:覆盖速度启动</param>
        public void Start(short option = 0)
        {
            lock (_lock)
            {
                short mask = (short)(1 << (_crd - 1));
                GtnErrorHelper.ThrowIfError(GTN_CrdStart(_core, mask, option), "GTN_CrdStart");
            }
        }

        /// <summary>
        /// 实时插补 - 单步启动坐标系运动
        /// </summary>
        /// <param name="option">0:普通启动,1:覆盖速度启动</param>
        public void StartStep(short option = 0)
        {
            lock (_lock)
            {
                short mask = (short)(1 << (_crd - 1));
                GtnErrorHelper.ThrowIfError(GTN_CrdStartStep(_core, mask, option), "GTN_CrdStartStep");
            }
        }

        #endregion

        #region 坐标系状态与控制

        /// <summary>
        /// 获取坐标系运行状态
        /// </summary>
        /// <param name="run">0=运动结束,1=运动中</param>
        /// <param name="segment">当前运行的段号</param>
        /// <param name="fifo">FIFO编号</param>
        public void GetCrdStatus(out short run, out int segment, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_CrdStatus(_core, _crd, out run, out segment, fifo), "GTN_CrdStatus");
            }
        }

        /// <summary>
        /// 坐标系是否正在运动
        /// </summary>
        public bool IsRunning(short fifo = 0)
        {
            short run;
            int segment;
            GetCrdStatus(out run, out segment, fifo);
            return run == 1;
        }

        /// <summary>
        /// 获取坐标系合成位置
        /// </summary>
        public double GetCrdPos()
        {
            lock (_lock)
            {
                double pos;
                GtnErrorHelper.ThrowIfError(GTN_GetCrdPos(_core, _crd, out pos), "GTN_GetCrdPos");
                return pos;
            }
        }

        /// <summary>
        /// 获取坐标系合成速度
        /// </summary>
        public double GetCrdVel()
        {
            lock (_lock)
            {
                double synVel;
                GtnErrorHelper.ThrowIfError(GTN_GetCrdVel(_core, _crd, out synVel), "GTN_GetCrdVel");
                return synVel;
            }
        }

        /// <summary>
        /// 设置坐标系平滑停止和急停减速度
        /// </summary>
        public void SetStopDec(double decSmoothStop, double decAbruptStop)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetCrdStopDec(_core, _crd, decSmoothStop, decAbruptStop), "GTN_SetCrdStopDec");
            }
        }

        /// <summary>
        /// 平滑停止坐标系运动
        /// </summary>
        public void SmoothStop()
        {
            lock (_lock)
            {
                short mask = (short)(1 << (_crd - 1));
                GtnErrorHelper.ThrowIfError(GTN_Stop(_core, mask, 0), "GTN_Stop");
            }
        }

        /// <summary>
        /// 急停坐标系运动
        /// </summary>
        public void EmergencyStop()
        {
            lock (_lock)
            {
                short mask = (short)(1 << (_crd - 1));
                GtnErrorHelper.ThrowIfError(GTN_Stop(_core, mask, mask), "GTN_Stop");
            }
        }

        /// <summary>
        /// 清除坐标系缓冲区
        /// </summary>
        public void Clear(short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_CrdClear(_core, _crd, fifo), "GTN_CrdClear");
            }
        }

        /// <summary>
        /// 设置用户段号
        /// </summary>
        public void SetUserSegNum(int segNum, short fifo = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetUserSegNum(_core, _crd, segNum, fifo), "GTN_SetUserSegNum");
            }
        }

        /// <summary>
        /// 获取当前运行的用户段号
        /// </summary>
        public int GetUserSegNum(short fifo = 0)
        {
            lock (_lock)
            {
                int segment;
                GtnErrorHelper.ThrowIfError(GTN_GetUserSegNum(_core, _crd, out segment, fifo), "GTN_GetUserSegNum");
                return segment;
            }
        }

        /// <summary>
        /// 获取剩余段数
        /// </summary>
        public int GetRemainderSegNum(short fifo = 0)
        {
            lock (_lock)
            {
                int segment;
                GtnErrorHelper.ThrowIfError(GTN_GetRemainderSegNum(_core, _crd, out segment, fifo), "GTN_GetRemainderSegNum");
                return segment;
            }
        }

        /// <summary>
        /// 设置坐标系限位停止模式
        /// </summary>
        public void SetLmtStopMode(short lmtStopMode)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetCrdLmtStopMode(_core, _crd, lmtStopMode), "GTN_SetCrdLmtStopMode");
            }
        }

        #endregion
    }
}
