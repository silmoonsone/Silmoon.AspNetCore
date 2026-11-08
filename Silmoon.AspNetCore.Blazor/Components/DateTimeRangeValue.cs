namespace Silmoon.AspNetCore.Blazor.Components
{
    public sealed record DateTimeRangeValue(DateTimeOffset StartTime, DateTimeOffset EndTime, string? Label = null);
}
