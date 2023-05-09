using FFMpegCore;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using TranscodeNowWebServer.Data;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace TranscodeNowWebServer.Pages.Options;

public partial class Options
{
    private VideoStream? InitialVideoStream;
    private AudioStream? InitialAudioStream;

    private GeneralOptions? _generalOptions;
    private AudioOptions? _audioOptions;
    private VideoOptions? _videoOptions;

    protected override void OnInitialized()
    {
        if (fileService.UploadedFileModel is null || fileService.UploadedFileModel.File is null)
        {
            Console.WriteLine("no file set");
            return;
        }

        _generalOptions = new GeneralOptions();
        SetInitialValues();
    }

    private void SetImageResolution(int width, int height)
    {
        _videoOptions!.Width = width;
        _videoOptions!.Height = height;
    }

    private void SetInitialValues()
    {
        var file = fileService.UploadedFileModel.Data!;
        if(file.VideoStreams.Any())
        {
            InitialVideoStream = file.VideoStreams.First();
            _videoOptions = new(InitialVideoStream);
            _generalOptions.OutputType = GeneralOptions.OutputTypes.Video;
        }

        if(file.AudioStreams.Any())
        {
            InitialAudioStream = file.AudioStreams.First();
            _audioOptions = new(InitialAudioStream);
            if(InitialVideoStream is null)
            {
                _generalOptions.OutputType = GeneralOptions.OutputTypes.Audio;
            }
        }

        if (InitialVideoStream is null && InitialAudioStream is null)
        {
            // no valid streams
            Console.WriteLine("No valid streams");
            return;
        }
    }

    private void OnButtonClick()
    {
        if (InitialVideoStream is null && InitialAudioStream is null)
        {
            Console.WriteLine("Cannot send transcode message: No streams available");
            return;
        }

        userOptionsService.UserOptions =
            GetSelectedOptions();

        var msg = new MqqtTranscodeMessage(
            fileService.UploadedFileModel,
            userOptionsService.UserOptions
            );

        SendTranscodeMessage(msg);
        navManager.NavigateTo("result");
    }

    private void SendTranscodeMessage(MqqtTranscodeMessage msg)
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        string queueName = "transcode-in";
        string message = JsonSerializer.Serialize(msg);
        var body = Encoding.UTF8.GetBytes(message);
        Console.WriteLine(message);
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    private UserOptions GetSelectedOptions()
    {
        var message = new UserOptions()
        {
            GeneralOptions = _generalOptions,
            VideoOptions = _videoOptions,
            AudioOptions = _audioOptions
        };
        return message;
    }

