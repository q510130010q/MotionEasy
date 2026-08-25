using GTN;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Control;
using GsnMotionEasy.Tool;

namespace GsnMotionEasy.Motion
{
    /// <summary>
    /// 五轴管理器入口。负责卡初始化、模式切换（单轴/五轴互斥）、Controller 生命周期。
    /// </summary>
    public class FiveAxisManager
    {
        private readonly GroupContext _ctx;
        public Control.FiveAxisGroupController Controller { get; }

        /// <summary>当前是否处于五轴 Group 模式（与单轴模式互斥）</summary>
        public bool IsGroupActive { get; private set; }

        public FiveAxisManager() : this(new GroupContext())
        {
        }

        public FiveAxisManager(GroupContext ctx)
        {
            _ctx = ctx;
            Controller = new Control.FiveAxisGroupController(ctx);
        }

        /// <summary>打开运动控制器（对应 Demo button1_Click 的 GTN_Open）</summary>
        public void OpenCard(short channelType = 5, short channelParam = 2)
        {
            short rtn = mc.GTN_Open(channelType, channelParam);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_Open");
        }

        /// <summary>复位 + 加载配置（对应 Demo button2_Click）</summary>
        public void LoadConfig(string cfgFile = "gtn_core1.cfg")
        {
            short rtn = mc.GTN_Reset(_ctx.Core);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_Reset");

            rtn = mc.GTN_LoadConfig(_ctx.Core, cfgFile);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_LoadConfig");

            rtn = mc.GTN_ClrSts(_ctx.Core, 1, 24);
            GtnErrorHelper.ThrowIfError(rtn, "GTN_ClrSts");
        }

        /// <summary>
        /// 进入五轴模式。调用此方法后单轴模式不可用，必须通过 Group 接口操作。
        /// 内部委托给 Controller.InitFiveAxis 完成全部 Group 建立。
        /// </summary>
        public void EnterFiveAxis(FiveAxisConfig config)
        {
            if (IsGroupActive) return;
            Controller.InitFiveAxis(config);
            IsGroupActive = true;
        }

        /// <summary>
        /// 退出五轴模式，轴回归单轴模式。对应 Demo button11_Click。
        /// </summary>
        public void ExitFiveAxis()
        {
            if (!IsGroupActive) return;
            Controller.Setup.Disable();
            Controller.Setup.UngroupAll();
            IsGroupActive = false;
        }
    }
}
