CREATE PROCEDURE [dbo].AuctionsGet
	@auctionId	int
AS
	SELECT	AuctionId,
			Name,
			[Description],
			[Status],
			UseReserves
	FROM	Auctions
	WHERE	@auctionId is null OR AuctionId = @auctionId