using Microsoft.JSInterop;

namespace TranscodeNowWebServer.Pages;

public partial class Download
{
    private async Task DownloadFileFromURL()
    {
        var fileName = $"transcoded_{fileService.UploadedFileModel.RandomFileName}";
        var friendlyFileName = $"transcoded_{fileService.UploadedFileModel.OriginalFileName}";
        var fileURL = $"https://localhost/uploads/{fileName}";
        await js.InvokeVoidAsync("triggerFileDownload", friendlyFileName, fileURL);
    }
}