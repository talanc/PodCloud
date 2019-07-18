using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CommandLine;
using YoutubeExplode;

namespace PodCloud
{
    class Program
    {
        private static YoutubeClient youtube = new YoutubeClient();

        static async Task<int> Main(string[] args)
        {
            // podcloud info UCbkjX3E0IhuUfPzL0FjSPaw

            var result = 0;

            result = await Parser.Default.ParseArguments<InfoVerb, SettingsVerb>(args)
                .MapResult(
                    (InfoVerb verb) => RunInfo(verb),
                    (SettingsVerb verb) => RunSettings(verb),
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
        }
        public static Task<int> RunSettings(SettingsVerb verb)
        {
            if (verb.Open)
            {
            }

            return Task.FromResult(0);
        }
    }
}
