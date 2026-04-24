namespace BirthdayCollator.Server.Configuration;

public sealed class BirthSourceOptions
{
    public bool EnableYearParser { get; set; } = false;
    public bool EnableDateParser { get; set; } = false;
    public bool EnableCategoryParser { get; set; } = false;
    public bool EnableGenarianParser { get; set; } = false;
    public bool EnableOnThisDayParser { get; set; } = true;
    public bool EnableImdbParser { get; set; } = false;
}