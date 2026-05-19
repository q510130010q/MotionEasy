using GenMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// 电子齿轮（Gear）运动模式 - 7.4章
    /// 电子齿轮模式能够将两轴或多轴联系起来，实现精确的同步运动，
    /// 从而替代传统的机械齿轮连接。被跟随的轴叫主轴，跟随的轴叫从轴。
    /// </summary>
    public class AxisMotionGear : IGearMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;

        public AxisMotionGear(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        #region 模式设置

        /// <summary>
        /// 设置指定轴为电子齿轮运动模式
        /// </summary>
        /// <param name="dir">方向: 0=正方向, 1=负方向</param>
        public void SetGearMode(short dir = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PrfGear(_core, _axis, dir), "GTN_PrfGear");
            }
        }

        #endregion

        #region 主轴设置

        /// <summary>
        /// 设置电子齿轮运动跟随主轴（默认跟随主轴规划位置）
        /// </summary>
        /// <param name="masterIndex">主轴轴号</param>
        /// <param name="masterType">主轴类型: 1=编码器, 2=规划位置, 3=轴</param>
        /// <param name="masterItem">主轴项（默认0）</param>
        public void SetMaster(short masterIndex, short masterType = GEAR_MASTER_AXIS, short masterItem = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_SetGearMaster(_core, _axis, masterIndex, masterType, masterItem),
                    "GTN_SetGearMaster");
            }
        }

        /// <summary>
        /// 设置主轴为编码器类型
        /// </summary>
        /// <param name="masterIndex">主轴轴号</param>
        public void SetMasterAsEncoder(short masterIndex)
        {
            SetMaster(masterIndex, GEAR_MASTER_ENCODER);
        }

        /// <summary>
        /// 设置主轴为规划位置类型
        /// </summary>
        /// <param name="masterIndex">主轴轴号</param>
        public void SetMasterAsProfile(short masterIndex)
        {
            SetMaster(masterIndex, GEAR_MASTER_PROFILE);
        }

        /// <summary>
        /// 设置主轴为轴类型
        /// </summary>
        /// <param name="masterIndex">主轴轴号</param>
        public void SetMasterAsAxis(short masterIndex)
        {
            SetMaster(masterIndex, GEAR_MASTER_AXIS);
        }

        /// <summary>
        /// 读取电子齿轮运动跟随主轴
        /// </summary>
        /// <param name="masterIndex">主轴轴号</param>
        /// <param name="masterType">主轴类型</param>
        /// <param name="masterItem">主轴项</param>
        public void GetMaster(out short masterIndex, out short masterType, out short masterItem)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_GetGearMaster(_core, _axis, out masterIndex, out masterType, out masterItem),
                    "GTN_GetGearMaster");
            }
        }

        #endregion

        #region 传动比设置

        /// <summary>
        /// 设置电子齿轮比和离合区
        /// </summary>
        /// <param name="masterEven">主轴脉冲数（分子）</param>
        /// <param name="slaveEven">从轴脉冲数（分母）</param>
        /// <param name="masterSlope">离合区位移（pulse），越大变化越平稳</param>
        /// <remarks>
        /// 传动比 = masterEven : slaveEven
        /// 例如 masterEven=2, slaveEven=1 表示主轴走2个脉冲，从轴走1个脉冲
        /// masterSlope 离合区越大，从轴传动比的变化过程越平稳
        /// </remarks>
        public void SetRatio(int masterEven, int slaveEven, int masterSlope = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_SetGearRatio(_core, _axis, masterEven, slaveEven, masterSlope),
                    "GTN_SetGearRatio");
            }
        }

        /// <summary>
        /// 读取电子齿轮比和离合区
        /// </summary>
        /// <param name="masterEven">主轴脉冲数</param>
        /// <param name="slaveEven">从轴脉冲数</param>
        /// <param name="masterSlope">离合区位移</param>
        public void GetRatio(out int masterEven, out int slaveEven, out int masterSlope)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_GetGearRatio(_core, _axis, out masterEven, out slaveEven, out masterSlope),
                    "GTN_GetGearRatio");
            }
        }

        /// <summary>
        /// 设置1:1传动比（无离合区）
        /// </summary>
        public void SetRatioOneToOne()
        {
            SetRatio(1, 1, 0);
        }

        /// <summary>
        /// 设置减速传动比（主轴快，从轴慢）
        /// </summary>
        /// <param name="ratio">减速比，如2表示主轴:从轴=2:1</param>
        /// <param name="masterSlope">离合区位移</param>
        public void SetReductionRatio(int ratio, int masterSlope = 0)
        {
            SetRatio(ratio, 1, masterSlope);
        }

        /// <summary>
        /// 设置增速传动比（主轴慢，从轴快）
        /// </summary>
        /// <param name="ratio">增速比，如2表示主轴:从轴=1:2</param>
        /// <param name="masterSlope">离合区位移</param>
        public void SetSpeedUpRatio(int ratio, int masterSlope = 0)
        {
            SetRatio(1, ratio, masterSlope);
        }

        #endregion

        #region 启动与停止

        /// <summary>
        /// 启动电子齿轮运动
        /// </summary>
        /// <param name="mask">轴掩码，默认启动当前从轴</param>
        public void Start(int mask = 0)
        {
            lock (_lock)
            {
                if (mask == 0)
                    mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_GearStart(_core, mask), "GTN_GearStart");
            }
        }

        /// <summary>
        /// 停止电子齿轮运动（平滑停止）
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_Stop(_core, 1 << (_axis - 1), 0), "GTN_Stop");
            }
        }

        /// <summary>
        /// 急停电子齿轮运动
        /// </summary>
        public void EmergencyStop()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_Stop(_core, 1 << (_axis - 1), 1 << (_axis - 1)), "GTN_Stop");
            }
        }

        #endregion

        #region 事件设置

        /// <summary>
        /// 设置电子齿轮事件
        /// </summary>
        /// <param name="gearEvent">事件类型: 1=启动, 2=通过, 5=区域</param>
        /// <param name="startPara0">参数0</param>
        /// <param name="startPara1">参数1</param>
        public void SetEvent(short gearEvent, int startPara0, int startPara1)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_SetGearEvent(_core, _axis, gearEvent, startPara0, startPara1),
                    "GTN_SetGearEvent");
            }
        }

        /// <summary>
        /// 读取电子齿轮事件
        /// </summary>
        /// <param name="pEvent">事件类型</param>
        /// <param name="pStartPara0">参数0</param>
        /// <param name="pStartPara1">参数1</param>
        public void GetEvent(out short pEvent, out int pStartPara0, out int pStartPara1)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_GetGearEvent(_core, _axis, out pEvent, out pStartPara0, out pStartPara1),
                    "GTN_GetGearEvent");
            }
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 一键配置并启动电子齿轮跟随
        /// </summary>
        /// <param name="masterAxis">主轴轴号</param>
        /// <param name="masterEven">主轴脉冲数（分子）</param>
        /// <param name="slaveEven">从轴脉冲数（分母）</param>
        /// <param name="masterSlope">离合区位移</param>
        /// <param name="dir">方向: 0=正方向, 1=负方向</param>
        /// <remarks>
        /// 完整流程：设置Gear模式 → 设置主轴 → 设置传动比 → 启动
        /// </remarks>
        public void QuickStart(short masterAxis, int masterEven, int slaveEven, int masterSlope = 0, short dir = 0)
        {
            lock (_lock)
            {
                // 1. 设置从轴为Gear模式
                GtnErrorHelper.ThrowIfError(GTN_PrfGear(_core, _axis, dir), "GTN_PrfGear");

                // 2. 设置主轴（默认跟随主轴规划位置）
                GtnErrorHelper.ThrowIfError(
                    GTN_SetGearMaster(_core, _axis, masterAxis, GEAR_MASTER_AXIS, 0),
                    "GTN_SetGearMaster");

                // 3. 设置传动比和离合区
                GtnErrorHelper.ThrowIfError(
                    GTN_SetGearRatio(_core, _axis, masterEven, slaveEven, masterSlope),
                    "GTN_SetGearRatio");

                // 4. 启动从轴
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_GearStart(_core, mask), "GTN_GearStart");
            }
        }

        /// <summary>
        /// 一键配置并启动电子齿轮跟随（使用编码器作为主轴）
        /// </summary>
        /// <param name="masterAxis">主轴轴号</param>
        /// <param name="masterEven">主轴脉冲数（分子）</param>
        /// <param name="slaveEven">从轴脉冲数（分母）</param>
        /// <param name="masterSlope">离合区位移</param>
        /// <param name="dir">方向: 0=正方向, 1=负方向</param>
        public void QuickStartWithEncoder(short masterAxis, int masterEven, int slaveEven, int masterSlope = 0, short dir = 0)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_PrfGear(_core, _axis, dir), "GTN_PrfGear");
                GtnErrorHelper.ThrowIfError(
                    GTN_SetGearMaster(_core, _axis, masterAxis, GEAR_MASTER_ENCODER, 0),
                    "GTN_SetGearMaster");
                GtnErrorHelper.ThrowIfError(
                    GTN_SetGearRatio(_core, _axis, masterEven, slaveEven, masterSlope),
                    "GTN_SetGearRatio");
                int mask = 1 << (_axis - 1);
                GtnErrorHelper.ThrowIfError(GTN_GearStart(_core, mask), "GTN_GearStart");
            }
        }

        #endregion
    }
}