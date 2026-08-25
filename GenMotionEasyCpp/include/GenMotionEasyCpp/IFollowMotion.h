#pragma once

class IFollowMotion {
public:
    virtual void SetFollowMaster(short index, short type, short item) = 0;
    virtual void FollowData(long masterSegment, double slaveSegment, short type, short fifo) = 0;
    virtual short GetFollowSpace(short fifo) = 0;
    virtual void ClearFollow(short fifo) = 0;
    virtual void StartFollow(long mask, long option) = 0;
    virtual void SwitchFollow(long mask) = 0;
    virtual void SetFollowEvent(short event, short dir, long pos) = 0;
    virtual void SetLoop(long loop) = 0;
    virtual void GetLoop(long* pLoop) = 0;
    virtual ~IFollowMotion() = default;
};
