
using Microsoft.AspNetCore.Components;
namespace TranscodeNowWebServer.Pages.Options;
public class AudioOptions
{
    private int? _audioBitrate;
    private string? _audioCodec;

    private string? _channels;

    public string? Channels
    {
        get { return _channels; }
        set { _channels = value; }
    }


    public int? AudioBitrate
    {
        get => _audioBitrate;
        set
        {
            if (value is null)
            {
                _audioBitrate = null;
                return;
            }
            _audioBitrate = value;
        }
    }

    private bool _isCustomBitrate;

    public bool IsCustomBitrate
    {
        get { return _isCustomBitrate; }
        set { _isCustomBitrate = value; }
    }


    public string? AudioCodec
    {
        get => _audioCodec;
        set { _audioCodec = value; }
    }

    private bool _normalizeAudio;

    public bool NormalizeAudio
    {
        get { return _normalizeAudio; }
        set { _normalizeAudio = value; }
    }

    private bool _removeTrack;

    public bool RemoveTrack
    {
        get { return _removeTrack; }
        set { _removeTrack = value; }
    }

    private int? _sampleRate;

    public int? SampleRate
    {
        get { return _sampleRate; }
        set { _sampleRate = value; }
    }

}