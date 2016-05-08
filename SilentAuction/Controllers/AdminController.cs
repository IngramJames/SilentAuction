using Microsoft.AspNet.Identity;
using SilentAuction.Caches;
using SilentAuction.Consts;
using SilentAuction.Common;
using SilentAuction.Models;
using SilentAuction.Persist;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;

namespace SilentAuction.Controllers
{
	[Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        /// <summary>
        /// Index page GET
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            AppLog.Debug("GET Admin/Index");
            AppLog.Debug("Return View");
            return View();
        }

        /// <summary>
        /// Start auction confirmation view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AuctionStart(int id)
        {
            AppLog.Debug(string.Format("GET Admin/AuctionStart/{0}", id));
            // return view of complete auction
            ApplicationDbContext db = new ApplicationDbContext();
            Auction auction = db.Auctions.Include("Lots.ImageFiles.ImageFile").First(a => a.AuctionId == id);

            AppLog.Debug("Return View");
            return View(auction);
        }

        /// <summary>
        /// Start auction
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AuctionStart(int id, FormCollection collection)
        {
            AppLog.Debug(string.Format("POST Admin/AuctionStart/{0}", id));

            // update auction status
            string userId = User.Identity.GetUserId();
            PersistAuction persistAuction = new PersistAuction();
            persistAuction.UpdateStatus(userId, id, AuctionStatus.Running);

            AppLog.Debug("Redirect to Auction List");
            return RedirectToAction("AuctionList");
        }

        /// <summary>
        /// Reopen auction confirmation view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AuctionReopen(int id)
        {
            AppLog.Debug(string.Format("GET Admin/AuctionRepoen/{0}", id));

            // get auction and lots
            ApplicationDbContext db = new ApplicationDbContext();
            Auction auction = db.Auctions.Include("Lots.ImageFiles.ImageFile").First(a => a.AuctionId == id);

            AppLog.Debug("Return View");
            return View(auction);
        }


        /// <summary>
        /// Reopen auction
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AuctionReopen(int id, FormCollection collection)
        {
            AppLog.Debug(string.Format("POST Admin/Reopen/{0}", id));

            // update auction status
            string userId = User.Identity.GetUserId();
            PersistAuction persistAuction = new PersistAuction();
            persistAuction.UpdateStatus(userId, id, AuctionStatus.Running);

            AppLog.Debug("Redirect to Auction List");
            return RedirectToAction("AuctionList");
        }

        /// <summary>
        /// Pause auction confirmation view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AuctionPause(int id)
        {
            AppLog.Debug(string.Format("GET Admin/AuctionPause/{0}", id));

            // get auction and lots
            ApplicationDbContext db = new ApplicationDbContext();
            Auction auction = db.Auctions.Include("Lots.ImageFiles.ImageFile").First(a => a.AuctionId == id);

            AppLog.Debug("Return View");
            return View(auction);
        }


        /// <summary>
        /// Pause auction
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AuctionPause(int id, FormCollection collection)
        {
            AppLog.Debug(string.Format("POST Admin/AuctionPause/{0}", id));

            // update auction status
            string userId = User.Identity.GetUserId();
            PersistAuction persistAuction = new PersistAuction();
            persistAuction.UpdateStatus(userId, id, AuctionStatus.Paused);

            AppLog.Debug("Redirect to Auction List");
            return RedirectToAction("AuctionList");
        }


        /// <summary>
        /// Close auction confirmation view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AuctionClose(int id)
        {
            AppLog.Debug(string.Format("GET Admin/AuctionClose/{0}", id));

            // get auction and lots
            ApplicationDbContext db = new ApplicationDbContext();
            Auction auction = db.Auctions.Include("Lots.ImageFiles.ImageFile").First(a => a.AuctionId == id);

            AppLog.Debug("Return View");
            return View(auction);
        }

        
        /// <summary>
        /// Close auction
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AuctionClose(int id, FormCollection collection)
        {
            AppLog.Debug(string.Format("POST Admin/AuctionClose/{0}", id));

            // update auction status
            string userId = User.Identity.GetUserId();
            PersistAuction persistAuction = new PersistAuction();
            persistAuction.UpdateStatus(userId, id, AuctionStatus.Closed);

            AppLog.Debug("Redirect to Auction List");
            return RedirectToAction("AuctionList");
        }

