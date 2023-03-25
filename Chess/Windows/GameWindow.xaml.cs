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
using System.Threading;
using Chess.Classes;
using Chess.Windows;

namespace Chess
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private string path = System.IO.Path.GetFullPath("Images") + "\\";
        private Client client;
        private List<Button> buttons;

        public bool IsPopUpOpen { get { return pFigure.IsOpen; } set { pFigure.IsOpen = value; } }
        public GameWindow(Client client)
        {
            this.client = client;
            client.Window = this;
            InitializeComponent();
            buttons = new List<Button>()
            {
                btnA1, btnA2, btnA3, btnA4, btnA5, btnA6, btnA7, btnA8,
                btnB1, btnB2, btnB3, btnB4, btnB5, btnB6, btnB7, btnB8,
                btnC1, btnC2, btnC3, btnC4, btnC5, btnC6, btnC7, btnC8,
                btnD1, btnD2, btnD3, btnD4, btnD5, btnD6, btnD7, btnD8,
                btnE1, btnE2, btnE3, btnE4, btnE5, btnE6, btnE7, btnE8,
                btnF1, btnF2, btnF3, btnF4, btnF5, btnF6, btnF7, btnF8,
                btnG1, btnG2, btnG3, btnG4, btnG5, btnG6, btnG7, btnG8,
                btnH1, btnH2, btnH3, btnH4, btnH5, btnH6, btnH7, btnH8
            };
            imgQueen.Source = new BitmapImage(new Uri(path + "queen_white.png"));
            imgRook.Source = new BitmapImage(new Uri(path + "rook_white.png"));
            imgBishop.Source = new BitmapImage(new Uri(path + "bishop_white.png"));
            imgKnight.Source = new BitmapImage(new Uri(path + "knight_white.png"));
            UpdateDesk();
            btnA8.BorderBrush = new SolidColorBrush(Colors.Yellow);
            WriteMode(client.Extra_figure);
            client.LoadWindowResetEvent.Set();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Dispatcher.BeginInvoke((Action)(() => client.Pick(btn.DataContext.ToString())));
        }
        private void FigureButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            pFigure.IsOpen = false;
            Dispatcher.BeginInvoke((Action)(() => client.ChooseFigure(btn.DataContext.ToString())));
        }

        public void SetCellState(string cell, string state)
        {
            foreach (Button btn in buttons)
            {
                if (btn.DataContext.ToString() == cell)
                {
                    btn.Background = state == "chosen" ? new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(btn.Background == new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#fbe2c3")) ? "#cdd26a" : "#aaa23a")) : (state == "chosen" ? new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(btn.Background == new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#cdd26a")) ? "#fbe2c3" : "#d27106")) : btn.Background);

                    (btn.Content as Image).Source = state == "check" ? (client.Figures[btn.DataContext.ToString()] == "king_white" ? new BitmapImage(new Uri(path + "king_white_check.png")) : new BitmapImage(new Uri(path + "king_black_check.png"))) : (state == "nocheck" ? (client.Figures[btn.DataContext.ToString()] == "king_white" ? new BitmapImage(new Uri(path + "king_white.png")) : new BitmapImage(new Uri(path + "king_black.png"))) : (btn.Content as Image).Source);

                    /*
                    if (state == "check")
                    {
                        if (client.Figures[cell] == "king_black")
                        {
                            (btn.Content as Image).Source = new BitmapImage(new Uri(path + "king_white_check.png"));
                            (btnA1.Content as Image).Source = new BitmapImage(new Uri(path + "king_white_check.png"));
                        }
                        if (client.Figures[cell] == "king_white")
                        {
                            (btn.Content as Image).Source = new BitmapImage(new Uri(path + "king_white_check.png"));
                        }
                    }
                    if (state == "nocheck")
                    {
                        if (client.Figures[cell] == "king_black")
                        {
                            (btn.Content as Image).Source = new BitmapImage(new Uri(path + "king_black.png"));
                        }
                        if (client.Figures[cell] == "king_white")
                        {
                            (btn.Content as Image).Source = new BitmapImage(new Uri(path + "king_white.png"));
                        }
                    }*/
                }
            }
        }

        public void LetChooseFigure()
        {
            pFigure.IsOpen = true;
        }

        public void UpdateDesk()
        {
            foreach (Button button in buttons)
            {
                //if ((button.Content as Image).Source == new BitmapImage(new Uri(path + "king_white_check.png")) || (button.Content as Image).Source == new BitmapImage(new Uri(path + "king_black_check.png")))
                (button.Content as Image).Source = client.Figures[button.DataContext.ToString()] == "" ? null : new BitmapImage(new Uri(path + client.Figures[button.DataContext.ToString()] + ".png"));
            }
        }

        public void AddMove(string move)
        {
            tbMoves.Text += move;
            if (move.Contains("."))
            {
                tbCurrentMove.Text = "Ход черных";
            }
            else
            {
                tbCurrentMove.Text = "Ход белых";
            }
        }

        private void WriteMode(string extra)
        {
            switch (extra)
            {
                case "tank":
                    lblMode.Content = "Танк";
                    movesTillNewFigure.Visibility = Visibility.Visible;
                    break;
                case "missile":
                    lblMode.Content = "Ракета";
                    movesTillNewFigure.Visibility = Visibility.Visible;
                    break;
                case "inquisition":
                    lblMode.Content = "Полиция";
                    break;
                case "tankmines":
                    lblMode.Content = "Танк + Мины";
                    minesCellsSection.Visibility = Visibility.Visible;
                    movesTillNewFigure.Visibility = Visibility.Visible;
                    break;
                case "":
                    lblMode.Content = "Классический";
                    break;
            }
        }

        public void MbShow(string message)
        {
            MessageBox.Show(this, message, "Изменение фигуры", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                client.Send("Draw?");
            }).Start();
        }
        public bool AskForDraw()
        {
            MessageBoxResult a = MessageBox.Show(this, "Противник предлагает ничью", "Ничья?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return a == MessageBoxResult.Yes;
        }

        private void btnGiveUp_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                client.Send("GiveUp");
                client.ResetGame();
            }).Start();
            if (MessageBox.Show(this, "Вы сдались", "Победа", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
            {
                Close();
            }
        }

        public void EndGame(string result, Classes.Color color = Classes.Color.White, bool victory = true)
        {
            switch (result)
            {
                case "GaveUp":
                    if (MessageBox.Show(this, "Противник сдался", "Победа", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        Close();
                    }
                    break;
                case "Draw":
                    if (MessageBox.Show(this, "Все согласились на ничью", "Ничья", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        Close();
                    }
                    break;
                case "Checkmate":
                    if (MessageBox.Show(this, $"{(color == Classes.Color.White ? "Белые" : "Черные")} поставили мат", victory ? "Победа" : "Поражение", MessageBoxButton.OK, 
                        victory ? MessageBoxImage.Information : MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        Close();
                    }
                    break;
                case "Stalemate":
                    if (MessageBox.Show(this, "Пат", "Ничья", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        Close();
                    }
                    break;
                default:
                    break;
            }
        }

        private void btnRules_Click(object sender, RoutedEventArgs e)
        {
            new RulesWindow().ShowDialog();
        }

        private void btn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!client.CanChooseMines) return;
            if (client.Mines.Count < 2)
            {
                string cell = (sender as Button).DataContext.ToString();
                if (Convert.ToInt32(cell[1].ToString()) < 6 && !client.Mines.Contains(cell))
                {
                    client.Mines.Add(cell);
                    if (client.Mines.Count == 1)
                    {
                        lblMine1.Content = $"1. {cell}";
                    }
                    else
                    {
                        lblMine2.Content = $"2. {cell}";
                    }
                    client.AddMineResetEvent.Set();
                }
            }
        }

        public void CanMove()
        {
            MessageBox.Show(this, "Противник расставил мины. Ваш ход.");
        }
        public void WaitMines()
        {
            MessageBox.Show(this, "Противник расставляет мины...");
        }

        public void ChooseMines()
        {
            MessageBox.Show(this, "Расставьте мины. Для того, чтобы поставить мину, нажмите правой кнопкой мыши на нужную клетку.");
        }

        public void RemoveMine(string cell)
        {
            if (lblMine1.Content.ToString() == $"1. {cell}")
            {
                if (lblMine2.Content.ToString() != "2.")
                {
                    string cell2 = lblMine2.Content.ToString().Split()[1];
                    lblMine1.Content = $"1. {cell2}";
                    lblMine2.Content = "2.";
                }
                else
                {
                    lblMine1.Content = "1.";
                }
            }
            else
            {
                lblMine2.Content = "2.";
            }
            UpdateDesk();
        }
    }
}
