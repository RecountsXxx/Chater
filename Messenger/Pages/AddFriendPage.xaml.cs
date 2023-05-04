using MessengerLiblary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Messenger.Pages
{
    public partial class AddFriendPage : Page
    {
        private int userId;
        public MessengerLiblary.MessengerLiblary MessengerLiblary = new MessengerLiblary.MessengerLiblary();

        public AddFriendPage(int userId, MessengerLiblary.MessengerLiblary messengerLiblary)
        {
            this.userId = userId;
            InitializeComponent();
            MessengerLiblary = messengerLiblary;
        }

        #region Buttons add and cancel
        private async void AddFriend_Click(object sender, MouseButtonEventArgs e)
        {
            if (!FriendNameTextBox.Text.Contains("\'"))
            {
                User friendId = MessengerLiblary.GetUserPerName(FriendNameTextBox.Text);
                if (friendId != null && friendId.Id != userId)
                {
                    if (!MessengerLiblary.CheckBlockedUser(userId, friendId.Id) && !MessengerLiblary.CheckBlockedUser(friendId.Id, userId))
                    {
                        string resposne = MessengerLiblary.AddToRequestFriendsTable(userId, friendId.Id);
                        if (resposne == "True")
                            MessageBox.Show(Application.Current.FindResource("m_youSendRequest")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);
                        else
                            MessageBox.Show(Application.Current.FindResource("m_youAlreadySended")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Question);
                    }
                    else
                        MessageBox.Show(Application.Current.FindResource("m_userBlocked")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                    MessageBox.Show(Application.Current.FindResource("m_usernameIsNotFound")?.ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
                MessageBox.Show(Application.Current.FindResource("m_dontEnterSymbol")?.ToString(), "Erorr", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}
