namespace Silmoon.AspNetCore.Blazor.Components
{
    public sealed record DataGridRowEvent<TItem>(TItem Item, string Key, string? Column, int RowIndex, int ColumnIndex);
}
