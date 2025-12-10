using backend.Business.Services;
using backend.Hubs;
using backend.Presentation.Protocol;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace backend.Presentation.Handlers;

/// <summary>
/// Handler for booking-related commands (book ticket, cancel booking, view history).
/// Processes booking requests with session validation and returns appropriate responses.
/// Integrates with SignalR for real-time seat availability notifications.
/// </summary>
public class BookingHandler
{
	private readonly IBookingService _bookingService;
	private readonly IAuthenticationService _authenticationService;
	private readonly IHubContext<BookingHub> _hubContext;

	public BookingHandler(IBookingService bookingService, IAuthenticationService authenticationService,
		IHubContext<BookingHub> hubContext)
	{
		_bookingService = bookingService;
		_authenticationService = authenticationService;
		_hubContext = hubContext;
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
			"HoldSeats" => await HandleHoldSeatsAsync(data),
			"ConfirmHeldSeats" => await HandleConfirmHeldSeatsAsync(data),
			"ReleaseHeldSeats" => await HandleReleaseHeldSeatsAsync(data),
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

		if (request.SeatIds is { Count: > 0 })
		{
			var result = await _bookingService.BookMultipleTicketsAsync(session.UserId, request.TrainId,
				request.SeatIds);

			if (result.Success)
			{
				await NotifySeatBookedAsync(request.TrainId, request.SeatIds);
			}

			return new Response
			{
				Success = result.Success,
				ErrorMessage = result.Success ? null : result.Message,
				Data = result.Success ? new { result.BookingIds, result.Message } : null
			};
		}
		else
		{
			var result = await _bookingService.BookTicketAsync(session.UserId, request.TrainId, request.SeatId);

			if (result.Success)
			{
				await NotifySeatBookedAsync(request.TrainId, [request.SeatId]);
			}

			return new Response
			{
				Success = result.Success,
				ErrorMessage = result.Success ? null : result.Message,
				Data = result.Success ? new { result.BookingId, result.Message } : null
			};
		}
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
		var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
		var result = await _bookingService.CancelBookingAsync(request.BookingId, session.UserId, isAdmin);

		if (result.Success && booking != null)
		{
			await NotifySeatReleasedAsync(booking.TrainId, [booking.SeatId]);
		}

		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { result.Message } : null
		};
	}

	private async Task NotifySeatBookedAsync(int trainId, List<int> seatIds)
	{
		var groupName = $"train_{trainId}";
		await _hubContext.Clients.Group(groupName)
			.SendAsync("SeatBooked", new { TrainId = trainId, SeatIds = seatIds });
	}

	private async Task NotifySeatReleasedAsync(int trainId, List<int> seatIds)
	{
		var groupName = $"train_{trainId}";
		await _hubContext.Clients.Group(groupName)
			.SendAsync("SeatReleased", new { TrainId = trainId, SeatIds = seatIds });
	}

	private async Task NotifySeatHeldAsync(int trainId, List<int> seatIds)
	{
		var groupName = $"train_{trainId}";
		await _hubContext.Clients.Group(groupName)
			.SendAsync("SeatHeld", new { TrainId = trainId, SeatIds = seatIds });
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

		var pageNumber = data["PageNumber"]?.Value<int>();
		var pageSize = data["PageSize"]?.Value<int>();

		if (pageNumber.HasValue && pageSize.HasValue)
		{
			if (pageNumber.Value < 1 || pageSize.Value < 1 || pageSize.Value > 100)
			{
				return new Response
				{
					Success = false,
					ErrorMessage =
						"Invalid pagination parameters. PageNumber must be >= 1, PageSize must be between 1 and 100."
				};
			}

			var pagedBookings = await _bookingService.GetAllBookingsAsync(pageNumber.Value, pageSize.Value);
			return new Response { Success = true, Data = pagedBookings };
		}

		var bookings = await _bookingService.GetAllBookingsAsync();
		return new Response { Success = true, Data = bookings };
	}

	private async Task<Response> HandleHoldSeatsAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<HoldSeatsRequest>();
		if (request == null || string.IsNullOrEmpty(request.SessionToken))
		{
			return new Response { Success = false, ErrorMessage = "Invalid hold seats request." };
		}

		var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
		if (session == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
		}

		if (request.SeatIds.Count == 0)
		{
			return new Response { Success = false, ErrorMessage = "No seats selected." };
		}

		var result = await _bookingService.HoldSeatsAsync(session.UserId, request.TrainId, request.SeatIds);

		if (result.Success)
		{
			await NotifySeatHeldAsync(request.TrainId, request.SeatIds);
		}

		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success
				? new { result.BookingIds, result.ExpiresAt, result.Message }
				: null
		};
	}

	private async Task<Response> HandleConfirmHeldSeatsAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<ConfirmHeldSeatsRequest>();
		if (request == null || string.IsNullOrEmpty(request.SessionToken))
		{
			return new Response { Success = false, ErrorMessage = "Invalid confirm request." };
		}

		var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
		if (session == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
		}

		if (request.BookingIds.Count == 0)
		{
			return new Response { Success = false, ErrorMessage = "No bookings selected." };
		}

		// Get booking details for SignalR notification
		var bookings = new List<(int TrainId, int SeatId)>();
		foreach (var bookingId in request.BookingIds)
		{
			var booking = await _bookingService.GetBookingByIdAsync(bookingId);
			if (booking != null && booking.UserId == session.UserId && booking.BookingStatus == "Pending")
			{
				bookings.Add((booking.TrainId, booking.SeatId));
			}
		}

		var result = await _bookingService.ConfirmHeldSeatsAsync(session.UserId, request.BookingIds);

		if (result.Success && bookings.Count > 0)
		{
			// Group by train and notify
			var groupedByTrain = bookings.GroupBy(b => b.TrainId);
			foreach (var group in groupedByTrain)
			{
				await NotifySeatBookedAsync(group.Key, group.Select(b => b.SeatId).ToList());
			}
		}

		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? result.Data : null
		};
	}

	private async Task<Response> HandleReleaseHeldSeatsAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<ReleaseHeldSeatsRequest>();
		if (request == null || string.IsNullOrEmpty(request.SessionToken))
		{
			return new Response { Success = false, ErrorMessage = "Invalid release request." };
		}

		var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
		if (session == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
		}

		if (request.BookingIds.Count == 0)
		{
			return new Response { Success = false, ErrorMessage = "No bookings selected." };
		}

		// Get booking details for SignalR notification
		var bookings = new List<(int TrainId, int SeatId)>();
		foreach (var bookingId in request.BookingIds)
		{
			var booking = await _bookingService.GetBookingByIdAsync(bookingId);
			if (booking != null && booking.UserId == session.UserId && booking.BookingStatus == "Pending")
			{
				bookings.Add((booking.TrainId, booking.SeatId));
			}
		}

		var result = await _bookingService.ReleaseHeldSeatsAsync(session.UserId, request.BookingIds);

		if (result.Success && bookings.Count > 0)
		{
			// Group by train and notify
			var groupedByTrain = bookings.GroupBy(b => b.TrainId);
			foreach (var group in groupedByTrain)
			{
				await NotifySeatReleasedAsync(group.Key, group.Select(b => b.SeatId).ToList());
			}
		}

		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { result.Message } : null
		};
	}
}