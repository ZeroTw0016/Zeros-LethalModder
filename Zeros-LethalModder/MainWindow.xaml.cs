using System;
using System.Globalization;
using System.IO;
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
            string[] zipFiles = Directory.GetFiles(selfPath, "*");

            foreach (string zipFile in zipFiles)
            {
                if(!zipFile.Contains(".xml"))
                {
                    File.Delete(zipFile);
                }
            }
            InitializeComponent();
            client = new WebClient();
            client.DownloadProgressChanged += WebClientDownloadProgressChanged;
            client.DownloadFileCompleted += WebClientDownloadFileCompleted;
            pBar = downloadProgressBar;

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
                    gameFolderpathData = GameFolderNode.InnerText;
                    folderPath.Content = gameFolderpathData;
                    string path = folderPath.Content.ToString();
                    Thickness Margin = GameFolderSelect.Margin;
                    var formatted = new FormattedText(path, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(folderPath.FontFamily, folderPath.FontStyle, folderPath.FontWeight, folderPath.FontStretch), folderPath.FontSize, folderPath.Foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                    Margin.Left = 187 + formatted.Width + 10;
                    GameFolderSelect.Margin = Margin;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading configuration: {ex.Message}");
                }
            }
        }

        private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update the progress bar
            pBar.Value = e.ProgressPercentage;
            if(e.ProgressPercentage == 100)
            {
                //MessageBox.Show("Download complete","Download",MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

                            // Ensure the directory for the entry exists
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
                folderPath.Content = dialog.SelectedPath;
                string path = folderPath.Content.ToString();
                Thickness Margin = GameFolderSelect.Margin;
                var formatted = new FormattedText(path, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(folderPath.FontFamily, folderPath.FontStyle, folderPath.FontWeight, folderPath.FontStretch), folderPath.FontSize, folderPath.Foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                Margin.Left = 187 + formatted.Width + 10;
                GameFolderSelect.Margin = Margin;
                gameFolderpathData = path;
                UpdateConfig();
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
            DownloadBepInEx(sender, e);
            DownloadModpack(sender, e);
        }
    }
}
