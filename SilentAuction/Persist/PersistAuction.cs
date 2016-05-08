using SilentAuction.Caches;
using SilentAuction.Classes;
using SilentAuction.Common;
using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SilentAuction.Persist
{
    public class PersistAuction
    {
        private const string _auctionUpdate = "AuctionUpdate";
        private const string _auctionLotsDelete = "AuctionLotsDelete";
        private const string _auctionLotInsert = "AuctionLotInsert";
        private const string _auctionUpdateStatus = "AuctionUpdateStatus";
        private const string _auctionsGet = "AuctionsGet";

        public void UpdateStatus(string userId, int auctionId, AuctionStatus status)
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            DbContextTransaction trans = dbContext.Database.BeginTransaction();

            DbCommand command = dbContext.Database.Connection.CreateCommand();
            command.Transaction = trans.UnderlyingTransaction;
            command.CommandText = _auctionUpdateStatus;
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "auctionId", auctionId));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "status", (int)status));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId));
            command.ExecuteNonQuery();
            trans.Commit();

            SystemCacheManager systemCache = SystemCacheManager.GetCache();
            systemCache.AuctionSetStatus(auctionId, status);
        }

        public void Create(Auction auction, List<ImageFile> imageFiles)
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            DbContextTransaction trans = dbContext.Database.BeginTransaction();

            if (imageFiles.Count > 0)
            {
                PersistImageFiles persistImageFiles = new PersistImageFiles();
                persistImageFiles.ImagesSave(dbContext, imageFiles);
                auction.Image = imageFiles[0];
            }

            dbContext.Auctions.Add(auction);
            dbContext.SaveChanges();
            trans.Commit();

            // Add auction to cache
            SystemCacheManager systemCache = SystemCacheManager.GetCache();
            systemCache.AuctionAdd(auction);
        }

        public void UpdateWithLots(string userId, int auctionId, string name, string description, bool useReserves, int[] lotOrder)
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            DbContextTransaction trans = dbContext.Database.BeginTransaction();

            Update(dbContext, trans, userId, auctionId, name, description, useReserves);
            AuctionLotsDelete(dbContext, trans, userId, auctionId);
            LotsInsert(dbContext, trans, userId, auctionId, lotOrder);

            trans.Commit();

            // If an auction and its lots have changed, then all our cached data could be wrong.
            // Reproducing the logic to switch around the cache is complex and therefore risky.
            // Just reload the caches.
            SystemCacheManager systemCache = SystemCacheManager.GetCache();
            systemCache.Initialize();
        }

        private void LotsInsert(ApplicationDbContext dbContext, DbContextTransaction trans, string userId, int auctionId, int[] lotIds)
        {
            int order = 1;
            foreach (int lotId in lotIds)
            {
                DbCommand command = dbContext.Database.Connection.CreateCommand();
                command.Transaction = trans.UnderlyingTransaction;
                command.CommandText = _auctionLotInsert;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "auctionId", auctionId));
                command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "lotId", lotId));
                command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "order", order));
                command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId));
                command.ExecuteNonQuery();
                order++;
            }
        }


        private void AuctionLotsDelete(ApplicationDbContext dbContext, DbContextTransaction trans, string userId, int auctionId)
        {
            DbCommand command = dbContext.Database.Connection.CreateCommand();
            command.Transaction = trans.UnderlyingTransaction;
            command.CommandText = _auctionLotsDelete;
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "auctionId", auctionId));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId));
            command.ExecuteNonQuery();
        }


        private void Update(ApplicationDbContext dbContext, DbContextTransaction trans, string userId, int auctionId, string name, string description, bool useReserves)
        {
            DbCommand command = dbContext.Database.Connection.CreateCommand();
            command.Transaction = trans.UnderlyingTransaction;
            command.CommandText = _auctionUpdate;
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "auctionId", auctionId));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "name", name));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "description", description));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Boolean, "useReserves", useReserves));
            command.ExecuteNonQuery();

            // Update cache
            SystemCacheManager scm = SystemCacheManager.GetCache();
            scm.RefreshAuction(auctionId);
        }

        private DbDataReader GetHighestBidsForLotsAndUser(ApplicationDbContext dbContext, int auctionId, int? lotId, string userId)
        {
            const string _auctionGetHighestBidsByUser = "dbo.AuctionGetHighestBidsByUser";

            DbCommand command = dbContext.Database.Connection.CreateCommand();
            command.CommandText = _auctionGetHighestBidsByUser;
            command.CommandType = System.Data.CommandType.StoredProcedure;

            DbParameter auctionIdParam = DBUtils.CreateParam(command, System.Data.DbType.String, "auctionId", auctionId);
            DbParameter lotIdParam = DBUtils.CreateParam(command, System.Data.DbType.String, "lotId", lotId);
            DbParameter userIdParam = DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId);
            command.Parameters.Add(auctionIdParam);
            command.Parameters.Add(lotIdParam);
            command.Parameters.Add(userIdParam);

            DbDataReader reader = command.ExecuteReader();

            return reader;
        }

        private Bid ReadBid(DbDataReader reader)
        {
            Bid bid = new Models.Bid();
            bid.LotId = reader.GetFieldValue<int>(0);
            bid.Amount = reader.GetFieldValue<decimal>(1);
            bid.Status = (BidStatus)reader.GetFieldValue<int>(2);

            return bid;
        }

        /// <summary>
        /// Return the highest bid on a specified lot by a specified user
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="auctionId"></param>
        /// <param name="lotId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Bid GetHighestBidByUserForLot(ApplicationDbContext dbContext, int auctionId, int lotId, string userId)
        {
            DbDataReader reader = GetHighestBidsForLotsAndUser(dbContext, auctionId, lotId, userId);
            Dictionary<int, Bid> bids = new Dictionary<int, Models.Bid>();
            if (reader.HasRows)
            {
                // just take the 1st one. Any other are duplicates. We just want the amount.
                reader.Read();
                Bid bid = ReadBid(reader);
                return bid;
            }

            return null;
        }

        /// <summary>
        /// Returns the highest bids made in an auction by the specified user.
        /// Includes past bids which failed or have since been outbid.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="auctionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Dictionary<int, Bid> GetHighestBidsByUser(ApplicationDbContext dbContext, int auctionId, string userId)
        {
            dbContext.Database.Connection.Open();
            try
            {
                DbDataReader reader = GetHighestBidsForLotsAndUser(dbContext, auctionId, null, userId);
                Dictionary<int, Bid> bids = new Dictionary<int, Models.Bid>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Bid bid = ReadBid(reader);

                        // Highest can have duplictes (user bids £3 four times; all get logged)
                        if (!bids.ContainsKey(bid.LotId))
                        {
                            bids.Add(bid.LotId, bid);
                        }
                    }
                }

                return bids;
            }
            finally
            {
                dbContext.Database.Connection.Close();
            }
        }

        public Auction AuctionGet(int auctionId)
        {
            Dictionary<int, Auction> auctions = AuctionsGet(auctionId);
            return auctions[auctionId];
        }

        private Dictionary<int, Auction> AuctionsGet(int? auctionId)
        {
            Dictionary<int, Auction> result = new Dictionary<int, Auction>();
            ApplicationDbContext dbContext = new ApplicationDbContext();
            dbContext.Database.Connection.Open();
            try
            {
                DbCommand command = dbContext.Database.Connection.CreateCommand();
                command.CommandText = _auctionsGet;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                DbParameter auctionIdParam = DBUtils.CreateParam(command, System.Data.DbType.String, "auctionId", auctionId);
                command.Parameters.Add(auctionIdParam);
                DbDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Auction auction = ReadAuction(reader);
                        result.Add(auction.AuctionId, auction);
                    }
                }
                return result;
            }
            finally
            {
                dbContext.Database.Connection.Close();
            }
        }

        public Dictionary<int, Auction> AuctionsGet()
        {
            return AuctionsGet(null);
        }

        private Auction ReadAuction(DbDataReader reader)
        {
            Auction auction = new Auction();
            auction.AuctionId = reader.GetFieldValue<int>(0);
            auction.Name = reader.GetFieldValue<string>(1);
            auction.Description = reader.GetFieldValue<string>(2);
            auction.Status = (AuctionStatus)reader.GetFieldValue<int>(3);
            auction.UseReserves = reader.GetFieldValue<bool>(4);

            // initialize lots
            auction.Lots = new KeyedCollectionLots();

            return auction;
        }

        public SortedList<string, UserResults> GetResultsByUser(int auctionId)
        {
            // get auction
            SystemCacheManager systemCache = SystemCacheManager.GetCache();
            Auction auction = systemCache.GetAuction(auctionId);

            // prepare result
            SortedList<string, UserResults> results = new SortedList<string, UserResults>();

            // iterate lots and add to the correct user bag
            foreach(Lot lot in auction.Lots)
            {
                Bid bid = auction.GetWinningBidForLot(lot);
                if (bid != null)
                {
                    ApplicationUser winningUser = bid.User;

                    // get or create user bag
                    UserResults userResults;
                    if (results.ContainsKey(winningUser.UserName))
                    {
                        userResults = results[winningUser.UserName];
                    }
                    else
                    {
                        userResults = new UserResults();
                        userResults.User = winningUser;
                        results.Add(winningUser.UserName, userResults);
                    }
                    userResults.Lots.Add(lot.LotId, lot);
                }
            }
            return results;
        }
    }
}