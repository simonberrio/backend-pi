using Microsoft.EntityFrameworkCore;
using Repositories.Models;

namespace Services.Helpers
{
    public static class CapacityExtensions
    {
        public const int ReservationMinutes = 15;

        // Cupos ocupados = aprobados + quienes están pagando con una reserva vigente
        public static Task<int> CountOccupiedSpotsAsync(this IQueryable<EventParticipant> query, int eventId, int? excludeParticipantId = null)
        {
            DateTime now = DateTime.UtcNow;
            int excludeId = excludeParticipantId ?? 0;

            return query.Where(x => x.EventId == eventId && x.Id != excludeId &&
                    (x.Status == ParticipantStatusEnums.Approved ||
                     (x.Status == ParticipantStatusEnums.AwaitingPayment &&
                      x.Payments.Any(p => p.Status == PaymentStatusEnums.Pending && p.ExpiresAt > now))))
                .CountAsync();
        }

        public static bool RequiresPayment(this Event evento) => evento.Price.HasValue && evento.Price.Value > 0;
    }
}
