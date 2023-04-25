using FFMpegCore;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Reflection;

namespace TranscodeNowWebServer.Pages.Transcode;

public partial class Transcode
{
    private VideoStream? InitialVideoStream;
    private AudioStream? InitialAudioStream;

    private VideoStream? AlteredVideoStream;
    private AudioStream? AlteredAudioStream;

    protected override async Task OnInitializedAsync()
    {
        if (fileService.UploadedFileModel is null || fileService.UploadedFileModel.File is null)
        {
            await Console.Out.WriteLineAsync("no file set");
            return;
        }
        SetInitialValues();
    }

    private void SetImageResolution(int width, int height)
    {
        ImageWidth = width;
        ImageHeight = height;
    }

    private void SetInitialValues()
    {
        Console.WriteLine("setting initial values");
        var file = fileService.UploadedFileModel.Data!;
        bool hasVideo = file.VideoStreams.Any();
        bool hasAudio = file.AudioStreams.Any();

        if (hasVideo)
        {
            InitialVideoStream = file.VideoStreams.First();
            AlteredVideoStream = new VideoStream();

            var vsFields = typeof(VideoStream).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in vsFields)
            {
                object value = field.GetValue(InitialVideoStream);
                field.SetValue(AlteredVideoStream, value);
            }

            var msFields = typeof(MediaStream).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in msFields)
            {
                object value = field.GetValue(InitialVideoStream);
                field.SetValue(AlteredVideoStream, value);
            }

            ImageHeight = InitialVideoStream.Height;
            ImageWidth = InitialVideoStream.Width;
        }

        if (hasAudio)
        {
            InitialAudioStream = file.AudioStreams.First();
            AlteredAudioStream = file.AudioStreams.First();
        }

        if (InitialVideoStream is null && InitialAudioStream is null)
        {
            // no valid streams
            return;
        }
        Console.WriteLine("set initial values");

    }

    private int test;

    public int Test
    {
        get => test; set
        {
            test = value;
            StateHasChanged();
        }
    }

    private void HandleVideoWidthChange(string width)
    {
        if(int.TryParse(width, out int value))
        {
            ImageWidth = value;
            return;
        }
    }

    private void HandleVideoHeightChange(string height)
    {
        if(int.TryParse(height, out int value))
        {
            ImageHeight = value;
            return;
        }
    }

    private void change(int val)
    {
        var fields = typeof(VideoStream)
            .GetFields(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        foreach (FieldInfo field in fields)
        {
            object initialValue = field.GetValue(InitialVideoStream);
            object alteredValue = field.GetValue(AlteredVideoStream);

            if (!Equals(initialValue, alteredValue))
                Console.WriteLine($"{field.Name}: {initialValue} -> {alteredValue}");
        }
    }

    private async void OnButtonClick()
    {
        if (InitialVideoStream is null && InitialAudioStream is null) return;

        if (AlteredVideoStream.Height != InitialVideoStream.Height)
        {
            await JSRuntime.InvokeVoidAsync("alert",
                $"Current:{InitialVideoStream.Height}---New:{AlteredVideoStream.Height}");
        }
    }
}