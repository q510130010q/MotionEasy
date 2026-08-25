#pragma once
#include <string>
#include <stdexcept>

class GtnException : public std::runtime_error {
public:
    short errorCode;
    explicit GtnException(short code, const std::string& op);
    static void ThrowIfError(short code, const std::string& operation);
    static std::string GetErrorMessage(short code);
};
