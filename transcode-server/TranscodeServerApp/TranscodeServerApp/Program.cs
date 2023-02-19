﻿using System.Diagnostics;

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

// Save the stream to a file on disk, and report the progress using the Progress<T> object
using var fileStream = new FileStream(fileName, FileMode.Create);
await stream.CopyToAsync(fileStream);

// Set the output file path
var outputPath = $"{fileName}__transcoded";

// Create a process to run ffmpeg
var process = new Process();

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