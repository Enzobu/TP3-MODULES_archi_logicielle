using Hotel.Housekeeping.Contracts;

namespace Hotel.Housekeeping;

internal sealed class HousekeepingService : IHousekeepingService
{
    private readonly IHousekeepingScheduleDataSource _dataSource;
    private readonly ICleaningPolicy _cleaningPolicy;
    private readonly ICleaningTaskNotifier _notifier;

    public HousekeepingService(
        IHousekeepingScheduleDataSource dataSource,
        ICleaningPolicy cleaningPolicy,
        ICleaningTaskNotifier notifier)
    {
        _dataSource = dataSource;
        _cleaningPolicy = cleaningPolicy;
        _notifier = notifier;
    }

    public IReadOnlyList<CleaningTask> GetSchedule(DateTime date)
    {
        var reservations = _dataSource.GetReservationsByDateRange(date, date.AddDays(1));
        var allTasks = new List<CleaningTask>();

        foreach (var reservation in reservations)
        {
            var tasks = _cleaningPolicy.GenerateTasks(reservation);
            var todayTasks = tasks.Where(task => task.Date.Date == date.Date).ToList();
            allTasks.AddRange(todayTasks);
        }

        return allTasks;
    }

    public void NotifyHousekeeping(DateTime date)
    {
        var tasks = GetSchedule(date);
        if (tasks.Count > 0)
            _notifier.NotifyNewTasks(tasks);
    }
}

internal interface ICleaningPolicy
{
    IReadOnlyList<CleaningTask> GenerateTasks(HousekeepingReservation reservation);
}

internal sealed class StandardCleaningPolicy : ICleaningPolicy
{
    public IReadOnlyList<CleaningTask> GenerateTasks(HousekeepingReservation reservation)
    {
        var tasks = new List<CleaningTask>();

        var current = reservation.CheckIn.AddDays(2);
        while (current < reservation.CheckOut)
        {
            tasks.Add(new CleaningTask
            {
                RoomId = reservation.RoomId,
                Date = current,
                Type = "LinenChange",
                ReservationId = reservation.Id
            });

            current = current.AddDays(2);
        }

        tasks.Add(new CleaningTask
        {
            RoomId = reservation.RoomId,
            Date = reservation.CheckOut,
            Type = "Departure",
            ReservationId = reservation.Id
        });

        return tasks;
    }
}

internal sealed class VipCleaningPolicy : ICleaningPolicy
{
    public IReadOnlyList<CleaningTask> GenerateTasks(HousekeepingReservation reservation)
    {
        var tasks = new List<CleaningTask>();

        for (var day = reservation.CheckIn.AddDays(1); day < reservation.CheckOut; day = day.AddDays(1))
        {
            tasks.Add(new CleaningTask
            {
                RoomId = reservation.RoomId,
                Date = day,
                Type = "VipCleaning",
                ReservationId = reservation.Id
            });
        }

        tasks.Add(new CleaningTask
        {
            RoomId = reservation.RoomId,
            Date = reservation.CheckOut,
            Type = "Departure",
            ReservationId = reservation.Id
        });

        return tasks;
    }
}
