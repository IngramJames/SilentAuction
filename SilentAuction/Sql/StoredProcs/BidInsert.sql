CREATE PROCEDURE [dbo].BidInsert
	@lotId		int,
	@userId		VARCHAR(256),
	@amount		decimal(18,2),
	@timeInUTC	datetime
AS
BEGIN
DECLARE @lock			VARCHAR(256)
DECLARE @auctionId		int
DECLARE @auctionStatus	int
	-- get lock id and auction id of lot
	SELECT
		@lock = LockedByUser,
		@auctionId = AuctionId
	FROM
		Lots
	WHERE
		LotId = @LotId

	-- return -1 if this lot is not locked
	IF @lock<>@userId
	BEGIN
		RETURN(-1)
	END

	-- return 3 if the auction is not open
	SELECT @auctionStatus = (SELECT [Status] FROM Auctions WHERE auctionId = @auctionId)
	IF @auctionStatus <> 1
	BEGIN
		RETURN(3)
	END

	-- get last amount which was bid
	DECLARE @lastAmount decimal(18,2)
	SELECT	@lastAmount = amount
	FROM	Bids
	WHERE	LotId = @LotId
	AND		status = 0

	-- pessimism: assume the bid will not be higher than the previous bid
DECLARE @status	int
	SET @status=2

	IF @lastAmount is null
	BEGIN
		-- no existing bid
		SET @status=0		-- this bid is the new open bid
	END
	ELSE
	BEGIN
		-- this bid is higher than the old bid
		IF @lastAmount<@amount
		BEGIN
			SET @status=0		-- this bid is the new open bid
		END
	END

	-- supercede any open bids for this lot; but ONLY if bid was successful
	IF @status=0
	BEGIN
		UPDATE	Bids
		SET		Status = 1
		WHERE	Status = 0
		AND		LotId = @LotId
	END

	-- insert new bid
	INSERT INTO Bids
	(
		amount,
		UserId,
		LotId,
		[Timestamp],
		Status
	)
	VALUES
	(
		@amount,
		@userId,
		@lotId,
		@timeInUTC,
		@status
		
	)

	-- update lot to reflect highest bid but ONLY if the bid is open (ie was successful)
	IF @status=0
	BEGIN
		DECLARE @bidId int
		SELECT @bidId = SCOPE_IDENTITY()

		UPDATE
			Lots
		SET
			HighestBid_BidId = @BidId
		WHERE
			LotId = @lotId
	
		RETURN 1		-- return success
	END

	RETURN 2			-- bid was logged but was too low
END