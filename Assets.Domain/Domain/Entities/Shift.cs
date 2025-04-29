namespace Assets.Domain.Domain.Entities;

public class Shift(int name, string start, string end)
{
    public int Name { get; set; } = name;
    public TimeSpan StartTime { get; set; } = TimeSpan.Parse(start);
    public TimeSpan EndTime { get; set; } = TimeSpan.Parse(end);
    public bool IsOvernight => EndTime <= StartTime;
}