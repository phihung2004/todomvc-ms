namespace Todo.Api.Features.Statistics
{
    public record StatsOverviewResponse(
    int Total,
    int Active,
    int Completed,
    int Overdue,           // DueAt < now && !IsCompleted
    int CompletedToday,
    int CompletedThisWeek,
    double CompletionRate,             // Completed / Total
    IReadOnlyList<DailyCount> CompletedByDay);  // 7 ngày gần nhất

    public record DailyCount(DateOnly Date, int Count);
}