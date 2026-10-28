namespace Silmoon.AspNetCore.Test.Components
{
    public sealed class DropdownMenuItem
    {
        public string Key { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public string? IconClass { get; init; }
        public string? Href { get; init; }
        public bool? Selected { get; init; }
        public bool Disabled { get; init; }
        public string? Class { get; init; }
        public IReadOnlyList<DropdownMenuItem> Children { get; init; } = [];
        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; init; }
    }
}
