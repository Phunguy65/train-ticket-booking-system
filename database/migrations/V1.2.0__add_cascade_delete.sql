-- Migration script to add CASCADE DELETE to foreign key constraints
-- This enables automatic deletion of related records when a Train is deleted
-- Deletion chain: Train -> Seat -> Booking
-- Step 1: Drop existing foreign key constraint on Booking table (SeatId)
IF EXISTS (
	SELECT
		*
	FROM
		sys.foreign_keys
	WHERE
		name = 'FK_Booking_SeatId_Seat'
		AND parent_object_id = OBJECT_ID('Booking')
) BEGIN
ALTER TABLE Booking DROP CONSTRAINT FK_Booking_SeatId_Seat;

PRINT 'Dropped constraint FK_Booking_SeatId_Seat';

END ELSE BEGIN PRINT 'Constraint FK_Booking_SeatId_Seat does not exist';

END
GO
-- Step 2: Drop existing foreign key constraint on Booking table (TrainId)
IF EXISTS (
	SELECT
		*
	FROM
		sys.foreign_keys
	WHERE
		name = 'FK_Booking_TrainId_Train'
		AND parent_object_id = OBJECT_ID('Booking')
) BEGIN
ALTER TABLE Booking DROP CONSTRAINT FK_Booking_TrainId_Train;

PRINT 'Dropped constraint FK_Booking_TrainId_Train';

END ELSE BEGIN PRINT 'Constraint FK_Booking_TrainId_Train does not exist';

END
GO
-- Step 3: Drop existing foreign key constraint on Seat table (TrainId)
IF EXISTS (
	SELECT
		*
	FROM
		sys.foreign_keys
	WHERE
		name = 'FK_Seat_TrainId_Train'
		AND parent_object_id = OBJECT_ID('Seat')
) BEGIN
ALTER TABLE Seat DROP CONSTRAINT FK_Seat_TrainId_Train;

PRINT 'Dropped constraint FK_Seat_TrainId_Train';

END ELSE BEGIN PRINT 'Constraint FK_Seat_TrainId_Train does not exist';

END
GO
-- Step 4: Recreate Seat foreign key with CASCADE DELETE
ALTER TABLE Seat
ADD CONSTRAINT FK_Seat_TrainId_Train FOREIGN KEY (TrainId) REFERENCES Train (TrainId) ON DELETE CASCADE;

PRINT 'Created constraint FK_Seat_TrainId_Train with ON DELETE CASCADE';

GO
-- Step 5: Recreate Booking foreign key (TrainId) with CASCADE DELETE
ALTER TABLE Booking
ADD CONSTRAINT FK_Booking_TrainId_Train FOREIGN KEY (TrainId) REFERENCES Train (TrainId) ON DELETE CASCADE;

PRINT 'Created constraint FK_Booking_TrainId_Train with ON DELETE CASCADE';

GO
-- Step 6: Recreate Booking foreign key (SeatId) with CASCADE DELETE
ALTER TABLE Booking
ADD CONSTRAINT FK_Booking_SeatId_Seat FOREIGN KEY (SeatId) REFERENCES Seat (SeatId) ON DELETE CASCADE;

PRINT 'Created constraint FK_Booking_SeatId_Seat with ON DELETE CASCADE';

GO
-- Verify the changes
SELECT
	fk.name AS ForeignKeyName,
	OBJECT_NAME(fk.parent_object_id) AS TableName,
	COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
	OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
	COL_NAME(
		fkc.referenced_object_id,
		fkc.referenced_column_id
	) AS ReferencedColumn,
	fk.delete_referential_action_desc AS DeleteAction
FROM
	sys.foreign_keys AS fk
	INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
WHERE
	fk.name IN (
		'FK_Seat_TrainId_Train',
		'FK_Booking_TrainId_Train',
		'FK_Booking_SeatId_Seat'
	)
ORDER BY
	TableName,
	ForeignKeyName;

PRINT 'CASCADE DELETE migration completed successfully';

PRINT 'Deletion behavior: Train -> Seat (CASCADE) -> Booking (CASCADE)';
