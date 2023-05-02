using MessengerLiblary;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;

namespace Messenger.Pages
{
    /// <summary>
    /// Логика взаимодействия для AudioCallPage.xaml
    /// </summary>
    public partial class AudioCallPage : Page
    {
        //когда ещё трубку не взята и выключаеться компьютер, исправить  object not reference проблема когда выключаеться звонящий
        //и когда уже взята трубка ошибка когда отключаеться звонящий
        //исправить онлайн
        //заблокировать все остальные елементы управления
        //сделать язык
        //чистить память

        User user = null;
        User callerUser = null;
        bool firstRun = true;
        int remotePort = 12345;
        int localPort = 54321;
        string ip = "127.0.0.1";
        UdpClient client = null;
        IPEndPoint remoteEP = null;
        Thread receiveThread;
        Thread senderThread;
        WaveInEvent waveIn;
        WaveOutEvent waveOutSound;
        DispatcherTimer timerCloseAudioPage = new DispatcherTimer();
        DispatcherTimer timerEnableButton = new DispatcherTimer();
        DispatcherTimer timerCloseCalling = new DispatcherTimer();
        MainWindow window;
        bool isCanceled = false;

        public AudioCallPage(User user,User callerUser, byte[] callerAvatar,bool sender,MainWindow window)
        {
            InitializeComponent();
            this.window = window;
            this.user = user;
            this.callerUser = callerUser;
            waveIn = new WaveInEvent();
            waveOutSound = new WaveOutEvent();
            MainWindow.CallerInId = callerUser.Id;
            remotePort = user.Port;
            localPort = callerUser.Port;
            ip = user.IpAddress;

            window.mainPageBorder.Visibility = Visibility.Collapsed;
            window.FriendGrid.Visibility = Visibility.Collapsed;
            window.MainGrid.ColumnDefinitions[1].Width = new GridLength(800.0);
            window.MainGrid.ColumnDefinitions[0].Width = new GridLength(0);

            try
            {
                client = new UdpClient(localPort);
                remoteEP = new IPEndPoint(IPAddress.Parse(ip), remotePort);
            }
            catch
            {
                statusLabel.Content = Application.Current.FindResource("m_youAlreadyCalled")?.ToString();
                timerCloseAudioPage.Start();
            }

            if (sender == true)
            {
                closeCallBorder.Margin = new Thickness(0, 150, 0, 0);
                upCallBorder.Visibility = Visibility.Collapsed;
                closeCallBorder.IsEnabled = false;
                statusLabel.Content = Application.Current.FindResource("m_phoning")?.ToString();
            }
            else
            {
                MemoryStream ms = new MemoryStream(File.ReadAllBytes(Directory.GetCurrentDirectory()+"\\Resources\\callingSound.wav"));
                var waveStream = new RawSourceWaveStream(ms, new WaveFormat(44100, 16, 2));
                waveOutSound.Init(waveStream);
                waveOutSound.Play();
                statusLabel.Content = Application.Current.FindResource("m_youCalled")?.ToString();
            }

            ImageBrush imageBrush = new ImageBrush();
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(callerAvatar);
            bitmapImage.EndInit();
            imageBrush = new ImageBrush();
            imageBrush.ImageSource = bitmapImage;
            avatarImage.Background = imageBrush;
            nameLabel.Content = callerUser.Name;

            timerCloseAudioPage.Interval = TimeSpan.FromSeconds(2);
            timerCloseAudioPage.Tick += TimerCloseAudioPage_Tick;

            timerCloseCalling.Interval = TimeSpan.FromSeconds(24);
            timerCloseCalling.Tick += TimerCloseCalling_Tick; ;
            timerCloseCalling.Start();

            timerEnableButton.Interval = TimeSpan.FromSeconds(2);
            timerEnableButton.Tick += TimerEnableButton_Tick; ;
            timerEnableButton.Start();

            Thread thead = new Thread(HandleClient);
            thead.Start();
        }

