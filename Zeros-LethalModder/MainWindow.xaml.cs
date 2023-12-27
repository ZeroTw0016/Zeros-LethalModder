using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Zeros_LethalModder
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string selfPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/ZeroModded";
        static string configFile = selfPath + "/config.xml";
        static string gameFolderpathData = "";
        public MainWindow()
        {
            if (!Directory.Exists(selfPath))
            {
                Directory.CreateDirectory(selfPath);
            }
            InitializeComponent();
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
                var formatted = new FormattedText(path, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,new Typeface(folderPath.FontFamily, folderPath.FontStyle, folderPath.FontWeight, folderPath.FontStretch),folderPath.FontSize, folderPath.Foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                Margin.Left = 187 + formatted.Width + 10;
                GameFolderSelect.Margin = Margin;
                gameFolderpathData = path;
                UpdateConfig();
            }
        }
    }
}
