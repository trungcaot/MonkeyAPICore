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
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IOpeningService _openingService;
        private readonly PagingOptions _defaultPagingOptions;


        public RoomController(IRoomService roomService,
            IOpeningService openingService,
            IOptions<PagingOptions> defaultPagingOptions)
        {
            _roomService = roomService;
            _openingService = openingService;
            _defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpGet(Name = nameof(GetRoomsAsync))]
        public async Task<IActionResult> GetRoomsAsync(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Room, RoomEntity> sortOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var rooms = await _roomService.GetRoomsAsync(pagingOptions,sortOptions, ct);

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


        //room/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomByIdAsync))]
        public async Task<IActionResult> GetRoomByIdAsync(Guid roomId, CancellationToken ct)
        {
            var room = await _roomService.GetRoomAsync(roomId, ct);
            if (room == null) return NotFound();

            return Ok(room);
        }
    }
}
