using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using TranscodeNowWebServer;
using TranscodeNowWebServer.Shared;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FFMpegCore;
using FFMpegCore.Exceptions;

namespace TranscodeNowWebServer.Pages;

public partial class Upload
{
    private DragAndDrop? fileUploader;

    protected async override Task OnInitializedAsync()
    {
        if (fileUploader == null)
        {
            throw new NullReferenceException(nameof(fileUploader));
        }

        return;
    }

    void ClearSelectedFile()
    {
        if (fileUploader is null)
            throw new NullReferenceException("File uploader not present");
        fileUploader.file = null;
    }

    public Task OnFileUploaded(string path)
    {
        IMediaAnalysis fileInfo = GetAVDetails(path);

        if (fileInfo is null)
            fileUploader.ErrorMessage = "The file you have selected is not supported.";

        fileUploader.file = null;
        fileUploader.EnableUpload();
        return Task.CompletedTask;
    }


    private IMediaAnalysis? GetAVDetails(string filePath)
    {
        // Get media information
        IMediaAnalysis mediaInfo = null;
        try
        {
            mediaInfo = FFProbe.Analyse(filePath);
        }
        catch (FFMpegException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }

        if (mediaInfo is null || IsFileSupported(mediaInfo) == false)
        {
            Console.WriteLine($"File not supported:{filePath}");
            return null;
        }

        var fileFormat = mediaInfo.Format.FormatName;

        var videoInfo = mediaInfo.VideoStreams.FirstOrDefault();

        if (videoInfo is not null)
        {
            // Get video information
            var videoCodec = videoInfo.CodecName;
            var videoBitrate = videoInfo.BitRate;
            var videoResolution = $"{videoInfo.Width}x{videoInfo.Height}";
            var videoFrameRate = videoInfo.FrameRate.ToString();
            Console.WriteLine(videoCodec);
            Console.WriteLine(videoBitrate);
            Console.WriteLine(videoResolution);
            Console.WriteLine(videoFrameRate);

        }
        // Get audio information
        var audioInfo = mediaInfo.AudioStreams.FirstOrDefault();
        if (audioInfo is not null)
        {
            var audioFormat = audioInfo.CodecName;
            var audioCodec = audioInfo.CodecName;
            var audioBitrate = audioInfo.BitRate;
            var audioChannels = audioInfo.Channels;
            var audioSamplingRate = audioInfo.SampleRateHz;
            Console.WriteLine(audioFormat);
            Console.WriteLine(audioCodec);
            Console.WriteLine(audioBitrate);
            Console.WriteLine(audioChannels);
            Console.WriteLine(audioSamplingRate);

        }
        return mediaInfo;
    }

    private bool IsFileSupported(IMediaAnalysis mediaInfo)
    {
        try
        {
            if (mediaInfo.VideoStreams.Any() || mediaInfo.AudioStreams.Any())
                return true;
        }
        catch (FFMpegException e)
        {
            Console.WriteLine(e.Message);
        }

        return false;
    }
}