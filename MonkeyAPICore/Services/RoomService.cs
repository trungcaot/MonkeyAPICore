using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonkeyAPICore.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace MonkeyAPICore.Services
{
    public class RoomService : IRoomService
    {
        private readonly MonkeyAPIContext _context;
        public RoomService(MonkeyAPIContext context)
        {
            _context = context;
        }

        public async Task<Room> GetRoomAsync(Guid id, CancellationToken ct)
        {
            var entity = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == id, ct);
            if (entity == null) return null;

            return Mapper.Map<Room>(entity); 
        }
    }
}
