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

		string url = "localhost";
		string username = config["FtpUserName"] 
			?? throw new NullReferenceException();
		string password = config["FtpPassword"] 
			?? throw new NullReferenceException();
		var creds = new NetworkCredential(username, password);
		return new FtpClient(url, creds);
	}


	public static bool UploadFile(string path)
	{
		using FtpClient ftp = CreateFtpClient();
		ftp.AutoConnect();
		var status = ftp.UploadFile(path, Path.GetFileName(path));
		return status == FtpStatus.Success;
	}
}
