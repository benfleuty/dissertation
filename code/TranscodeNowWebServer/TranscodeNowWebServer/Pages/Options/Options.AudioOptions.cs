
public partial class Options
{
    private int? _audioBitrate;
    private string? _audioCodec;

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