CREATE PROCEDURE [dbo].AuctionUpdate
	@auctionId		int,
	@userId			VARCHAR(256),
	@name			VARCHAR(100),
	@description	VARCHAR(1000),
	@useReserves	bit
AS
BEGIN
	UPDATE	Auctions
	SET		Name = @name,
			Description = @description,
			UseReserves = @useReserves
	WHERE	AuctionId = @auctionId
END