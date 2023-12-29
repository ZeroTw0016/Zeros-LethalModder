using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace Zeros_LethalModder
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static WebClient client;
        static string modpackURL = "https://github.com/ZeroTw0016/file-storage/raw/main/lethalCompany/Zeros-Lethal-Modpack.zip?download=";
        static string bepinExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip";
        static string selfPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/ZeroModded";
        static string configFile = selfPath + "/config.xml";
        static string gameFolderpathData = "";
        static ProgressBar pBar;
        static bool downloadRunning = false;

        public MainWindow()
        {
            if (!Directory.Exists(selfPath))
            {
                Directory.CreateDirectory(selfPath);
            }
            string[] Files = Directory.GetFiles(selfPath, "*");
            string[] Directories = Directory.GetDirectories(selfPath, "*");

            foreach (string file in Files)
            {
                if(!file.Contains(".xml") && !file.Contains(".exe"))
                {
                    File.Delete(file);
                }
            }
            foreach (string dir in Directories)
            {
                Directory.Delete(dir, true);
            }
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            client = new WebClient();
            client.DownloadProgressChanged += WebClientDownloadProgressChanged;
            client.DownloadFileCompleted += WebClientDownloadFileCompleted;
            pBar = downloadProgressBar;
            ProgressText.HorizontalContentAlignment = HorizontalAlignment.Center;

            if (!File.Exists(configFile))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement rootElement = xmlDoc.CreateElement("Configuration");
                xmlDoc.AppendChild(rootElement);

                XmlElement GameFolderElement = xmlDoc.CreateElement("GameFolder");
                GameFolderElement.InnerText = "";
                rootElement.AppendChild(GameFolderElement);

                xmlDoc.Save(configFile);
            }
            else
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(configFile);

                    XmlNode GameFolderNode = xmlDoc.SelectSingleNode("/Configuration/GameFolder");
                    if(GameFolderNode.InnerText.Length > 0)
                    {
                        gameFolderpathData = GameFolderNode.InnerText;
                        folderPath.Content = gameFolderpathData;
                        folderPath.ToolTip = gameFolderpathData;
                        Setup_Button.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading configuration: {ex.Message}");
                }
            }
        }

        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pBar.Value = e.ProgressPercentage;
        }

        private void WebClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            downloadRunning = false;
            if (e.Error != null)
            {
                MessageBox.Show($"Error: {e.Error.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string zipFile = Directory.GetFiles(selfPath, "*.zip")[0];
                try
                {
                    using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(zipFile)))
                    {
                        ZipEntry entry;
                        while ((entry = zipStream.GetNextEntry()) != null)
                        {
                            string entryPath = Path.Combine(zipFile.Replace(".zip",""), entry.Name);

                            Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                            if (!entry.IsDirectory)
                            {
                                using (FileStream entryStream = File.Create(entryPath))
                                {
                                    zipStream.CopyTo(entryStream);
                                }
                            }
                        }
                    }      
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                File.Delete(zipFile);
            }
        }

        private static void UpdateConfig()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFile);

            XmlNode setting1Node = xmlDoc.SelectSingleNode("/Configuration/GameFolder");
            setting1Node.InnerText = gameFolderpathData;

            xmlDoc.Save(configFile);
        }

        private void GameFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                if(dialog.SelectedPath.Contains("Lethal Company"))
                {
                    folderPath.Content = dialog.SelectedPath;
                    string path = folderPath.Content.ToString();
                    gameFolderpathData = path;
                    folderPath.ToolTip = gameFolderpathData;
                    UpdateConfig();
                    Setup_Button.IsEnabled = true;
                }
            }
        }

        private void DownloadModpack(object sender, RoutedEventArgs e)
        {
            string githubFileUrl = modpackURL; // Replace with your GitHub file URL
            string saveFilePath = selfPath + "/Zeros-Lethal-Modpack.zip"; // Specify the local path where the file should be saved


            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }


            try
            {
                if (!downloadRunning)
                {
                    client.DownloadFileAsync(new Uri(githubFileUrl), saveFilePath);
                    downloadRunning = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadBepInEx(object sender, RoutedEventArgs e)
        {
            string githubFileUrl = bepinExUrl; // Replace with your GitHub file URL
            string saveFilePath = selfPath + "/BepInEx.zip"; // Specify the local path where the file should be saved


            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }


            try
            {
                if (!downloadRunning)
                {
                    client.DownloadFileAsync(new Uri(githubFileUrl), saveFilePath);
                    downloadRunning = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void fullSetup(object sender, RoutedEventArgs e)
        {
            ProgressText.Content = "Downloading BepInEx";
            DownloadBepInEx(sender, e);
            await Task.Delay(1000);
            while (File.Exists(selfPath + "/BepInEx.zip"))
            {
                await Task.Delay(1000);
            }
            await Task.Delay(1000);
            pBar.Value = 0;
            ProgressText.Content = "Downloading Modpack";
            DownloadModpack(sender, e);
            await Task.Delay(1000);
            while (File.Exists(selfPath + "/Zeros-Lethal-Modpack.zip"))
            {
                await Task.Delay(1000);
            }
            ProgressText.Content = "Setting up BepInEx";
            string[] deleteMatch = { "winhttp.dll", "doorstop_config.ini", "changelog.txt", "BepInEx" };
            foreach (string match in deleteMatch)
            {
                string s = "\\" + match;
                if(!match.Contains("."))
                {
                    if(Directory.Exists(gameFolderpathData + s) && !s.Contains(".git"))
                    {
                        Directory.Delete(gameFolderpathData + s, true);
                    }
                }
                else
                {
                    if(File.Exists(gameFolderpathData + s))
                    {
                        File.Delete(gameFolderpathData + s);
                    }
                }
            }

            await Task.Delay(1000);

            foreach (var path in deleteMatch)
            {
                if(path.Contains("."))
                {
                    File.Move(selfPath + "\\BepInEx\\" + path, gameFolderpathData + "\\" + path);
                }
                else
                {
                    Directory.Move(selfPath + "\\BepInEx\\" + path, gameFolderpathData + "\\" + path);
                }
            }
            ProgressText.Content = "Temporarily starting Game";
            var LethalCompany = Process.Start(gameFolderpathData + "\\Lethal Company.exe");
            await Task.Delay(15000);
            LethalCompany.Kill();
            ProgressText.Content = "Killed Game";
            Directory.Delete(selfPath + "\\BepInEx", true);
            ProgressText.Content = "Setting up Modpack";
            string[] contents = Directory.GetDirectories(selfPath + "\\Zeros-Lethal-Modpack");

            foreach (string dir in contents)
            {
                string destination = gameFolderpathData + "\\BepInEx\\" + dir.Split('\\').Last();
                if (Directory.Exists(destination))
                {
                    Directory.Delete(destination, true);
                }
                Directory.Move(dir, destination);
            }

            Directory.Delete(selfPath + "\\Zeros-Lethal-Modpack", true);
            await Task.Delay(1000);
            ProgressText.Content = "Done";
            SystemSounds.Beep.Play();

            if(startcheckbox.IsChecked == true)
            {
                Process.Start(gameFolderpathData + "\\Lethal Company.exe");
                this.Close();
            }
        }
    }
}
