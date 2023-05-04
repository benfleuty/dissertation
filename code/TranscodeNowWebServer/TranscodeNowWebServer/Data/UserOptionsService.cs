using TranscodeNowWebServer.Interfaces;

namespace TranscodeNowWebServer.Data;

public class UserOptionsService : IUserOptions
{
    public UserOptions UserOptions { get; set; }
}
