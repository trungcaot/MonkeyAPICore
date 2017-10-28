﻿using MonkeyAPICore.Models;
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

        Task<IEnumerable<Room>> GetRoomsAsync(
            CancellationToken ct);
    }
}
