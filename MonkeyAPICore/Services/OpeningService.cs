﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MonkeyAPICore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyAPICore.Services
{
    public class OpeningService : IOpeningService
    {
        private readonly MonkeyAPIContext _context;
        private readonly IDateLogicService _dateLogicService;

        public OpeningService(MonkeyAPIContext context, IDateLogicService dateLogicService)
        {
            _context = context;
            _dateLogicService = dateLogicService;
        }

        public async Task<PagedResults<Opening>> GetOpeningsAsync(
            PagingOptions pagingOptions,
            CancellationToken ct)
        {
            var rooms = await _context.Rooms.ToArrayAsync();

            var allOpenings = new List<Opening>();

            foreach (var room in rooms)
            {
                // Generate a sequence of raw opening slots
                var allPossibleOpenings = _dateLogicService.GetAllSlots(
                        DateTimeOffset.UtcNow,
                        _dateLogicService.FurthestPossibleBooking(DateTimeOffset.UtcNow))
                    .ToArray();

                var conflictedSlots = await GetConflictingSlots(
                    room.Id,
                    allPossibleOpenings.First().StartAt,
                    allPossibleOpenings.Last().EndAt,
                    ct);

                // Remove the slots that have conflicts and project
                var openings = allPossibleOpenings
                    .Except(conflictedSlots, new BookingRangeComparer())
                    .Select(slot => new OpeningEntity
                    {
                        RoomId = room.Id,
                        Rate = room.Rate,
                        StartAt = slot.StartAt,
                        EndAt = slot.EndAt
                    })
                    .Select(model => Mapper.Map<Opening>(model));

                allOpenings.AddRange(openings);
            }

            var pagedOpenings = allOpenings
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value);

            return new PagedResults<Opening>
            {
                Items = pagedOpenings,
                TotalSize = allOpenings.Count
            };
        }

        public async Task<IEnumerable<BookingRange>> GetConflictingSlots(
            Guid roomId,
            DateTimeOffset start,
            DateTimeOffset end,
            CancellationToken ct)
        {
            return await _context.Bookings
                .Where(b => b.Room.Id == roomId && _dateLogicService.DoesConflict(b, start, end))
                // Split each existing booking up into a set of atomic slots
                .SelectMany(existing => _dateLogicService.GetAllSlots(existing.StartAt, existing.EndAt))
                .ToArrayAsync(ct);
        }
    }
}