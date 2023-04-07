using TranscodeNowWebServer.Data;

namespace TranscodeNowWebServer.Interfaces
{
    public interface IUploadedFileService
    {
        UploadedFileModel UploadedFileModel { get; set; }
    }
}
