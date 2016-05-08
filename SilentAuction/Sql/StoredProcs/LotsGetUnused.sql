CREATE PROCEDURE [dbo].LotsGetUnused
AS
	SELECT
		*
	FROM
		Lots
	WHERE
		AuctionId is null