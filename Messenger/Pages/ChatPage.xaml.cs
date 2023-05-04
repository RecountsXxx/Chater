using MessengerLiblary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAudio.Wave;
using System.Drawing;
using System.IO;
using Brushes = System.Windows.Media.Brushes;
using Messenger.Styles;
using System.Windows.Shapes;
using System.Windows.Forms;
using Message = MessengerLiblary.Message;
using ListBox = System.Windows.Controls.ListBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Microsoft.Windows.Themes;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using Application = System.Windows.Application;

namespace Messenger.Pages
{
    public partial class ChatPage : Page
    {
        private User user = null;
        private User receiveUser = null;

        private DispatcherTimer timerUpdateChats = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
        private DispatcherTimer timerSendMessage = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.5) };

        private WaveIn _recorder;
        private WaveFileWriter _fileWriter;
        public MessengerLiblary.MessengerLiblary MessengerLiblary = new MessengerLiblary.MessengerLiblary();
        public MessengerLiblary.MessengerLiblary MessengerLiblaryTimer = new MessengerLiblary.MessengerLiblary();
        public MainWindow window; //для отключение ui елементов при звонке

        private bool boolTimerSendMessage = true; //отправка сообщение только раз в 1 секунду
        private bool isRecording = false; //записываеться ли голосовое сообщение
        private string _fileName; //имя файла для аудио сообщение
        private string times = ""; //время отправкки последнего сообщение
        private int _voiceCreateFileId = 0; //id для создание аудио файла
        private bool IsFirstStartUpReceiveMessage = true; //проверка что бь не приходилл первое сообещние из таблицыы
        private int messagesFetch = 0; //счетчик сколько получить следующих сообщение, при скролле
        private int userId = 0;
        private int receivedId = 0;
        private bool receiverOnline;

        public ChatPage(User user, User receiveUser, MessengerCallsLiblary messengerLiblaryCalls,MainWindow window)
        {
            MessengerLiblary.Connect("127.0.0.1", 8000);
            MessengerLiblaryTimer.Connect("127.0.0.1", 8000);
            this.window = window;
            this.user = user;
            this.receiveUser = receiveUser;
            userId = user.Id;
            receivedId = receiveUser.Id;
            receiverOnline = receiveUser.IsOnline;

            MainWindow.MessengerLiblaryCalls = messengerLiblaryCalls;
            InitializeComponent();
        }


        #region Timers
        private void TimerSendMessage_Tick(object? sender, EventArgs e)
        {
            boolTimerSendMessage = true;

        }
        private  async void TimerUpdateChats_Tick(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {             
                if (receiverOnline == true)
                {
                    if (MessengerLiblaryTimer.CheckMessagesTime(userId, receivedId) != times)
                    {     
                        Message message = MessengerLiblaryTimer.CheckMessagesChat(userId, receivedId);
                        if (message != null)
                        {       
                            times = MessengerLiblaryTimer.CheckMessagesTime(userId, receivedId);
             
                            if (!IsFirstStartUpReceiveMessage)
                            {
                                await Dispatcher.BeginInvoke(() =>
                                {
                                    if (message.Text != null)
                                    {
                                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, message.Text, message.TimeStamp.ToString(), MessageSide.Left, "Text", MessengerLiblary));
                                    }
                                    if (message.ImageMessage != null)
                                    {
                                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, message.ImageMessage, message.TimeStamp.ToString(), MessageSide.Left, "Image", MessengerLiblary));
                                    }
                                    if (message.VoiceMessage != null)
                                    {
                                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, message.VoiceMessage, message.TimeStamp.ToString(), MessageSide.Left, "Voice", MessengerLiblary));
                                    }
                                    if (message.VideoMessage != null)
                                    {
                                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, message.VideoMessage, message.TimeStamp.ToString(), MessageSide.Left, "Video", MessengerLiblary));
                                    }
                                    if (message.ZipMessage != null)
                                    {
                                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, message.ZipMessage, message.TimeStamp.ToString(), MessageSide.Left, "Zip", MessengerLiblary));
                                    }
                                    ToBottom(chatListBox);
                                });
                            }
                            IsFirstStartUpReceiveMessage = false;
                        }
                    }
                }
            });
        }
        #endregion

        #region SendMessages
        private async void buttonSendVideoMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Run(async() =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Video Files (*.mp4)|*.mp4";
                dialog.ShowDialog();
                if (dialog.FileName.Length > 2)
                {
                    Message message = new Message(0, DateTime.Now, user.Id, receiveUser.Id) { VideoMessage = dialog.FileName };
                    message.Id = await MessengerLiblary.SendMessage(message, File.ReadAllBytes(dialog.FileName), "Video");

                    byte[] videoBytes = File.ReadAllBytes(dialog.FileName);
                    string path = "Cache/" + message.Id + ".mp4";
                    File.WriteAllBytes(path, videoBytes);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, path, message.TimeStamp.ToString(), MessageSide.Right, "Video", MessengerLiblary));
                    }));
                    boolTimerSendMessage = false;
                    timerSendMessage.Start();
                }
            });
        }
        private async void buttonSendRarMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Run(async () =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "ZIP Files (*.rar)|*.rar";
                dialog.ShowDialog();
                if (dialog.FileName.Length > 2)
                {
                    Message message = new Message(0, DateTime.Now, user.Id, receiveUser.Id) { ZipMessage = dialog.FileName };
                    message.Id = await MessengerLiblary.SendMessage(message, File.ReadAllBytes(dialog.FileName), "Zip");

                    byte[] videoBytes = File.ReadAllBytes(dialog.FileName);
                    string path = "Cache/" + message.Id + ".zip";
                    File.WriteAllBytes(path, videoBytes);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, path, message.TimeStamp.ToString(), MessageSide.Right, "Zip", MessengerLiblary));
                    }));
                    boolTimerSendMessage = false;
                    timerSendMessage.Start();

                }
            });

        }
        private async void buttonSendImageMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Run(async() => { 
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif";
            dialog.ShowDialog();
            if (dialog.FileName.Length > 2)
            {
                Message message = new Message(0, DateTime.Now, user.Id, receiveUser.Id) { ImageMessage = dialog.FileName };
                message.Id = await MessengerLiblary.SendMessage(message, File.ReadAllBytes(dialog.FileName), "Image");

                byte[] videoBytes = File.ReadAllBytes(dialog.FileName);
                string path = "Cache/" + message.Id + ".png";
                File.WriteAllBytes(path, videoBytes);

                    Dispatcher.Invoke(() => chatListBox.Items.Add(new ChatMessageTemplate(message.Id, path, message.TimeStamp.ToString(), MessageSide.Right, "Image", MessengerLiblary)));
                boolTimerSendMessage = false;
                timerSendMessage.Start();

            }
        });
        }
        private async void buttonSendVoiceMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Run(async () =>
            {
                if (isRecording)
                {
                    Dispatcher.Invoke(() =>
                    {
                        isRecording = false;
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = new MemoryStream(File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\Images\\VoiceMessage.png"));
                        bitmapImage.EndInit();
                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = bitmapImage;
                        buttonSendVoiceMessage.Source = imageBrush.ImageSource;
                        _recorder.StopRecording();
                        _fileWriter.Dispose();
                        _recorder.Dispose();
                    });


                    Message message = new Message(0, DateTime.Now, user.Id, receiveUser.Id) { VoiceMessage = _fileName };
                    message.Id = await MessengerLiblary.SendMessage(message, File.ReadAllBytes(_fileName), "Voice");
                    _voiceCreateFileId = message.Id;
                    Dispatcher.Invoke(() =>
                    {
                        chatListBox.Items.Add(new ChatMessageTemplate(message.Id, _fileName, message.TimeStamp.ToString(), MessageSide.Right, "Voice", MessengerLiblary));
                    });
                    boolTimerSendMessage = false;
                    timerSendMessage.Start();
                }
                else
                {
                    isRecording = true;
                    Dispatcher.Invoke(() =>
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = new MemoryStream(File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\Images\\VoiceMessageOff.png"));
                        bitmapImage.EndInit();
                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = bitmapImage;

                        buttonSendVoiceMessage.Source = imageBrush.ImageSource;


                        _recorder = new WaveIn();
                        _recorder.WaveFormat = new WaveFormat(44100, 16, 2);
                        _recorder.DataAvailable += Recorder_DataAvailable;


                        _fileName = $"Cache/{_voiceCreateFileId}.wav";
                        _fileWriter = new WaveFileWriter(_fileName, _recorder.WaveFormat);
                        _recorder.StartRecording();
                    });
                }
            });
        }
        private async void buttonSendMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string Message = MessageTextBox.Text;
            await Task.Run(async () =>
            {
                if (boolTimerSendMessage)
                {
                    if (Message.Length > 0)
                    {
                        Message message = new Message(0, DateTime.Now, user.Id, receiveUser.Id) { Text = Message };
                        message.Id = await MessengerLiblary.SendMessage(message, null, "Text");
                        Dispatcher.Invoke(new Action(() =>
                        {
                            chatListBox.Items.Add(new ChatMessageTemplate(message.Id, message.Text, message.TimeStamp.ToString(), MessageSide.Right, "Text", MessengerLiblary));
                            ToBottom(chatListBox);
                            MessageTextBox.Text = "";
                        }));
                        boolTimerSendMessage = false;
                        timerSendMessage.Start();
                    }


                }
                else
                {
                    MessageBox.Show(Application.Current.FindResource("m_wait")?.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            });
        }

        #endregion

        #region Other funcs
        private async Task GetMessages()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    List<Message> msg = MessengerLiblary.GetMessages(receiveUser.Id, user.Id);
                    if (msg != null)
                    {
                        for (int i = msg.Count - 1; i > -1; i--)
                        {
                            if (msg[i].SenderId == user.Id)
                            {
                                if (msg[i].Text != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].Text, msg[i].TimeStamp.ToString(), MessageSide.Right, "Text", MessengerLiblary));
                                }
                                if (msg[i].ImageMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].ImageMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Image", MessengerLiblary));
                                }
                                if (msg[i].VoiceMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].VoiceMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Voice", MessengerLiblary));
                                }
                                if (msg[i].VideoMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].VideoMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Video", MessengerLiblary));
                                }
                                if (msg[i].ZipMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].ZipMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Zip", MessengerLiblary));
                                }
                            }
                            else
                            {
                                if (msg[i].Text != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].Text, msg[i].TimeStamp.ToString(), MessageSide.Left, "Text", MessengerLiblary));
                                }
                                if (msg[i].ImageMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].ImageMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Image", MessengerLiblary));
                                }
                                if (msg[i].VoiceMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].VoiceMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Voice", MessengerLiblary));
                                }
                                if (msg[i].VideoMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].VideoMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Video", MessengerLiblary));
                                }
                                if (msg[i].ZipMessage != null)
                                {
                                    chatListBox.Items.Add(new ChatMessageTemplate(msg[i].Id, msg[i].ZipMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Zip", MessengerLiblary));
                                }
                            }

                        }
                        ToBottom(chatListBox);
                    }
                });
            });
        }
        private void Recorder_DataAvailable(object? sender, WaveInEventArgs e)
        {
            _fileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            
        }
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (MessageTextBox.IsFocused == true)
                {
                    buttonSendMessage_MouseLeftButtonDown(null, null);
                }
            }
        }
        private ScrollViewer FindViewer(DependencyObject root)
        {
            var queue = new Queue<DependencyObject>(new[] { root });

            do
            {
                var item = queue.Dequeue();
                if (item is ScrollViewer) { return (ScrollViewer)item; }
                var count = VisualTreeHelper.GetChildrenCount(item);
                for (var i = 0; i < count; i++) { queue.Enqueue(VisualTreeHelper.GetChild(item, i)); }
            } while (queue.Count > 0);

            return null;
        }
        public void ToBottom(ListBox listBox)
        {
            var scrollViewer = FindViewer(listBox);

            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += (o, args) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (args.ExtentHeightChange > 0) { scrollViewer.ScrollToBottom(); }
                    }));
                };
            }
        }
        private void chatListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (chatListBox.Items.Count > 0)
                chatListBox.ScrollIntoView(chatListBox.Items[chatListBox.Items.Count - 1]);
        }
        private void DeleteMesages_Click(object sender, RoutedEventArgs e)
        {
            if (chatListBox.SelectedIndex != -1)
            {
                ChatMessageTemplate item = (ChatMessageTemplate)chatListBox.SelectedItem;

                MessengerLiblary.DeleteMessage(user.Id, receiveUser.Id, item.Id, item.fileExtensions);
                chatListBox.Items.Remove(item);
            }
        }
        private void OpenFolderPath_Click(object sender, RoutedEventArgs e)
        {
            if (chatListBox.SelectedIndex != -1)
            {
                ChatMessageTemplate item = (ChatMessageTemplate)chatListBox.SelectedItem;
                if (item.format != "Text")
                {
                    string path = System.IO.Path.GetFullPath("Cache\\" + System.IO.Path.GetFileName(item.Source));
                    Process.Start(new ProcessStartInfo("explorer", $"/n, /select, " + path));
                }
                else
                {
                    MessageBox.Show("Erorr - text message");
                }
            }
        }
        #endregion

        #region Loading and Unloading and Scroll changed
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(async() =>
            {
                await GetMessages();

                Dispatcher.Invoke(() =>
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    ImageBrush imageBrush = new ImageBrush();

                    if (receiveUser != null)
                    {
                        if (user.Name != receiveUser.Name)
                        {
                            if (receiveUser.LastReceived != null)
                            {
                                lastReceivedLabel.Content = "Last received: " + receiveUser.LastReceived.ToString();
                            }
                            if (receiveUser.LastReceived == DateTime.MinValue)
                            {
                                lastReceivedLabel.Content = "Online";
                            }
                            chatReceiverNameLabel.Content = receiveUser.Name;
                            bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = new MemoryStream(MessengerLiblary.GetFile(receiveUser.Avatar));
                            bitmapImage.EndInit();
                            imageBrush = new ImageBrush();
                            imageBrush.ImageSource = bitmapImage;
                            AvatarFriendBorder.Background = imageBrush;

                            timerUpdateChats.Tick += TimerUpdateChats_Tick;
                            timerUpdateChats.Start();
                        }
                        timerSendMessage.Tick += TimerSendMessage_Tick;
                    }
                });
            });
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            timerSendMessage.Stop();
            timerUpdateChats.Stop();
            MessengerLiblary.Disconnect();
            if(_recorder != null)
            {
                _recorder.Dispose();
                _fileWriter.Dispose();
            }
            user = null;
            receiveUser = null;
            _recorder = null;
            _fileWriter = null;
            _fileName = null;
            times = null;
        }
        private async void chatListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset <= 100 && e.ExtentHeight > 0 && e.VerticalOffset > 0)
            {
                messagesFetch += 15;
                await Task.Run(() =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        List<Message> msg = MessengerLiblary.GetNextMessages(receiveUser.Id, user.Id, messagesFetch);
                        if (msg != null)
                        {
                            for (int i = 0; i < msg.Count - 1; i++)
                            {
                                if (msg[i].SenderId == user.Id)
                                {
                                    if (msg[i].Text != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].Text, msg[i].TimeStamp.ToString(), MessageSide.Right, "Text", MessengerLiblary));
                                    }
                                    if (msg[i].ImageMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].ImageMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Image", MessengerLiblary));
                                    }
                                    if (msg[i].VoiceMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].VoiceMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Voice", MessengerLiblary));
                                    }
                                    if (msg[i].VideoMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].VideoMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Video", MessengerLiblary));
                                    }
                                    if (msg[i].ZipMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].ZipMessage, msg[i].TimeStamp.ToString(), MessageSide.Right, "Zip", MessengerLiblary));
                                    }
                                }
                                else
                                {
                                    if (msg[i].Text != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].Text, msg[i].TimeStamp.ToString(), MessageSide.Left, "Text", MessengerLiblary));
                                    }
                                    if (msg[i].ImageMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].ImageMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Image", MessengerLiblary));
                                    }
                                    if (msg[i].VoiceMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].VoiceMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Voice", MessengerLiblary));
                                    }
                                    if (msg[i].VideoMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].VideoMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Video", MessengerLiblary));
                                    }
                                    if (msg[i].ZipMessage != null)
                                    {
                                        chatListBox.Items.Insert(0, new ChatMessageTemplate(msg[i].Id, msg[i].ZipMessage, msg[i].TimeStamp.ToString(), MessageSide.Left, "Zip", MessengerLiblary));
                                    }
                                }

                            }

                        }
                    });
                });
            }
        }
        #endregion

        #region Calls
        private void AudioCall_Click(object sender, MouseButtonEventArgs e)
        {
            if (receiverOnline == true)
            {
                MainWindow.timerCheckOutCalls.Stop();
                MainWindow.MessengerLiblaryCalls.AudioCall(receivedId, userId);
                NavigationService.Navigate(new AudioCallPage(user, receiveUser, MessengerLiblary.GetFile(receiveUser.Avatar), true,window));
            }
            else
            {
                MessageBox.Show("User is not online");
            }
        }
        private void VideoCall_Click(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

    }
}
