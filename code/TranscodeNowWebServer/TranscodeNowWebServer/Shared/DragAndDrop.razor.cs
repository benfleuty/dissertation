using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace TranscodeNowWebServer.Shared
{
    public partial class DragAndDrop
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