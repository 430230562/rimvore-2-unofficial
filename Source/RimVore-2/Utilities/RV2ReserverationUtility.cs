using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Verse.AI.ReservationManager;

namespace RimVore2
{
    public static class RV2ReserverationUtility
    {
        public static bool IsReserved(this Pawn pawn)
        {
            IEnumerable<Reservation> reservations = pawn.Map?.reservationManager?.ReservationsReadOnly;
            if(reservations.EnumerableNullOrEmpty())
            {
                return false;
            }
            return reservations.Any(reservation => reservation.Target == pawn);
        }
    }
}
