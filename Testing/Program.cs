

using Testing;

var shiftService = new ShiftService();  

var shifts = shiftService.GetShiftsByShiftOrderForDate();  
foreach (var (workingDate, shiftName, ranges) in shifts)
{  
    Console.WriteLine(shiftName);
    foreach (var (dateStart, dateEnd) in ranges)
    {     
        Console.WriteLine($"{dateStart} - {dateEnd}");
    } 
    Console.WriteLine();
} 