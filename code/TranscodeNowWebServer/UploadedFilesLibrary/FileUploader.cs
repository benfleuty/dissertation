using FluentFTP;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace UploadedFilesLibrary;

public class FileUploader
{

    private static FtpClient CreateFtpClient()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<FileUploader>();

        var config = builder.Build();

        string url = "172.18.0.4";
        string username = config["FtpUserName"]
            ?? throw new NullReferenceException();
        string password = config["FtpPassword"]
            ?? throw new NullReferenceException();
        var creds = new NetworkCredential(username, password);

        return new FtpClient(url, creds);
    }

    public static async Task<bool> UploadFile(string path, Action<FtpProgress>? progress = null)
    {
        using var ftp = CreateFtpClient();
        ftp.AutoConnect();
        await Console.Out.WriteLineAsync("connected");
        var status = ftp.UploadFile(path,
                                    Path.GetFileName(path),
                                    default,
                                    default,
                                    default,
                                    progress);

        return status == FtpStatus.Success;
    }

}
