////////////////////////////////////////////////////////////////////////////////////////////////////
// NoesisGUI - http://www.noesisengine.com
// Copyright (c) 2013 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

static const uint32_t MPC_MediaLoaded = 0;
static const uint32_t MPC_MediaFailed = 0xffffffff;
static const uint32_t MPC_MediaEnded = 1;
static const uint32_t MPC_NewFrame = 2;
static const uint32_t MPC_Play = 3;
static const uint32_t MPC_Pause = 4;
static const uint32_t MPC_Stop = 5;
static const uint32_t MPC_Seek = 6;
static const uint32_t MPC_Volume = 7;
static const uint32_t MPC_FrameAck = 8;
    
struct MediaPlayerCommand
{
    uint32_t cmd;
    uint64_t arg[2];
};
