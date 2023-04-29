using MessengerLiblary;
using System;
using System.Collections.Generic;
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

namespace Messenger.Pages
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }
        public MainPage(int id)
        {
            InitializeComponent();
            MainWindow.MessengerLiblaryCalls.ConnectCallsServer("127.0.0.1", 8001, id);
            MainWindow.timerCheckOutCalls.Start();      
        }
    }
}
