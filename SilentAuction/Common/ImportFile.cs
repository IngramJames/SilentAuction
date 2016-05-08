using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Common
{
    public class ImportFile
    {

        public string Filename { get; set; }
        public HttpPostedFileBase HttpFile { get; set; }
        public bool IsHttp { get; set; }
        public List<Lot> GoodLots { get; set; }
        public int DataRows { get; set; }
    }
}