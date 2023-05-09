using System.Text.Json.Serialization;

namespace TranscodeNowWebServer.Pages.Options;

public class GeneralOptions
{
	private string? _outputFileName;

	public string? OutputFileName
	{
		get { return _outputFileName; }
		set { _outputFileName = value; }
	}

	private OutputTypes _outputType;

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public OutputTypes OutputType
	{
		get { return _outputType; }
		set { _outputType = value; }
	}


	public enum OutputTypes
	{
		Audio,
		Video
	}
}
