using SilentAuction.Caches;
using SilentAuction.Classes;
using SilentAuction.Common;
using SilentAuction.Consts;
using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using System.Data.Common;
using SilentAuction.Persist;

namespace SilentAuction.Controllers
{
    [Authorize]
    public class AuctionController : Controller
    {
        // GET: Index
        public ActionResult Index(int id)
        {
            SystemCacheManager systemCache = SystemCacheManager.GetCache();
            Auction auction = systemCache.GetAuction(id);

            //todo: Cache(?) turned out quite complex.
            // is it necessary? This is only on a full page refresh.
            // Get the highest bids for the current user
            PersistAuction persistAuction = new PersistAuction();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            string userId = User.Identity.GetUserId();
            Dictionary<int, Bid> highestBidsByUser = persistAuction.GetHighestBidsByUser(dbContext, auction.AuctionId, userId);
            ViewBag.HighestBidsByUser = highestBidsByUser;

            // work out user's total exposure
            decimal totalExposure = 0;
            Dictionary<int, Lot> lotsByKey = auction.LotsByKey();
            foreach (KeyValuePair<int, Bid> kvp in highestBidsByUser)
            {
                Bid bid = kvp.Value;
                if(bid.Status == BidStatus.Open)
                {
                    bool validBid = true;       // optimism

                    // bid was the highest: but is it higher than a reserve?
                    if(auction.UseReserves)
                    {
                        int lotId = bid.LotId;
                        Lot lot = lotsByKey[lotId];
                        if(lot.Reserve > bid.Amount)
                        {
                            validBid = false;
                        }
                    }

                    if (validBid)
                    {
                        totalExposure += bid.Amount;
                    }
                }
            }

            ViewBag.TotalExposure = totalExposure;

            return View(auction);
        }

        // POST: Auction/Bid
        [HttpPost]
        public ActionResult Bid(FormCollection collection)
        {
            string auctionIdString = collection["auctionId"];
            string lotIdString = collection["lotId"];
            string bidString = collection["bid"];

            int lotId = int.Parse(lotIdString);
            decimal bidAmount = decimal.Parse(bidString);

            ConfigurationParameters configParams = new ConfigurationParameters();
            ConfigurationParameter dpParam = configParams.GetParameterByKey(Consts.ConfigurationParameterConsts.ParamName_CurrencyDecimalPlaces);

            int multiplier = 100;
            if(!dpParam.SettingAsBool)
            {
                multiplier = 1;
            }

            // take only the number of decimal places which are allowed
            int amountAdjusted = (int)(bidAmount * multiplier);
            decimal bidAmountAdjusted = (decimal)amountAdjusted / multiplier;

            // check that only a maximum of 2 dps are given
            if(bidAmountAdjusted != bidAmount)
            {
                return Json(new { result = "validationFail", errorMessage = Resources.Errors.DecimalPlacesInvalid });
            }

            string formattedAmount = Formatting.FormatCurrency(bidAmount);

            string userName = User.Identity.Name;
            string userId = User.Identity.GetUserId();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            
            // Log the bid
            Persist.PersistLot persist = new Persist.PersistLot();

            // react to the return status
            ErrorCode status = persist.Bid(lotId, userId, bidAmount, dbContext);
            string errorDetails;
            string errorMessage;
            switch (status)
            {
                // bid was too low
                case ErrorCode.LotBidTooLow:
                    // get this user's highest bid to return to the front-end.
                    PersistAuction persistAuction = new PersistAuction();
                    int auctionId = int.Parse(auctionIdString);
                    Bid highestBidByUser = persistAuction.GetHighestBidByUserForLot(dbContext, auctionId, lotId, userId);
                    string formattedHighestBid = Formatting.FormatCurrency(highestBidByUser.Amount);

                    // TODO: get from cache
                    // XXX: does this wipe the user details on the highest bid, which is why we get callstacks elsewhere sometimes?
                    Lot lot = dbContext.Lots.Include("HighestBid").First(l => l.LotId == lotId);

                    string userControlsLot = "N";     // pessimism
                    if(lot.HighestBid.UserId == userId)
                    {
                        userControlsLot = "Y";
                    }

                    return Json(new { result = "bidTooLow", yourHighestBid = formattedHighestBid, youControlLot = userControlsLot });

                case ErrorCode.AuctionNotRunning:
                    // auction isn't running. Bid cannot be made.
                    return Json(new { result = "auctionNotRunning" });

                // Lot lock failed
                case ErrorCode.LotLockFailed:
                    // This is bad; a previous transaction must have gone wrong somehow, because locks should be released
                    // as soon as a bid completes.
                    // return a serious error.
                    errorDetails = string.Format(Resources.Errors.LotLockFailed, lotIdString, userName);
                    errorMessage = ErrorUtils.GenerateErrorMessage(Consts.ErrorCode.LotLockFailed, errorDetails);
                    return Json(new { result = "error", errorMessage = errorMessage });

                // Lot already locked
                case ErrorCode.LotAlreadyLocked:
                    // This is bad. No locks should persist outside their own transactions.
                    errorDetails = string.Format(Resources.Errors.LotLocked, lotIdString, userName);
                    errorMessage = ErrorUtils.GenerateErrorMessage(Consts.ErrorCode.LotLockFailed, errorDetails);
                    return Json(new { result = "error", errorMessage = errorMessage });
                
                // Lock unlock failed
                case ErrorCode.LotUnlockFailed:
                    // This is bad; nobody can bid on this item now.
                    errorDetails = string.Format(Resources.Errors.LotUnlockFailed, lotIdString, userName);
                    errorMessage = ErrorUtils.GenerateErrorMessage(Consts.ErrorCode.LotLockFailed, errorDetails);
                    return Json(new { result = "error", errorMessage = errorMessage });

                // Lock lot stolen
                case ErrorCode.LotLockStolen:
                    // This is bad. Shouldn't be possible.
                    errorDetails = string.Format(Resources.Errors.LotLockStolen, lotIdString);
                    errorMessage = ErrorUtils.GenerateErrorMessage(Consts.ErrorCode.LotLockFailed, errorDetails);
                    return Json(new { result = "error", errorMessage = errorMessage });
                
                // Unknown error
                case ErrorCode.UnknownError:
                    // This is terrible.
                    return Json(new { result = "error", errorMessage = ErrorUtils.GetUnknownErrorMessage() });
            }


            return Json(new { result = "OK", formattedBid = formattedAmount });
        }

