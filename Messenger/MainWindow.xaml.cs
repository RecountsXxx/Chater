using MessengerLiblary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Messenger.Pages;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Messenger
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timerUpdateFriends = new DispatcherTimer();
        private DispatcherTimer timerUpdateMessages = new DispatcherTimer();
        private DispatcherTimer timerWaitChangedChats = new DispatcherTimer();
        public static DispatcherTimer timerCheckOutCalls = new DispatcherTimer();

        private List<Friend> friends = new List<Friend>();
        private User user = null;
        private User receiveUser = null;
        public MessengerLiblary.MessengerLiblary MessengerLiblary = new MessengerLiblary.MessengerLiblary();
        public static MessengerCallsLiblary MessengerLiblaryCalls = new MessengerCallsLiblary();

        private NotifyIcon notify = new NotifyIcon();
        private Mutex mutex = new Mutex();

        private int callFromId = 0; //id человека кому звонят
        public static int CallerInId = 0; //id человека который звонит
        private string times = string.Empty; //последнее время прихода сообщение
        private bool endWaitingChangedChats = true; //таймер на смену чата, что бь не так быстро переключаться между чатами
        private bool firstStartupToReceiveMessage = true; //первый запуск приложение, сделано что бь не приходило первое сообщение из таблицы а токо новое

        public MainWindow(User user)
        {
            this.user = user;
            MessengerLiblary.Connect("127.0.0.1", 8000);
            InitializeComponent();
            FramePage.Content = new MainPage(user.Id);
            UpdateFriendList();
            UpdateRequestFriendList();
            App.id = user.Id;

            timerCheckOutCalls.Interval = TimeSpan.FromSeconds(1);
            timerCheckOutCalls.Tick += TimerCheckOutCalls_Tick;

            timerWaitChangedChats.Interval = TimeSpan.FromSeconds(1);
            timerWaitChangedChats.Tick += TimerWaitChangedChats_Tick;

            timerUpdateFriends.Interval = TimeSpan.FromSeconds(15);
            timerUpdateFriends.Tick += TimerUpdateFriend_Tick;
            timerUpdateFriends.Start();

            timerUpdateMessages.Interval = TimeSpan.FromSeconds(1);
            timerUpdateMessages.Tick += TimerUpdateChat_Tick;
            timerUpdateMessages.Start();
        }

        #region Timer
        private void TimerCheckOutCalls_Tick(object? sender, EventArgs e)
        {
            callFromId = MessengerLiblaryCalls.CheckOutCalls(user.Id);
            if (callFromId > 0)
            {
                User callerUser = MessengerLiblary.GetUserPerId(callFromId);
                SlowOpacity(new AudioCallPage(user,callerUser, MessengerLiblary.GetFile(callerUser.Avatar),false,this));
                timerCheckOutCalls.Stop();
            }
            
        }
        private void TimerWaitChangedChats_Tick(object? sender, EventArgs e)
        {
            endWaitingChangedChats = true;
            timerWaitChangedChats.Stop();
        }
        private void TimerUpdateFriend_Tick(object? sender, EventArgs e)
        {
            mutex.WaitOne();
            UpdateFriendList();
            mutex.ReleaseMutex();
            mutex.WaitOne();
            UpdateRequestFriendList();
            mutex.ReleaseMutex();


        }
        private async void TimerUpdateChat_Tick(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                mutex.WaitOne();

                if (MessengerLiblary.CheckMessagesTime(user.Id) != times)
                {
                    MessengerLiblary.Message msg = MessengerLiblary.CheckMessages(user.Id);
                    if (msg != null)
                    {
                        times = MessengerLiblary.CheckMessagesTime(user.Id);
                        if (!firstStartupToReceiveMessage)
                        {
                            User userSender = MessengerLiblary.GetUserPerId(msg.SenderId);
                            notify.Icon = new System.Drawing.Icon(Directory.GetCurrentDirectory() + @"\Images\icon.ico");
                            notify.Visible = true;
                            notify.Text = Application.Current.FindResource("m_Title")?.ToString();
                            ShowInTaskbar = true;
                            notify.ShowBalloonTip(2, Application.Current.FindResource("m_Title")?.ToString(), Application.Current.FindResource("m_youHaveMessage")?.ToString() + userSender.Name, ToolTipIcon.Info);
                            firstStartupToReceiveMessage = false;
                        }
                        firstStartupToReceiveMessage = false;
                    }

                }

                mutex.ReleaseMutex();
            }
        }
        #endregion

        #region Friends
        private async void FriendListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FriendListBox.SelectedIndex != -1)
            {

                if (endWaitingChangedChats)
                {
                    string receiveName = friends[FriendListBox.SelectedIndex].Name;
                    receiveUser = MessengerLiblary.GetUserPerName(receiveName);
                    SlowOpacity(new ChatPage(user, receiveUser, MessengerLiblaryCalls,this));
                    endWaitingChangedChats = false;
                    timerWaitChangedChats.Start();
                }
                else
                {
                    System.Windows.MessageBox.Show(Application.Current.FindResource("m_sooFast")?.ToString());
                }

            }
        }
        private async void AddFriend_Click(object sender, MouseButtonEventArgs e)
        {
            SlowOpacity(new AddFriendPage(user.Id, MessengerLiblary));
        }
        public void UpdateFriendList_Click(object sender, MouseButtonEventArgs e)
        {
            mutex.WaitOne();
            UpdateFriendList();
            mutex.ReleaseMutex();
        }
        #endregion

        #region Settings
        private async void OpenMenu_Click(object sender, MouseButtonEventArgs e)
        {
            GC.Collect();
            SlowOpacity(new SettingsPage(user, MessengerLiblary));
        }
        #endregion

        #region Context menu
        private void DeleteFriend_Click(object sender, RoutedEventArgs e)
        {
            if (FriendListBox.SelectedItem != null)
            {
                MessengerLiblary.DeleteFriend(user.Id, MessengerLiblary.GetUserPerName(friends[FriendListBox.SelectedIndex].Name).Id);
                int index = friends.IndexOf(friends.Where(x => x.Name == friends[FriendListBox.SelectedIndex].Name).FirstOrDefault());
                friends.RemoveAt(index);
                FriendListBox.Items.RemoveAt(index);
            }
        }
        private void DeleteAllMesages_Click(object sender, RoutedEventArgs e)
        {
            if (FriendListBox.SelectedItem != null && receiveUser != null)
            {
                MessengerLiblary.DeleteAllMessages(user.Id, receiveUser.Id);
                System.Windows.MessageBox.Show(Application.Current.FindResource("m_chatCleaned")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);
                FramePage.Content = new ChatPage(user, receiveUser,MessengerLiblaryCalls,this);
            }
        }
        private void BlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (FriendListBox.SelectedItem != null && receiveUser != null)
            {
                MessengerLiblary.BlockUser(user.Id, MessengerLiblary.GetUserPerName(friends[FriendListBox.SelectedIndex].Name).Id);
                int index = friends.IndexOf(friends.Where(x => x.Name == friends[FriendListBox.SelectedIndex].Name).FirstOrDefault());
                friends.RemoveAt(index);
                FriendListBox.Items.RemoveAt(index);
            }
        }
        #endregion

        #region Search textbox
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FriendListBox.Items.Clear();
            List<Friend> friendList = new List<Friend>();
            string str = SearchTextBox.Text;
            if (str.Equals(""))
            {
                friendList.AddRange(friends);
            }
            else
            {
                foreach (var item in friends)
                {
                    if (item.Name.Contains(str))
                    {
                        friendList.Add(item);
                    }
                }
            }

            foreach (var item in friendList)
            {
                User userFriend = MessengerLiblary.GetUserPerId(item.id);
                PhotoTextItem text = new PhotoTextItem() { avatar = GetImageBrushOnBytes(MessengerLiblary.GetFile(userFriend.Avatar)), Brush = CheckOnlineFriend(userFriend.IsOnline), Text = userFriend.Name };
                FriendListBox.Items.Add(text);
            }

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
            this.Close();
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
        private async void MainPage_Click(object sender, MouseButtonEventArgs e)
        {
            receiveUser = null;
            user = MessengerLiblary.GetUserPerId(user.Id);
            SlowOpacity(new MainPage());
            GC.Collect();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            timerCheckOutCalls.Stop();
            timerUpdateFriends.Stop();
            timerUpdateMessages.Stop();
            timerWaitChangedChats.Stop();
            if (!SettingsPage.onExitedAccount)
            {
                if (callFromId > 0)
                {
                    MessengerLiblaryCalls.CloseAudioCall(user.Id, callFromId);
                }
                else
                {
                    MessengerLiblaryCalls.CloseAudioCall(user.Id, CallerInId);
                }
            }
        }
        #endregion

        #region Funcs
        public async void SlowOpacity(Page page)
        {
            await Task.Factory.StartNew(() =>
            {
                double opacity = 0;
                for (double i = 1.0; i > 0.0; i -= 0.1)
                {
                    opacity = i;
                    Thread.Sleep(15);
                    Dispatcher.Invoke(() =>
                    {
                        FramePage.Opacity = i;
                    });

                }
                Dispatcher.BeginInvoke(() =>
                {
                    FramePage.Content = page;
                });
                for (double i = 0.0; i < 1.1; i += 0.1)
                {
                    opacity = i;
                    Thread.Sleep(15);
                    Dispatcher.Invoke(() =>
                    {
                        FramePage.Opacity = i;
                    });
                }
            });
        }
        public async void UpdateFriendList()
        {
            FriendListBox.Items.Clear();
            friends.Clear();
            List<Friend> friendsTemp = MessengerLiblary.GetFriends(user.Id);
            if (friendsTemp != null)
            {
                foreach (var item in friendsTemp)
                {
                    User userFriend = MessengerLiblary.GetUserPerId(item.id);
                    if (userFriend.Id != user.Id)
                    {
                        friends.Add(new Friend(item.Name, user.Id, item.id));

                        PhotoTextItem text = new PhotoTextItem() { avatar = GetImageBrushOnBytes(MessengerLiblary.GetFile(userFriend.Avatar)), Brush = CheckOnlineFriend(userFriend.IsOnline), Text = userFriend.Name };
                        FriendListBox.Items.Add(text);
                    }
                    else
                    {
                        friends.Add(new Friend(item.Name, user.Id, item.id));

                        PhotoTextItem text = new PhotoTextItem() { avatar = GetImageBrushOnBytes(File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\Images\\saved.png")), Brush = Brushes.Transparent, Text = "Saved" };
                        FriendListBox.Items.Add(text);
                    }
                }
            }
        }
        public async void UpdateRequestFriendList()
        {
            RequestsListBox.Items.Clear();
            List<Friend> friendsTemp = MessengerLiblary.GetRequestedFriend(user.Id);
            if (friendsTemp != null)
            {
                foreach (var item in friendsTemp)
                {
                    User userFriend = MessengerLiblary.GetUserPerId(item.id);
                    PhotoTextItem text = new PhotoTextItem() { avatar = GetImageBrushOnBytes(MessengerLiblary.GetFile(userFriend.Avatar)), Brush = CheckOnlineFriend(userFriend.IsOnline), Text = userFriend.Name };
                    RequestsListBox.Items.Add(text);
                }


            }
        }
        public Brush CheckOnlineFriend(bool isOnline)
        {
            if (isOnline)
                return Brushes.GreenYellow;
            else
                return Brushes.OrangeRed;
        }
        public ImageBrush GetImageBrushOnBytes(byte[] byteArray)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(byteArray);
            bitmapImage.EndInit();

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = bitmapImage;
            return imageBrush;
        }
        #endregion

        #region Request friend buttons
        private void showRequests_Click(object sender, MouseButtonEventArgs e)
        {
            FriendListBox.Visibility = Visibility.Collapsed;
            RequestsListBox.Visibility = Visibility.Visible;
            UpdateRequestFriendList();
        }
        private void showFriends_Click(object sender, MouseButtonEventArgs e)
        {
            RequestsListBox.Visibility = Visibility.Collapsed;
            FriendListBox.Visibility = Visibility.Visible;
            UpdateFriendList();
        }
        private async void AcceptFriend_Click(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            MessengerLiblary.AddFriend(user.Id, MessengerLiblary.GetUserPerName(border.Tag.ToString()).Id);
            UpdateRequestFriendList();
        }
        private async void NoAcceptFriend_Click(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            MessengerLiblary.RemoveFromRequestedFriendTabe(user.Id, MessengerLiblary.GetUserPerName(border.Tag.ToString()).Id);
            UpdateRequestFriendList();
        }
        #endregion

    }
}
