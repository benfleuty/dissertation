using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace TranscodeNowWebServer.Pages;

public partial class Download
{
    private async Task DownloadFileFromURL()
    {
        // this hack is so ducking dirty that i need at least 3 showers
        // chromium will accept a mkv or webm under the disguise of an mp4
        await Console.Out.WriteLineAsync(userOptionsService.UserOptions.GeneralOptions.OutputFileName);
        if (userOptionsService.UserOptions.GeneralOptions.OutputFileName.EndsWith("mp4"))
        {
            var fileName = userOptionsService.UserOptions.GeneralOptions.OutputFileName;
            var fileURL = $"https://localhost/uploads/{fileName}";
            await js.InvokeVoidAsync("triggerFileDownload", fileName, fileURL);
        }
        else
        {
            var actualFileName = userOptionsService.UserOptions.GeneralOptions.OutputFileName;
            var fakeFileName = actualFileName + ".mp4";
            File.Move(
                Path.Combine(env.WebRootPath,"uploads", actualFileName),
                Path.Combine(env.WebRootPath,"uploads", fakeFileName));

            var fileUrl = $"https://localhost/uploads/{fakeFileName}";
            await js.InvokeVoidAsync("triggerFileDownload", actualFileName, fileUrl);
        }
    }

}