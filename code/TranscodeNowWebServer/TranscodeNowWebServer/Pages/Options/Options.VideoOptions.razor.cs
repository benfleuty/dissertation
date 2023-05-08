using FFMpegCore;
using FFMpegCore.Enums;
using UploadedFilesLibrary;

namespace TranscodeNowWebServer.Pages.Options;

// Video
public class VideoOptions
{
    private VideoStream _videoStream;

    private int? _cropBottom;
    private int? _cropLeft;
    private int? _cropRight;
    private int? _cropTop;
    private int? _endTimeHours;
    private int? _endTimeMinutes;
    private int? _endTimeSeconds;
    private double? _frameRate;
    private int? _height;
    private bool _hFlip = false;
    private string? _outputFormat;          // TODO: IMPLEMENT
    private int? _rotation;
    private int? _startTimeHours = 0;
    private int? _startTimeMinutes = 0;
    private int? _startTimeSeconds = 0;
    private bool _vFlip = false;
    private long? _bitrate;
    private string? _codec;            // TODO: IMPLEMENT
    private int? _width;

    public VideoOptions(VideoStream videoStream)
    {
        _videoStream = videoStream;
        EndTimeHours = videoStream.Duration.Hours;
        EndTimeMinutes = videoStream.Duration.Minutes;
        EndTimeSeconds = videoStream.Duration.Seconds;
        FrameRate = videoStream.FrameRate;
        Height = videoStream.Height;
        Rotation = videoStream.Rotation;
        Height = videoStream.Height;
        Width = videoStream.Width;
        BitRate = videoStream.BitRate;
    }

    public int? CropBottom
    {
        get { return _cropBottom; }
        set
        {
            if (value is null) { _cropBottom = null; return; }
            int val = value.Value;
            if (val == 0) { _cropBottom = null; return; }
            int top = CropTop ?? 0;
            int max = _videoStream.Height;

            if (val + top > max) val = CropBottom ?? 0;
            else if (val < 0) val = 0;

            _cropBottom = val;
        }
    }

    public int? CropLeft
    {
        get { return _cropLeft; }
        set
        {
            if (value is null) { _cropLeft = null; return; }
            int val = value.Value;
            if (val == 0) { _cropLeft = null; return; }
            int right = CropRight ?? 0;
            int max = _videoStream.Width;

            if (val + right > max) val = CropLeft ?? 0;
            else if (val < 0) val = 0;

            _cropLeft = val;

        }
    }

    public int? CropRight
    {
        get { return _cropRight; }
        set
        {
            if (value is null) { _cropRight = null; return; }
            int val = value.Value;
            if (val == 0) { _cropRight = null; return; }
            int left = CropLeft ?? 0;
            int max = _videoStream.Width;

            if (val + left > max) val = CropRight ?? 0;
            else if (val < 0) val = 0;

            _cropRight = val;

        }
    }

    public int? CropTop
    {
        get { return _cropTop; }
        set
        {
            if (value is null) { _cropTop = null; return; }
            int val = value.Value;
            if (val == 0) { _cropTop = null; return; }
            int bottom = CropBottom ?? 0;
            int max = _videoStream.Height;

            if (val + bottom > max) val = CropTop ?? 0;
            else if (val < 0) val = 0;

            _cropTop = val;

        }
    }

    public int? EndTimeHours
    {
        get => _endTimeHours;
        set
        {
            if (value is null) { _endTimeHours = null; return; }
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
            if (value is null) { _endTimeMinutes = null; return; }
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
            if (value is null) { _endTimeSeconds = null; return; }
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
            double result = value ?? _frameRate ?? _videoStream.FrameRate;
            if (result < 1) result = 1;
            else if (result > 120) result = 120;
            _frameRate = result;
        }
    }

    public int? Height
    {
        get => _height;
        set
        {
            int val = value ?? _height ?? _videoStream.Height;
            if (val > 4096) val = 4096;
            if (val < 1) val = 1;

            _height = val;
        }
    }

    public bool HFlip
    {
        get => _hFlip;
        set
        {
            _hFlip = value;
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
            int val = value ?? _rotation ?? _videoStream.Rotation;

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
            if (value is null) { _startTimeHours = null; return; }
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
            if (value is null) { _startTimeMinutes = null; return; }
            int result = value.Value;
            TimeSpan fileTime = _videoStream.Duration;
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
            if (value is null) { _startTimeSeconds = null; return; }
            int result = value.Value;
            TimeSpan fileTime = _videoStream.Duration;
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

    public bool VFlip
    {
        get => _vFlip;
        set
        {
            _vFlip = value;
        }
    }

    public long? BitRate
    {
        get => _bitrate;
        set
        {
            long val = value ?? _bitrate ?? _videoStream.BitRate;

            if (val > 50000) val = 50000;
            else if (val < 0) val = 0;

            _bitrate = val;
        }
    }

    public string? VideoCodec
    {
        get => _codec;
        set { _codec = value; }
    }

    public int? Width
    {
        get => _width;
        set
        {
            int val = value ?? _width ?? _videoStream.Width;

            if (val > 4096) val = 4096;
            if (val < 1) val = 1;

            _width = val;
        }
    }
}