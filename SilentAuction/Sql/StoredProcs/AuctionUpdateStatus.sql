CREATE PROCEDURE [dbo].AuctionUpdateStatus
	@auctionId		int,
	@userId			VARCHAR(256),
	@status			int
AS
BEGIN
	UPDATE	Auctions
	SET		[Status] = @status
	WHERE	AuctionId = @auctionId
END