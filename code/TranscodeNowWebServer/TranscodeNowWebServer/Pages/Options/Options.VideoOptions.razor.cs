namespace TranscodeNowWebServer.Pages.Options;

// Video
public partial class Options
{
    private double? _frameRate;
    private int? _imageHeight = null;
    private int? _imageWidth = null;

    private string? _outputFormat;

    private TimeSpan? _startTime;

    private TimeSpan? _stopTime;

    private int? _videoBitrate;

    private string? _videoCodec;

    private TimeSpan? _videoDuration;

    public double? FrameRate
    {
        get { return _frameRate; }
        set { _frameRate = value; }
    }

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
    public string? OutputFormat
    {
        get { return _outputFormat; }
        set { _outputFormat = value; }
    }

    public TimeSpan? StartTime
    {
        get { return _startTime; }
        set { _startTime = value; }
    }

    public TimeSpan? StopTime
    {
        get { return _stopTime; }
        set { _stopTime = value; }
    }

    public int? VideoBitrate
    {
        get { return _videoBitrate; }
        set { _videoBitrate = value; }
    }
    public string? VideoCodec
    {
        get { return _videoCodec; }
        set { _videoCodec = value; }
    }

    public TimeSpan? VideoDuration
    {
        get { return _videoDuration; }
        set { _videoDuration = value; }
    }

    private int? _rotation;

    public int? Rotation
    {
        get { return _rotation; }
        set { _rotation = value; }
    }

    private bool? _hFlip;

    public bool? HFlip
    {
        get { return _hFlip; }
        set { _hFlip = value; }
    }

    private bool? _vFlip;

    public bool? VFlip
    {
        get { return _vFlip; }
        set { _vFlip = value; }
    }


}

// Audio
public partial class Options
{
    private int? _audioBitrate;
    private string? _audioCodec;

    public int? AudioBitrate
    {
        get { return _audioBitrate; }
        set { _audioBitrate = value; }
    }

    public string? AudioCodec
    {
        get { return _audioCodec; }
        set { _audioCodec = value; }
    }
}