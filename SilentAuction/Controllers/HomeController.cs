using SilentAuction.Common;
using SilentAuction.Caches;
using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SilentAuction.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
            AppLog.Debug("Get Home/Index");

            SystemCacheManager systemCache = SystemCacheManager.GetCache();
            SortedDictionary<string, Auction> runningAuctions = systemCache.AuctionsGetRunning();
            SortedDictionary<string, Auction> closedAuctions = systemCache.AuctionsGetClosed();

            ViewBag.RunningAuctions = runningAuctions;
            ViewBag.ClosedAuctions = closedAuctions;

            AppLog.Debug("return view");
            return View();
		}

		public ActionResult About()
		{
            AppLog.Debug("Get Home/About");
            AppLog.Debug("return view");
            return View();
		}

		public ActionResult Contact()
		{
            AppLog.Debug("Get Home/Contact");
            AppLog.Debug("return view");
            return View();
		}

        public ActionResult Credits()
        {
            AppLog.Debug("Get Home/Credits");
            AppLog.Debug("return view");
            return View();
        }
	}
}