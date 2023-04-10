namespace TranscodeNowWebServer.Pages;

public partial class Transcode
{
    private int? ImageHeight = null;
    private int? ImageWidth = null;

    private bool FileInfoReady = false;

    protected override async Task OnInitializedAsync()
    {
        if (fileService.UploadedFileModel is null || fileService.UploadedFileModel.File is null)
        {
            navManager.NavigateTo("transcodenow");
        }
        FileInfoReady = true;
        Console.WriteLine($"FileInfoReady: {FileInfoReady}");
    }

    private void SetImageResolution(int height, int width)
    {
        ImageHeight = height;
        ImageWidth = width;
    }


}