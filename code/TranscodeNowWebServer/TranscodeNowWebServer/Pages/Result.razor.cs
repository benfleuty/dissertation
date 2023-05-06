using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Exceptions;
using TranscodeNowWebServer.Data;
using System.Text.Json;
using UploadedFilesLibrary;

namespace TranscodeNowWebServer.Pages;

public partial class Result
{
    IModel? channel;
    string? fileName;
    bool listen = true;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        var model = UploadedFileService.UploadedFileModel;

        if (model == null)
        {
            Console.WriteLine($"{Now} UploadedFileService.UploadedFileModel is null");
            return;
        }

        fileName = model.RandomFileName;
        if (fileName == null)
        {
            Console.WriteLine($"{Now} fileName is null");
            return;
        }
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRenderAsync(firstRender);
        if (!firstRender) { return Task.CompletedTask; }
        if (fileName is null) { return Task.CompletedTask; }

        string queueName = fileName;

        var factory = new ConnectionFactory() { HostName = "rabbitmq" };

        using var connection = factory.CreateConnection();

        channel = connection.CreateModel();

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += HandleMessage;
        var arguments = new Dictionary<string, object> { { "x-expires", 100 * 60 * 60 * 24 } };
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
        while (listen)
        {
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            Console.WriteLine($"{Now} Listening for transcode result for {fileName}");
            Thread.Sleep(5000);
        }

        return Task.CompletedTask;
    }

    private static string Now => DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
    private void HandleMessage(object? sender, BasicDeliverEventArgs ea)
    {
        if (fileName is null)
        {
            Console.WriteLine($"{Now} Ignoring message hit because fileName is not set");
            return;
        }
        if (channel is null)
        {
            Console.WriteLine($"{Now} Ignoring message hit because channel is not set");
            return;
        }
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        if (message.Contains(fileName!))
        {
            Console.WriteLine($"Received message: {message}");
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            listen = false;
            channel.Close();
            GetFileFromFileserver();
        }
    }

    private void GetFileFromFileserver()
    {
        string downloadPath = $"{env.WebRootPath}/uploads";
        if (!Directory.Exists(downloadPath)) { Directory.CreateDirectory(downloadPath); }
        var file = $"transcoded_{fileName}";
        downloadPath = Path.Combine(downloadPath, file);
        FileUploader.DownloadFile(file, downloadPath);
        channel = null;
        nav.NavigateTo("/download");
    }
}