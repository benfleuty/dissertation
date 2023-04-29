using Microsoft.AspNetCore.Components.Forms;
using TranscodeNowWebServer.Data;
using UploadedFilesLibrary;

namespace TranscodeNowWebServer.Pages;

public partial class Upload
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
        CurrentStep = Steps.VerifyFile;

        var result = await GetFileData(e);
        if (result.Item1 == false)
        {
            Message = result.Item2;
            return;
        }

        result = await ClientsideFileValidation();
        if (result.Item1 == false)
        {
            Message = result.Item2;
            return;
        }

        result = await UploadFileAsync();
        if (result.Item1 == false)
        {
            Message = result.Item2;
            return;
        }

        result = await ServersideFileValidation();
        if (result.Item1 == false)
        {
            CurrentStep = Steps.GetUserFile;
            Message = result.Item2;
            return;
        }
        
        navManager.NavigateTo("/options");
    }
    private Task<(bool, string)> GetFileData(InputFileChangeEventArgs e)
    {
        string msg = string.Empty;
        bool success = false;
        if (e.File == null)
        {
            msg = "No file has been selected";
            success = false;
            return Task.FromResult((success, msg));
        }
        var uploadedFile = new UploadedFileModel
        {
            OriginalFileName = e.File.Name,
            RandomFileName = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{Path.GetExtension(e.File.Name)}",
            File = e.File
        };
        fileService.UploadedFileModel = uploadedFile;
        success = true;
        return Task.FromResult((success, msg));
    }


    private Task<(bool, string)> ClientsideFileValidation()
    {
        string msg = string.Empty;
        bool success = false;

        if (fileService.UploadedFileModel.File == null)
        {
            msg = "No file given";
            success = false;
        }
        else if (ValidFileExtension(out string mimeType) == false)
        {
            msg = $"The file you have selected is not in a supported format.\nYou provided a {mimeType} type file - only audio or video formats are supported.";
            success = false;
        }
        else success = true;

        return Task.FromResult((success, msg));
    }


    private bool ValidFileExtension(out string mimeType)
    {
        mimeType = fileService.UploadedFileModel.File!
            .ContentType
            .Split("/")
            .First()
            .ToLowerInvariant();

        return mimeType == "audio" || mimeType == "video";
    }

    private async Task<(bool, string)> UploadFileAsync()
    {
        string msg = "There is a problem on our end and your file cannot be uploaded. Please try again.";
        bool success = false;

        CurrentSubStep = SubSteps.UploadFile;
        uploadPath = $"{env.WebRootPath}/uploads";

        success = await UploadDirectoryExistsAsync(uploadPath);

        if (success == false)
            return (success, msg);

        if (fileService.UploadedFileModel == null)
            return (success, msg);

        var fileModel = fileService.UploadedFileModel;
        bool hasOriginalFileName = fileModel.OriginalFileName != null;
        bool hasRandomFileName = fileModel.RandomFileName != null;
        bool hasFile = fileModel.File != null;
        success = hasOriginalFileName && hasRandomFileName && hasFile;

        if (success == false)
            return (success, msg);


        string filePath = Path.Combine(uploadPath, fileModel.RandomFileName!);

        // Create a file stream to read the selected file
        Stream? streamRead;
        try
        {
            streamRead = fileModel.File!.OpenReadStream(fileModel.File.Size);
        }
        catch (IOException e)
        {
            await Console.Out.WriteLineAsync(e.Message);
            return (success, msg);
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
            return (success, msg);
        }

        long totalBytes = fileModel.File.Size;
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

        if (File.Exists(filePath) == false)
            return (success, msg);

        return (true, string.Empty);

        async Task<bool> UploadDirectoryExistsAsync(string uploadPath)
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

    private Task<(bool, string)> ServersideFileValidation()
    {
        string msg = string.Empty;
        bool success = false;

        var ffmpeg = new FFMpeg(Path.Combine(uploadPath, fileService.UploadedFileModel.RandomFileName!));
        if (ffmpeg.IsFileSupported() == false)
        {
            msg = $"The file you have selected is not supported by our system.";
            success = false;
        }
        else
        {
            fileService.UploadedFileModel.Data = ffmpeg.Details;
            success = true;
        }

        return Task.FromResult((success, msg));
    }

}