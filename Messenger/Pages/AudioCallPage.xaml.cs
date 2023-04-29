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

namespace Messenger.Pages
{
    /// <summary>
    /// Логика взаимодействия для AudioCallPage.xaml
    /// </summary>
    public partial class AudioCallPage : Page
    {
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
        WaveInEvent waveIn = new WaveInEvent();
        WaveOutEvent waveOutSound = new WaveOutEvent();
        DispatcherTimer timerCloseAudioPage = new DispatcherTimer();
        bool isCanceled = false;
        public AudioCallPage(User user,User callerUser, byte[] callerAvatar,bool sender)
        {

            this.user = user;
            this.callerUser = callerUser;
            InitializeComponent();
            if(sender == true)
            {
                closeCallBorder.Margin = new Thickness(0, 150, 0, 0);
                upCallBorder.Visibility = Visibility.Collapsed;
                    remotePort = 54321;
                    localPort = 12345;
                    ip = "127.0.0.255";
            }
            else
            {
                MemoryStream ms = new MemoryStream(File.ReadAllBytes("C:\\Users\\Bogdan\\source\\repos\\MiniMessenger\\Messenger\\Resources\\callingSound.wav"));
                var waveStream = new RawSourceWaveStream(ms, new WaveFormat(44100, 16, 2));
                waveOutSound.Init(waveStream);
                waveOutSound.Play();
            }

            //client = new UdpClient(localPort);
            //remoteEP = new IPEndPoint(IPAddress.Parse(ip), remotePort);

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

            Thread thead = new Thread(HandleClient);
            thead.Start();
        }

        private void TimerCloseAudioPage_Tick(object? sender, EventArgs e)
        {
            this.NavigationService.Navigate(new MainPage(user.Id));
            timerCloseAudioPage.Stop();
        }

        private void HandleClient()
        {
            while (true)
            {
                Thread.Sleep(500);
                string response =MainWindow.MessengerLiblaryCalls.CheckStatusAudioCall(user.Id,callerUser.Id);
                //Dispatcher.BeginInvoke(() => statusLabel.Content = response);
                if (response.Contains("User is cancelled call"))
                {
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
                    Dispatcher.BeginInvoke(() => statusLabel.Content = "Вызов завершён");
                    break;
                }
                if(response == "NICE")
                {
                    if (firstRun)
                    {
                        waveOutSound.Stop();
                        waveOutSound.Dispose();
                        Dispatcher.BeginInvoke(() => statusLabel.Content = "Вызов идёт");
     

                            //receiveThread = new System.Threading.Thread(() =>
                            //{

                            //    while (true)
                            //    {
                            //        if (!isCanceled)
                            //        {
     
                            //            byte[] bytes = client.Receive(ref remoteEP);
                            //            var audioStream = new MemoryStream(bytes);

                            //            var waveOut = new WaveOutEvent();
                            //            var waveStream = new RawSourceWaveStream(audioStream, new WaveFormat(48000, 24, 2));
                            //            waveOut.Init(waveStream);
                            //            waveOut.Play();
                            //        }
                            //        else
                            //        {
                            //            client.Close();
                            //        }
                            //    }

                            //});
                            //receiveThread.Start();
                            //senderThread = new System.Threading.Thread(() =>
                            //{

                            //    string message;

                            //    waveIn.WaveFormat = new WaveFormat(48000, 24, 2);

                            //    var writer = new MemoryStream();
                            //    var writerStream = new WaveFileWriter(writer, waveIn.WaveFormat);

                            //    waveIn.DataAvailable += (sender, e) =>
                            //    {
                            //        byte[] bytes = e.Buffer;
                            //        client.Send(bytes, bytes.Length, remoteEP);
                            //    };
                            //    waveIn.StartRecording();

                            //});
                            senderThread.Start();
                            firstRun = false;

                    }   
                }
            }
        }
        private void closeCall_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.MessengerLiblaryCalls.CloseAudioCall(user.Id,callerUser.Id);
            waveOutSound.Stop();
            waveOutSound.Dispose();
            //NavigationService.Navigate(new MainPage());
        }
        private void upCall_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.MessengerLiblaryCalls.UpAudioCall(user.Id, callerUser.Id);
            closeCallBorder.Margin = new Thickness(0, 150, 0, 0);
            upCallBorder.Visibility = Visibility.Collapsed;
            statusLabel.Content = "Звоним";
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //client.Close();
            //client.Dispose();
            //if (senderThread != null && receiveThread != null)
            //{
            //    senderThread.Interrupt();
            //    receiveThread.Interrupt();
            //}
        }
    }
}