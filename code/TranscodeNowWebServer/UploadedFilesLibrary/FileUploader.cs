using FluentFTP;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.NetworkInformation;

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
		string username = config["FtpUserName"];
		string password = config["FtpPassword"];
		var creds = new NetworkCredential(username, password);
		return new FtpClient(url, creds);
	}


	public static async Task<bool> UploadFile(string path)
	{
		FtpClient ftp = CreateFtpClient();
		using FileStream fs = File.OpenRead(path);
		var status = ftp.UploadFile(path, Path.GetFileName(path));
		return status == FtpStatus.Success;
	}
}
