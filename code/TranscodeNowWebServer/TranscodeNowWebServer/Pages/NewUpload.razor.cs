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
using System.Net.Http;
using System.IO;

namespace TranscodeNowWebServer.Pages;

public partial class NewUpload
{
    private int step = 0;
    private IBrowserFile? selectedFile;
    private Steps CurrentStep = Steps.GetUserFile;
    private SubSteps CurrentSubStep = SubSteps.None;
    private int? _progress;
    private string uploadPath = string.Empty;

    private string Message = string.Empty;

    private enum Steps
    {
        GetUserFile,
        VerifyFile,
        GetTranscodeOptions,
        ReadyToUpload
    }

    private enum SubSteps
    {
        None,
        UploadFile
    }
    public int? Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            StateHasChanged();
        }
    }

    private async Task OnSelectedFileChange(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
        await ValidateSelectedFile();
    }

    private async Task ValidateSelectedFile()
    {
        CurrentStep = Steps.VerifyFile;
        if (selectedFile == null)
        {
            Message = "No file given";
            return;
        }


        bool successfullyUploaded = await UploadFileAsync(selectedFile);

        if (successfullyUploaded)
        {
            Message = "File uploaded successfully";
            //CurrentStep = Steps.GetTranscodeOptions;
        }
        else
        {
            Message = "File upload failed";
        }
    }


    private async Task<bool> UploadFileAsync(IBrowserFile file)
    {
        CurrentSubStep = SubSteps.UploadFile;
        uploadPath = $"{env.WebRootPath}/uploads";

        if (!await UploadDirectoryExists(uploadPath))
        {
            return false;
        }

        string filePath = Path.Combine(uploadPath, file.Name);

        var formData = new MultipartFormDataContent();

        // Create a file stream to read the selected file
        Stream? streamRead = null;
        try
        {
            streamRead = file.OpenReadStream(file.Size);
        }
        catch (IOException e)
        {
            await Console.Out.WriteLineAsync(e.Message);
            return false;
        }

        // Create a file stream to write the selected file to the uploads directory
        FileStream? fstreamWrite = null;
        try
        {
            fstreamWrite = File.Create(filePath);
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
            return false;
        }

        long totalBytes = file.Size;
        long bytesWritten = 0;

        while (bytesWritten < totalBytes)
        {
            byte[] buffer = new byte[81920];
            int bytesRead = await streamRead.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            await fstreamWrite.WriteAsync(buffer, 0, bytesRead);
            bytesWritten += bytesRead;

            // Calculate the progress percentage
            Progress = (int)Math.Round((double)bytesWritten / totalBytes * 100);
        }

        await streamRead.CopyToAsync(fstreamWrite);
        streamRead.Close();
        fstreamWrite.Close();

        return File.Exists(filePath);

        async Task<bool> UploadDirectoryExists(string uploadPath)
        {
            if (!Directory.Exists(uploadPath))
            {
                try
                {
                    Directory.CreateDirectory(uploadPath);
                }
                catch (Exception e)
                {
                    await Console.Out.WriteLineAsync(e.Message);
                    return false;
                }
            }

            return true;
        }
    }
}