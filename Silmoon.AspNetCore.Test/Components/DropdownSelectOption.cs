namespace Silmoon.AspNetCore.Test.Components
{
    public class DropdownSelectOption<TValue>
    {
        public TValue Value { get; set; } = default!;
        public string Text { get; set; } = string.Empty;
        public string? Href { get; set; }
        public string? Language { get; set; }
        public string? Direction { get; set; }
        public bool Disabled { get; set; }
    }
}
