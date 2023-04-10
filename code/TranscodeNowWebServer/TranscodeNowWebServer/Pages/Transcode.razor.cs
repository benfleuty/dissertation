namespace TranscodeNowWebServer.Pages;

public partial class Transcode
{
    private int? ImageHeight = null;
    private int? ImageWidth = null;

    private bool FileInfoReady = false;

    struct Values
    {
        public int? ImageHeight { get; set; }
        public int? ImageWidth { get; set; }
        public string Extension { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        if (fileService.UploadedFileModel is null || fileService.UploadedFileModel.File is null)
        {
            await Console.Out.WriteLineAsync("no file set");
            return;
        }
        FileInfoReady = true;
        Console.WriteLine($"FileInfoReady: {FileInfoReady}");
    }

    private void SetImageResolution(int height, int width)
    {
        ImageHeight = height;
        ImageWidth = width;
    }

    private void SetInitialValues()
    {

    }


}