namespace Silmoon.AspNetCore.Blazor.Components
{
    public class DropdownSelectOption<TValue>
    {
        public TValue Value { get; set; } = default!;
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public string? Href { get; set; }
        public string? Language { get; set; }
        public string? Direction { get; set; }
        public bool Disabled { get; set; }
    }
}
