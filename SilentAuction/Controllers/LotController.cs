using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SilentAuction.Controllers
{
    public class LotController : Controller
    {

        // GET: Lot/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // GET: Lot/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Lot/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
