-- Migration script to add indexes for pagination optimization
-- These indexes improve performance of ORDER BY clauses in paginated queries
-- Check if index exists before creating to make script idempotent
IF NOT EXISTS (
	SELECT
		*
	FROM
		sys.indexes
	WHERE
		name = 'IX_AuditLog_CreatedAt'
		AND object_id = OBJECT_ID('[AuditLog]')
) BEGIN
CREATE INDEX IX_AuditLog_CreatedAt ON [AuditLog] (CreatedAt DESC);

PRINT 'Created index IX_AuditLog_CreatedAt';

END ELSE BEGIN PRINT 'Index IX_AuditLog_CreatedAt already exists';

END
GO IF NOT EXISTS (
	SELECT
		*
	FROM
		sys.indexes
	WHERE
		name = 'IX_User_CreatedAt'
		AND object_id = OBJECT_ID('[User]')
) BEGIN
CREATE INDEX IX_User_CreatedAt ON [User] (CreatedAt DESC);

PRINT 'Created index IX_User_CreatedAt';

END ELSE BEGIN PRINT 'Index IX_User_CreatedAt already exists';

END
GO
-- Additional indexes for Train and Booking tables to optimize pagination
IF NOT EXISTS (
	SELECT
		*
	FROM
		sys.indexes
	WHERE
		name = 'IX_Train_DepartureTime'
		AND object_id = OBJECT_ID('[Train]')
) BEGIN
CREATE INDEX IX_Train_DepartureTime ON [Train] (DepartureTime);

PRINT 'Created index IX_Train_DepartureTime';

END ELSE BEGIN PRINT 'Index IX_Train_DepartureTime already exists';

END
GO IF NOT EXISTS (
	SELECT
		*
	FROM
		sys.indexes
	WHERE
		name = 'IX_Booking_BookingDate'
		AND object_id = OBJECT_ID('[Booking]')
) BEGIN
CREATE INDEX IX_Booking_BookingDate ON [Booking] (BookingDate DESC);

PRINT 'Created index IX_Booking_BookingDate';

END ELSE BEGIN PRINT 'Index IX_Booking_BookingDate already exists';

END
GO PRINT 'Pagination indexes migration completed successfully';
