using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Train_Screensaver_Client.Logic
{
    [Serializable]
    public class Config
    {
        public string server { get; set; }
        public ushort port { get; set; }
        public int framerate { get; set; }
        public string[] wagonSources { get; set; }
        public int[] trainIndexes { get; set; }


        public BitmapImage[] LoadImages()
        {
            BitmapImage[] images = new BitmapImage[wagonSources.Length];
            
            for(int i = 0; i < images.Length; i++)
            {
                try
                {
                    if (System.IO.Path.IsPathRooted(wagonSources[i]))
                        images[i] = new BitmapImage(new Uri(wagonSources[i]));
                    else
                        images[i] = new BitmapImage(new Uri(System.IO.Path.Combine(Configurator.folder, wagonSources[i])));
                }
                catch //if the image source doesn't exist
                {
                    using (var ms = new MemoryStream(Properties.Resources.NotFound))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        images[i] = image;
                    }
                }

                images[i].Freeze();
            }

            return images;
        }
    }

    public static class Configurator
    {
        public static string folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Train_Screensaver");
        public static string configFile = "config.json";

        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public static Config LoadConfig()
        {
            string file = System.IO.Path.Combine(folder, configFile);

            if (!File.Exists(file))
                CreateConfig();

            Config config;

            try
            {
                string json = File.ReadAllText(file);
                config = JsonSerializer.Deserialize<Config>(json, jsonOptions);
                if(config.framerate <= 0)
                {
                    config.framerate = 1;
                }
            }
            catch
            {
                config = new Config()
                {
                    server = "",
                    port = 25308,
                    framerate = 30,
                    wagonSources = new string[] { "example1.png", "example2.jpg" },
                    trainIndexes = new int[] { 0, 1, 1 },
                };
            }
            
            return config;
        }

        public static void CreateConfig()
        {
            Config config = new Config()
            {
                server = "",
                port = 25308,
                framerate = 30,
                wagonSources = new string[] { "example1.png", "example2.jpg" },
                trainIndexes = new int[] { 0, 1, 1 },
            };

            Directory.CreateDirectory(folder);

            string json = JsonSerializer.Serialize(config, jsonOptions);
            string file = System.IO.Path.Combine(folder, configFile);

            File.WriteAllText(file, json);
        }

        public static bool ConfigExists()
        {
            return File.Exists(System.IO.Path.Combine(folder, configFile));
        }
    }
}
