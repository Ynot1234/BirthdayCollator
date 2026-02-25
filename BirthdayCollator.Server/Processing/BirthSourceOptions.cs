namespace BirthdayCollator.Server.Processing;

public sealed class BirthSourceOptions
{
    public bool EnableYearParser { get; set; } = true;
    public bool EnableDateParser { get; set; } = true;
    public bool EnableCategoryParser { get; set; } = true;

    public bool EnableGenarianParser { get; set; } = false;

    public bool EnableOnThisDayParser { get; set; } = true;
}