        /// <summary>
        /// System settings GET
        /// </summary>
        /// <returns></returns>
        public ActionResult SystemSettings()
        {
            AppLog.Debug("GET Admin/SystemSettings");

            ConfigurationParameters configParams = new ConfigurationParameters();
            Dictionary<ParameterCategory, Dictionary<string, ConfigurationParameter>> paramsByCategory = configParams.GetSystemParametersByCategory();

            AppLog.Debug("Return View");
            return View(paramsByCategory);
        }

        /// <summary>
        /// Edit system setting view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SystemSettingEdit(int id)
        {
            AppLog.Debug(string.Format("GET Admin/SystemSettingEdit", id));

            ConfigurationParameters configParams = new ConfigurationParameters();
            ConfigurationParameter param = configParams.GetParameterById(id);
            ViewBag.Parameter = param;

            AppLog.Debug("Return View");
            return View(param);
        }

        /// <summary>
        /// Edit system setting
        /// </summary>
        /// <param name="id"></param>
        /// <param name="formValues"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SystemSettingEdit(int id, FormCollection formValues)
        {
            string methodName = string.Format("POST Admin/SystemSettingEdit/{0}", id);
            AppLog.Debug(methodName);

            if (ModelState.IsValid)
            {
                // get parameter
                ApplicationDbContext db = new ApplicationDbContext();
                ConfigurationParameter param = db.SystemParameters.First(l => l.Id == id);

                // update and save
                UpdateModel(param);
                db.SaveChanges();

                AppLog.Debug("Redirect to SystemSettingEdit");
                return RedirectToAction("SystemSettings");
            }
            else
            {
                AppLog.Error(methodName + " Bad model: Returning View");
                return View();
            }
        }



        /// <summary>
        /// Lot list
        /// </summary>
        /// <returns></returns>
        public ActionResult LotList()
		{
            AppLog.Debug("GET Admin/LotList");

            // get lots
            ApplicationDbContext db = new ApplicationDbContext();
			var lots = db.Lots.Include("ImageFiles.ImageFile").ToList();

            AppLog.Debug("Return View");
            return View(db.Lots);
		}

		/// <summary>
        /// Edit lot view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public ActionResult LotEdit(int id)
		{
            AppLog.Debug(string.Format("GET Admin/LotEdit/{0}", id));

            // get lot to edit
            ApplicationDbContext db = new ApplicationDbContext();
            Lot lot = db.Lots.Include("ImageFiles.ImageFile").First(l => l.LotId == id);

            // sort images into correct order
            List<LotImageFile> imageFiles = LotImageFile.SortByOrder(lot.ImageFiles);
            lot.ImageFiles = imageFiles;

            AppLog.Debug("Return View");
            return View(lot);
		}

        /// <summary>
        /// Edit lot
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
		public ActionResult LotEdit(int id, FormCollection collection)
		{
            string methodName = string.Format("POST Admin/LotEdit/{0}", id);
            AppLog.Debug(methodName);

            if (ModelState.IsValid)
			{
                // get lot
				ApplicationDbContext db = new ApplicationDbContext();
                Lot lot = db.Lots.First(l => l.LotId == id);

                // update and save
                UpdateModel(lot);
				db.SaveChanges();

                AppLog.Debug("Redirect to LotList");
                return RedirectToAction("LotList");
			}
			else
			{
                AppLog.Error(methodName + " INVALID MODEL. Redirect back to view");
				return View();
			}
		}

