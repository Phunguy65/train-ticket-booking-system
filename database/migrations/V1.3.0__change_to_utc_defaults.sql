-- Migration script to change GETDATE() to GETUTCDATE() for UTC timezone handling
-- This ensures all auto-generated timestamps are stored in UTC
-- Affects: User.CreatedAt, Train.CreatedAt, Booking.BookingDate, AuditLog.CreatedAt
-- Step 1: Drop and recreate User.CreatedAt default constraint
IF EXISTS (
	SELECT
		*
	FROM
		sys.default_constraints
	WHERE
		name = 'DF_User_CreatedAt'
		AND parent_object_id = OBJECT_ID('[User]')
) BEGIN
ALTER TABLE [User] DROP CONSTRAINT DF_User_CreatedAt;

PRINT 'Dropped constraint DF_User_CreatedAt';

END ELSE BEGIN PRINT 'Constraint DF_User_CreatedAt does not exist';

END
GO
ALTER TABLE [User]
ADD CONSTRAINT DF_User_CreatedAt DEFAULT (GETUTCDATE()) FOR CreatedAt;

PRINT 'Created constraint DF_User_CreatedAt with GETUTCDATE()';

GO
-- Step 2: Drop and recreate Train.CreatedAt default constraint
IF EXISTS (
	SELECT
		*
	FROM
		sys.default_constraints
	WHERE
		name = 'DF_Train_CreatedAt'
		AND parent_object_id = OBJECT_ID('[Train]')
) BEGIN
ALTER TABLE [Train] DROP CONSTRAINT DF_Train_CreatedAt;

PRINT 'Dropped constraint DF_Train_CreatedAt';

END ELSE BEGIN PRINT 'Constraint DF_Train_CreatedAt does not exist';

END
GO
ALTER TABLE [Train]
ADD CONSTRAINT DF_Train_CreatedAt DEFAULT (GETUTCDATE()) FOR CreatedAt;

PRINT 'Created constraint DF_Train_CreatedAt with GETUTCDATE()';

GO
-- Step 3: Drop and recreate Booking.BookingDate default constraint
IF EXISTS (
	SELECT
		*
	FROM
		sys.default_constraints
	WHERE
		name = 'DF_Booking_BookingDate'
		AND parent_object_id = OBJECT_ID('[Booking]')
) BEGIN
ALTER TABLE [Booking] DROP CONSTRAINT DF_Booking_BookingDate;

PRINT 'Dropped constraint DF_Booking_BookingDate';

END ELSE BEGIN PRINT 'Constraint DF_Booking_BookingDate does not exist';

END
GO
ALTER TABLE [Booking]
ADD CONSTRAINT DF_Booking_BookingDate DEFAULT (GETUTCDATE()) FOR BookingDate;

PRINT 'Created constraint DF_Booking_BookingDate with GETUTCDATE()';

GO
-- Step 4: Drop and recreate AuditLog.CreatedAt default constraint
IF EXISTS (
	SELECT
		*
	FROM
		sys.default_constraints
	WHERE
		name = 'DF_AuditLog_CreatedAt'
		AND parent_object_id = OBJECT_ID('[AuditLog]')
) BEGIN
ALTER TABLE [AuditLog] DROP CONSTRAINT DF_AuditLog_CreatedAt;

PRINT 'Dropped constraint DF_AuditLog_CreatedAt';

END ELSE BEGIN PRINT 'Constraint DF_AuditLog_CreatedAt does not exist';

END
GO
ALTER TABLE [AuditLog]
ADD CONSTRAINT DF_AuditLog_CreatedAt DEFAULT (GETUTCDATE()) FOR CreatedAt;

PRINT 'Created constraint DF_AuditLog_CreatedAt with GETUTCDATE()';

GO
-- Verify the changes
SELECT
	dc.name AS ConstraintName,
	OBJECT_NAME(dc.parent_object_id) AS TableName,
	COL_NAME(dc.parent_object_id, dc.parent_column_id) AS ColumnName,
	dc.definition AS DefaultDefinition
FROM
	sys.default_constraints AS dc
WHERE
	dc.name IN (
		'DF_User_CreatedAt',
		'DF_Train_CreatedAt',
		'DF_Booking_BookingDate',
		'DF_AuditLog_CreatedAt'
	)
ORDER BY
	TableName,
	ColumnName;

PRINT 'UTC default constraints migration completed successfully';

PRINT 'All auto-generated timestamps now use GETUTCDATE() for UTC storage';
