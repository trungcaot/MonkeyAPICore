using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonkeyAPICore.Models;
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
        private readonly MonkeyAPIContext _context;
        public RoomController(MonkeyAPIContext context)
        {
            _context = context;
        }

        [HttpGet(Name = nameof(GetRooms))]
        public IActionResult GetRooms()
        {
            throw new NotImplementedException();
        }

        //room/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomByIdAsync))]
        public async Task<IActionResult> GetRoomByIdAsync(Guid roomId, CancellationToken ct)
        {
            var entity = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId, ct);
            if (entity == null) return NotFound();

            var resource = new Room
            {
                Href = Url.Link(nameof(GetRoomByIdAsync), null),
                Name = entity.Name,
                Rate = entity.Rate / 100.0m
            };
            return Ok(resource);
        }
    }
}
