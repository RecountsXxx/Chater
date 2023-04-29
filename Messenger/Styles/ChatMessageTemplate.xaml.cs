
using Microsoft.VisualBasic.Devices;
using Microsoft.Windows.Themes;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static MessengerLiblary.Message;

namespace Messenger.Styles
{
    /// <summary>
    /// Логика взаимодействия для ChatMessageTemplate.xaml
    /// </summary>
    public partial class ChatMessageTemplate : UserControl
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public string Date { get; set; }
        public MessageSide SenderSide { get; set; }
        public bool IsPlayed = false;
        public string format { get; set; }
        public string fileExtensions { get; set; }
        public MessengerLiblary.MessengerLiblary MessengerLiblary = null;

        public ChatMessageTemplate(int Id,string Source, string Date, MessageSide aligment,string format,MessengerLiblary.MessengerLiblary messengerLiblary)
        {
            InitializeComponent();
           this.MessengerLiblary = messengerLiblary;
            this.Id = Id;
            this.Date = Date;
            SenderSide = aligment;
            this.Source = Source;
            this.format= format;

            string Theme = "";
            if (File.Exists(Directory.GetCurrentDirectory() + "/ThemeSettings.txt"))
            {
                string str = File.ReadAllText(Directory.GetCurrentDirectory() + "/ThemeSettings.txt").Replace("\r\n", "");
                if (str == "System.Windows.Controls.ComboBoxItem: Dark" || str == "System.Windows.Controls.ComboBoxItem: Темна")
                {
                    Theme = "Dark";
                }
                if (str == "System.Windows.Controls.ComboBoxItem: Light" || str == "System.Windows.Controls.ComboBoxItem: Світла")
                {
                    Theme = "Light";
                }
            }

            if (format != "Text")
            {
                if (!File.Exists("Cache/" + System.IO.Path.GetFileName(Source)))
                {
                    File.WriteAllBytes("Cache/" + System.IO.Path.GetFileName(Source), MessengerLiblary.GetFile(Source));
                    Source = "Cache/" + System.IO.Path.GetFileName(Source);
                }
                else
                    Source = "Cache/" + System.IO.Path.GetFileName(Source);

            }
            if (format == "Video")
            {
                fileExtensions = ".mp4";
                voiceMessage.Visibility = Visibility.Collapsed;
                imageMessage.Visibility = Visibility.Collapsed;
                textMessage.Visibility = Visibility.Collapsed;
                ZipMessage.Visibility = Visibility.Collapsed;
               
                textboxVideo.Text = Date;

                if (aligment == MessageSide.Right)
                {
                    borderVideo.BorderBrush = Brushes.LightBlue;
                    textboxVideo.HorizontalAlignment = HorizontalAlignment.Center;
                }
                else
                {
                    borderVideo.BorderBrush = Brushes.White;
                    textboxVideo.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
            if (format == "Voice")
            { 
                fileExtensions = ".wav";

                imageMessage.Visibility = Visibility.Collapsed;
                textMessage.Visibility = Visibility.Collapsed;
                videoMessage.Visibility = Visibility.Collapsed;
                ZipMessage.Visibility = Visibility.Collapsed;

                if (audioMessageElement.NaturalDuration.HasTimeSpan)
                textBoxVoiceDuration.Text = "Duration: " + audioMessageElement.NaturalDuration.TimeSpan.ToString("mm':'ss");
                textBoxVoice.Text = Date;
                if (aligment == MessageSide.Right)
                {
                    if (Theme == "Light")
                    {
                        borderVoice.Background = Brushes.LightBlue;
                    }
                    if (Theme == "Dark")
                    {
                        borderVoice.Background = Brushes.DimGray;
                    }
                    borderVoice.HorizontalAlignment = HorizontalAlignment.Right;
                    textBoxVoice.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    if (Theme == "Light")
                    {
                        borderVoice.Background = Brushes.White;
                    }
                    if (Theme == "Dark")
                    {
                        borderVoice.Background = Brushes.Gray;
                    }
                    borderVoice.HorizontalAlignment = HorizontalAlignment.Left;
                    textBoxVoice.HorizontalAlignment = HorizontalAlignment.Left;
                }
            }
            if (format == "Image")
            {
                fileExtensions = ".png";

                textMessage.Visibility = Visibility.Collapsed;
                voiceMessage.Visibility = Visibility.Collapsed;
                videoMessage.Visibility = Visibility.Collapsed;
                ZipMessage.Visibility = Visibility.Collapsed;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(System.IO.Path.GetFullPath(Source)));
                bitmap.EndInit();
                imageMessageImg.Source = bitmap;

                textboxImage.Text = Date;
                if (aligment == MessageSide.Right)
                {
                    borderTextBoxImage.HorizontalAlignment = HorizontalAlignment.Right;
                    borderImage.BorderBrush = Brushes.LightBlue;
                }
                else
                {
                    borderTextBoxImage.HorizontalAlignment = HorizontalAlignment.Left;
                    borderImage.BorderBrush = Brushes.White;
                }
            }
            if (format == "Zip")
            {
                imageMessage.Visibility = Visibility.Collapsed;
                textMessage.Visibility = Visibility.Collapsed;
                voiceMessage.Visibility = Visibility.Collapsed;
                videoMessage.Visibility = Visibility.Collapsed;

                ZipDateMessage.Text = Date;
                if (aligment == MessageSide.Right)
                {
                    borderZip.BorderBrush = Brushes.LightBlue;
                    borderZip.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    borderZip.HorizontalAlignment = HorizontalAlignment.Left;
                    borderZip.BorderBrush = Brushes.White;
                }
            }
            if (format == "Text")
            {
                imageMessage.Visibility = Visibility.Collapsed;
                voiceMessage.Visibility = Visibility.Collapsed;
                videoMessage.Visibility = Visibility.Collapsed;
                ZipMessage.Visibility = Visibility.Collapsed;   
                textBoxTextMessage.Text = Source;
                textBoxDateMessage.Text = Date;
                if (aligment == MessageSide.Right)
                {
                    if (Theme == "Light")
                    {
                        borderText.Background = Brushes.LightBlue;
                    }
                    if(Theme == "Dark")
                    {
                        borderText.Background = Brushes.DimGray;
                    }
                    textBoxDateMessage.HorizontalAlignment = HorizontalAlignment.Right;
                    textMessage.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    if (Theme == "Light")
                    {
                        borderText.Background = Brushes.White;
                    }
                    if (Theme == "Dark")
                    {
                        borderText.Background = Brushes.Gray;
                    }
                    textBoxDateMessage.HorizontalAlignment = HorizontalAlignment.Left;
                    textMessage.HorizontalAlignment = HorizontalAlignment.Left;
                }
            }
        }
        #region Video
        private void MediaElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            videoMessageView.Source = new Uri(Source, UriKind.Relative);
            videoMessageView.Play();
                playBorderVideo.Visibility = Visibility.Collapsed;
                videoMessageView.Visibility = Visibility.Visible;
                IsPlayed = true;
            
        }
        private void MediaElement_StopVideo(object sender, MouseButtonEventArgs e)
        {
                videoMessageView.Stop();
            videoMessageView.Close();
            IsPlayed = false;
            playBorderVideo.Visibility = Visibility.Visible;
                videoMessageView.Visibility = Visibility.Collapsed;
            videoMessageView.Stretch = Stretch.Fill;
        }
        private void videoMessageView_MediaEnded(object sender, RoutedEventArgs e)
        {
            videoMessageView.Close();
            videoMessageView.Visibility = Visibility.Collapsed;
            playBorderVideo.Visibility = Visibility.Visible;
            videoMessageView.Stretch = Stretch.Fill;
            IsPlayed = false;
        }
        #endregion

