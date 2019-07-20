using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace PodCloud
{
    class Program
    {
        private static YoutubeClient youtube = new YoutubeClient();

        static async Task<int> Main(string[] args)
        {
            // podcloud info UCbkjX3E0IhuUfPzL0FjSPaw

            var result = 0;

            result = await Parser.Default.ParseArguments<InfoVerb, SettingsVerb, DownloadVerb>(args)
                .MapResult(
                    (InfoVerb verb) => RunInfo(verb),
                    (SettingsVerb verb) => RunSettings(verb),
                    (DownloadVerb verb) => RunDownload(verb),
                    errs => Task.FromResult(1)
                );

            return result;
        }

        [Verb("info", HelpText = "Display info on channel or video")]
        public class InfoVerb
        {
            [Value(0, Hidden = true)]
            public string Info { get; set; }

            [Value(1, Required = true, HelpText = "ID of channel/video")]
            public IEnumerable<string> Source { get; set; }
        }

        static async Task<int> RunInfo(InfoVerb verb)
        {
            foreach (var source in verb.Source)
            {
                if (YoutubeClient.ValidateChannelId(source))
                {
                    var channel = await youtube.GetChannelAsync(source);
                    Console.WriteLine(channel.Title);

                    var uploads = await youtube.GetChannelUploadsAsync(source, 1);
                    for (var i = 0; i < Math.Min(3, uploads.Count); i++)
                    {
                        var upload = uploads[i];
                        Console.WriteLine($"- {upload.Title} (id: {upload.Id}, date: {upload.UploadDate.Date.ToShortDateString()}, duration: {upload.Duration})");
                    }
                }
                else if (YoutubeClient.ValidateVideoId(source))
                {
                    var video = await youtube.GetVideoAsync(source);
                    Console.WriteLine(video.Title);
                }
            }

            return 0;
        }

        [Verb("settings", HelpText = "View or change settings.")]
        public class SettingsVerb
        {
            [Option("open", HelpText = "Open settings file.", SetName = "open")]
            public bool Open { get; set; }

            [Option("conn-str", HelpText = "Set storage connection string.", SetName = "set")]
            public string ConnectionString { get; set; }

            // preferred video format

            // output audio format
        }
        public static Task<int> RunSettings(SettingsVerb verb)
        {
            if (verb.Open)
            {
            }

            return Task.FromResult(0);
        }

        [Verb("download", HelpText = "Download videos.")]
        public class DownloadVerb
        {
            [Option('i', "input", Required = true, HelpText = "Input video URL or ID")]
            public string Input { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output path")]
            public string Output { get; set; }
        }
        static async Task<int> RunDownload(DownloadVerb verb)
        {
            var videoId = verb.Input;
            var outputExt = Path.GetExtension(verb.Output);

            var infos = await youtube.GetVideoMediaStreamInfosAsync(videoId);

            var info = infos.Muxed
                .OrderBy(curr => curr.Size)
                .FirstOrDefault(curr => outputExt.Equals("." + curr.Container.GetFileExtension(), StringComparison.OrdinalIgnoreCase));

            if (info == null)
            {
                Console.WriteLine($"Could not find suitable video file for '{outputExt}'");
                return 1;
            }

            Console.WriteLine("Downloading, this may take a while...");
            await youtube.DownloadMediaStreamAsync(info, verb.Output);

            return 0;
        }
    }
}
