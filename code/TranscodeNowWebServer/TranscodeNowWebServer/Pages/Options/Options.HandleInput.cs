namespace TranscodeNowWebServer.Pages.Options;

// Video
public partial class Options
{
    private void HandleRotationChange(string rotation)
    {
        if (int.TryParse(rotation, out int value))
        {
            Rotation = value;
        }
    }

    private void HandleVideoBitrateChange(string bitrate)
    {
        if (int.TryParse(bitrate, out int value))
        {
            VideoBitrate = value;
        }
    }

    private void HandleVideoCodecChange(string codec)
    {
        VideoCodec = codec;
    }

    private void HandleVideoFrameRateChange(string fps)
    {
        if (double.TryParse(fps, out double value))
        {
            FrameRate = value;
        }
    }

    private void HandleVideoHeightChange(string height)
    {
        if (int.TryParse(height, out int value))
        {
            ImageHeight = value;
        }
    }

    private void HandleVideoWidthChange(string width)
    {
        if (int.TryParse(width, out int value))
        {
            ImageWidth = value;
        }
    }
}

// Audio
public partial class Options
{
    private void HandleAudioBitrateChange(string bitrate)
    {
        if (int.TryParse(bitrate, out int value))
        {
            AudioBitrate = value;
        }
    }

    

    private void HandleAudioCodecChange(string codec)
    {
        AudioCodec = codec;
    }

}
