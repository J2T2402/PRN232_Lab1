namespace PRN232.LMS.Services.Models.Common;

public class ListQueryModel
{
    private const int DefaultPage = 1;
    private const int DefaultSize = 10;
    private const int MaxSize = 100;

    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = DefaultPage;
    public int Size { get; set; } = DefaultSize;
    public string? Expand { get; set; }

    public int NormalizedPage => Page < 1 ? DefaultPage : Page;
    public int NormalizedSize => Size < 1 ? DefaultSize : Math.Min(Size, MaxSize);
}
