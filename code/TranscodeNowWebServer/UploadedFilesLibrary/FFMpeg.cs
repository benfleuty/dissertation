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

        private IMediaAnalysis? _details = null;
        public IMediaAnalysis? Details
        {
            get
            {
                _details ??= GetMediaAnalysis();

                return _details;
            }
            set { _details = value; }

        }

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
            
            return mediaInfo;
        }


        private bool? IsFileSupported(IMediaAnalysis mediaInfo)
        {
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
