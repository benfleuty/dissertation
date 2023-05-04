using FFMpegCore;
using FFMpegCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UploadedFilesLibrary
{
    public class FFMpeg
    {
        public string? FilePath { get; set; }
        public FFMpeg(string filePath)
        {
            FilePath = filePath;
            GetMediaAnalysis();
        }

        public IMediaAnalysis? Details { get; private set; }

        private IMediaAnalysis? GetMediaAnalysis()
        {
            // Get media information
            IMediaAnalysis? mediaInfo;


            if (FilePath == null)
            {
                return null;
            }

            try
            {
                mediaInfo = FFProbe.Analyse(FilePath);
            }
            catch (FFMpegException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            if (IsFileSupported(mediaInfo) == false)
            {
                return null;
            }

            Details = mediaInfo;

            return mediaInfo;
        }
        public bool? IsFileSupported()
        {
            return IsFileSupported(Details!);
        }

        public bool? IsFileSupported(IMediaAnalysis mediaInfo)
        {
            if (mediaInfo is null)
                return false;

            try
            {
                return mediaInfo.VideoStreams.Any() || mediaInfo.AudioStreams.Any();
            }
            catch (FFMpegException e)
            {
                Console.WriteLine($"File not supported:{FilePath}");
                Console.WriteLine(e.Message);
                return null;
            }
        }

    }
}
