namespace Hotel.Housekeeping.Contracts;

public interface IHousekeepingService
{
    IReadOnlyList<CleaningTask> GetSchedule(DateTime date);
    void NotifyHousekeeping(DateTime date);
}

public interface IHousekeepingScheduleDataSource
{
    IReadOnlyList<HousekeepingReservation> GetReservationsByDateRange(DateTime from, DateTime to);
}

public interface ICleaningTaskNotifier
{
    void NotifyNewTasks(IReadOnlyList<CleaningTask> tasks);
}

public sealed class HousekeepingReservation
{
    public string Id { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}

public sealed class CleaningTask
{
    public string RoomId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ReservationId { get; set; } = string.Empty;
}
