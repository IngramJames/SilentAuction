using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Common
{
    public class ImportError
    {
        public string Filename { get; set; }
        public int LineNo { get; set; }
        public string Error { get; set; }

        public ImportError(string filename, string error)
        {
            Filename = filename;
            Error = error;
            LineNo = -1;
        }

        public ImportError(string filename, int lineNo, string error)
        {
            Filename = filename;
            Error = error;
            LineNo = lineNo;
        }
    }
}