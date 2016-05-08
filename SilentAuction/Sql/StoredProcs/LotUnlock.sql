CREATE PROCEDURE [dbo].LotUnlock
	@lotId	int,
	@userId	VARCHAR(256)
AS
DECLARE @lock	VARCHAR(256)
	
	SELECT
		@lock = LockedByUser
	FROM
		Lots
	WHERE
		LotId = @LotId


	-- verify that this user has the lock
	IF(@lock <> @userId)
	BEGIN
		-- trying to unlock somebody else's lot. This is bad.
		RETURN(-1)
	END

	-- attempt update. 
	UPDATE
		Lots
	SET
		LockedByUser = NULL,
		LockedAt=NULL
	WHERE
		LotId=@LotId

	-- just for sanity, verify that this record is now unlocked
	SELECT
		@lock = LockedByUser
	FROM
		Lots
	WHERE
		LotId = @LotId

	-- attempted update but something went amiss. Very weird. Cop out immediately because this looks bad and we have no idea why it happened.
	IF(@lock is not null)
	BEGIN
		RETURN(-2)
	END

	RETURN 1
