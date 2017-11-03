using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MonkeyAPICore.Models;
using MonkeyAPICore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyAPICore.Controllers
{
    [Route("/[controller]")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IOpeningService _openingService;
        private readonly IDateLogicService _dateLogicService;
        private readonly IBookingService _bookingService;

        private readonly PagingOptions _defaultPagingOptions;


        public RoomsController(IRoomService roomService,
            IOpeningService openingService,
            IBookingService bookingService,
            IDateLogicService dateLogicService,
            IOptions<PagingOptions> defaultPagingOptions)
        {
            _roomService = roomService;
            _openingService = openingService;
            _dateLogicService = dateLogicService;
            _defaultPagingOptions = defaultPagingOptions.Value;
            _bookingService = bookingService;
        }

        [HttpGet(Name = nameof(GetRoomsAsync))]
        public async Task<IActionResult> GetRoomsAsync(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Room, RoomEntity> sortOptions,
            [FromQuery] SearchOptions<Room, RoomEntity> searchOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var rooms = await _roomService.GetRoomsAsync(pagingOptions,
                sortOptions, 
                searchOptions, 
                ct);

            var collection = PagedCollection<Room>.Create<RoomsResponse>(
                Link.ToCollection(nameof(GetRoomsAsync)),
                rooms.Items.ToArray(),
                rooms.TotalSize,
                pagingOptions);
            collection.Openings = Link.ToCollection(nameof(GetRoomsAsync));
            
            return Ok(collection);
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpeningsAsync))]
        public async Task<IActionResult> GetAllRoomOpeningsAsync(
            [FromQuery] PagingOptions pagingOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var openings = await _openingService.GetOpeningsAsync(pagingOptions, ct);

            var collection = PagedCollection<Opening>.Create(
                Link.ToCollection(nameof(GetAllRoomOpeningsAsync)),
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions
                );

            return Ok(collection);
        }


        //rooms/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomByIdAsync))]
        public async Task<IActionResult> GetRoomByIdAsync(Guid roomId, CancellationToken ct)
        {
            var room = await _roomService.GetRoomAsync(roomId, ct);
            if (room == null) return NotFound();

            return Ok(room);
        }

        // TODO authentication!
        // POST /rooms/{roomId}/bookings
        [HttpPost("{roomId}/bookings", Name = nameof(CreateBookingForRoomAsync))]
        public async Task<IActionResult> CreateBookingForRoomAsync(
            Guid roomId,
            [FromBody] BookingForm bookingForm,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var room = await _roomService.GetRoomAsync(roomId, ct);
            if (room == null) return NotFound();

            var minimumStay = _dateLogicService.GetMinimumStay();
            bool tooShort = (bookingForm.EndAt.Value - bookingForm.StartAt.Value) < minimumStay;
            if (tooShort) return BadRequest(
                new ApiError($"The minimum booking duration is {minimumStay.TotalHours}."));

            var conflictedSlots = await _openingService.GetConflictingSlots(
                roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value, ct);
            if (conflictedSlots.Any()) return BadRequest(
                new ApiError("This time conflicts with an existing booking."));

            // Get the user ID (TODO)
            var userId = Guid.NewGuid();

            var bookingId = await _bookingService.CreateBookingAsync(
                userId, roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value, ct);

            return Created(
                Url.Link(nameof(BookingsController.GetBookingByIdAsync),
                new { bookingId }),
                null);
        }
    }
}
