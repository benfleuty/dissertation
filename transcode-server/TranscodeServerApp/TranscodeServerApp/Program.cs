﻿using System.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using FluentFTP;
using System.Net;
using Microsoft.Extensions.Configuration;
using TranscodeServerApp;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Starting");
        string queueName = "transcode-in";

        while (true)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "rabbitmq" };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += HandleMessage;
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                Console.WriteLine($"{Now}: Listening for messages on queue: {queueName}");
                while (true)
                {
                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Now} Error: {ex.Message}\nFailed to connect to rabbitmq. Trying again in 1 second.");
                Thread.Sleep(1000);
            }
        }
    }

    static void HandleMessage(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        if (message is null)
        {
            Console.WriteLine("null message");
            return;
        }

        OnMessageReceived(message);
    }

    static void OnMessageReceived(string message)
    {
        Console.WriteLine($"{Now} Received message: {message}");
        Rootobject? data;
        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            data = JsonSerializer.Deserialize<Rootobject>(message, opts);
        }
        catch (Exception e)
        {
            Console.WriteLine($"{Now} Exception parsing json: {e.Message}");
            return;
        }

        if (data is null)
        {
            Console.WriteLine($"{Now} Received null message");
            return;
        }

        outputName = data.UserOptions.GeneralOptions.OutputFileName;

        Console.WriteLine("Getting command");

        string? command = GetCommand(data);
        if (command is null)
        {
            Console.WriteLine($"{Now} Invalid parameter for command");
            return;
        }
        Console.WriteLine($"Got command {command}");
        var localFileName = data.FileModel.RandomFileName;
        if (localFileName == null)
        {
            Console.WriteLine($"{Now} Null file name");
            return;
        }

        Console.WriteLine($"Getting file {localFileName}");
        if (!DownloadFile(localFileName))
        {
            Console.WriteLine($"{Now} Failed to download file {localFileName}");
            return;
        }
        Console.WriteLine($"Got file {localFileName}");

        // Create a process to run ffmpeg
        var process = new Process();

        // Configure the process to run ffmpeg with the downloaded file as input
        process.StartInfo.FileName = "ffmpeg";
        process.StartInfo.Arguments = command;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        // Start the process and wait for it to complete
        Console.WriteLine($"Start transcode {localFileName} to {outputName} using\n{process.StartInfo.FileName} {process.StartInfo.Arguments}");
        process.Start();
        process.WaitForExit();
        Console.WriteLine($"Transcoded {localFileName} to {outputName}");


        // Print the output of ffmpeg to the console
        string stdErr = process.StandardError.ReadToEnd();
        if (stdErr.Length > 0)
        {
            Console.WriteLine($"Std Err: {stdErr}");
            // todo better error finding/parsing
        }

        string output = "Transcode complete";
        if (!File.Exists(localFileName))
        {
            output = "Failed to transcode";
        }

        Console.WriteLine(output);

        Console.WriteLine("Uploading transcoded file");
        MqqtTranscodedMessage msg = new();
        msg.OriginalFileName = localFileName;
        msg.NewFileName = outputName;
        msg.State = "Success";
        if (!UploadFile(outputName))
        {
            Console.WriteLine($"Failed to upload file {outputName}");
            msg.State = "Failed";
            msg.Message = "Failed to upload file";
        }

        try
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            string queueName = outputName;
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            var arguments = new Dictionary<string, object> { { "x-expires", 100 * 60 * 60 * 24 } };
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

            channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{Now} Error: {ex.Message}\nFailed to send transcode result to rabbitmq.");
        }

        try
        {
            Console.WriteLine($"Delete {data.FileModel.RandomFileName}");
            File.Delete(data.FileModel.RandomFileName);
            Console.WriteLine($"Deleted {data.FileModel.RandomFileName}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not delete {data.FileModel.RandomFileName}: {e.Message}");
        }

        try
        {
            Console.WriteLine($"Delete {outputName}");
            File.Delete(outputName);
            Console.WriteLine($"Deleted {outputName}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not delete {outputName}: {e.Message}");
        }

        Console.WriteLine("finished");
    }



    private static string? GetCommand(Rootobject data)
    {
        var model = data.FileModel;
        var options = data.UserOptions;

        List<string> commandParts = new();
        // command name
        commandParts.Add("-hide_banner");
        commandParts.Add($"-i {model.RandomFileName}");

        // apply user's selected options

        // Get audio commands
        if (options.AudioOptions is not null)
        {
            Console.WriteLine("Getting audio commands");
            GetAudioCommands(ref commandParts, options.AudioOptions);
            Console.WriteLine("Got audio commands");

            // If the user is converting from video to audio, we need to add the -vn flag
            // and set VideoOptions to null. This allows for the audio options to be applied
            // and the video options to be ignored.
            if (options.GeneralOptions.OutputType == "Audio" && options.VideoOptions is not null)
            {
                commandParts.Add("-vn");
                options.VideoOptions = null;
            }
        }
        else Console.WriteLine("Skipping audio commands");

        // Get video commands
        if (options.VideoOptions is not null)
        {
            Console.WriteLine("Getting video commands");
            GetVideoCommands(ref commandParts, options.VideoOptions);
            Console.WriteLine("Got video commands");
        }
        else Console.WriteLine("Skipping video commands");


        // output file name as last parameter
        string overwrite = "-y";
        commandParts.Add(overwrite);
        commandParts.Add(options.GeneralOptions.OutputFileName);
        return string.Join(" ", commandParts);
    }

    private static void GetVideoCommands(ref List<string> commandParts, Videooptions options)
    {
        List<string> dashVf = new();
        // height and width
        if (options.Height.HasValue && options.Width.HasValue)
        {
            if (options.Height < 1 || options.Width < 1)
            {
                Console.WriteLine($"{Now} Invalid height or width {options.Width}x{options.Height}");
                return;
            }
            string res = $"scale={options.Width}:{options.Height}";
            dashVf.Add(res);
        }

        if (options.CropBottom.HasValue ||
            options.CropLeft.HasValue ||
            options.CropRight.HasValue ||
            options.CropTop.HasValue)
        {
            int top = options.CropTop ?? 0;
            int bottom = options.CropBottom ?? 0;
            int left = options.CropLeft ?? 0;
            int right = options.CropRight ?? 0;
            string crop = $"crop=in_w-{left + right}:in_h-{top + bottom}";
            dashVf.Add(crop);
        }

        bool hasStartTime = options.StartTimeHours.HasValue || options.StartTimeMinutes.HasValue || options.StartTimeSeconds.HasValue;
        bool hasEndTime = options.EndTimeHours.HasValue || options.EndTimeMinutes.HasValue || options.EndTimeSeconds.HasValue;

        if (hasStartTime || hasEndTime)
        {
            string startTime = hasStartTime ? $"{options.StartTimeHours ?? 0}:{options.StartTimeMinutes ?? 0}:{options.StartTimeSeconds ?? 0}" : "00:00:00";
            string? endTime = hasEndTime ? $"{options.EndTimeHours ?? 0}:{options.EndTimeMinutes ?? 0}:{options.EndTimeSeconds ?? 0}" : null;

            string startTimeCmd = $"-ss {startTime}";
            string endTimeCmd = endTime is not null ? $"-to {endTime}" : string.Empty;

            string time = $"{startTimeCmd} {endTimeCmd}";
            commandParts.Add(time);
        }

        if (options.FrameRate.HasValue)
        {
            int frameRate = options.FrameRate.Value;
            string framerate = $"-r {frameRate}";
            commandParts.Add(framerate);
        }

        if (options.HFlip.HasValue || options.VFlip.HasValue)
        {
            if (options.HFlip.HasValue && options.HFlip.Value)
            {
                dashVf.Add("hflip");
            }

            if (options.VFlip.HasValue && options.VFlip.Value)
            {
                dashVf.Add("vflip");
            }
        }

        if (options.Rotation.HasValue && options.Rotation > 0)
        {
            string rotate = $"rotate=angle={options.Rotation}*PI/180";
            dashVf.Add(rotate);
        }

        if (options.BitRate.HasValue)
        {
            string bitRate = $"-b:v {options.BitRate}k";
            commandParts.Add(bitRate);
        }

        if (dashVf.Count > 0)
        {
            string allDashVf = $"-vf \"{string.Join(",", dashVf)}\"";
            commandParts.Add(allDashVf);
        }
    }

    private static void GetAudioCommands(ref List<string> commandParts, Audiooptions options)
    {
        if (options.RemoveTrack.HasValue && options.RemoveTrack.Value)
        {
            string removeTrack = "-an";
            commandParts.Add(removeTrack);
            return;
        }

        List<string> dashAf = new();

        if (options.Bitrate.HasValue)
        {
            string bitRate = $"-b:a {options.Bitrate}k";
            commandParts.Add(bitRate);
        }

        if (options.Normalize.HasValue && options.Normalize.Value)
        {
            string normalize = "loudnorm=I=-23:TP=-1:LRA=11";
            dashAf.Add(normalize);
        }

        if (options.Channels.HasValue)
        {
            string channels = $"-ac {options.Channels.Value}";
            commandParts.Add(channels);
        }

        //if (options.SampleRate.HasValue)
        //{
        //    string sampleRate = $"-ar {options.SampleRate.Value}k";
        //    commandParts.Add(sampleRate);
        //}

        if (dashAf.Count > 0)
        {
            string allDashAf = $"-vf \"{string.Join(",", dashAf)}\"";
            commandParts.Add(allDashAf);
        }
    }

    static string outputName = string.Empty;

    private static FtpClient CreateFtpClient()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<Program>();

        var config = builder.Build();

        string url = "fileserver";
        string username = "ftpuser";
        string password = "gwmxrpUH6cXTM5rC";
        var creds = new NetworkCredential(username, password);

        return new FtpClient(url, creds);
    }

    private static bool DownloadFile(string fileName)
    {
        using var ftp = CreateFtpClient();
        ftp.AutoConnect();
        string localPath = $"./{fileName}";
        FtpStatus status;
        try
        {
            status = ftp.DownloadFile(localPath,
                                        fileName,
                                        default,
                                        default,
                                        default);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        return status == FtpStatus.Success;
    }
    private static bool UploadFile(string fileName)
    {
        using var ftp = CreateFtpClient();
        ftp.AutoConnect();
        FtpStatus status;
        try
        {
            status = ftp.UploadFile(fileName,
                                    fileName,
                                    default,
                                    default,
                                    default);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        Console.WriteLine(status.ToString());
        return status == FtpStatus.Success;
    }

    private static string Now => DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
}



public class Rootobject
{
    public Filemodel? FileModel { get; set; }
    public Useroptions? UserOptions { get; set; }
}

public class Filemodel
{
    public int? Id { get; set; }
    public string? OriginalFileName { get; set; }
    public string? RandomFileName { get; set; }
}

public class Useroptions
{
    public Generaloptions? GeneralOptions { get; set; }
    public Videooptions? VideoOptions { get; set; }
    public Audiooptions? AudioOptions { get; set; }
}

public class Generaloptions
{
    public string? OutputFileName { get; set; }
    public string? OutputType { get; set; }
    public string? VideoFormat { get; set; }
    public string? AudioFormat { get; set; }
}

public class Videooptions
{
    public int? CropBottom { get; set; }
    public int? CropLeft { get; set; }
    public int? CropRight { get; set; }
    public int? CropTop { get; set; }
    public int? EndTimeHours { get; set; }
    public int? EndTimeMinutes { get; set; }
    public int? EndTimeSeconds { get; set; }
    public int? FrameRate { get; set; }
    public int? Height { get; set; }
    public bool? HFlip { get; set; }
    public object? OutputFormat { get; set; }
    public int? Rotation { get; set; }
    public int? StartTimeHours { get; set; }
    public int? StartTimeMinutes { get; set; }
    public int? StartTimeSeconds { get; set; }
    public bool? VFlip { get; set; }
    public int? BitRate { get; set; }
    public object? VideoCodec { get; set; }
    public int? Width { get; set; }
}

public class Audiooptions
{
    public int? Channels { get; set; }
    public int? Bitrate { get; set; }
    public bool? Normalize { get; set; }
    public bool? RemoveTrack { get; set; }
    public int? SampleRate { get; set; }
}
