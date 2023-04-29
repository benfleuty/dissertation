using Microsoft.AspNetCore.Components;

namespace TranscodeNowWebServer.Pages.Options;

public partial class Options
{
    private int? _imageHeight = null;
    private int? _imageWidth = null;

    public int? ImageHeight
    {
        get => _imageHeight;
        set
        {
            if (!int.TryParse(value.ToString(), out int result)) return;
            _imageHeight = result;
            AlteredVideoStream!.Height = result;
            StateHasChanged();
        }
    }
    public int? ImageWidth
    {
        get => _imageWidth;
        set
        {
            if (!int.TryParse(value.ToString(), out int result)) return;
            _imageWidth = result;
            AlteredVideoStream!.Width = result;
            StateHasChanged();
        }
    }
}