        /// <summary>
        /// Gets the latest highest bids on a specified auction.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetUpdatedBids(int id)
        {
            SystemCacheManager scm = SystemCacheManager.GetCache();
            Auction auction = scm.GetAuction(id);

            List<Lot> lotsWithBids = new List<Lot>();
            foreach(Lot lot in auction.Lots)
            {
                if(lot.HighestBid !=null)
                {
                    lotsWithBids.Add(lot);
                }
            }

            // prepare return data
            string[,] bids = new string[lotsWithBids.Count, 3];
            int n = 0;
            decimal totalForThisUser = 0;
            foreach(Lot lot in lotsWithBids)
            {
                // does this user control this lot?
                string userId = User.Identity.GetUserId();
                bool highestBidCurrentUser = (lot.HighestBid.User.Id == userId);

                bids[n,0] = lot.LotId.ToString();
                bids[n,1] = Formatting.FormatCurrency(lot.HighestBid.Amount);
                bids[n,2] = highestBidCurrentUser.ToString();
                n++;

                if(highestBidCurrentUser)
                {
                    totalForThisUser += lot.HighestBid.Amount;
                }
            }

            return Json(new { bids = bids, status = (int)auction.Status, yourTotalExposure = Formatting.FormatCurrency(totalForThisUser) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResultsByUser(int id)
        {
            PersistAuction persistsAuction = new PersistAuction();
            SortedList<string, UserResults> userResults = persistsAuction.GetResultsByUser(id);

            //TODO: if Reserves are in play, then some lots are simply missing.
            // This looks odd, because the user can see they have the leading bid.
            // It should be clear that the user has NOT won items whose Reserve has not been met.
            // Need to find a way to represent this.

            ViewBag.UserResults = userResults;
            return View();
        }
    }
}
