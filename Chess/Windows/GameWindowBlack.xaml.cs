﻿using System;
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

namespace Chess
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindowBlack : Window
    {
        private string path = System.IO.Path.GetFullPath("Images") + "\\";
        private Client client;
        private List<Button> buttons;
        public GameWindowBlack(Client client)
        {
            this.client = client;
            client.WindowBlack = this;
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
            imgQueen.Source = new BitmapImage(new Uri(path + "queen_black.png"));
            imgRook.Source = new BitmapImage(new Uri(path + "rook_black.png"));
            imgBishop.Source = new BitmapImage(new Uri(path + "bishop_black.png"));
            imgKnight.Source = new BitmapImage(new Uri(path + "knight_black.png"));
            UpdateDesk();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            client.Pick(btn.DataContext.ToString());
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
                    //if (state == "chosen") btn.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(btn.Background == new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#fbe2c3")) ? "#cdd26a" : "#aaa23a"));
                    //if (state == "usual") btn.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(btn.Background == new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#cdd26a")) ? "#fbe2c3" : "#d27106"));
                    btn.Background = state == "chosen" ? new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(btn.Background == new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#fbe2c3")) ? "#cdd26a" : "#aaa23a")) : (state == "chosen" ? new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(btn.Background == new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#cdd26a")) ? "#fbe2c3" : "#d27106")) : btn.Background);

                    (btn.Content as Image).Source = state == "check" ? (client.Figures[btn.DataContext.ToString()] == "king_white" ? new BitmapImage(new Uri(path + "king_white_check.png")) : new BitmapImage(new Uri(path + "king_black_check.png"))) : (state == "nocheck" ? (client.Figures[btn.DataContext.ToString()] == "king_white" ? new BitmapImage(new Uri(path + "king_white.png")) : new BitmapImage(new Uri(path + "king_black.png"))) : (btn.Content as Image).Source);
                }
            }
        }

        public void UpdateDesk()
        {
            foreach (Button button in buttons)
            {
                (button.Content as Image).Source = client.Figures[button.DataContext.ToString()] == "" ? null : new BitmapImage(new Uri(path + client.Figures[button.DataContext.ToString()] + ".png"));
            }
        }

        public void LetChooseFigure()
        {
            pFigure.IsOpen = true;
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

        public void MbShow(string message)
        {
            MessageBox.Show(this, message);
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                client.Send("Draw?");
            }).Start();
        }

        private void btnGiveUp_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                client.Send("GiveUp");
                client.ResetGame();
            }).Start();
            
            if (MessageBox.Show(this, "Вы сдались", "Поражение", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
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
                    if (MessageBox.Show(this, $"{(color == Classes.Color.White ? "Белые" : "Черные")} поставили мат", (bool)victory ? "Победа" : "Поражение", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        Close();
                    }
                    break;
                default:
                    break;
            }
        }

        public bool AskForDraw()
        {
            return MessageBox.Show(this, "Противник предлагает ничью", "Ничья?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
