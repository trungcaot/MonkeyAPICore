using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MonkeyAPICore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyAPICore.Services
{
    public class BookingService : IBookingService
    {
        private readonly MonkeyAPIContext _context;
        private readonly IDateLogicService _dateLogicService;

        public BookingService(
            MonkeyAPIContext context,
            IDateLogicService dateLogicService)
        {
            _context = context;
            _dateLogicService = dateLogicService;
        }

        public async Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            CancellationToken ct)
        {
            var room = await _context.Rooms
                .SingleOrDefaultAsync(r => r.Id == roomId, ct);
            if (room == null) throw new ArgumentException("Invalid room id.");

            var minimumStay = _dateLogicService.GetMinimumStay();
            var total = (int)((endAt - startAt).TotalHours / minimumStay.TotalHours) * room.Rate;

            var id = Guid.NewGuid();

            var newBooking = _context.Bookings.Add(new BookingEntity
            {
                Id = id,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow,
                StartAt = startAt.ToUniversalTime(),
                EndAt = endAt.ToUniversalTime(),
                Room = room,
                Total = total
            });

            var created = await _context.SaveChangesAsync(ct);
            if (created < 1) throw new InvalidOperationException("Could not create the booking.");

            return id;
        }

        public async Task<Booking> GetBookingAsync(
            Guid bookingId,
            CancellationToken ct)
        {
            var entity = await _context.Bookings
                .SingleOrDefaultAsync(b => b.Id == bookingId, ct);

            if (entity == null) return null;

            return Mapper.Map<Booking>(entity);
        }
    }
}
