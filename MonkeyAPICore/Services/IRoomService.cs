using MonkeyAPICore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyAPICore.Services
{
    public interface IRoomService
    {
        Task<Room> GetRoomAsync(Guid id,
            CancellationToken ct);

        Task<PagedResults<Room>> GetRoomsAsync(
            PagingOptions pagingOptions,
            SortOptions<Room,RoomEntity> sortOptions,
            CancellationToken ct);
    }
}