        #region Audio
        private void StartAudioMessage_Click(object sender, MouseButtonEventArgs e)
        {


            if (IsPlayed)
            {

                audioMessageElement.Stop();
                audioMessageElement.Close();
                IsPlayed = false;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.DecodePixelWidth = 200;
                bitmap.StreamSource = new MemoryStream(File.ReadAllBytes("C:\\Users\\Bogdan\\source\\repos\\MiniMessenger\\Messenger\\Images\\play.png"));
                bitmap.EndInit();
                audioImage.Source = bitmap;
            }
            else
            {
                IsPlayed = true;
                audioMessageElement.Source = new Uri(Source,UriKind.Relative);
                audioMessageElement.Play();
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.DecodePixelWidth = 200;
                bitmap.StreamSource = new MemoryStream(File.ReadAllBytes("C:\\Users\\Bogdan\\source\\repos\\MiniMessenger\\Messenger\\Images\\stop.png"));
                bitmap.EndInit();
                audioImage.Source = bitmap;
            }
        }
        private void audioMessageElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            IsPlayed = false;
            audioMessageElement.Close();
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = 200;
            bitmap.StreamSource = new MemoryStream(File.ReadAllBytes("C:\\Users\\Bogdan\\source\\repos\\MiniMessenger\\Messenger\\Images\\play.png"));
            bitmap.EndInit();
            audioImage.Source = bitmap;
        }
        #endregion

    }
}
