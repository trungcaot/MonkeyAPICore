using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonkeyAPICore.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

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

        public async Task<IEnumerable<Room>> GetRoomsAsync(CancellationToken ct)
        {
            var query = _context.Rooms
                    .ProjectTo<Room>();

            return await query.ToArrayAsync();
        }
    }
}
