using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using FluentFTP;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UploadedFilesLibrary;
using TranscodeNowWebServer.Data;
using TranscodeNowWebServer.Pages;

namespace TranscodeNowWebServer.Shared
{
    public partial class DragAndDrop
    {
        ElementReference dragAndDropContainer;
        ElementReference fileUploadControl;
        IJSObjectReference? _module;
        IJSObjectReference? _dropZoneInstance;
        bool DisableUploadButton = false;
        bool DisableInputFile = false;
        string uploadPath = string.Empty;
        public string? ErrorMessage = null;
        private int? _progress;
        private InputFile fileInputRef;

        [CascadingParameter] Upload Parent { get; set; }

        [Parameter]
        public EventCallback<IBrowserFile> FileChanged { get; set; }

        public int? Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/dragAndDrop.js") ?? throw new FileNotFoundException("Could not load dragAndDrop.js");
            _dropZoneInstance = await _module.InvokeAsync<IJSObjectReference>("initialiseFileDropZone", dragAndDropContainer, fileUploadControl);
            uploadPath = $"{env.WebRootPath}/uploads";
            if (Directory.Exists(uploadPath) == false)
                Directory.CreateDirectory(uploadPath);
            foreach (string file in Directory.GetFiles(uploadPath))
            {
                File.Delete(file);
            }
        }

        public IBrowserFile? file;
        struct MIME
        {
            public string type;
            public string subtype;
            public MIME(string t, string s)
            {
                type = t;
                subtype = s;
            }

            public override string ToString()
            {
                return $"{type}/{subtype}";
            }

            public bool IsSupported => type == "audio" || type == "video";
        }

        MIME uploadedFileMime;
        async void OnChange(InputFileChangeEventArgs e)
        {
            file = null;
            Progress = null;
            var uploaded = e.File;
            var parts = uploaded.ContentType.Split("/");
            string type = parts[0];
            string subtype = parts[1];
            uploadedFileMime = new MIME(type, subtype);
            file = uploaded;
            DisableUploadButton = false;
            await FileChanged.InvokeAsync(file);
        }

        async Task<Stream?> GetStreamFromFile()
        {
            if (file is null)
            {
                if (_module is null)
                    throw new NullReferenceException();
                await _module.InvokeVoidAsync("log", "No file given - will not upload");
                return null;
            }

            try
            {
                return file.OpenReadStream(file.Size);
            }
            catch (Exception)
            {
                if (_module is null)
                    throw new NullReferenceException();
                await _module.InvokeVoidAsync("log", "There was an error reading the file");
                return null;
            }
        }

        async Task<bool> InsertFileNameIntoDatabase(UploadedFileModel fileModel)
        {
            try
            {
                await sql.SaveData(StoredProcedures.FILE_INSERT, "localhost-docker", fileModel);
                return true;
            }
            catch (Exception e)
            {
                if (_module is null)
                    throw new NullReferenceException();
                await _module.InvokeVoidAsync("log", $"There was an error writing to the SQL database\n{e.Message}");
                return false;
            }
        }

        async Task<bool> UploadToFileServer(Stream stream, UploadedFileModel uploadedFileModel)
        {
            if (stream.Length < 1)
            {
                // todo throw up an error message as no file is given
                if (_module is null)
                    throw new NullReferenceException();
                await _module.InvokeVoidAsync("log", "No file given - will not upload");
                DisableUploadButton = false;
                return false;
            }

            string filePath = Path.Combine(uploadPath, uploadedFileModel.RandomFileName);
            FileStream fsW = File.Create(filePath);


            long totalBytes = file.Size;
            long bytesWritten = 0;

            while (bytesWritten < totalBytes)
            {
                byte[] buffer = new byte[81920];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }

                await fsW.WriteAsync(buffer, 0, bytesRead);
                bytesWritten += bytesRead;

                // Calculate the progress percentage
                Progress = (int)Math.Round((double)bytesWritten / totalBytes * 100);
            }
            await stream.CopyToAsync(fsW);
            stream.Close();
            fsW.Close();

            return true;

            return await FileUploader.UploadFile(filePath, progress);
            void progress(FtpProgress p) => Progress = (int)p.Progress;
        }

        bool SendTranscodeMessage()
        {
            var factory = new ConnectionFactory()
            {
                HostName = config["RabbitMQ.IP"]
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ConfirmSelect();
            // declare a durable queue
            channel.QueueDeclare("transcode", true, false, false, null);
            // create the message body as a byte array
            byte[] messageBody = Encoding.UTF8.GetBytes("Hello, world!");
            // create the message properties with delivery mode set to 2 (persistent)
            var properties = channel.CreateBasicProperties();
            properties.ContentType = "text/plain";
            properties.DeliveryMode = 2;
            // publish the message with publisher confirm
            channel.BasicPublish("", "transcode", properties, messageBody);
            if (!channel.WaitForConfirms(TimeSpan.FromSeconds(10)))
            {
                Console.WriteLine("Message publish failed.");
                return false;
            }

            return true;
        }

        public void DisableUpload()
        {
            DisableUploadButton = true;
            DisableInputFile = true;
        }

        public void EnableUpload()
        {
            DisableUploadButton = false;
            DisableInputFile = false;
        }

        async Task BtnUploadFile()
        {
            DisableUpload();

            var stream = await GetStreamFromFile();
            if (stream is null || file is null)
            {
                DisableUpload();
                return;
            }

            ErrorMessage = null;
            var newFileName = Path.ChangeExtension(Path.GetRandomFileName(), Path.GetExtension(file.Name));
            // create file model
            var fileModel = new UploadedFileModel()
            {
                OriginalFileName = Path.GetFileNameWithoutExtension(file.Name),
                RandomFileName = newFileName
            };

            // upload to fileserver
            var uploadTask = await UploadToFileServer(stream, fileModel);
            var uploadResult = uploadTask;
            if (uploadResult == false)
            {
                if (_module is null)
                    throw new NullReferenceException();
                await _module.InvokeVoidAsync("log", "There was an error uploading the file");
                file = null;
                Progress = null;
                ErrorMessage = "We could not upload your file";
                EnableUpload();
                return;
            }

            await Parent.OnFileUploaded(Path.Combine(uploadPath,newFileName));

            // enter filename into database
            var insertTask = await InsertFileNameIntoDatabase(fileModel);
            var databaseResult = insertTask;
            if (databaseResult == false)
            {
                if (_module is null)
                    throw new NullReferenceException();
                await _module.InvokeVoidAsync("log", "There was an error inserting the file into the database");
                file = null;
                Progress = null;
                ErrorMessage = "We could not upload your file";
                EnableUpload() ;
                return;
            }

            // send message that file is ready to be transcoded
            if (SendTranscodeMessage() == false)
            {
                if (_module is null)
                    throw new NullReferenceException();
                await _module.InvokeVoidAsync("log", "There was an error publishing to the MQTT channel");
                file = null;
                Progress = null;
                ErrorMessage = "We could not start the transcoder";
                EnableUpload() ;
                return;
            }

            File.Delete(Path.Combine(uploadPath, fileModel.RandomFileName));
            // redirect to transcoding page
            nav.NavigateTo("/transcode");
        }


    }
}