        /// <summary>
        /// Fonts and colors view
        /// TODO: use American spelling
        /// </summary>
        /// <returns></returns>
        public ActionResult FontsAndColours()
        {
            AppLog.Debug("GET Admin/FontsAndColours");
            AppLog.Debug("Return View");
            return View();
        }

        /// <summary>
        /// Fonts and colors
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FontsAndColours(FormCollection collection)
        {
            AppLog.Debug("POST Admin/FontsAndColours");
            
            // TODO: check to see if I need any code for authenication cookie everywhere
            string fontTitle = collection["titleFont"];
            string fontBody = collection["defaultFont"];

            // get font parameters
            ApplicationDbContext db = new ApplicationDbContext();
            ConfigurationParameter paramBody = db.SystemParameters.First(l => l.Key == Consts.ConfigurationParameterConsts.ParamName_DefaultFontBody);
            ConfigurationParameter paramTitle = db.SystemParameters.First(l => l.Key == Consts.ConfigurationParameterConsts.ParamName_DefaultFontTitle);

            // update and save
            paramBody.SettingAsString = fontBody;
            paramTitle.SettingAsString = fontTitle;
            db.SaveChanges();

            AppLog.Debug("Return view");
            return View();
        }


        /// <summary>
        /// Create lot view
        /// </summary>
        /// <returns></returns>
        public ActionResult LotCreate()
		{
            AppLog.Debug("GET Admin/LotCreate");
            AppLog.Debug("Return View");
            return View();
		}

