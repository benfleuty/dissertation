using TranscodeNowWebServer.Interfaces;
using TranscodeNowWebServer.Pages.Options;

namespace TranscodeNowWebServer.Data
{
    public class UserOptions
    {
        public GeneralOptions? GeneralOptions { get; set; }
        public VideoOptions? VideoOptions { get; set; }
        public AudioOptions? AudioOptions { get; set; }
    }
}
