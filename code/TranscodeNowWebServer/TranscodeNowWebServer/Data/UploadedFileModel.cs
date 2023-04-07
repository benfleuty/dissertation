﻿using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;
using TranscodeNowWebServer.Interfaces;

namespace TranscodeNowWebServer.Data
{
    public class UploadedFileModel
    {
        public int Id { get; set; }
        public string? OriginalFileName { get; set; }
        public string? RandomFileName { get; set; }

        [Write(false)]
        public IBrowserFile? Data { get; set; }
    }
}