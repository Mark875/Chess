using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Chess.Classes;
using System.Threading;

namespace Chess.Windows
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private int dots = 3;
        public Client Client { get; set; }
        public MainWindow MainWindow { get; set; }
        public ManualResetEvent getClientResetEvent = new ManualResetEvent(false);
        private bool alive = true;
        public SearchWindow()
        {
            InitializeComponent();


            Thread countThread = new Thread(() =>
            {
                getClientResetEvent.WaitOne();
                while (Client.MyColor == Chess.Classes.Color.NoColor && alive)
                {
                    Thread.Sleep(1000);
                    Tick();
                }
                if (alive)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        DialogResult = true;
                        Close();
                    }));
                }
            });
            countThread.Start();
        }

        private void Tick()
        {
            dots++;
            if (dots == 4)
            {
                dots = 1;
            }

            string d = "";
            for (int i = 0; i < dots; i++)
            {
                d += ".";
            }
            Dispatcher.BeginInvoke((Action<string>)(d => lblSearch.Content = "Подбор противника" + d), d);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Visibility = Visibility.Visible;
            alive = false;
            this.DialogResult = false;
            Close();
        }
    }
}
