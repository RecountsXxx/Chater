using MessengerLiblary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
     public static int id { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "/ThemeSettings.txt"))
            {
                string str = File.ReadAllText(Directory.GetCurrentDirectory() + "/ThemeSettings.txt").Replace("\r\n", "");
                if (str == "System.Windows.Controls.ComboBoxItem: Dark" || str == "System.Windows.Controls.ComboBoxItem: Темна")
                {
                    var uri = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries[0].Source = uri;
                    ResourceDictionary resurce = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resurce);
                }
                if (str == "System.Windows.Controls.ComboBoxItem: Light" || str == "System.Windows.Controls.ComboBoxItem: Світла")
                {
                    var uri = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries[0].Source = uri;
                    ResourceDictionary resurce = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resurce);
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "/ThemeSettings.txt", false))
                {
                    writer.WriteLine("System.Windows.Controls.ComboBoxItem: Light");
                }
            }
            if (File.Exists(Directory.GetCurrentDirectory() + "/LanguageSettings.txt"))
            {
                string str = File.ReadAllText(Directory.GetCurrentDirectory() + "/LanguageSettings.txt").Replace("\r\n", "");
                if (str == "System.Windows.Controls.ComboBoxItem: English" || str == "System.Windows.Controls.ComboBoxItem: Англійська")
                {
                    var uri = new Uri("Resources/lang_us.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries[2].Source = uri;
                    ResourceDictionary resurce = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resurce);
                }
                if (str == "System.Windows.Controls.ComboBoxItem: Ukrainian" || str == "System.Windows.Controls.ComboBoxItem: Українська")
                {
                    var uri = new Uri("Resources/lang_ua.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries[2].Source = uri;
                    ResourceDictionary resurce = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resurce);
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "/LanguageSettings.txt", false))
                {
                    writer.WriteLine("System.Windows.Controls.ComboBoxItem: Ukrainian");
                }
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Cache"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Cache");
            }
            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            MessengerLiblary.MessengerLiblary messengerLiblary = new MessengerLiblary.MessengerLiblary();
            messengerLiblary.Connect("127.0.0.1", 8000);
            messengerLiblary.SetNotActive(id);
            base.OnExit(e);
        }
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            MessageBox.Show("qwee");
            base.OnSessionEnding(e);
        }
    }
}
