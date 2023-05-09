
namespace TranscodeServerApp;
public class MqqtTranscodeMessage
{
    public MqqtTranscodeMessage(UploadedFileModel fileModel, Useroptions userOptions)
    {
        FileModel = fileModel;
        UserOptions = UserOptions;
    }

    public UploadedFileModel FileModel { get; set; }
    public Useroptions UserOptions { get; set; }

}