using MessengerLiblary;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MessengerLiblary;
using Microsoft.VisualBasic.ApplicationServices;
using User = MessengerLiblary.User;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Runtime.InteropServices;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private DispatcherTimer timerShowMessage = new DispatcherTimer();
        public MessengerLiblary.MessengerLiblary MessengerLiblary = new  MessengerLiblary.MessengerLiblary();

        public AuthWindow()
        {
            try
            {
                MessengerLiblary.Connect("127.0.0.1", 8000);
            }
            catch
            {
                MessageBox.Show(Application.Current.FindResource("m_shutdown")?.ToString(), "Erorr", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            if (!IsConnectedToInternet())
            {
                MessageBox.Show(Application.Current.FindResource("m_noEthernet")?.ToString(), "Erorr", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            checkRememberMe();
           
            InitializeComponent();

            timerShowMessage.Interval = TimeSpan.FromSeconds(3);
            timerShowMessage.Tick += TimerShowMessage_Tick;
        }
      
        #region Timer
        private async void TimerShowMessage_Tick(object? sender, EventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.1)
                {
                    Thread.Sleep(15);
                    Dispatcher.Invoke(() =>
                    {
                        messageLabel.Opacity = i;
                    });

                }
                timerShowMessage.Stop();
            });
        }
        #endregion

        #region Window buttons
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void Window_DragDrop(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessengerLiblary.client.Connected == true)
                MessengerLiblary.Disconnect();

        }
        #endregion

        #region Login and Reg
        private async void RegisterAccount_Click(object sender, MouseButtonEventArgs e)
        {
            timerShowMessage.Stop();
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Text;
            if (!username.Contains('\'') && !password.Contains('\'') && !username.Contains(' ') && !password.Contains(' '))
            {
                if (username.Length >= 3 && password.Length >= 3)
                {
                    if (username.Length <= 16 && password.Length <=16)
                    {
                        timerShowMessage.Start();
                        bool result = false;
                        SlowOpacity(Application.Current.FindResource("m_waiting")?.ToString(), Brushes.GreenYellow);
                        await Task.Run(() => result = MessengerLiblary.Register(username, password));
                        if (result)
                            SlowOpacity(Application.Current.FindResource("m_succesfull")?.ToString(), Brushes.GreenYellow);
                        else
                            SlowOpacity(Application.Current.FindResource("m_userIsContains")?.ToString(), Brushes.OrangeRed);
                    }
                    else
                    {
                        timerShowMessage.Start();
                        SlowOpacity(Application.Current.FindResource("m_longFields")?.ToString(), Brushes.OrangeRed);
                    }
                }
                else
                {
                    timerShowMessage.Start();
                    SlowOpacity(Application.Current.FindResource("m_enterOtherFields")?.ToString(), Brushes.OrangeRed);
                }
            }
            else
            {
                timerShowMessage.Start();
                SlowOpacity(Application.Current.FindResource("m_dontEnterSymbol")?.ToString(), Brushes.OrangeRed);
            }
        }
        private async void LoginAccount_Click(object sender, MouseButtonEventArgs e)
        {
            timerShowMessage.Stop();
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Text;
            if (!username.Contains('\'') && !password.Contains('\'') && !username.Contains(' ') && !password.Contains(' '))
            {
                if (username.Length <= 16 && password.Length <= 16)
                {
                    if (username.Length >= 3 && password.Length >= 3)
                    {
                        timerShowMessage.Start();
                        User user = null;
                        SlowOpacity(Application.Current.FindResource("m_waiting")?.ToString(), Brushes.GreenYellow);
                        await Task.Run(() => user = MessengerLiblary.Login(username, password));
                        if (user != null)
                        {
                            SlowOpacity(Application.Current.FindResource("m_succesfull")?.ToString(), Brushes.GreenYellow);
                            MainWindow window = new MainWindow(user);
                            window.Show();
                            this.Close();
                        }
                        else
                            SlowOpacity(Application.Current.FindResource("m_invalidFields")?.ToString(), Brushes.OrangeRed);
                    }
                    else
                    {
                        timerShowMessage.Start();
                        SlowOpacity(Application.Current.FindResource("m_enterOtherFields")?.ToString(), Brushes.OrangeRed);
                    }
                }
                else
                {
                    timerShowMessage.Start();
                    SlowOpacity(Application.Current.FindResource("m_longFields")?.ToString(), Brushes.OrangeRed);
                }
            }
            else
            {
                timerShowMessage.Start();
                SlowOpacity(Application.Current.FindResource("m_dontEnterSymbol")?.ToString(), Brushes.OrangeRed);
            }
        }
        private async void SlowOpacity(string message, Brush brush)
        {
            await Task.Factory.StartNew(() =>
            {
                for (double i = 0.0; i < 1.1; i += 0.1)
                {
                    Thread.Sleep(15);
                    Dispatcher.Invoke(() =>
                    {
                        messageLabel.Content = message;
                        messageLabel.Foreground = brush;
                        messageLabel.Opacity = i;
                    });
                }
            });
        }
        #endregion

        #region Interner connection and Check remember me
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }
        private async void checkRememberMe()
        {
            string fileName = "UserSettings.txt";
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

                        User user = null;
                        await Task.Run(() => user = MessengerLiblary.Login(rememberMe.Split(" ")[2], rememberMe.Split(" ")[3]));
                        if (user != null)
                        {
                            MainWindow window = new MainWindow(user);
                            window.Show();
                            this.Close();
                        }
                        else
                        {
                            using (FileStream qs = new FileStream("UserSettings.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                            {
                                buffer = Encoding.UTF8.GetBytes("False");
                                qs.Write(buffer, 0, buffer.Length);
                            }
                        }

                    }
                }
            }
        }
        #endregion
    }
}