    private Dictionary<string, (string, MarkupString)> _helpMessages = new()
    {
        // General Settings
        { "filename",
            ("File name",
            new( "This is the file name that will be give to the file that we transcode and send back to you. By default, we prepend <em>transcode_</em> to the filename." ))
        },
        { "fileOutput",
            ("Output File Type",
            new( "Select which file type you would like to convert to. Audio files can have a blank, black screen added to them to make them a video. Conversely, video files can be converted to audio only files." ))
        },
        // Video Settings
        { "resolution",
            ("Video Resolution",
            new( "Resolution is the number of pixels used to display an image or video on a screen. Higher resolutions have more pixels, making the picture clearer and more detailed. Lower resolutions have fewer pixels, which can make the image look blurry.<br/><br/>Examples of resolutions and their uses:<ul class=\"list-group\"> <li class=\"list-group-item\"> 480p (640x480 pixels) - This is a low resolution often used for older videos, online streaming on slow internet connections, or small screens like older phones or tablets.</li><li class=\"list-group-item\"> 720p (1280x720 pixels) - This is a medium resolution, commonly called HD (High Definition). It is used for online streaming, smaller TVs, and some smartphones.</li><li class=\"list-group-item\"> 1080p (1920x1080 pixels) - This is a higher resolution, also known as Full HD. It provides clearer images and is often used for larger TVs, computer monitors, and streaming movies or shows.</li><li class=\"list-group-item\"> 4K (3840x2160 pixels) - This is a very high resolution, called Ultra HD or UHD. It provides very detailed images and is used for large TVs, high-quality streaming, and professional video production.</li></ul>" ))
        },
        { "videoBitrate",
            ("Video Bitrate",
            new( "Video bitrate is the amount of data used to represent a video's quality. A higher bitrate means more data is used, resulting in better video quality but larger file sizes. A lower bitrate uses less data, which can lead to lower quality but smaller files.<br/><br/>Examples of video bitrates and their uses:<ul class=\"list-group\"> <li class=\"list-group-item\"> Low bitrate (300-800 kbps) - This is used for low-quality online streaming, like video calls or slow internet connections. Videos may appear blurry or pixelated at this level.</li><li class=\"list-group-item\"> Medium bitrate (800-2000 kbps) - This bitrate offers better quality, suitable for standard-definition (SD) videos or streaming on mobile devices with limited data plans.</li><li class=\"list-group-item\"> High bitrate (2000-5000 kbps) - This level provides good quality for high-definition (HD) videos, like 720p or 1080p. It is used for streaming movies or shows on platforms like Netflix or YouTube.</li><li class=\"list-group-item\"> Very high bitrate (10,000-20,000 kbps) - This bitrate is used for ultra-high-definition (UHD) videos, like 4K resolution. It offers excellent quality but requires fast internet connections and more storage space.</li></ul>" ))
        },
        { "framerate",
            ("Framerate",
            new( "Framerate is the number of individual images, or frames, shown in a video per second. It is measured in frames per second (fps). Higher framerates create smoother motion, while lower framerates may appear choppy or slow.<br/><br/>Examples of framerates and their uses:<ul class=\"list-group\"> <li class=\"list-group-item\"> Low framerate (12-15 fps) - This is used for basic animations, old movies, or security cameras. Motion may appear jerky or choppy at this level.</li><li class=\"list-group-item\"> Standard framerate (24-30 fps) - This is common for most videos, including movies, TV shows, and online streaming. It provides a natural-looking motion and is widely accepted for most uses.</li><li class=\"list-group-item\"> High framerate (48-60 fps) - This level is used for smoother motion in sports broadcasts, video games, and some high-quality online streaming. It captures fast-moving action more clearly.</li><li class=\"list-group-item\"> Very high framerate (120+ fps) - This framerate is used for special applications, such as virtual reality (VR) or slow-motion playback. It provides extremely smooth motion but requires more processing power and storage.</li></ul>" ))
        },
        { "transformation",
            ("Transforming Video",
            new( "Transformation refers to changes made to a video's appearance or orientation without affecting its content.<ul class=\"list-group\"> <li class=\"list-group-item\"> Rotation - This involves changing the video's orientation by turning it around a central point. Rotation can be done in integer increments of degrees. This is useful for correcting videos recorded in the wrong orientation, such as a vertical video that should be horizontal.<br/>Example: Rotating a recorded smartphone video from portrait to landscape mode for better viewing on a TV or computer.</li> <li class=\"list-group-item\"> Flipping - This transformation creates a mirror image of the video by reversing it horizontally or vertically. Flipping can be helpful for correcting or creating special effects in videos.<br/>Example: Flipping a video horizontally to create a mirror reflection effect or to correct a video where the content appears reversed.</li> <li class=\"list-group-item\"> Cropping - This process involves removing unwanted parts of the video by cutting out a smaller rectangular area within the original frame. Cropping can help focus on the essential parts of a scene or remove distracting elements.<br/>Example: Cropping a video to remove black bars on the sides, focus on a specific subject within the frame, or remove unwanted objects from the edges of the video.</li></ul>" ))
        },
        { "videoDuration",
            ("Video Duration",
            new( "This set of controls allows you to set the time that the video will start and end.<br/><ul class=\"list-group\"><li class=\"list-group-item\">Start Time - The time that the video starts at. This can be used to skip the beginning of a video, such as when you started recording before being aligned with your subject.</li><<li class=\"list-group-item\">End Time - The time that the video ends at. This can be used to shorten a video, such as when you recorded for longer than intended.</li></ul>" ))
        },
        // Audio Settings
        { "audioChannels",
            ("Audio Channels",
            new( "Audio channels refer to the number of separate audio signals used to create a sound experience in a video or recording.<ul class=\"list-group\"> <li class=\"list-group-item\"> Mono (Monophonic) - Mono audio uses a single audio channel to produce sound. The same audio signal is sent to all speakers or headphones, resulting in no sense of direction or spatial separation. Mono is useful for simpler audio setups, voice recordings, or when ensuring compatibility with a wide range of devices.<br/>Example: Listening to a podcast or a voice recording where the focus is on the spoken content, not the spatial separation of sounds.</li><li class=\"list-group-item\"> Stereo (Stereophonic) - Stereo audio uses two separate audio channels, typically one for the left speaker and one for the right speaker. This creates a sense of direction and spatial separation, providing a more immersive and realistic sound experience. Stereo is commonly used for music, movies, and video games to enhance the audio quality.<br/>Example: Listening to music or watching a movie with separate audio channels for left and right speakers, creating a more engaging and immersive sound experience.</li></ul>" ))
        },
        { "audioBitrate",
            ("Audio Bitrate",
            new( "Audio bitrate is the amount of data used to represent the quality of an audio file or stream. A higher bitrate means more data is used, resulting in better audio quality but larger file sizes. A lower bitrate uses less data, which can lead to lower quality but smaller files.<br/><br/>Examples of audio bitrates and their uses:<ul class=\"list-group\"> <li class=\"list-group-item\"> Low bitrate (32-64 kbps) - This is used for low-quality audio, like voice recordings, audio books, or streaming on slow internet connections. Audio may sound muffled or lack detail at this level.</li><li class=\"list-group-item\"> Medium bitrate (96-128 kbps) - This bitrate offers better audio quality, suitable for streaming music on mobile devices or internet radio stations. It provides a balance between sound quality and file size.</li><li class=\"list-group-item\"> High bitrate (192-320 kbps) - This level provides good audio quality for music or podcast streaming and is often used in MP3 files. It offers clearer sound and better detail compared to lower bitrates.</li><li class=\"list-group-item\"> Lossless bitrate (600-1400 kbps) - This bitrate is used for high-quality audio formats, like FLAC or Apple Lossless, which provide excellent sound quality without any loss of detail. It is ideal for audiophiles, professional audio production, or archiving music.</li></ul>" ))
        },
        { "normalizeAudio",
            ("Normalize Audio",
            new( "Audio normalization is the process of adjusting the volume levels of an audio file or stream to achieve a consistent and optimal loudness. This can help avoid sudden changes in volume or make it easier to hear quieter sounds.<br/><br/>Examples of audio normalization uses:<ul class=\"list-group\"> <li class=\"list-group-item\"> Balancing audio levels - If you have multiple audio clips or songs in a playlist with different volume levels, normalization can adjust them to a consistent level, making it more comfortable to listen to.</li><li class=\"list-group-item\"> Podcasts and interviews - In conversations with multiple speakers, normalization can help ensure that all voices are heard at a similar volume level, making it easier for the listener to follow the conversation.</li><li class=\"list-group-item\"> Video production - When creating a video with various audio sources, such as music, sound effects, and dialogue, normalization can help balance the volume levels, ensuring a more professional and polished result.</li><li class=\"list-group-item\"> Broadcasting and streaming - Radio stations, TV channels, and online streaming platforms often use audio normalization to maintain a consistent volume level across different programs, commercials, and content, providing a better listening experience for the audience.</li></ul>" ))
        },
        { "removeAudio",
            ("Remove Audio Track",
            new( "This option will remove the audio track from the video. The resulting track will be silent." ))
        }//,
        //{ "sampleRate",
        //    ("Sample Rate",
        //    new( "Sample rate is the number of times per second an audio signal is measured, or sampled, to create a digital representation of the sound. It is measured in samples per second, or Hertz (Hz). A higher sample rate captures more details and provides better audio quality, but it also requires more storage space and processing power.<br/><br/>Examples of sample rates and their uses:<ul class=\"list-group\"> <li class=\"list-group-item\"> Low sample rate (8,000 Hz) - This is used for low-quality audio, like telephone calls or low-bitrate voice recordings. Audio may sound muffled or lack detail at this level.</li><li class=\"list-group-item\"> Medium sample rate (22,050 Hz) - This sample rate offers better audio quality, suitable for basic music or sound effects. It provides a balance between sound quality and file size.</li><li class=\"list-group-item\"> High sample rate (44,100 Hz) - This is the most common sample rate used for CD-quality audio, providing clear sound and good detail. It is often used for music, podcasts, and other digital audio recordings.</li><li class=\"list-group-item\"> Very high sample rate (48,000 Hz or higher) - This sample rate is used for professional audio production, like film soundtracks, high-quality music, or broadcasting. It offers excellent sound quality and captures more subtle details, but requires more storage space and processing power.</li></ul>" ))
        //}
    };

    private string ModalTitle = "No option selected";
    private MarkupString ModalMessage = new("Please select an option to see more information about it.");
    private void HandleHelpClick(string caller)
    {
        (string, MarkupString) result;
        if (!_helpMessages.TryGetValue(caller, out result))
        {
            Console.WriteLine($"{DateTime.UtcNow} Could not get caller {caller}");
            return;
        }

        ModalTitle = result.Item1;
        ModalMessage = result.Item2;
        js.InvokeVoidAsync("showModal");
    }
}