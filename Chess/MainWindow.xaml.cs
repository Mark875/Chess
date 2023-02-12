using System;
using System.Collections.Generic;
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
using System.Net.Sockets;
using System.Net;
using Chess.Classes;
using Chess.Windows;
using System.Threading;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client client = new Client();
        private CheckBox cb_checked;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            client.Connect();
            if ((bool)cbInquisition.IsChecked)
            {
                client.IsInquisitionInGame = true;
            }
            string extra = "";
            if ((bool)cbTank.IsChecked)
            {
                extra = "tank";
            }
            else if ((bool)cbMissile.IsChecked)
            {
                extra = "missile";
            }
            else if ((bool)cbInquisition.IsChecked)
            {
                extra = "inquisition";
            }
            else if ((bool)cbTraitor.IsChecked)
            {
                extra = "traitor";
            }
            else if ((bool)cbTankMines.IsChecked)
            {
                extra = "tankmines";
            }
            client.StartPlaying(extra);
            SearchWindow searchWindow = new SearchWindow();
            searchWindow.Client = client;
            searchWindow.MainWindow = this;
            searchWindow.getClientResetEvent.Set();
            this.Visibility = Visibility.Collapsed;


            bool a = (bool)searchWindow.ShowDialog();
            if (!a)
            {
                client.Send("Cancel");
                return;
            }


            if (client.MyColor == Classes.Color.White)
            {
                GameWindow window = new GameWindow(client);
                this.Visibility = Visibility.Collapsed;
                window.ShowDialog();
                InitializeComponent();
                this.Visibility = Visibility.Visible;
            }
            else if (client.MyColor == Classes.Color.Black)
            {
                GameWindowBlack windowBlack = new GameWindowBlack(client);
                this.Visibility = Visibility.Collapsed;
                windowBlack.ShowDialog();
                InitializeComponent();
                this.Visibility = Visibility.Visible;
            }

        }

        private void ComboboxClicked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                if (cb_checked == null)
                {
                    cb_checked = cb;
                }
                else
                {
                    cb_checked.IsChecked = false;
                    cb_checked = cb;
                }
            }
            else
            {
                cb_checked = null;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            client.Send("Bye");
            Application.Current.Shutdown();
        }
    }
}