        private void TimerCloseCalling_Tick(object? sender, EventArgs e)
        {

            MainWindow.MessengerLiblaryCalls.CloseAudioCall(user.Id, callerUser.Id);
            timerCloseCalling.Stop();
            timerCloseAudioPage.Start();
        }
        private void TimerEnableButton_Tick(object? sender, EventArgs e)
        {
            closeCallBorder.IsEnabled = true;
        }
        private void TimerCloseAudioPage_Tick(object? sender, EventArgs e)
        {
            try
            {
                this.NavigationService.Navigate(new MainPage(user.Id));
            }
            catch
            {
                this.NavigationService.Navigate(new MainPage(callerUser.Id));
            }
            timerCloseAudioPage.Stop();
        }

        private void HandleClient()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(500);
                    string response = MainWindow.MessengerLiblaryCalls.CheckStatusAudioCall(user.Id, callerUser.Id);
                    if (response.Contains("User is cancelled call"))
                    {

                        isCanceled = true;
                        if (senderThread != null && receiveThread != null)
                        {
                            waveIn.Dispose();
                            senderThread.Interrupt();
                            receiveThread.Interrupt();
                        }
                        timerCloseAudioPage.Start();
                        waveOutSound.Stop();
                        waveOutSound.Dispose();
                        MainWindow.MessengerLiblaryCalls.DisconnectCallsServer();
                        Dispatcher.BeginInvoke(() => statusLabel.Content = Application.Current.FindResource("m_callIsCanceled")?.ToString());
                        Thread.CurrentThread.Interrupt();
                        break;
                    }
                    if (response == "NICE")
                    {
                        timerCloseCalling.Stop();
                        if (firstRun)
                        {
                            waveOutSound.Stop();
                            waveOutSound.Dispose();
                            Dispatcher.BeginInvoke(() => statusLabel.Content = Application.Current.FindResource("m_callIsRun")?.ToString());


                            receiveThread = new System.Threading.Thread(() =>
                            {

                                while (true)
                                {
                                    try
                                    {
                                        if (!isCanceled)
                                        {
                                            byte[] bytes = client.Receive(ref remoteEP);
                                            var audioStream = new MemoryStream(bytes);

                                            var waveOut = new WaveOutEvent();
                                            var waveStream = new RawSourceWaveStream(audioStream, new WaveFormat(48000, 24, 2));
                                            waveOut.Init(waveStream);
                                            waveOut.Play();
                                        }
                                        else
                                        {
                                            client.Close();
                                            break;
                                        }
                                    }
                                    catch
                                    {
                                        break;
                                    }
                                }

                            });
                            receiveThread.Start();
                            senderThread = new System.Threading.Thread(() =>
                            {
                                string message;
                                waveIn.WaveFormat = new WaveFormat(48000, 24, 2);
                                var writer = new MemoryStream();
                                var writerStream = new WaveFileWriter(writer, waveIn.WaveFormat);

                                waveIn.DataAvailable += (sender, e) =>
                                {
                                    byte[] bytes = e.Buffer;
                                    client.Send(bytes, bytes.Length, remoteEP);
                                };
                                waveIn.StartRecording();
                            });
                            senderThread.Start();
                            firstRun = false;

                        }
                    }

                }
                catch
                {

                }
            }
        }
        private void closeCall_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.MessengerLiblaryCalls.CloseAudioCall(user.Id,callerUser.Id);
            waveOutSound.Stop();
            waveOutSound.Dispose();
        }
        private void upCall_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.MessengerLiblaryCalls.UpAudioCall(user.Id, callerUser.Id);
            closeCallBorder.Margin = new Thickness(0, 150, 0, 0);
            upCallBorder.Visibility = Visibility.Collapsed;      
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.mainPageBorder.Visibility = Visibility.Visible;
            window.FriendGrid.Visibility = Visibility.Visible;
            window.MainGrid.ColumnDefinitions[1].Width = new GridLength(600.0);
            window.MainGrid.ColumnDefinitions[0].Width = new GridLength(200);

            client.Close();
            client.Dispose();
            if (senderThread != null && receiveThread != null)
            {
                senderThread.Interrupt();
                receiveThread.Interrupt();
            }
            timerCloseAudioPage.Stop();
        }
    }
}