using Microsoft.JSInterop;

namespace TranscodeNowWebServer.Pages;

public partial class Download
{
    private async Task DownloadFileFromURL()
    {
        var fileName = $"transcoded_{fileService.UploadedFileModel.RandomFileName}";
        string friendlyFileName;
        if (userOptionsService?.UserOptions?.GeneralOptions?.OutputFileName is not null) friendlyFileName = userOptionsService.UserOptions.GeneralOptions.OutputFileName;
        else friendlyFileName = $"transcoded_{fileService.UploadedFileModel.OriginalFileName}";
        var fileURL = $"https://localhost/uploads/{fileName}";
        await js.InvokeVoidAsync("triggerFileDownload", friendlyFileName, fileURL);
    }
}