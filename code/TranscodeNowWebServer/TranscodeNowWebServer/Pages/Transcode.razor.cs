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

namespace TranscodeNowWebServer.Pages;

public partial class Transcode
{
    private int? ImageHeight = null;
    private int? ImageWidth = null;

    private bool FileInfoReady = false;

    protected override async Task OnInitializedAsync()
    {
        if(fileService.UploadedFileModel is null || fileService.UploadedFileModel.Data is null)
        {
            navManager .NavigateTo("transcodenow");
        }
        FileInfoReady = true;
        Console.WriteLine($"FileInfoReady: {FileInfoReady}");
    }

    private void SetImageResolution(int height, int width)
    {
        ImageHeight = height;
        ImageWidth = width;
    }


}