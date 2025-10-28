namespace KeyShow;

public class Settings
{
    // Visningstid i millisekunder
    public int DisplayTimeMs { get; set; } = 500;

    // Position för hela fönstret: "TopLeft", "TopRight", "BottomLeft", "BottomRight"
    public string Position { get; set; } = "BottomRight";
}