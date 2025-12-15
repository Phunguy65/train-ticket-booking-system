-- Migration V1.4.0: Add seat hold support
-- Adds HoldExpiresAt column to Booking table for temporary seat reservations
-- Creates index for efficient cleanup of expired holds
-- Add HoldExpiresAt column to track when temporary seat holds expire
-- NULL value indicates permanent booking (Confirmed status)
-- Non-NULL value indicates temporary hold (Pending status)
-- All datetime values stored in UTC timezone
ALTER TABLE [Booking]
ADD HoldExpiresAt DATETIME2 NULL;

GO
-- Create non-clustered index for efficient hold cleanup queries
-- Filtered index only includes rows with non-NULL HoldExpiresAt (active holds)
-- Enables fast queries: WHERE BookingStatus = 'Pending' AND HoldExpiresAt < GETUTCDATE()
CREATE NONCLUSTERED
INDEX IX_Booking_HoldExpiresAt ON [Booking] (HoldExpiresAt)
WHERE
	HoldExpiresAt IS NOT NULL;

GO
-- Add index for user-specific hold queries
-- Enables fast queries: WHERE UserId = @UserId AND BookingStatus = 'Pending'
CREATE NONCLUSTERED
INDEX IX_Booking_UserId_Status ON [Booking] (UserId, BookingStatus) INCLUDE (HoldExpiresAt, TrainId, SeatId);

GO