		/// <summary>
        /// Create lot
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
		[HttpPost]
		public ActionResult LotCreate(FormCollection collection)
		{
            string methodName = "POST Admin/LotCreate";
            AppLog.Debug(methodName);

            if (ModelState.IsValid)
			{
                // TODO: extract to persistence layer
                // begin transaction
				ApplicationDbContext db = new ApplicationDbContext();
                DbContextTransaction trans = db.Database.BeginTransaction();

                // Save image files to uploads folder
                PersistImageFiles persistImageFiles = new PersistImageFiles();
				List<ImageFile> files = persistImageFiles.SaveUploadedFiles(Request, Server.MapPath(@"\"));

				// Get lot data
				string name = Request.Form["Name"].ToString();
				string description = Request.Form["Description"].ToString();
				decimal reserve = decimal.Parse(Request.Form["Reserve"].ToString());

				// Save lot
				Lot lot = new Lot(name, description, reserve);
				PersistLot persistLot = new PersistLot();
				persistLot.Create(db, lot, files);

                // commit trans
				trans.Commit();

                // Note: does not redirect if javascript POST occurred (ie DropZone got us here).
                // That redirect has to be handled in the dropzone code on the successmultiple event.
                AppLog.Debug("Redirect to LotList");
                return RedirectToAction("LotList");
			}
			else
			{
                AppLog.Error(methodName + "Invalid Model.");
				return View();
			}
		}


        /// <summary>
        /// Edit lot images view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult LotImagesEdit(int id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            Lot lot = db.Lots.Include("ImageFiles.ImageFile").First(l => l.LotId == id);

            // sort images into correct order
            List<LotImageFile> imageFiles = LotImageFile.SortByOrder(lot.ImageFiles);
            lot.ImageFiles = imageFiles;

            AppLog.Debug("Return View");
            return View(lot);
        }

        /// <summary>
        /// Edit lot images
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LotImagesEdit(int id, FormCollection collection)
        {
            string methodName = string.Format("POST Admin/LotImagesEdit/{0}", id);
            AppLog.Debug(methodName);

            if (ModelState.IsValid)
            {
                // TODO: move to persist layer
                // get lot
                ApplicationDbContext db = new ApplicationDbContext();
                Lot lot = db.Lots.Include("ImageFiles.ImageFile").First(l => l.LotId == id);

                string orderList = collection["imageOrder"];
                int[] imageOrder = GetIntsFromStringList(orderList, ",");
                List<LotImageFile> updatedImageList = new List<LotImageFile>();

                // set image orders for existing images
                int index=0;
                foreach(int imageId in imageOrder)
                {
                    // get image file specified by order
                    LotImageFile lif = new LotImageFile();
                    lif.LotId = id;
                    lif.ImageFileId = imageId;
                    lif.Order = index;
                    updatedImageList.Add(lif);
                    index++;
                }

                // Save image files to uploads folder
                PersistImageFiles persistImageFiles = new PersistImageFiles();
                List<ImageFile> newFiles = persistImageFiles.SaveUploadedFiles(Request, Server.MapPath(@"\"));

                // begin trans
                DbContextTransaction trans = db.Database.BeginTransaction();
                persistImageFiles.ImagesSaveForLot(db, newFiles, lot, false);

                // add new images to end of the existing image list
                foreach (ImageFile imageFile in newFiles)
                {
                    LotImageFile lif = new LotImageFile();
                    lif.LotId = id;
                    lif.ImageFileId = imageFile.ImageFileId;
                    lif.Order = index;
                    updatedImageList.Add(lif);
                    index++;
                }

                // remove all old connections
                while(lot.ImageFiles.Count > 0)
                {
                    lot.ImageFiles.Remove(lot.ImageFiles.First());
                }

                // save data so far to generate IDs
                db.SaveChanges();

                // link up image files
                foreach (LotImageFile lotImageFile in updatedImageList)
                {
                    lot.ImageFiles.Add(lotImageFile);
                }

                // save and commit
                db.SaveChanges();
                trans.Commit();

                // update in the cache
                SystemCacheManager scm = SystemCacheManager.GetCache();
                scm.RefreshLot(id);

                string toUrl = "LotEdit/" + id.ToString();
                AppLog.Debug("Redirect to " + toUrl);
                return RedirectToAction(toUrl);
            }
            else
            {
                return View();
            }
        }

        /// <summary>
        /// Passed a delimited string with numbers, extracts the int values into an int array.
        /// Invalid numbers are returned as zero in the array.
        /// </summary>
        /// <param name="delimitedList">delimited list of numbers</param>
        /// <param name="delimiter">the character used to delimit the numbers</param>
        /// <returns>array of ints</returns>
        private int[] GetIntsFromStringList(string delimitedList, string delimiter)
        {
            string[] imageOrderStrings = delimitedList.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

            // convert strings to ints
            int[] intArray = new int[imageOrderStrings.Length];
            for (int n = 0; n < imageOrderStrings.Length; n++)
            {
                int order = -1;
                int.TryParse(imageOrderStrings[n], out order);
                intArray[n] = order;
            }

            return intArray;
        }


        /// <summary>
        /// Import Lot view
        /// </summary>
        /// <returns></returns>
        public ActionResult LotImport()
        {
            AppLog.Debug("GET Admin/LotImport");

            AppLog.Debug("Return View");
            return View();
        }


        /// <summary>
        /// Import lots which were dragged onto a page.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LotImport(FormCollection collection)
        {
            AppLog.Debug("POST Admin/LotImport");
            PersistLotImport importer = new PersistLotImport();

            // Parse files
            Dictionary<string, ImportFile> importFiles = importer.ImportFromRequest(Request);

            // get summary of good lots for returning Json
            string[,] importedLots = importer.GetGoodLotsSummary(importFiles);

            // handle errors which were found during import
            if (importer.ErrorCount > 0)
            {
                // error occurred.
                // return Json errors and good data, to give meaningful feedback to user
                string[,] errorArray = importer.GetAllErrors();
                return Json(new { error = errorArray, goodLots = importedLots });
            }

            ApplicationDbContext dbContext = new ApplicationDbContext();

            // get good lots to save
            List<Lot> lots = importer.GetLotsForSaving(importFiles);

            // save
            PersistLot persister = new PersistLot();
            persister.Create(dbContext, lots);

            AppLog.Debug("returning Json imported lots");
            return Json(new { goodLots = importedLots });
        }



        /// <summary>
        /// Create auction view
        /// </summary>
        /// <returns></returns>
        public ActionResult AuctionCreate()
		{
            AppLog.Debug("GET Admin/AuctionCreate");
            AppLog.Debug("Return View");
            return View();
		}

		/// <summary>
        /// Create auction
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
		[HttpPost]
		public ActionResult AuctionCreate(FormCollection collection)
		{
            string methodName = "POST Admin/AuctionCreate";
            AppLog.Debug(methodName);
            if (ModelState.IsValid)
			{
                // get auction from form data
                Auction auction = new Auction();
                auction.Name = collection[AuctionConsts.AuctionField_Name];
                auction.Description = collection[AuctionConsts.AuctionField_Description];
                auction.UseReserves = Utils.GetCheckboxValue(collection[AuctionConsts.AuctionField_UseReserves]);

                // Save image files to uploads folder
                PersistImageFiles persistImageFiles = new PersistImageFiles();
                List<ImageFile> imageFiles = persistImageFiles.SaveUploadedFiles(Request, Server.MapPath(@"\"));

                // save actual auction to DB
                PersistAuction persistAuction = new PersistAuction();
                persistAuction.Create(auction, imageFiles);

                AppLog.Debug("Redirect to AuctionList");
                return RedirectToAction("AuctionList");
			}
			else
			{
                AppLog.Error(methodName + " Model Invalid");
				return View();
			}
		}


        /// <summary>
        /// Auction list view
        /// </summary>
        /// <returns></returns>
        public ActionResult AuctionList()
		{
            AppLog.Debug("GET Admin/AuctionList");

            // get auctions
            ApplicationDbContext db = new ApplicationDbContext();
            var auctions = db.Auctions.Include("Image");

            AppLog.Debug("Return View");
            return View(auctions);
		}


		/// <summary>
        /// Edit auction view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public ActionResult AuctionEdit(int id)
		{
            AppLog.Debug(string.Format("GET Admin/AuctionEdit/{0}", id));

            // Get all lots used by Auction
            ApplicationDbContext db = new ApplicationDbContext();
            Auction auction = db.Auctions.Include("Lots.ImageFiles.ImageFile").First(a => a.AuctionId == id);

            // Get lots in the order they appear in the auction
            List<Lot> auctionLots = Lot.SortLotsByAuctionOrder(auction.Lots);
            auction.Lots = auctionLots;

            // get unused lots
            // TODO: shouldn't this populate images too?
            IEnumerable<Lot> unusedLots = db.Lots.SqlQuery("LotsGetUnused");

            // pass free lots to the view, to allow their selection
            ViewBag.AvailableLots = unusedLots.ToList();

            AppLog.Debug("Return View");
            return View(auction);
		}

		/// <summary>
        /// Auction edit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
		[HttpPost]
		public ActionResult AuctionEdit(int id, FormCollection collection)
		{
            string methodName = string.Format("POST Admin/AuctionEdit/{0}", id);
            AppLog.Debug(methodName);
            if (ModelState.IsValid)
			{
                // extract and validate form data
                string lotOrderString = collection["lotOrder"];
                string auctionName = collection[AuctionConsts.AuctionField_Name];
                string auctionDescription = collection[AuctionConsts.AuctionField_Description];
                bool useReserves = Utils.GetCheckboxValue(collection[AuctionConsts.AuctionField_UseReserves]);
                int[] lotOrder = GetIntsFromStringList(lotOrderString, ",");
                string userId = User.Identity.GetUserId();

                // update the auction
                PersistAuction persistAuction = new PersistAuction();
                persistAuction.UpdateWithLots(userId, id, auctionName, auctionDescription, useReserves,lotOrder);

                AppLog.Debug("Redirecting to AuctionList");
                return RedirectToAction("AuctionList");
			}
			else
			{
                AppLog.Error(methodName + " Invalid model");
				return View();
			}
		}

    }
}
