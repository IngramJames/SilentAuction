CREATE PROCEDURE [dbo].LotLock
	@lotId		int,
	@userId		VARCHAR(256),
	@dateInUTC	datetime
AS
DECLARE @lock	VARCHAR(256)
	SELECT
		@lock = LockedByUser
	FROM
		Lots
	WHERE
		LotId = @LotId

	-- return 0 if this lot is already locked
	IF LEN(@lock) >0
	BEGIN
		RETURN(0)
	END

	-- attempt update. If another user is also doing this at the same time, then one will fail to perform the update and will have to wait for the other user to finish their trtansaction
	UPDATE
		Lots
	SET
		LockedByUser = @UserId,
		LockedAt = @dateInUTC
	WHERE
		LotId=@LotId

	-- just for sanity, verify that this record is now locked by the expected user
	SELECT
		@lock = LockedByUser
	FROM
		Lots
	WHERE
		LotId = @LotId

	-- attempted update but something went amiss. Very weird. Cop out immediately because this looks bad and we have no idea why it happened.
	IF(@lock <> @userId)
	BEGIN
		RETURN(-1)
	END

	RETURN 1
