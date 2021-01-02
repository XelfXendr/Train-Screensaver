using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Train_Screensaver_Client.Logic;

namespace Train_Screensaver_Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                string first = e.Args[0].ToLower().Trim(); //first argument (/c, /p or /s)
                string second = null; //second argument - int specifying window handle

                //Parsing args
                if (first.Length > 2)
                {
                    second = first.Substring(3).Trim();
                    first = first.Substring(0, 2);
                }
                else if (e.Args.Length > 1)
                    second = e.Args[1];

                //processing args
                switch (first)
                {
                    case "/c":  //Show configuration form
                        ShowConfiguration();
                        break;

                    case "/p":  //Show screen-saver preview
                        //Preview not yet supported
                        Application.Current.Shutdown();
                        break;

                    case "/s":  //Show screen-saver
                        ShowScreenSaver();
                        break;

                    default: //Invalid
                        MessageBox.Show("Command-line argument \"" + first + "\" is not valid.", "ScreenSaver", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Application.Current.Shutdown();
                        break;
                }
            }
            else
                ShowConfiguration();
        }

        static void ShowScreenSaver()
        {
            var window = new ScreensaverWindow();
            window.Show();
        }

        static void ShowConfiguration()
        {
            try
            {
                if (!Configurator.ConfigExists())
                    Configurator.CreateConfig();
                Process.Start("explorer.exe", Configurator.folder);
            }
            catch
            {
                MessageBox.Show("Cannot access configuration folder:\n" + Configurator.folder, "ScreenSaver", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Application.Current.Shutdown();
        }
    }
}
