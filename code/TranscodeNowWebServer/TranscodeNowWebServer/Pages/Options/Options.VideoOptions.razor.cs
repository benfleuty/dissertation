namespace TranscodeNowWebServer.Pages.Options;

// Video
public partial class Options
{
    private int? _cropBottom;
    private int? _cropLeft;
    private int? _cropRight;
    private int? _cropTop;
    private int? _endTimeHours;
    private int? _endTimeMinutes;
    private int? _endTimeSeconds;
    private double? _frameRate;
    private bool? _hFlip;
    private int? _imageHeight;
    private int? _imageWidth;

    private string? _outputFormat;

    private int? _rotation;

    private int? _startTimeHours;
    private int? _startTimeMinutes;
    private int? _startTimeSeconds;

    private bool? _vFlip;
    private long? _videoBitrate;

    private string? _videoCodec;

    public int? CropBottom
    {
        get { return _cropBottom; }
        set
        {
            if (value is null)
            {
                _cropBottom = null;
                return;
            }
            int val = value.Value;
            int top = CropTop ?? 0;
            int max = InitialVideoStream.Height;

            if (val + top > max) val = (int)CropBottom;
            else if (val < 0) val = 0;

            _cropBottom = val;
            StateHasChanged();
        }
    }

    public int? CropLeft
    {
        get { return _cropLeft; }
        set
        {
            if (value is null)
            {
                _cropLeft = null;
                return;
            }
            int val = value.Value;
            int right = CropRight ?? 0;
            int max = InitialVideoStream.Width;

            if (val + right > max) val = (int)CropLeft;
            else if (val < 0) val = 0;

            _cropLeft = val;
            StateHasChanged();
        }
    }

    public int? CropRight
    {
        get { return _cropRight; }
        set
        {
            if (value is null)
            {
                _cropRight = null;
                return;
            }
            int val = value.Value;
            int left = CropLeft ?? 0;
            int max = InitialVideoStream.Width;

            if (val + left > max) val = (int)CropRight;
            else if (val < 0) val = 0;

            _cropRight = val;
            StateHasChanged();
        }
    }

    public int? CropTop
    {
        get { return _cropTop; }
        set
        {
            if (value is null)
            {
                _cropTop = null;
                return;
            }
            int val = value.Value;
            int bottom = CropBottom ?? 0;
            int max = InitialVideoStream.Height;

            if (val + bottom > max) val = (int)CropTop;
            else if (val < 0) val = 0;

            _cropTop = val;
            StateHasChanged();
        }
    }

    public int? EndTimeHours
    {
        get => _endTimeHours;
        set
        {
            if (value is null)
            {
                _endTimeHours = null;
                return;
            }
            int result = value.Value;
            if (value < 0) result = 0;
            else if (value > 10) result = 10;

            if (result < StartTimeHours)
                result = StartTimeHours.GetValueOrDefault(0);

            _endTimeHours = result;
        }
    }

    public int? EndTimeMinutes
    {
        get => _endTimeMinutes;
        set
        {
            if (value is null)
            {
                _endTimeMinutes = null;
                return;
            }
            int result = value.Value;
            if (value < 0) result = 0;
            else if (value > 10) result = 10;

            if (result < StartTimeMinutes &&
                EndTimeHours <= StartTimeHours)
                result = StartTimeMinutes.GetValueOrDefault(0);

            _endTimeMinutes = result;
        }
    }

    public int? EndTimeSeconds
    {
        get => _endTimeSeconds;
        set
        {
            if (_endTimeSeconds is null)
            {
                _endTimeSeconds = null;
                return;
            }
            int result = value.Value;
            if (string.IsNullOrEmpty(result.ToString())) result = 0;
            else if (value < 0) result = 0;
            else if (value > 10) result = 10;

            if (result < StartTimeSeconds &&
                (EndTimeHours <= StartTimeHours || EndTimeMinutes <= StartTimeMinutes))
                result = StartTimeSeconds.GetValueOrDefault(0);

            _endTimeSeconds = result;
        }
    }

    public double? FrameRate
    {
        get => _frameRate;
        set
        {
            double result = value ?? _frameRate ?? InitialVideoStream.FrameRate;
            if (result < 1) result = 1;
            else if (result > 120) result = 120;
            _frameRate = result;
        }
    }

    public bool? HFlip
    {
        get => _hFlip;
        set
        {
            _hFlip = value;
        }
    }

    public int? ImageHeight
    {
        get => _imageHeight;
        set
        {
            int val = value ?? _imageHeight ?? InitialVideoStream.Height;
            if (val > 4096) val = 4096;
            if (val < 1) val = 1;

            _imageHeight = val;
            AlteredVideoStream!.Height = val;
        }
    }

    public int? ImageWidth
    {
        get => _imageWidth;
        set
        {
            int val = value ?? _imageWidth ?? InitialVideoStream.Width;

            if (val > 4096) val = 4096;
            if (val < 1) val = 1;

            _imageWidth = val;
            AlteredVideoStream!.Width = val;
        }
    }
    public string? OutputFormat
    {
        get => _outputFormat;
        set { _outputFormat = value; }
    }
    public int? Rotation
    {
        get => _rotation;
        set
        {
            int val = value ?? _rotation ?? InitialVideoStream.Rotation;

            if (val < 0) val += 360;
            if (val > 359) val = 359;

            _rotation = val;
        }
    }

    public int? StartTimeHours
    {
        get => _startTimeHours;
        set
        {
            if (value is null)
            {
                _startTimeHours = null;
                return;
            }
            int result = value.Value;
            if (value < 0) result = 0;
            else if (value > 10) result = 10;

            _startTimeHours = result;
        }
    }
    public int? StartTimeMinutes
    {
        get => _startTimeMinutes;
        set
        {
            if (value is null)
            {
                _startTimeMinutes = null;
                return;
            }
            int result = value.Value;
            TimeSpan fileTime = InitialVideoStream.Duration;
            int hours = fileTime.Hours;
            if (value < 0) result = 0;
            else if (value > 59) result = 59;

            if (hours == 0)
            {
                if (result > fileTime.Minutes) result = fileTime.Minutes;
            }
            _startTimeMinutes = result;
        }
    }
    public int? StartTimeSeconds
    {
        get => _startTimeSeconds;
        set
        {
            if (value is null)
            {
                _startTimeSeconds = null;
                return;
            }
            int result = value.Value;
            TimeSpan fileTime = InitialVideoStream.Duration;
            int mins = fileTime.Minutes;
            if (string.IsNullOrEmpty(result.ToString())) result = 0;
            else if (value < 0) result = 0;
            else if (value > 59) result = 59;

            if (mins == 0)
            {
                if (result > fileTime.Seconds) result = fileTime.Seconds;
            }
            _startTimeSeconds = result;
        }
    }
    public bool? VFlip
    {
        get => _vFlip;
        set
        {
            _vFlip = value;
        }
    }

    public long? VideoBitrate
    {
        get => _videoBitrate;
        set
        {
            long val = value ?? _videoBitrate ?? InitialVideoStream.BitRate;

            if (val > 50000) val = 50000;
            else if (val < 0) val = 0;

            _videoBitrate = val;
        }
    }
    public string? VideoCodec
    {
        get => _videoCodec;
        set { _videoCodec = value; }
    }
}