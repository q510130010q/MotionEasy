using System;
using GTN;
using static GTN.mc;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// IGroupSetup 实现。对应 Demo Form1.cs button3_Click 的 Group 建立部分（行 454-501）与 button11_Click 的解散部分（行 1955-1959）。
    /// </summary>
    public class GroupSetup : IGroupSetup
    {
        private readonly GroupContext _ctx;

        public GroupSetup(GroupContext ctx) => _ctx = ctx;

        public void CreateGroup(FiveAxisConfig config)
        {
            var list0 = _ctx.CreateListInfo();
            short rtn;

            rtn = mc.GTN_GroupDisable(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GroupDisable");

            // 设置脉冲当量（5 轴统一，Group 关闭时才能设置）
            var scale = new TProfileScale
            {
                alpha = new double[4],
                beta = new double[4],
                count = 1
            };
            scale.alpha[0] = config.PulseAlpha;
            scale.beta[0] = config.PulseBeta;

            foreach (var axis in new[] { config.AxisX, config.AxisY, config.AxisZ, config.AxisP, config.AxisS })
            {
                rtn = mc.GTN_SetAxisScale(_ctx.Core, axis, ref scale, ref list0);
                GtnErrorHelper.ThrowIfError(rtn, $"GTN_SetAxisScale(axis={axis})");
            }

            rtn = mc.GTN_GroupDisable(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GroupDisable(before ungroup)");

            rtn = mc.GTN_UngroupAllAxes(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_UngroupAllAxes");

            // 5 轴按顺序加入 Group：X=1, Y=2, Z=3, P=4, S=5
            short indent = 1;
            foreach (var axis in new[] { config.AxisX, config.AxisY, config.AxisZ, config.AxisP, config.AxisS })
            {
                rtn = mc.GTN_AddAxisToGroup(_ctx.Core, _ctx.Group, axis, indent, ref list0);
                GtnErrorHelper.ThrowIfError(rtn, $"GTN_AddAxisToGroup(axis={axis},indent={indent})");
                indent++;
            }
        }

        public void Enable()
        {
            short rtn = mc.GTN_GroupEnable(_ctx.Core, _ctx.Group, IntPtr.Zero);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GroupEnable");
        }

        public void Disable()
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_GroupDisable(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_GroupDisable");
        }

        public void UngroupAll()
        {
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_UngroupAllAxes(_ctx.Core, _ctx.Group, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_UngroupAllAxes");
        }

        public bool IsEnabled
        {
            get
            {
                TGroupStatus status;
                short rtn = mc.GTN_GetGroupStatus(_ctx.Core, _ctx.Group, out status);
                GtnErrorHelper.ThrowIfError(rtn, "GTN_GetGroupStatus");
                return status.state != mc.GROUP_STATE_DISABLED;
            }
        }

        public void SetAxisMotionSmooth(short axis, double time, double k)
        {
            short rtn = mc.GTN_SetAxisMotionSmooth(_ctx.Core, axis, time, k);
            GtnErrorHelper.ThrowIfError(rtn, $"GTN_SetAxisMotionSmooth(axis={axis})");
        }

        public void SetAxisMotionConstraint(short axis, double jerkMax, double decMax, double dvMax, double velMax, double accMax)
        {
            var prm = new TAxisMotionConstraint
            {
                jerkMax = jerkMax,
                decMax = decMax,
                dvMax = dvMax,
                velMax = velMax,
                accMax = accMax,
                reserve1 = new short[3] { 1, 0, 0 },
                reserve2 = new double[8],
            };
            var list0 = _ctx.CreateListInfo();
            short rtn = mc.GTN_SetAxisMotionConstraint(_ctx.Core, axis, ref prm, ref list0);
            GtnErrorHelper.ThrowIfError(rtn, $"GTN_SetAxisMotionConstraint(axis={axis})");
        }
    }
}
