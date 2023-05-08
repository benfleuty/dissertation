
using Microsoft.AspNetCore.Components;
namespace TranscodeNowWebServer.Pages.Options;
public partial class Options
{
    private int? _audioBitrate;
    private string? _audioCodec;

    private bool? _isStereo;

    public bool? IsStereo
    {
        get { return _isStereo; }
        set { _isStereo = value; Console.WriteLine(IsStereo); }
    }


    public int? AudioBitrate
    {
        get => _audioBitrate;
        set { _audioBitrate = value; }
    }

    public string? AudioCodec
    {
        get => _audioCodec;
        set { _audioCodec = value; }
    }
}