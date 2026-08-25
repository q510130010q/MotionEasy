#include "GenMotionEasyCpp/GtnError.h"
#include <unordered_map>

static const std::unordered_map<short, std::string>& GetErrorMessages() {
    static const std::unordered_map<short, std::string> m = {
        {0, "\u6307\u4ee4\u6267\u884c\u6210\u529f"},
        {1, "\u6307\u4ee4\u6267\u884c\u9519\u8bef"},
        {2, "\u8bb8\u53ef\u8bc1\u4e0d\u652f\u6301"},
        {7, "\u53c2\u6570\u9519\u8bef"},
        {8, "\u56fa\u4ef6\u4e0d\u652f\u6301\u8be5\u6307\u4ee4"},
        {-1, "\u4e3b\u673a\u548c\u8fd0\u52a8\u63a7\u5236\u5668\u901a\u8baf\u5931\u8d25"},
        {-6, "\u6253\u5f00\u63a7\u5236\u5668\u5931\u8d25"},
        {-7, "\u8fd0\u52a8\u63a7\u5236\u5668\u6ca1\u6709\u54cd\u5e94"},
        {-8, "\u591a\u7ebf\u7a0b\u8d44\u6e90\u5fd9"},
        {-10, "\u4e3b\u673a\u548c\u8fd0\u52a8\u63a7\u5236\u5668\u901a\u8baf\u5931\u8d25"},
        {-13, "\u7f16\u7801\u5668\u521d\u59cb\u5316\u5931\u8d25"},
        {-14, "\u7f16\u7801\u5668\u521d\u59cb\u5316\u5931\u8d25"},
        {-15, "\u52a8\u6001\u5e93\u7248\u672c\u4e0d\u5339\u914d"},
        {15, "\u52a8\u6001\u5e93\u7248\u672c\u4e0d\u5339\u914d"},
        {16, "\u4e0d\u5177\u5907\u7248\u672c\u5339\u914d\u529f\u80fd"},
    };
    return m;
}

GtnException::GtnException(short code, const std::string& op)
    : std::runtime_error("\u56fa\u9ad8\u5361\u64cd\u4f5c\u5931\u8d25 [" + op + "] : " + GetErrorMessage(code))
    , errorCode(code) {}

std::string GtnException::GetErrorMessage(short code) {
    auto& m = GetErrorMessages();
    auto it = m.find(code);
    if (it != m.end()) return it->second;
    return "\u672a\u77e5\u9519\u8bef\u7801 " + std::to_string(code);
}

void GtnException::ThrowIfError(short code, const std::string& operation) {
    if (code != 0) {
        throw GtnException(code, operation);
    }
}
