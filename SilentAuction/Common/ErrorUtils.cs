using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SilentAuction.Common
{
    public static class ErrorUtils
    {
        public static string GenerateErrorMessage(Consts.ErrorCode errorCode, string detailedMessage)
        {
            StringBuilder sb = new StringBuilder(1000);
            sb.AppendLine(string.Format(Resources.Errors.ErrorOccurred, Consts.ErrorCode.LotLockFailed.ToString()));
            sb.AppendLine(detailedMessage);
            sb.AppendLine(Resources.Errors.PleaseNotifyAuctionManager);
            return sb.ToString();
        }

        public static string GetUnknownErrorMessage()
        {
            string message = Resources.Errors.UnknownError + Resources.Errors.PleaseNotifyAuctionManager;
            return message;
        }

    }
}