using Microsoft.AspNetCore.Components.Forms;
using TranscodeNowWebServer.Data;

namespace TranscodeNowWebServer.Pages;

public partial class NewUpload
{
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
        if (GetFileData(e) == false) { return; }
        await ValidateSelectedFile();
    }

    private bool GetFileData(InputFileChangeEventArgs e)
    {
        if (e.File == null)
        {
            return false;
        }

        var uploadedFile = new UploadedFileModel
        {
            OriginalFileName = e.File.Name,
            RandomFileName = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{Path.GetExtension(e.File.Name)}",
            Data = e.File
        };
        fileService.UploadedFileModel = uploadedFile;
        return true;
    }

    private async Task ValidateSelectedFile()
    {
        CurrentStep = Steps.VerifyFile;
        if (fileService.UploadedFileModel.Data == null)
        {
            Message = "No file given";
            return;
        }


        bool successfullyUploaded = await UploadFileAsync(fileService.UploadedFileModel.Data);

        if (successfullyUploaded)
        {
            Message = "File uploaded successfully";
            CurrentStep = Steps.GetTranscodeOptions;
            navManager.NavigateTo("transcode");
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

        if (fileService.UploadedFileModel.OriginalFileName == null ||
            fileService.UploadedFileModel.RandomFileName == null ||
            fileService.UploadedFileModel.Data == null)
        {
            return false;
        }

        string filePath = Path.Combine(uploadPath, fileService.UploadedFileModel.RandomFileName);

        // Create a file stream to read the selected file
        Stream? streamRead;
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
        FileStream? fstreamWrite;
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
            int bytesRead = await streamRead.ReadAsync(buffer);
            if (bytesRead == 0)
            {
                break;
            }

            await fstreamWrite.WriteAsync(buffer.AsMemory(0, bytesRead));
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