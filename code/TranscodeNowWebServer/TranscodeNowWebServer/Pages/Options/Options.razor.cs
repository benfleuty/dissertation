using FFMpegCore;
using RabbitMQ.Client;
using System.Reflection;
using System.Text.Json;
using System.Text;
using TranscodeNowWebServer.Data;
using Microsoft.JSInterop;

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
                object? value = field.GetValue(InitialVideoStream);
                field.SetValue(AlteredVideoStream, value);
            }

            var msFields = typeof(MediaStream).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in msFields)
            {
                object? value = field.GetValue(InitialVideoStream);
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
                object? value = field.GetValue(InitialAudioStream);
                field.SetValue(AlteredAudioStream, value);
            }

            if (!hasVideo)
            {
                var msFields = typeof(MediaStream).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo field in msFields)
                {
                    object? value = field.GetValue(InitialAudioStream);
                    field.SetValue(AlteredAudioStream, value);
                }
            }
        }

        if (InitialVideoStream is null && InitialAudioStream is null)
        {
            // no valid streams
            Console.WriteLine("No valid streams");
            return;
        }
    }

    private void OnButtonClick()
    {
        if (InitialVideoStream is null && InitialAudioStream is null) return;

        if (!IsChangesMade()) return;

        userOptionsService.UserOptions =
            GetSelectedOptions(InitialVideoStream is not null, InitialAudioStream is not null);

        var msg = new MqqtTranscodeMessage(
            fileService.UploadedFileModel,
            userOptionsService.UserOptions
            );

        SendTranscodeMessage(msg);
        navManager.NavigateTo("result");
    }

    private void SendTranscodeMessage(MqqtTranscodeMessage msg)
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        string queueName = "transcode-in";
        string message = JsonSerializer.Serialize(msg);
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    private UserOptions GetSelectedOptions(bool hasVideo, bool hasAudio)
    {
        var message = new UserOptions();
        if (hasVideo) GetVideoOptions(ref message);
        if (hasAudio) GetAudioOptions(ref message);
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