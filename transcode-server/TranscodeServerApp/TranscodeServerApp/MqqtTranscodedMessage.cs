namespace TranscodeServerApp;

public class MqqtTranscodedMessage
{
    public string? Message { get; set; }
    public string? State { get; set; }
    public string? OriginalFileName { get; set; }
    public string? NewFileName { get; set; }
}
