using SilentAuction.Common;
using SilentAuction.Consts;
using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace SilentAuction.Caches
{
    /// <summary>
    /// Caching of global data. Implemented as Get* methods instead of properties
    /// to encourage safe coding. Clients of this service should get the cached data once
    /// and then use it. This allows the cache to be updated by replacing the data structure
    /// which is returned for further requests, while clients can continue to use the outdated
    /// data which they first accessed.
    /// </summary>
    public class SystemCacheManager
    {
        private Dictionary<int, Auction> _auctions;
        private Dictionary<int, Lot> _lots;

        /// <summary>
        /// Auctions, Lots, Highest bids.
        /// </summary>
        public Dictionary<int, Auction> GetAuctions()
        {
            // TODO: allow sys param to turn off caching and get from database each time.
            return _auctions;
        }

        public Auction GetAuction(int auctionId)
        {
            return _auctions[auctionId];
        }

        /// <summary>
        /// All lots, and current highest bids.
        /// </summary>
        public Dictionary<int, Lot> GetLots()
        {
            return _lots;
        }

        public Auction GetAuctionForLot(int lotId)
        {
            Lot lot = _lots[lotId];
            if(lot.AuctionId == null)
            {
                return null;
            }

            Auction auction = _auctions[(int)lot.AuctionId];
            return auction;
        }

        public void RefreshAuction(int auctionId)
        {
            Persist.PersistAuction persistAuction = new Persist.PersistAuction();
            Auction auction = persistAuction.AuctionGet(auctionId);
            CreateRelationships(auction);
            if(_auctions.ContainsKey(auctionId))
            {
                _auctions.Remove(auctionId);
            }
            _auctions.Add(auctionId, auction);
        }

        private void RefreshAuctions()
        {
            Persist.PersistAuction persistAuction = new Persist.PersistAuction();
            _auctions = persistAuction.AuctionsGet();
        }

        public void RefreshLots()
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            _lots = dbContext.Lots.Include("ImageFiles.ImageFile").Include("HighestBid").ToDictionary(kvp => kvp.LotId);
        }

        public void RefreshLot(int lotId)
        {
            // get updated lot from DB
            ApplicationDbContext dbContext = new ApplicationDbContext();
            Lot lot = dbContext.Lots.Include("ImageFiles.ImageFile").Include("HighestBid").First(l => l.LotId == lotId);

            if(lot.AuctionId != null)
            {
                int auctionId = (int)lot.AuctionId;
                Auction auction = _auctions[auctionId];

                // remove old lot. Clucky because this is a collection
                Lot oldLot = auction.Lots.First(l=>l.LotId == lotId);
                auction.Lots.Remove(oldLot);

                // replace with newly read lot.
                auction.Lots.Add(lot);
            }
        }

        /// <summary>
        /// Pop the relevant lots into the relevant auctions
        /// </summary>
        private void CreateRelationships(Auction auction)
        {
            foreach(KeyValuePair<int, Lot> kvp in _lots)
            {
                Lot lot = kvp.Value;

                // does lot have an auction?
                if(lot.AuctionId!=null)
                {
                    int lotAuctionId = (int)lot.AuctionId;

                    // only for specified auction.
                    // or all auctions if no single auction specified.
                    if (auction == null)
                    {
                        Auction cachedAuction = _auctions[lotAuctionId];
                        cachedAuction.Lots.Add(lot);
                    }
                    else
                    {
                        if (lotAuctionId == auction.AuctionId)
                        {
                            // only for this specific instance of an auction
                            auction.Lots.Add(lot);
                        }
                    }
                }
            }
        }

        public void Initialize()
        {
            RefreshAuctions();
            RefreshLots();
            CreateRelationships(null);
            HttpRuntime.Cache.Insert(
                Consts.CacheConsts.SystemCache,
                this,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                null);
        }

        public static SystemCacheManager GetCache()
        {
            return (SystemCacheManager)HttpRuntime.Cache.Get(Consts.CacheConsts.SystemCache);
        }

        public void AuctionAdd(Auction auction)
        {
            _auctions.Add(auction.AuctionId, auction);
        }

        public SortedDictionary<string, Auction> AuctionsGetClosed()
        {
            return GetAuctionsByStatus(AuctionStatus.Closed);
        }

        public SortedDictionary<string, Auction> AuctionsGetRunning()
        {
            return GetAuctionsByStatus(AuctionStatus.Running);
        }

        private SortedDictionary<string, Auction> GetAuctionsByStatus(AuctionStatus status)
        {
            // getting name via reflection is slow.
            // do want callstack.
            // maybe method and class consts for easy cut-n=paste logging?
            // it'd be NICE if AppLog class could just DO it but that's gonna take analysis of the callstack etc
            // which will be so slow as to cause grinding.
            AppLog.Debug(string.Format("{0}.GetAuctionsByStatus({1}) start", this.GetType().FullName, status));

            SortedDictionary<string, Auction> result = new SortedDictionary<string, Auction>();
            foreach (KeyValuePair<int, Auction> kvp in _auctions)
            {
                Auction auction = kvp.Value;
                AppLog.Debug(string.Format("Auction {0} has status {1}", auction.AuctionId, auction.Status));
                if (auction.Status == status)
                {
                    result.Add(auction.Name, auction);
                    AppLog.Debug("Added.");
                }
            }
            AppLog.Debug(string.Format("GetAuctionsByStatus({0}) end", status));
            return result;
        }

        public void AuctionSetStatus(int auctionId, AuctionStatus status)
        {
            // trivial update. Just pop it in.
            Auction auction = _auctions[auctionId];
            auction.Status = status;
        }

    }
}