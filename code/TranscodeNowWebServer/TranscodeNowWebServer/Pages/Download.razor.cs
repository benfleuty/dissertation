using Microsoft.JSInterop;

namespace TranscodeNowWebServer.Pages;

public partial class Download
{
    private async Task DownloadFileFromURL()
    {
        var fileName = userOptionsService.UserOptions.GeneralOptions.OutputFileName;
        var fileURL = $"https://localhost/uploads/{fileName}";
        await js.InvokeVoidAsync("triggerFileDownload", fileName, fileURL);
    }
}