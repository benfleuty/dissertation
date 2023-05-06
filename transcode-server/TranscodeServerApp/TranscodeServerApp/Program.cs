using System.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TranscodeNowWebServer.Data;
using FluentFTP;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;
using FluentFTP.Helpers;
using System.Reflection;
using TranscodeServerApp;
using RabbitMQ.Client.Exceptions;

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
                while (true)
                {
                    Console.WriteLine($"{Now}: Listening for messages on queue: {queueName}");
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
        var data = JsonSerializer.Deserialize<MqqtTranscodeMessage>(message);
        if (data is null)
        {
            Console.WriteLine($"{Now} Received null message");
            return;
        }

        Console.WriteLine("Getting command");
        string? command = GetCommand(data);
        if (command is null)
        {
            Console.WriteLine($"{Now} Invalid parameter for command");
            return;
        }
        Console.WriteLine($"Got command {command}");

        var fileName = data.FileModel.RandomFileName;
        if (fileName == null)
        {
            Console.WriteLine($"{Now} Null file name");
            return;
        }

        Console.WriteLine($"Getting file {fileName}");
        if (!DownloadFile(fileName))
        {
            Console.WriteLine($"{Now} Failed to download file {fileName}");
            return;
        }
        Console.WriteLine($"Got file {fileName}");

        // Create a process to run ffmpeg
        var process = new Process();

        // Configure the process to run ffmpeg with the downloaded file as input
        process.StartInfo.FileName = "ffmpeg";
        process.StartInfo.Arguments = command;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        // Start the process and wait for it to complete
        Console.WriteLine($"Start transcode {fileName} to {GetOutputFileName(fileName)} using\n{process.StartInfo.FileName} {process.StartInfo.Arguments}");
        process.Start();
        process.WaitForExit();
        Console.WriteLine($"Transcoded {fileName} to {GetOutputFileName(fileName)}");


        // Print the output of ffmpeg to the console
        string stdErr = process.StandardError.ReadToEnd();
        if (stdErr.Length > 0)
        {
            Console.WriteLine($"Std Err: {stdErr}");
            // todo better error finding/parsing
        }

        string output = "Transcode complete";
        if (!File.Exists(fileName))
        {
            output = "Failed to transcode";
        }

        Console.WriteLine(output);

        Console.WriteLine("Uploading transcoded file");
        string outFileName = GetOutputFileName(fileName);
        MqqtTranscodedMessage msg = new();
        msg.OriginalFileName = fileName;
        msg.NewFileName = outFileName;
        msg.State = "Success";
        if (!UploadFile(outFileName))
        {
            Console.WriteLine($"Failed to upload file {outFileName}");
            msg.State = "Failed";
            msg.Message = "Failed to upload file";
        }

        try
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            string queueName = fileName;
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            var arguments = new Dictionary<string, object> { { "x-expires", 100 * 60 * 60 * 24 } };
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

            channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{Now} Error: {ex.Message}\nFailed to send transcode result to rabbitmq.");
        }

        Console.WriteLine("finished");

    }

    private static string? GetCommand(MqqtTranscodeMessage data)
    {
        var model = data.FileModel;
        var options = data.UserOptions;

        List<string> commandParts = new();
        // command name
        commandParts.Add("-hide_banner");
        commandParts.Add($"-i {data.FileModel.RandomFileName}");

        // apply user's selected options

        if (options.Height.HasValue && options.Width.HasValue)
        {
            if (options.Height < 1 || options.Width < 1)
            {
                Console.WriteLine($"{Now} Invalid height or width {options.Width}x{options.Height}");
                return null;
            }

            commandParts.Add($"-vf scale={options.Width}:{options.Height}");
        }

        // output file name as last parameter
        string outputFileName = GetOutputFileName(model.RandomFileName);
        commandParts.Add(outputFileName);

        return string.Join(" ", commandParts);
    }

    static string GetOutputFileName(string fileName) => "transcoded_" + fileName;

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