using Dapper.Contrib.Extensions;
using FFMpegCore;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TranscodeNowWebServer.Interfaces;

namespace TranscodeNowWebServer.Data;

public class UploadedFileModel
{
    public int Id { get; set; }
    public string? OriginalFileName { get; set; }
    public string? RandomFileName { get; set; }

    [JsonIgnore]
    [Write(false)]
    public IBrowserFile? File { get; set; }

    [JsonIgnore]
    [Write(false)]
    public IMediaAnalysis? Data { get; set; }
}