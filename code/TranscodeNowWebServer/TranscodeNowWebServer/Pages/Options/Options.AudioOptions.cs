
using FFMpegCore;
using Microsoft.AspNetCore.Components;
namespace TranscodeNowWebServer.Pages.Options;
public class AudioOptions
{
    private readonly AudioStream _audioStream;

    private int? _audioBitrate;

    private int? _channels;

    private bool _normalizeAudio;

    private bool _removeTrack;

    private int? _sampleRate;

    public AudioOptions(AudioStream audioStream)
    {
        _audioStream = audioStream;
        //AudioBitrate = (int)(_audioStream.BitRate / 1000);
        //Channels = _audioStream.Channels;
        //SampleRate = _audioStream.SampleRateHz / 1000;
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
    public int? Channels
    {
        get { return _channels; }
        set { _channels = value; }
    }
    public bool NormalizeAudio
    {
        get { return _normalizeAudio; }
        set { _normalizeAudio = value; }
    }
    public bool RemoveTrack
    {
        get { return _removeTrack; }
        set { _removeTrack = value; }
    }
    public int? SampleRate
    {
        get { return _sampleRate; }
        set { _sampleRate = value; }
    }
}