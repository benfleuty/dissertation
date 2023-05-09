using System.Text.Json.Serialization;

namespace TranscodeNowWebServer.Pages.Options;

public class GeneralOptions
{
    private AudioFormats _audioFormat;
    private string? _outputFileName;

    private OutputTypes _outputType;

    private VideoFormats _videoFormat;

    public enum AudioFormats
    {
        mp3,
        wav,
        ogg
    }

    public enum OutputTypes
    {
        Audio,
        Video
    }

    public enum VideoFormats
    {
        mp4,
        mkv,
        webm
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AudioFormats AudioFormat
    {
        get { return _audioFormat; }
        set { _audioFormat = value; }
    }

    public string? OutputFileName
    {
        get { return _outputFileName; }
        set { _outputFileName = value; }
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OutputTypes OutputType
    {
        get { return _outputType; }
        set { _outputType = value; }
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VideoFormats VideoFormat
    {
        get { return _videoFormat; }
        set { _videoFormat = value; }
    }
}
