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

        public async Task<PagedResults<Room>> GetRoomsAsync(PagingOptions pagingOptions,
            SortOptions<Room, RoomEntity> sortOptions,
            SearchOptions<Room, RoomEntity> searchOptions,
            CancellationToken ct)
        {
            IQueryable<RoomEntity> query = _context.Rooms;
            // step 1: searching data by parameters
            query = searchOptions.Apply(query);
            // step 2: sorting data after searching
            query = sortOptions.Apply(query);


            var size = await query.CountAsync(ct);

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<Room>()
                .ToArrayAsync(ct);
            return new PagedResults<Room>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}
