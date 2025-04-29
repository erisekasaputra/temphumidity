using Assets.Domain.Domain.Entities;

namespace Testing
{
    public class ShiftService
    {
        public readonly List<Shift> Shifts =
        [ 
            new Shift(1, "07:00:00", "15:59:59"),
            new Shift(2, "16:00:00", "23:59:59"),
            new Shift(3, "00:00:00", "06:59:59")
        ];

        public (int shiftName, DateTime? start, DateTime? end, DateTime? shiftDate) GetCurrentShiftDateStartEnd()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime now = utcNow.AddHours(7); // Local time (WIB)

            List<(DateTime workingDate, int shiftName, List<(DateTime dateStart, DateTime dateEnd)> values)> shifts = GetShiftsByShiftOrderForDate();

            foreach (var (workingDate, shiftName, values) in shifts)
            {
                foreach ((var dateStart, var dateEnd) in values)
                {
                    if (now >= dateStart && now < dateEnd)
                    {
                        return (shiftName, dateStart, dateEnd, workingDate);
                    }
                }
            }

            return (-1, null, null, null);
        }

        public List<(DateTime workingDate, int shiftName, List<(DateTime start, DateTime end)> hourlyRanges)> GetShiftsByShiftOrderForDate()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime now = utcNow.AddHours(7);  

            Console.WriteLine(now);
            var currentShift = GetCurrentShift(now, out DateTime referenceDate);
  
            if (currentShift.Name == Shifts.Last().Name || currentShift.IsOvernight)
            {  
                referenceDate = referenceDate.AddDays(-1);
            }

            var result = new List<(DateTime, int, List<(DateTime, DateTime)>)>();

            DateTime currentStart = referenceDate.Date + Shifts[0].StartTime;
            for (int i = 0; i < Shifts.Count; i++)
            {
                var shift = Shifts[i];
                DateTime start = currentStart;
                DateTime end = shift.IsOvernight
                    ? start.AddDays(1).Date + shift.EndTime
                    : start.Date + shift.EndTime;
 
                result.Add((
                    start.Date,
                    shift.Name,
                    GetHourlyRanges(start, end)
                ));  
                currentStart = end.AddSeconds(1); // atau ganti dengan end[i + 1] jika i + 1 exists
            } 

            return result;
        }

        
        private Shift GetCurrentShift(DateTime now, out DateTime referenceDate)
        {
            foreach (var baseDate in new[] { now.Date, now.Date.AddDays(-1) })
            {
                foreach (var shift in Shifts)
                {
                    DateTime start = baseDate + shift.StartTime;
                    DateTime end = shift.IsOvernight
                        ? start.AddDays(1).Date + shift.EndTime
                        : start.Date + shift.EndTime;

                    if (now >= start && now < end)
                    {
                        referenceDate = baseDate;
                        return shift;
                    }
                }
            }  
            throw new Exception("Current shift not found.");
        } 

        public static List<(DateTime start, DateTime end)> GetHourlyRanges(DateTime start, DateTime end)
        {
            var result = new List<(DateTime start, DateTime end)>();
            DateTime current = start;

            int i = 0;
            while (current < end)
            { 
                DateTime next = current.AddMinutes(59).AddSeconds(59);
                if (i != 0) 
                    next.AddSeconds(-1); 

                if (next > end) next = end;
                    result.Add((current, next));
                current = next.AddSeconds(1);
                i++;
            }

            return result;
        }
    }
}