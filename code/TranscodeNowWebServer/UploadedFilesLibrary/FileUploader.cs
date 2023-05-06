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

        string url = "fileserver";
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
        var status = ftp.UploadFile(path,
                                    Path.GetFileName(path),
                                    default,
                                    default,
                                    default,
                                    progress);

        return status == FtpStatus.Success;
    }


    public static bool DownloadFile(string fileName, string destPath)
    {
        using var ftp = CreateFtpClient();
        ftp.AutoConnect();
        FtpStatus status;
        try
        {
            status = ftp.DownloadFile(destPath,
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

}
