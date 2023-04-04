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

namespace TranscodeNowWebServer.Shared
{
    public partial class NewDragAndDrop
    {
        public IBrowserFile? UploadedFile { get; set; }
        public bool Disabled { get; set; }

        [Parameter]
        public EventCallback<InputFileChangeEventArgs> OnChange { get; set; }

        private async Task HandleFileInputChange(InputFileChangeEventArgs e)
        {
            await OnChange.InvokeAsync(e);
        }
    }
}