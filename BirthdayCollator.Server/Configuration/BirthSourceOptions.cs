namespace BirthdayCollator.Server.Configuration;

public sealed class BirthSourceOptions
{
    public bool EnableYearParser { get; set; } = true;
    public bool EnableDateParser { get; set; } = true;
    public bool EnableCategoryParser { get; set; } = false;

    public bool EnableGenarianParser { get; set; } = true;

    public bool EnableOnThisDayParser { get; set; } = true;
}
