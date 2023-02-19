using System.Diagnostics;
using System.Text.RegularExpressions;

var http = new HttpClient();
var url = "http://distribution.bbb3d.renderfarming.net/video/mp4/bbb_sunflower_1080p_60fps_normal.mp4";

//using (var progress = new ProgressBar())
//{
//	for (int i = 0; i <= 100; i++)
//	{
//		progress.Report((double)i / 100);
//		Thread.Sleep(20);
//	}
//}

Console.WriteLine($"Downloading {url}");

//var response = await http.GetAsync(url);

Console.WriteLine($"Downloaded {url}");

// Check if the response is successful
//if (response.IsSuccessStatusCode == false)
//{
//	Console.WriteLine($"Response was not a success: {response.StatusCode}");
//	// todo handle failure
//	return;
//}

Console.WriteLine("Response was a success");

//var stream = await response.Content.ReadAsStreamAsync();
var stream = File.OpenRead("input.mp4");
var fileName = Path.GetRandomFileName();

// Save the stream to a file on disk
using var fileStream = new FileStream(fileName, FileMode.Create);
await stream.CopyToAsync(fileStream);

// Validate that it is a valid MIME type
var process = new Process();

// Configure the process to run ffmpeg with the downloaded file as input
process.StartInfo.FileName = "file";
process.StartInfo.Arguments = $"--mime-type -b {fileName}";
process.StartInfo.UseShellExecute = false;
process.StartInfo.RedirectStandardOutput = true;
process.StartInfo.RedirectStandardError = true;

process.Start();
process.WaitForExit();

string mimeType = process.StandardOutput.ReadToEnd();
// if not and audio or video type
if(Regex.Match(mimeType, "^(audio|video)/.*").Success == false)
{
	// todo handle invalid file
	Console.WriteLine($"The given file is not supported:{mimeType}");
	return;
}

Console.WriteLine($"Supported MIME type:{mimeType}");


// Set the output file path
var outputPath = $"{fileName}__transcoded";

// Create a process to run ffmpeg
process = new Process();

// Configure the process to run ffmpeg with the downloaded file as input
process.StartInfo.FileName = "ffmpeg";
process.StartInfo.Arguments = $"-i \"{fileName}\" -c:v libx264 -preset ultrafast -crf 22 -c:a copy \"{outputPath}.mkv\"";
process.StartInfo.UseShellExecute = false;
process.StartInfo.RedirectStandardOutput = true;
process.StartInfo.RedirectStandardError = true;

// Start the process and wait for it to complete
Console.WriteLine($"Starting to transcode {fileName} to {outputPath}\nusing {process.StartInfo.FileName} {process.StartInfo.Arguments}");
process.Start();
process.WaitForExit();
Console.WriteLine($"Transcoded {fileName} to {outputPath}");


// Print the output of ffmpeg to the console
Console.WriteLine(process.StandardOutput.ReadToEnd());
Console.WriteLine(process.StandardError.ReadToEnd());

var files = Directory.GetFiles(".");
foreach (var file in files)
{
	Console.WriteLine(file);
}