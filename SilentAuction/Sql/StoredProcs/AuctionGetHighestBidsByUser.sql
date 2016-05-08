CREATE PROCEDURE [dbo].AuctionGetHighestBidsByUser
	@auctionId	int,
	@lotId		int,
	@userId		VARCHAR(256)
AS
SELECT		Bids.LotId, 
			Bids.amount,
			Bids.status
FROM		Bids 
INNER JOIN	Lots
ON			Bids.LotId = Lots.LotId
WHERE		Bids.userId = @userId
AND			Lots.auctionId = @auctionId
AND			(@lotID is null OR Bids.LotId = @lotId)		-- all lots or only the one specified
AND			Bids.Amount = 
			( -- only take the highest recent bid by this user against any given lot.
			  -- there may be more than one, if the user has made duplicate bids
			SELECT		MAX(Amount) 
			FROM		Bids b2 
			WHERE		b2.LotId = Bids.LotId 
			AND			b2.UserId = @userId
			)
