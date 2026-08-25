#pragma once

class IJogMotion {
public:
    virtual ~IJogMotion() = default;
    virtual void SetJogMode(double acc, double dec, double smooth = 0) = 0;
    virtual void JogMove(double vel) = 0;
    virtual void Stop() = 0;
};
