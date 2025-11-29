using backend.Business.Models;
using backend.DataAccess.Repositories;
using backend.DataAccess.UnitOfWork;

namespace backend.Business.Services;

/// <summary>
/// Service implementation for booking operations.
/// Handles ticket booking with pessimistic locking to prevent race conditions on seat availability.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ITrainRepository _trainRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public BookingService(
        IBookingRepository bookingRepository,
        ISeatRepository seatRepository,
        ITrainRepository trainRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _bookingRepository = bookingRepository;
        _seatRepository = seatRepository;
        _trainRepository = trainRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IEnumerable<Seat>> GetSeatMapAsync(int trainId)
    {
        return await _seatRepository.GetByTrainIdAsync(trainId);
    }

    public async Task<(bool Success, string Message, int BookingId)> BookTicketAsync(int userId, int trainId,
        int seatId)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            var train = await _trainRepository.GetByIdAsync(trainId);
            if (train == null)
            {
                _unitOfWork.Rollback();
                return (false, "Train not found.", 0);
            }

            if (train.Status != "Active")
            {
                _unitOfWork.Rollback();
                return (false, "Train is not available for booking.", 0);
            }

            var seat = await _seatRepository.GetByIdWithLockAsync(seatId);
            if (seat == null)
            {
                _unitOfWork.Rollback();
                return (false, "Seat not found.", 0);
            }

            if (seat.TrainId != trainId)
            {
                _unitOfWork.Rollback();
                return (false, "Seat does not belong to this train.", 0);
            }

            if (!seat.IsAvailable)
            {
                _unitOfWork.Rollback();
                return (false, "Seat is already booked.", 0);
            }

            var booking = new Booking
            {
                UserId = userId,
                TrainId = trainId,
                SeatId = seatId,
                BookingStatus = "Confirmed",
                TotalAmount = train.TicketPrice,
                PaymentStatus = "Paid"
            };

            var bookingId = await _bookingRepository.CreateAsync(booking);
            await _seatRepository.UpdateAvailabilityAsync(seatId, false);

            _unitOfWork.Commit();

            await _auditService.LogAsync(userId, "Ticket Booked", "Booking", bookingId,
                $"User booked seat {seat.SeatNumber} on train {train.TrainNumber}.");

            return (true, "Ticket booked successfully.", bookingId);
        }
        catch (Exception ex)
        {
            _unitOfWork.Rollback();
            return (false, $"Booking failed: {ex.Message}", 0);
        }
    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId, bool isAdmin)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
        {
            return (false, "Booking not found.");
        }

        if (!isAdmin && booking.UserId != userId)
        {
            return (false, "You are not authorized to cancel this booking.");
        }

        if (booking.BookingStatus == "Cancelled")
        {
            return (false, "Booking is already cancelled.");
        }

        try
        {
            _unitOfWork.BeginTransaction();

            await _bookingRepository.CancelBookingAsync(bookingId);
            await _seatRepository.UpdateAvailabilityAsync(booking.SeatId, true);

            _unitOfWork.Commit();

            await _auditService.LogAsync(userId, "Booking Cancelled", "Booking", bookingId,
                $"Booking {bookingId} cancelled by {(isAdmin ? "admin" : "user")}.");

            return (true, "Booking cancelled successfully.");
        }
        catch (Exception ex)
        {
            _unitOfWork.Rollback();
            return (false, $"Cancellation failed: {ex.Message}");
        }
    }

    public async Task<IEnumerable<Booking>> GetBookingHistoryAsync(int userId)
    {
        return await _bookingRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _bookingRepository.GetAllAsync();
    }

    public async Task<PagedResult<Booking>> GetAllBookingsAsync(int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _bookingRepository.GetAllAsync(pageNumber, pageSize);
        return new PagedResult<Booking>
        {
            Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize
        };
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId)
    {
        return await _bookingRepository.GetByIdAsync(bookingId);
    }
}

