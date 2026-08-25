using System;
using System.Collections.Generic;

namespace GsnMotionEasy.Tool
{
    public static class GtnErrorHelper
    {

        private static readonly Dictionary<short, string> ErrorMessages = new Dictionary<short, string>
        {
            { 0, "指令执行成功" },
            { 1,"指令执行错误"},
            { 2,"license不支持" },
            {7,"参数错误" },
            {8,"固件不支持该指令" },
            {-1,"主机和运动控制器通讯失败" },
            {-6,"打开控制器失败" },
            {-7,"运动控制器没有响应" },
            {-8,"多线程资源忙" },
            {-10,"主机和运动控制器通讯失败" },
            {-13,"编码器初始化失败"},
            {-14,"编码器初始化失败" },
            {-15,"动态库版本不匹配" },
            {15,"动态库版本不匹配" },
            {16,"不具备版本匹配功能" },
            {11007,"指令流为空" },
            {11020,"Group没有关闭，无法设置当量" },
            {11021,"group处于运动状态" },
            {11703,"请确认LAFunc.dll版本是否匹配" },
            {11802,"位置参数错误，输入的终点位置和起点位置为相同点" },
        };


        public static string GetErrorMessage(short errorCode)
        {
            return ErrorMessages.TryGetValue(errorCode, out var msg) ? msg : $"未知错误码 {errorCode}";
        }

        public static void ThrowIfError(short errorCode, string operation)
        {
            if (errorCode != 0)
            {
                throw new GtnException($"固高卡操作失败 [{operation}] : {GetErrorMessage(errorCode)}", errorCode);
            }
        }
    }

    public class GtnException : Exception
    {
        public short ErrorCode { get; }
        public GtnException(string message, short errorCode) : base(message) => ErrorCode = errorCode;
    }
}
