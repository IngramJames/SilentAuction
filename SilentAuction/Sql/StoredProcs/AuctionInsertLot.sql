CREATE PROCEDURE [dbo].AuctionLotInsert
	@auctionId		int,
	@userId			VARCHAR(256),
	@lotId			int,
	@order			int
AS
BEGIN
	UPDATE	Lots
	SET		AuctionId = @auctionId,
			AuctionOrder = @order
	WHERE	lotId = @lotId
END