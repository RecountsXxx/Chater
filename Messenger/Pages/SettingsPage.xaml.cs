using MessengerLiblary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Messenger.Pages
{
    public partial class SettingsPage : Page
    {
        private User user = null;
        private bool IsCheckedPassword = true;
        private bool IsCheckedUsername = true;
        public MessengerLiblary.MessengerLiblary MessengerLiblary = new MessengerLiblary.MessengerLiblary();

        public SettingsPage(User user, MessengerLiblary.MessengerLiblary messenger)
        {
            this.MessengerLiblary = messenger;
            this.user = user;
            InitializeComponent();
        }

        #region Edit buttons
        private void RenamePassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsCheckedPassword)
            {
                PasswordTextBox.IsReadOnly = false;
                IsCheckedPassword = false;
            }
            else
            {
                    if (PasswordTextBox.Text.Length >= 3)
                    {
                        string password = PasswordTextBox.Text;
                        MessengerLiblary.ChangePassword(user.Id, password);
                        MessageBox.Show(Application.Current.FindResource("m_youChangePassword")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);
                        WriteRememerMe("Remember - " + user.Name + " " + password);
                    }
                else
                {
                    MessageBox.Show(Application.Current.FindResource("m_tooLowPassword")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);
                }
                    PasswordTextBox.IsReadOnly = true;
                    IsCheckedPassword = true;
            }
        }
        private async void RenameLogin_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsCheckedUsername)
            {
                IsCheckedUsername = false;
                UsernameTextBox.IsReadOnly = false;

            }
            else
            {
                if (UsernameTextBox.Text.Length >= 3)
                {
                    string username = UsernameTextBox.Text;
                    if (MessengerLiblary.GetUserPerName(username) == null)
                    {
                        MessengerLiblary.ChangeUsername(user.Id, username, user.Name);
                        if (RememberMeCheckBox.IsChecked == true)
                        {
                            WriteRememerMe("Remember - " + username + " " + user.Password);
                        }
                        user.Name = username;
                        MessageBox.Show(Application.Current.FindResource("m_youChangeUsername")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);
                    }
                    else
                    {
                        MessageBox.Show(Application.Current.FindResource("m_userIsContains")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);

                    }
                }
                else
                {
                    MessageBox.Show(Application.Current.FindResource("m_tooLowUsername")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);
                }
                UsernameTextBox.IsReadOnly = true;
                IsCheckedUsername = true;
            }
        }
        private void EditAvatar_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif";
            dialog.ShowDialog();
            if(dialog.FileName.Length > 0)
            {
                MessengerLiblary.SetAvatar(user.Name,File.ReadAllBytes(dialog.FileName));
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(File.ReadAllBytes(dialog.FileName));
                bitmapImage.EndInit();
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = bitmapImage;
                AvatarBorder.Background = imageBrush;
                MessageBox.Show("You change avatar", "Message", MessageBoxButton.OK, MessageBoxImage.Question);

            }
        }
        #endregion

        #region Remember me
        private void RememberMeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WriteRememerMe("Remember - " + user.Name + " "+ user.Password);
        }
        private void RememberMeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WriteRememerMe("False");
        }
        public void WriteRememerMe(string str)
        {
            using (FileStream fs = new FileStream("UserSettings.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                fs.Write(buffer, 0, buffer.Length);
            }
        }
        #endregion

        #region Language and styles
        private void languageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = languageComboBox.SelectedItem.ToString();
            if (languageComboBox.SelectedIndex ==0) {
                    var uri = new Uri("Resources/lang_us.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries[2].Source = uri;
                    ResourceDictionary resurce = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resurce);
            }
            if (languageComboBox.SelectedIndex == 1)
            {
                var uri = new Uri("Resources/lang_ua.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[2].Source = uri;
                ResourceDictionary resurce = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resurce);
            }
            using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "/LanguageSettings.txt", false))
            {
                writer.WriteLine(str);
            }
        }
        private void themeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = themeComboBox.SelectedItem.ToString();
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
       
            using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "/ThemeSettings.txt", false))
            {
                writer.WriteLine(str);
            }
            MainWindow window1 = new MainWindow(user);
            window1.Show();

            var window = Window.GetWindow(this) as MainWindow;
            window.Close();
  
        }
        #endregion

        #region Other buttuns
        private void ClearCache_Click(object sender, MouseButtonEventArgs e)
        {
            string folderPath = "Cache";
            string[] files = Directory.GetFiles(folderPath);

            foreach (string file in files)
            {
                File.Delete(file);
            }
            MessageBox.Show(Application.Current.FindResource("m_clearCache")?.ToString(), "Message", MessageBoxButton.OK,MessageBoxImage.Question);
        }
        private async void UnlockUser_Click(object sender, MouseButtonEventArgs e)
        {
            if (lockedUsersListBox.SelectedItem != null)
            {
                string name = lockedUsersListBox.SelectedItem.ToString().Split(" ")[0];
                User tempUser = MessengerLiblary.GetUserPerName(name);
                MessengerLiblary.UnlockUser(user.Id,tempUser.Id) ;
                lockedUsersListBox.Items.Remove(lockedUsersListBox.SelectedItem);
            }
        }
        private void ExitInAccount_Click(object sender, MouseButtonEventArgs e)
        {

            WriteRememerMe("False");
            MessengerLiblary.client.Close();
            AuthWindow window = new AuthWindow();
            window.Show();

            var temp = Window.GetWindow(this) as MainWindow;
            temp.Close();
        }
        #endregion

        #region Loading
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (File.Exists(Directory.GetCurrentDirectory() + "/ThemeSettings.txt"))
            {
                string str = File.ReadAllText(Directory.GetCurrentDirectory() + "/ThemeSettings.txt").Replace("\r\n", "");
                if (str == "System.Windows.Controls.ComboBoxItem: Dark" || str == "System.Windows.Controls.ComboBoxItem: Темна")
                {
                    themeComboBox.SelectedIndex = 1;
                }
                if (str == "System.Windows.Controls.ComboBoxItem: Light" || str == "System.Windows.Controls.ComboBoxItem: Світла")
                {
                    themeComboBox.SelectedIndex = 0;
                }
            }
            if (File.Exists(Directory.GetCurrentDirectory() + "/LanguageSettings.txt"))
            {
                string str = File.ReadAllText(Directory.GetCurrentDirectory() + "/LanguageSettings.txt").Replace("\r\n", "");
                if (str == "System.Windows.Controls.ComboBoxItem: English" || str == "System.Windows.Controls.ComboBoxItem: Англійська")
                {
                    languageComboBox.SelectedIndex = 0;
                }
                if (str == "System.Windows.Controls.ComboBoxItem: Ukrainian" || str == "System.Windows.Controls.ComboBoxItem: Українська")
                {
                    languageComboBox.SelectedIndex = 1;
                }
            }
            languageComboBox.SelectionChanged += languageComboBox_SelectionChanged;
            themeComboBox.SelectionChanged += themeComboBox_SelectionChanged;

            BitmapImage bitmapImage = new BitmapImage();
            ImageBrush imageBrush = new ImageBrush();

            List<Friend> blockedUsers = MessengerLiblary.GetBlockedUsers(user.Id);
            if (blockedUsers != null)
            {
                foreach (var item in blockedUsers)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lockedUsersListBox.Items.Add(item.Name + " \t\t blocked");
                    });
                }
            }

            bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(MessengerLiblary.GetFile(user.Avatar));
            bitmapImage.EndInit();
            imageBrush = new ImageBrush();
            imageBrush.ImageSource = bitmapImage;
            AvatarBorder.Background = imageBrush;

            string fileName = "LogFile.txt";
            if (File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    string rememberMe = string.Empty;
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        rememberMe = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    }
                    if (rememberMe.Contains("Remember"))
                    {
                        RememberMeCheckBox.IsChecked = true;
                    }
                    else
                    {
                        RememberMeCheckBox.IsChecked = false;
                    }
                }
            }

            UsernameTextBox.Text = user.Name;



        }
        #endregion
    }
}
