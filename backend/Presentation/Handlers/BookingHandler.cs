using backend.Business.Services;
using backend.Presentation.Protocol;
using Newtonsoft.Json.Linq;

namespace backend.Presentation.Handlers;

/// <summary>
/// Handler for booking-related commands (book ticket, cancel booking, view history).
/// Processes booking requests with session validation and returns appropriate responses.
/// </summary>
public class BookingHandler
{
    private readonly IBookingService _bookingService;
    private readonly IAuthenticationService _authenticationService;

    public BookingHandler(IBookingService bookingService, IAuthenticationService authenticationService)
    {
        _bookingService = bookingService;
        _authenticationService = authenticationService;
    }

    public async Task<Response> HandleAsync(string action, JObject? data)
    {
        return action switch
        {
            "GetSeatMap" => await HandleGetSeatMapAsync(data),
            "BookTicket" => await HandleBookTicketAsync(data),
            "CancelBooking" => await HandleCancelBookingAsync(data),
            "GetBookingHistory" => await HandleGetBookingHistoryAsync(data),
            "GetAllBookings" => await HandleGetAllBookingsAsync(data),
            _ => new Response { Success = false, ErrorMessage = "Unknown booking action." }
        };
    }

    private async Task<Response> HandleGetSeatMapAsync(JObject? data)
    {
        if (data == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid request data." };
        }

        var request = data.ToObject<GetSeatMapRequest>();
        if (request == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid seat map request." };
        }

        var seats = await _bookingService.GetSeatMapAsync(request.TrainId);
        return new Response { Success = true, Data = seats };
    }

    private async Task<Response> HandleBookTicketAsync(JObject? data)
    {
        if (data == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid request data." };
        }

        var request = data.ToObject<BookTicketRequest>();
        if (request == null || string.IsNullOrEmpty(request.SessionToken))
        {
            return new Response { Success = false, ErrorMessage = "Invalid booking request." };
        }

        var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
        if (session == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
        }

        var result = await _bookingService.BookTicketAsync(session.UserId, request.TrainId, request.SeatId);
        return new Response
        {
            Success = result.Success,
            ErrorMessage = result.Success ? null : result.Message,
            Data = result.Success ? new { result.BookingId, result.Message } : null
        };
    }

    private async Task<Response> HandleCancelBookingAsync(JObject? data)
    {
        if (data == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid request data." };
        }

        var request = data.ToObject<CancelBookingRequest>();
        if (request == null || string.IsNullOrEmpty(request.SessionToken))
        {
            return new Response { Success = false, ErrorMessage = "Invalid cancellation request." };
        }

        var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
        if (session == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
        }

        var isAdmin = session.Role == "Admin";
        var result = await _bookingService.CancelBookingAsync(request.BookingId, session.UserId, isAdmin);
        return new Response
        {
            Success = result.Success,
            ErrorMessage = result.Success ? null : result.Message,
            Data = result.Success ? new { result.Message } : null
        };
    }

    private async Task<Response> HandleGetBookingHistoryAsync(JObject? data)
    {
        if (data == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid request data." };
        }

        var request = data.ToObject<GetBookingHistoryRequest>();
        if (request == null || string.IsNullOrEmpty(request.SessionToken))
        {
            return new Response { Success = false, ErrorMessage = "Session token is required." };
        }

        var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
        if (session == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
        }

        var bookings = await _bookingService.GetBookingHistoryAsync(session.UserId);
        return new Response { Success = true, Data = bookings };
    }

    private async Task<Response> HandleGetAllBookingsAsync(JObject? data)
    {
        if (data == null)
        {
            return new Response { Success = false, ErrorMessage = "Invalid request data." };
        }

        var request = data.ToObject<AuthenticatedRequest>();
        if (request == null || string.IsNullOrEmpty(request.SessionToken))
        {
            return new Response { Success = false, ErrorMessage = "Session token is required." };
        }

        var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
        if (session == null || session.Role != "Admin")
        {
            return new Response { Success = false, ErrorMessage = "Admin access required." };
        }

        var bookings = await _bookingService.GetAllBookingsAsync();
        return new Response { Success = true, Data = bookings };
    }
}

