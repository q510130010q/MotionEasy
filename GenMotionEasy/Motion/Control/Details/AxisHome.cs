using GenMotionEasy.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GTN.glink;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{

    /// <summary>
    /// 驱动器回零
    /// </summary>
    public class AxisHome : IAxisHome
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;
        private bool _homingTag = false;

        public AxisHome(short core, short axis,object lockObj) 
        {
            this._core = core;
            this._axis = axis;
            this._lock = lockObj;
        }

        /// <summary>
        /// 驱动回零
        /// </summary>
        /// <param name="method">回零方式</param>
        /// <param name="switchSpeed">搜索开关速度</param>
        /// <param name="indexSpeed">搜索index速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="offset">偏移量</param>
        /// <param name="probeFunction"></param>
        /// <param name="DoneMinutes">回零完成等待时间（分钟）</param>
        /// <param name="isHomeDone">是否检查回零完成</param>
        public async Task Home(double switchSpeed, double indexSpeed, double acc, short method, int offset = 0, ushort probeFunction = 0,  bool isHomeDone = false,int DoneMinutes = 10)
        {

            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_SetHomingMode(_core, _axis, 6), "GTN_SetHomingMode");

                GtnErrorHelper.ThrowIfError(GTN_SetEcatHomingPrm(_core, _axis, method, switchSpeed, indexSpeed, acc, offset, probeFunction), "GTN_SetEcatHomingPrm");
                
                GtnErrorHelper.ThrowIfError(GTN_StartEcatHoming(_core, _axis), "GTN_StartEcatHoming");

            }
            if (isHomeDone)
            {
                await HomeDone(DoneMinutes);
            }

        }
        //回零完后清除回零状态
        /// <summary>
        /// 回零完成
        /// </summary>
        /// <param name="DoneMinutes">回零完成等待时间（分钟）</param>
        /// <returns></returns>
        public async Task<bool> HomeDone(int DoneMinutes)
        {
            using (var cts = new CancellationTokenSource(new TimeSpan(0, DoneMinutes, 0)))
            {
                try
                {
                    _homingTag = false;
                    while (!cts.IsCancellationRequested)
                    {
                        ushort homingStatus;
                        short rtn = GTN_GetEcatHomingStatus(_core, _axis, out homingStatus);
                        if (homingStatus == 3)
                        {
                            lock (_lock)
                            {
                                GtnErrorHelper.ThrowIfError(GTN_SetHomingMode(_core, _axis, 8), "GTN_SetHomingMode");
                                GtnErrorHelper.ThrowIfError(GTN_ZeroPos(_core, _axis, 1), "GTN_ZeroPos");
                                GtnErrorHelper.ThrowIfError(GTN_ClrSts(_core, _axis, 1), "GTN_ClrSts");
                                _homingTag = true;
                                ClearHoming();
                            }
                            cts.Cancel();
                        }
                        await Task.Delay(200, cts.Token);
                    }
                    return _homingTag;
                }
                catch (TaskCanceledException)
                {
                    return _homingTag;
                }
                catch (Exception)
                {
                    return _homingTag;
                }

            }
        }
        /// <summary>
        /// 获取回零状态
        /// </summary>
        /// <returns>true成功</returns>
        public bool GetHomeStatus()
        {
            return _homingTag;
        }

        /// <summary>
        /// 重置回零状态
        /// </summary>
        public void ResetHomeStatus()
        {
            _homingTag = false;
        }

        private async Task ClearHoming()
        {
            await Task.Delay(1000);
            GtnErrorHelper.ThrowIfError(GTN_ZeroPos(_core, _axis, 1), "GTN_ZeroPos");
        }

    }
}
