using FFMpegCore;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Runtime.InteropServices;
using TranscodeNowWebServer.Data;

namespace TranscodeNowWebServer.Pages.Options;

public partial class Options
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
            AlteredAudioStream = new AudioStream();

            var asFields = typeof(AudioStream).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in asFields)
            {
                object value = field.GetValue(InitialAudioStream);
                field.SetValue(AlteredAudioStream, value);
            }

            if (!hasVideo)
            {
                var msFields = typeof(MediaStream).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo field in msFields)
                {
                    object value = field.GetValue(InitialAudioStream);
                    field.SetValue(AlteredAudioStream, value);
                }
            }

            ImageHeight = InitialVideoStream.Height;
            ImageWidth = InitialVideoStream.Width;
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
        if (int.TryParse(width, out int value))
        {
            ImageWidth = value;
            return;
        }
    }

    private void HandleVideoHeightChange(string height)
    {
        if (int.TryParse(height, out int value))
        {
            ImageHeight = value;
            return;
        }
    }

    private async void OnButtonClick()
    {
        if (InitialVideoStream is null && InitialAudioStream is null) return;

        if (!IsChangesMade()) return;

        userOptionsService.UserOptions = GetSelectedOptions(InitialVideoStream is not null, InitialAudioStream is not null);

        navManager.NavigateTo("transcode");
    }

    private UserOptions GetSelectedOptions(bool hasVideo, bool hasAudio)
    {
        var message = new UserOptions();
        if(hasVideo) GetVideoOptions(ref message);
        if(hasAudio) GetAudioOptions(ref message);
        return message;
    }

    private void GetAudioOptions(ref UserOptions message)
    {
        return;
    }

    private void GetVideoOptions(ref UserOptions message)
    {
        message.Height = AlteredVideoStream.Height;
        message.Width = AlteredVideoStream.Width;
    }

    private bool IsChangesMade()
    {
        var fields = typeof(VideoStream)
           .GetFields(
           BindingFlags.Instance |
           BindingFlags.Public |
           BindingFlags.NonPublic
       );

        bool changesMade = false;

        foreach (FieldInfo field in fields)
        {
            object initialValue = field.GetValue(InitialVideoStream);
            object alteredValue = field.GetValue(AlteredVideoStream);

            changesMade |= !Equals(initialValue, alteredValue);
        }
        return changesMade;
    }
}