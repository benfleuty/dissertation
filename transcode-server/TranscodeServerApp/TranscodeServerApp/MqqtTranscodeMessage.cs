namespace TranscodeNowWebServer.Data
{
    public class MqqtTranscodeMessage
    {
        public MqqtTranscodeMessage(UploadedFileModel fileModel, UserOptions userOptions)
        {
            FileModel = fileModel;
            UserOptions = userOptions;
        }

        public UploadedFileModel FileModel { get; set; }
        public UserOptions UserOptions { get; set; }

    }
}
