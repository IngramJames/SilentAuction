using SilentAuction.Caches;
using SilentAuction.Consts;
using SilentAuction.Common;
using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SilentAuction.Persist
{
	public class PersistLot
	{
        private const string _bidInsert = "dbo.BidInsert";
        private const string _lotLock = "dbo.LotLock";
        private const string _lotUnlock = "dbo.LotUnlock";

        public void Create(ApplicationDbContext dbContext, Lot lot)
		{
			// Save lot
			dbContext.Lots.Add(lot);
			dbContext.SaveChanges();
		}

		public void Create(ApplicationDbContext dbContext, Lot lot, IEnumerable<ImageFile> imageFiles)
		{
            // Save the actual lot
            Create(dbContext, lot);

			// Save and associate image files
			PersistImageFiles persistImageFiles = new PersistImageFiles();
			persistImageFiles.ImagesSaveForLot(dbContext, imageFiles, lot, true);
		}

        public void Create(ApplicationDbContext dbContext, List<Lot> lots)
        {
            DbContextTransaction trans = dbContext.Database.BeginTransaction();
            try
            {
                foreach (Lot lot in lots)
                {
                    Create(dbContext, lot);
                }
                trans.Commit();
            }
            catch(Exception e)
            {
                trans.Rollback();
                throw(e);
            }
        }


        /// <summary>
        /// Get a lock on a lot, prior to updating.
        /// </summary>
        /// <param name="lotId"></param>
        /// <param name="userId"></param>
        /// <param name="dbContext"></param>
        /// <param name="trans">Transaction that this is happening within. Manadatory.</param>
        /// <returns></returns>
        public ErrorCode LotLock(int lotId, string userId, ApplicationDbContext dbContext, DbContextTransaction trans)
        {
            DbCommand command = dbContext.Database.Connection.CreateCommand();
            command.Transaction = trans.UnderlyingTransaction;
            command.CommandText = _lotLock;
            command.CommandType = System.Data.CommandType.StoredProcedure;

            DbParameter lotIdParam = DBUtils.CreateParam(command, System.Data.DbType.String, "lotId", lotId);
            DbParameter userIdParam = DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId);
            DbParameter timeParam = DBUtils.CreateParam(command, System.Data.DbType.DateTime, "dateInUTC", DateTime.UtcNow);
            command.Parameters.Add(lotIdParam);
            command.Parameters.Add(userIdParam);
            command.Parameters.Add(timeParam);

            DbParameter returnParam = DBUtils.CreateReturnParam(command, System.Data.DbType.Int32, "return");
            command.Parameters.Add(returnParam);

            command.ExecuteNonQuery();

            int returnVal = (int)returnParam.Value;
            switch (returnVal)
            {
                case -1:
                    return ErrorCode.LotLockFailed;
                case 0:
                    return ErrorCode.LotAlreadyLocked;
                case 1:
                    return ErrorCode.OK;
                default:
                    return ErrorCode.UnknownError;
            }
        }

        public ErrorCode LotUnlock(int lotId, string userId, ApplicationDbContext dbContext, DbContextTransaction trans)
        {
            DbCommand command = dbContext.Database.Connection.CreateCommand();
            command.Transaction = trans.UnderlyingTransaction;
            command.CommandText = _lotUnlock;
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "lotId", lotId));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId));

            DbParameter returnParam = DBUtils.CreateReturnParam(command, System.Data.DbType.Int32, "return");
            command.Parameters.Add(returnParam);

            command.ExecuteNonQuery();

            int returnVal = (int)returnParam.Value;
            switch (returnVal)
            {
                case -1:
                    return ErrorCode.LotLockStolen;
                case 0:
                    return ErrorCode.LotUnlockFailed;
                case 1:
                    return ErrorCode.OK;
                default:
                    return ErrorCode.UnknownError;
            }
        }

        public ErrorCode BidInsert(int lotId, string userId, decimal bidAmount, ApplicationDbContext dbContext, DbContextTransaction trans)
        {
            DbCommand command = dbContext.Database.Connection.CreateCommand();
            command.Transaction = trans.UnderlyingTransaction;
            command.CommandText = _bidInsert;
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Int32, "lotId", lotId));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.String, "userId", userId));
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.Decimal, "amount", bidAmount));

            // we pass through timestamps, to make sure they are in UTC, not DB-local time
            command.Parameters.Add(DBUtils.CreateParam(command, System.Data.DbType.DateTime2, "timeInUTC", DateTime.UtcNow));
            DbParameter returnParam = DBUtils.CreateReturnParam(command, System.Data.DbType.Int32, "return");
            command.Parameters.Add(returnParam);
            command.ExecuteNonQuery();

            int returnVal = (int)returnParam.Value;
            switch (returnVal)
            {
                case -1:
                    return ErrorCode.LotLockStolen;
                case 1:
                    return ErrorCode.OK;
                case 2:
                    return ErrorCode.LotBidTooLow;
                case 3:
                    return ErrorCode.AuctionNotRunning;
                default:
                    return ErrorCode.UnknownError;
            }

        }

        public ErrorCode Bid(int lotId, string userId, decimal bidAmount, ApplicationDbContext dbContext)
        {
            DbContextTransaction trans = dbContext.Database.BeginTransaction();

            // get lock on lot
            ErrorCode statusLock = LotLock(lotId, userId, dbContext, trans);

            // rollback on error
            if (statusLock != ErrorCode.OK)
            {
                trans.Rollback();
                return statusLock;
            }

            // insert bid
            ErrorCode statusSave = BidInsert(lotId, userId, bidAmount, dbContext, trans);
            
            // rollback on error
            if (statusSave != ErrorCode.OK && statusSave != ErrorCode.LotBidTooLow)
            {
                trans.Rollback();
                return statusSave;
            }

            // unlock lot
            ErrorCode statusUnlock = LotUnlock(lotId, userId, dbContext, trans);

            // rollback on error
            if (statusUnlock != ErrorCode.OK)
            {
                trans.Rollback();
                return statusUnlock;
            }

            // commit and return the status from the save
            trans.Commit();

            // update cache
            SystemCacheManager scm = SystemCacheManager.GetCache();
            scm.RefreshLot(lotId);

            return statusSave;
        }

    }
}