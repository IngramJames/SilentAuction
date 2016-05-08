using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SilentAuction.Common
{
    public class LogData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int? ObjectId { get; set; }
        public string Text { get; set; }
        public LogLevel Level { get; set; }

        public LogData(LogLevel level, string text, int? objectId)
        {
            Level = level;
            Text = text;
            ObjectId = objectId;

            UserId = HttpContext.Current.User.Identity.GetUserId();
            UserName = HttpContext.Current.User.Identity.Name;
        }

        public LogData(LogLevel level, string text) 
            : this(level, text, null)
        { 
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(256);
            sb.AppendFormat("{0} {1}: {2}", UserName, Level.ToString(), Text);
            if(ObjectId != null)
            {
                sb.AppendFormat(" objectId={0}", ObjectId);
            }

            return sb.ToString();
        }
    }
}