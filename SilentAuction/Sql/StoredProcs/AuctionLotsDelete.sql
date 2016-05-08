CREATE PROCEDURE [dbo].AuctionLotsDelete
	@auctionId		int,
	@userId			VARCHAR(256)
AS
BEGIN
	UPDATE	Lots
	SET		AuctionId = null,
			AuctionOrder = null
	WHERE	AuctionId = @auctionId
END