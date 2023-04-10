using FFMpegCore;

namespace TranscodeNowWebServer.Pages;

public partial class Transcode
{
    private int? ImageHeight = null;
    private int? ImageWidth = null;

    private List<VideoStream> InitialVideoStreams;
    private List<VideoStream> AlteredVideoStreams;
    private List<AudioStream> InitialAudioStreams;
    private List<AudioStream> AlteredAudioStreams;

    protected override async Task OnInitializedAsync()
    {
        if (fileService.UploadedFileModel is null || fileService.UploadedFileModel.File is null)
        {
            await Console.Out.WriteLineAsync("no file set");
            return;
        }
        SetInitialValues();
    }

    private void SetImageResolution(int height, int width)
    {
        ImageHeight = height;
        ImageWidth = width;
    }

    private void SetInitialValues()
    {
        var file = fileService.UploadedFileModel.Data!;
        bool hasVideo = file.VideoStreams.Any();
        bool hasAudio = file.AudioStreams.Any();

        if (hasVideo)
        {
            InitialVideoStreams = new List<VideoStream>();
            AlteredVideoStreams = new List<VideoStream>();

            foreach (var videoStream in file.VideoStreams)
            {
                InitialVideoStreams.Add(videoStream);
                AlteredVideoStreams.Add(videoStream);
            }
        }

        if (hasAudio)
        {
            InitialAudioStreams = new List<AudioStream>();
            AlteredAudioStreams = new List<AudioStream>();

            foreach (var AudioStream in file.AudioStreams)
            {
                InitialAudioStreams.Add(AudioStream);
                AlteredAudioStreams.Add(AudioStream);
            }
        }

        if (InitialVideoStreams is null && InitialAudioStreams is null)
        {
            // no valid streams
            return;
        }


    }
}