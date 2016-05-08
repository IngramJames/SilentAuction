using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Consts
{
    public enum ErrorCode
    {
        OK = 0,                             // No errors
        UnknownError = 1,                   // unknown error
        LotLockFailed = 100,                // failed to lock a lot
        LotAlreadyLocked = 101,             // Lot already locked by another user
        LotLockStolen = 102,                // tried to unlock a lot which another user has locked
        LotUnlockFailed = 103,              // tried but failed to release a lock
        LotBidTooLow = 104,                 // bid was logged but was too low
        AuctionNotRunning = 105,            // bid was made against an auction which is not running
    }

}