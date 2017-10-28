using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


        public RoomController(IRoomService roomService,
            IOpeningService openingService)
        {
            _roomService = roomService;
            _openingService = openingService;
        }

        [HttpGet(Name = nameof(GetRoomsAsync))]
        public async Task<IActionResult> GetRoomsAsync(CancellationToken ct)
        {
            var rooms = await _roomService.GetRoomsAsync(ct);

            var collectionLink = Link.ToCollection(nameof(GetRoomsAsync));
            var collection = new Collection<Room>
            {
                Self = collectionLink,
                Value = rooms.ToArray()
            };

            return Ok(collection);
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpeningsAsync))]
        public async Task<IActionResult> GetAllRoomOpeningsAsync(CancellationToken ct)
        {
            var openings = await _openingService.GetOpeningsAsync(ct);

            var collection = new Collection<Opening>()
            {
                Self = Link.ToCollection(nameof(GetAllRoomOpeningsAsync)),
                Value = openings.ToArray()
            };

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
