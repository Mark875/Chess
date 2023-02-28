using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace ChessServer.Classes
{
    class Game
    {
        private List<Player> players = new List<Player>();
        private GameState gameState;
        private bool gameOver = false;
        private string extra_figure = "";
        private int move;
        private List<string> letters = new List<string>() { "", "a", "b", "c", "d", "e", "f", "g", "h" };
        private List<string> cells = new List<string>()
        {
            "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8",
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8",
            "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8",
            "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8",
            "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8",
            "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8",
            "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8",
        };
        private Dictionary<string, FigureType> typeOfFigure = new Dictionary<string, FigureType>()
        {
            { "pawn", FigureType.Pawn },
            { "rook", FigureType.Rook },
            { "bishop", FigureType.Bishop },
            { "knight", FigureType.Knight },
            { "queen", FigureType.Queen },
            { "king", FigureType.King },
            { "tank", FigureType.Tank },
            { "missile", FigureType.Missile },
            { "inquisition", FigureType.Inquisition }
        };
        private Dictionary<string, Color> mines = new Dictionary<string, Color>();
        private List<string> allHitByBlackCells = new List<string>();
        private List<string> allHitByWhiteCells = new List<string>();
        private Dictionary<string, Figure> figures;
        private Player player_white;
        private Player player_black;
        //private Figure black_king;
        //private Figure white_king;
        private string inquisition_cell;
        private int inquisition_id;


        public GameState GameState { get { return gameState; } }
        public ManualResetEvent ReceiveMessageResetEvent { get; set; } = new ManualResetEvent(false);
        public List<Player> Players { get { return players; } }
        public string Extra_figure { get { return extra_figure; } }
        public string Message { get; set; } = "";
        public bool ContinuePlaying { get; set; } = true;

        public Game(List<Player> players, string extra_figures)
        {
            this.players = players;
            this.extra_figure = extra_figures;
            Start();
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
            Console.WriteLine($"Add player {players.Count}");
            if (players.Count == 2) gameState = GameState.MoveWhite;
        }

        public void Start()
        {
            Thread gameThread = new Thread(() =>
            {
                while (true)
                {
                    if (players.Count < 2)
                    {
                        gameState = GameState.Wait;
                        continue;
                    }

                    Classic();
                }
            });
            gameThread.Start();
        }

        private void Classic()
        {
            players[0].Color = Color.White;
            players[1].Color = Color.Black;
            player_white = players[0];
            player_black = players[1];

            Send(new List<Player>() { player_white }, "Color white");
            Send(new List<Player>() { player_black }, "Color black");

            for (int i = 0; i < 8; i++)
            {
                player_white.Figures.Add(new Figure(FigureType.Pawn, Color.White, i + 1, 2));
                player_black.Figures.Add(new Figure(FigureType.Pawn, Color.Black, i + 1, 7));
            }

            player_white.Figures.Add(new Figure(FigureType.Rook, Color.White, 1, 1));       // a1 - 8
            player_black.Figures.Add(new Figure(FigureType.Rook, Color.Black, 1, 8));       // a8 - 8
            player_white.Figures.Add(new Figure(FigureType.Rook, Color.White, 8, 1));       // h1 - 9
            player_black.Figures.Add(new Figure(FigureType.Rook, Color.Black, 8, 8));       // h8 - 9
            player_white.Figures.Add(new Figure(FigureType.Knight, Color.White, 2, 1));     // b1 - 10
            player_black.Figures.Add(new Figure(FigureType.Knight, Color.Black, 2, 8));     // b8 - 10
            player_white.Figures.Add(new Figure(FigureType.Knight, Color.White, 7, 1));     // g1 - 11
            player_black.Figures.Add(new Figure(FigureType.Knight, Color.Black, 7, 8));     // g8 - 11
            player_white.Figures.Add(new Figure(FigureType.Bishop, Color.White, 3, 1));     // c1 - 12
            player_black.Figures.Add(new Figure(FigureType.Bishop, Color.Black, 3, 8));     // c8 - 12
            player_white.Figures.Add(new Figure(FigureType.Bishop, Color.White, 6, 1));     // f1 - 13
            player_black.Figures.Add(new Figure(FigureType.Bishop, Color.Black, 6, 8));     // f8 - 13
            player_white.Figures.Add(new Figure(FigureType.Queen, Color.White, 4, 1));      // d1 - 14
            player_black.Figures.Add(new Figure(FigureType.Queen, Color.Black, 4, 8));      // d8 - 14
            player_white.Figures.Add(new Figure(FigureType.King, Color.White, 5, 1));       // e1 - 15
            player_black.Figures.Add(new Figure(FigureType.King, Color.Black, 5, 8));       // e8 - 15
            //white_king = player_white.Figures[player_white.Figures.Count - 1];
            //black_king = player_black.Figures[player_black.Figures.Count - 1];

            figures = new Dictionary<string, Figure>()
            {
                { "a1", player_white.Figures[8] }, { "b1", player_white.Figures[10] }, { "c1", player_white.Figures[12] }, { "d1", player_white.Figures[14] }, { "e1", player_white.Figures[15] }, { "f1", player_white.Figures[13] }, { "g1", player_white.Figures[11] }, { "h1", player_white.Figures[9] },
                { "a2", player_white.Figures[0] }, { "b2", player_white.Figures[1] },   { "c2", player_white.Figures[2] },   { "d2", player_white.Figures[3] },  { "e2", player_white.Figures[4] }, { "f2", player_white.Figures[5] },   { "g2", player_white.Figures[6] },   { "h2", player_white.Figures[7] },
                { "a3", null },           { "b3", null },             { "c3", null },             { "d3", null },            { "e3", null },           { "f3", null },             { "g3", null },             { "h3", null },
                { "a4", null },           { "b4", null },             { "c4", null },             { "d4", null },            { "e4", null },           { "f4", null },             { "g4", null },             { "h4", null },
                { "a5", null },           { "b5", null },             { "c5", null },             { "d5", null },            { "e5", null },           { "f5", null },             { "g5", null },             { "h5", null },
                { "a6", null },           { "b6", null },             { "c6", null },             { "d6", null },            { "e6", null },           { "f6", null },             { "g6", null },             { "h6", null },
                { "a7", player_black.Figures[8] }, { "b7", player_black.Figures[10] }, { "c7", player_black.Figures[12] }, { "d7", player_black.Figures[14] }, { "e7", player_black.Figures[15] }, { "f7", player_black.Figures[13] }, { "g7", player_black.Figures[11] }, { "h7", player_black.Figures[9] },
                { "a8", player_black.Figures[0] }, { "b8", player_black.Figures[1] },   { "c8", player_black.Figures[2] },   { "d8", player_black.Figures[3] },  { "e8", player_black.Figures[4] }, { "f8", player_black.Figures[5] },   { "g8", player_black.Figures[6] },   { "h8", player_black.Figures[7] },
            };

            UpdateAllHitCells();
            move = 1;
            if (extra_figure == "inquisition")
            {
                int fig1 = player_white.Figures.IndexOf(player_white.Figures.Find(x => x.Type == FigureType.Bishop));
                player_white.Figures[fig1].Type = FigureType.Inquisition;

                int fig2 = player_black.Figures.IndexOf(player_black.Figures.Find(x => x.Type == FigureType.Bishop));
                player_black.Figures[fig2].Type = FigureType.Inquisition;

                figures[player_white.Figures[fig1].Cell] = player_white.Figures[fig1];
                figures[player_black.Figures[fig2].Cell] = player_black.Figures[fig2];
                //Send(players, $"Figure inquisition {player_white.Figures[fig1].Cell} {player_black.Figures[fig2].Cell}");
            }
            if (extra_figure == "tankmines")
            {
                Send(player_white, "CHOOSE MINES");
                Send(player_black, "Wait mines");
                ReceiveMessageResetEvent.WaitOne();
                ReceiveMessageResetEvent.Reset();
                string[] words = Message.Split();
                mines.Add(words[1], Color.White);
                mines.Add(words[2], Color.White);
                Send(player_white, "Wait mines");
                Send(player_black, $"EnemyMines {words[1]} {words[2]}");
                Thread.Sleep(50);
                Send(player_black, $"CHOOSE MINES");
                ReceiveMessageResetEvent.WaitOne();
                ReceiveMessageResetEvent.Reset();
                words = Message.Split();
                mines.Add(words[1], Color.Black);
                mines.Add(words[2], Color.Black);
                Send(player_white, $"EnemyMines {words[1]} {words[2]}");
            }
            while (!gameOver && ContinuePlaying)
            {
                Move(player_white, player_black);
                Move(player_black, player_white);
                move++;
                Thread.Sleep(5);
                if (move == 5)
                {
                    if (extra_figure == "missile")
                    {
                        int fig1 = player_white.Figures.IndexOf(player_white.Figures.Find(x => x.Type == FigureType.Rook));
                        player_white.Figures[fig1].Type = FigureType.Missile;

                        int fig2 = player_black.Figures.IndexOf(player_black.Figures.Find(x => x.Type == FigureType.Rook));
                        player_black.Figures[fig2].Type = FigureType.Missile;

                        figures[player_white.Figures[fig1].Cell] = player_white.Figures[fig1];
                        figures[player_black.Figures[fig2].Cell] = player_black.Figures[fig2];
                        Send(players, $"Figure missile {player_white.Figures[fig1].Cell} {player_black.Figures[fig2].Cell}");
                    }
                    else if (extra_figure == "tank" || extra_figure == "tankmines")
                    {
                        int fig1 = player_white.Figures.IndexOf(player_white.Figures.Find(x => x.Type == FigureType.Rook));
                        player_white.Figures[fig1].Type = FigureType.Tank;

                        int fig2 = player_black.Figures.IndexOf(player_black.Figures.Find(x => x.Type == FigureType.Rook));
                        player_black.Figures[fig2].Type = FigureType.Tank;

                        figures[player_white.Figures[fig1].Cell] = player_white.Figures[fig1];
                        figures[player_black.Figures[fig2].Cell] = player_black.Figures[fig2];
                        Send(players, $"Figure tank {player_white.Figures[fig1].Cell} {player_black.Figures[fig2].Cell}");
                    }
                    else if (extra_figure == "traitor")
                    {
                        Random random = new Random();

                        int id = random.Next(15);

                        while (!player_white.Figures[id].IsActive || !player_black.Figures[id].IsActive || player_white.Figures[id].Type == FigureType.King)
                        {
                            id = random.Next(15);

                            FigureType type = player_white.Figures[id].Type;
                            int x_white = player_white.Figures[id].X;
                            int y_white = player_white.Figures[id].Y;
                            int x_black = player_black.Figures[id].X;
                            int y_black = player_black.Figures[id].Y;
                            player_black.Figures[id] = new Figure(type, Color.Black, x_white, y_white);
                            player_white.Figures[id] = new Figure(type, Color.White, x_black, y_black);

                            figures[player_black.Figures[id].Cell] = player_black.Figures[id];
                            figures[player_white.Figures[id].Cell] = player_white.Figures[id];

                            Send(players, $"Traitors {player_white.Figures[id].Cell} {player_black.Figures[id].Cell}");
                        }
                    }
                }
            }
        }

        public void PlayerGaveUp(Color color)
        {
            if (color == Color.White)
            {
                Send(player_black, "GaveUp");
            }
            else
            {
                Send(player_white, "GaveUp");
            }
            player_white.Figures = new List<Figure>();
            player_black.Figures = new List<Figure>();
            player_white.GameIndex = -1;
            player_black.GameIndex = -1;
            player_white = null;
            player_black = null;
            players = new List<Player>();
        }

        private void Send(List<Player> players, string message)
        {
            foreach (Player player in players) player.Socket.Send(Encoding.ASCII.GetBytes(message));
        }

        private void Send(Player player, string message)
        {
            player.Socket.Send(Encoding.ASCII.GetBytes(message));
        }

        private void Move(Player player, Player enemy)
        {
            Send(players, "Move " + ((player.Color == Color.White) ? "white" : "black"));
            Thread.Sleep(100);
            ReceiveMessageResetEvent.WaitOne();
            ReceiveMessageResetEvent.Reset();

            // Обработка хода

            string[] words = Message.Split();
            string f = words[1].Split('_')[0];
            FigureType type = typeOfFigure[f];
            string from = words[2];
            string to = words[3];
            int a = NumberOfColumn(from[0].ToString());
            int b = Convert.ToInt32(from[1].ToString());
            int a1 = NumberOfColumn(to[0].ToString());
            int b1 = Convert.ToInt32(to[1].ToString());
            bool beat = words[4] == "beat";
            string new_fig = words[5].Split('_')[0];
            bool goneToEnd = type == FigureType.Pawn ? (b1 == 8 || b1 == 1 ? true : false) : false;
            bool castle = type == FigureType.King && ((player.Color == Color.White && from == "e1" && (to == "g1" || to == "c1")) || (player.Color == Color.Black && from == "e8" && (to == "g8" || to == "c8")));
            int rook = 0;
            bool enPassant = type == FigureType.Pawn && words[6] == "enPassant";
            string enPassantPawnCell = "";
            string prev_inq_pos = "";

            Dictionary<string, Figure> old_desk = new Dictionary<string, Figure>();

            foreach (string cell in figures.Keys)
            {
                old_desk[cell] = figures[cell] == null ? null : new Figure(figures[cell].Type, figures[cell].Color, figures[cell].X, figures[cell].Y);
            }

            int fig = player.Figures.IndexOf(player.Figures.Find(x => x.Type == type && x.X == a && x.Y == b));
            if (type == FigureType.Tank && beat)
            {
                figures[to] = null;
            }
            else if (beat)
            {
                player.Figures[fig].Move(to);
                figures[from] = null;
                figures[to] = player.Figures[fig];
            }

            if (type == FigureType.Missile && !player.Figures[fig].IsCharged && to == from)
            {
                player.Figures[fig].IsCharged = true;
            }

            if (type == FigureType.Missile && !beat && player.Figures[fig].IsCharged && to != from)
            {
                player.Figures[fig].IsCharged = false;
            }

            if (castle)
            {
                if (player.Color == Color.White)
                {
                    if (to == "g1")
                    {
                        rook = player.Figures.IndexOf(player.Figures.Find(x => x.Type == FigureType.Rook && x.Cell == "h1"));
                        player.Figures[rook].Move("f1");
                    }
                    else
                    {
                        rook = player.Figures.IndexOf(player.Figures.Find(x => x.Type == FigureType.Rook && x.Cell == "a1"));
                        player.Figures[rook].Move("d1");
                    }
                }
            }
            if (beat)
            {
                enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == to))].IsActive = false;
            }
            bool holyInquisition = extra_figure == "inquisition" && beat && ContainsInquisition(enemy.Color, to);
            if (holyInquisition)
            {
                player.Figures[fig].IsActive = false;
                figures[to] = enemy.Figures[inquisition_id];
                prev_inq_pos = enemy.Figures[inquisition_id].Cell;
                enemy.Figures[inquisition_id].Move(to);
                figures[inquisition_cell] = null;
            }
            if (enPassant)
            {
                enPassantPawnCell = ColumnOfNumber(a1) + (b1 + (player.Color == Color.White ? -1 : 1)).ToString();
                figures[enPassantPawnCell] = null;
                enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == enPassantPawnCell))].IsActive = false;
            }
            if (goneToEnd)
            {
                Console.WriteLine(new_fig);
                switch (new_fig)
                {
                    case "queen":
                        player.Figures[fig].Type = FigureType.Queen;
                        break;
                    case "rook":
                        player.Figures[fig].Type = FigureType.Rook;
                        break;
                    case "knight":
                        player.Figures[fig].Type = FigureType.Knight;
                        Console.WriteLine("knight");
                        break;
                    case "bishop":
                        player.Figures[fig].Type = FigureType.Bishop;
                        break;
                }
            }

            List<int> player_ids = new List<int>();
            List<int> enemy_ids = new List<int>();
            int min_row = b < b1 ? b : b1;
            int max_row = min_row == b ? b1 : b;
            if (f == "missile")
            {
                for (int i = min_row; i <= max_row; i++)
                {
                    string c = from[0].ToString() + i.ToString();
                    if (figures[c] != null && figures[c].Type != FigureType.King)
                    {
                        if (figures[c].Color == player.Color)
                        {
                            int figure = player.Figures.IndexOf(player.Figures.Find(x => x.Type == figures[c].Type && x.Cell == figures[c].Cell));
                            player.Figures[figure].IsActive = false;
                            player_ids.Add(figure);
                        }
                        else
                        {
                            int figure = enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Type == figures[c].Type && x.Cell == figures[c].Cell));
                            enemy.Figures[figure].IsActive = false;
                            enemy_ids.Add(figure);
                        }
                    }
                }
            }


            while (Check(player))
            {
                figures = new Dictionary<string, Figure>();
                foreach (string cell in old_desk.Keys)
                {
                    figures[cell] = old_desk[cell] == null ? null : new Figure(old_desk[cell].Type, old_desk[cell].Color, old_desk[cell].X, old_desk[cell].Y);
                }
                fig = player.Figures.IndexOf(player.Figures.Find(x => x.Type == type && x.X == a1 && x.Y == b1));
                player.Figures[fig].Move(from);
                if (beat)
                {
                    enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == to))].IsActive = true;
                }
                if (holyInquisition)
                {
                    player.Figures[fig].IsActive = true;
                    enemy.Figures[inquisition_id].Move(prev_inq_pos);
                }
                if (goneToEnd)
                {
                    player.Figures[fig].Type = FigureType.Pawn;
                }
                if (castle)
                {
                    if (player.Color == Color.White)
                    {
                        player.Figures[rook].Move(player.Figures[rook].Cell == "d1" ? "a1" : "h1");
                    }
                    else
                    {
                        player.Figures[rook].Move(player.Figures[rook].Cell == "d8" ? "a8" : "h8");
                    }
                }
                foreach (int id in player_ids)
                {
                    player.Figures[id].IsActive = true;
                }
                foreach (int id in enemy_ids)
                {
                    enemy.Figures[id].IsActive = true;
                }

                Send(new List<Player>() { player }, $"Move again");
                Console.WriteLine("Move again");
                Thread.Sleep(100);
                ReceiveMessageResetEvent.WaitOne();
                ReceiveMessageResetEvent.Reset();

                words = Message.Split();
                f = words[1].Split('_')[0];
                type = typeOfFigure[f];
                from = words[2];
                to = words[3];
                a = NumberOfColumn(from[0].ToString());
                b = Convert.ToInt32(from[1].ToString());
                a1 = NumberOfColumn(to[0].ToString());
                b1 = Convert.ToInt32(to[1].ToString());
                beat = words[4] == "beat";
                goneToEnd = type == FigureType.Pawn && (b1 == 8 || b1 == 0);
                new_fig = words[5].Split('_')[0];
                castle = type == FigureType.King && ((player.Color == Color.White && from == "e1" && (to == "g1" || to == "c1")) || (player.Color == Color.Black && from == "e8" && (to == "g8" || to == "c8")));
                rook = 0;
                enPassant = type == FigureType.Pawn && words[6] == "enPassant";
                enPassantPawnCell = "";
                holyInquisition = extra_figure == "inquisition" && beat && ContainsInquisition(enemy.Color, to);
                prev_inq_pos = "";

                old_desk = new Dictionary<string, Figure>();

                foreach (string cell in figures.Keys)
                {
                    old_desk[cell] = figures[cell] == null ? null : new Figure(figures[cell].Type, figures[cell].Color, figures[cell].X, figures[cell].Y);
                }

                fig = player.Figures.IndexOf(player.Figures.Find(x => x.Type == type && x.X == a && x.Y == b));
                if (type == FigureType.Tank)
                {
                    figures[to] = null;
                }
                else
                {
                    player.Figures[fig].Move(to);
                    figures[from] = null;
                    figures[to] = player.Figures[fig];
                }


                if (castle)
                {
                    if (player.Color == Color.White)
                    {
                        if (to == "g1")
                        {
                            rook = player.Figures.IndexOf(player.Figures.Find(x => x.Type == FigureType.Rook && x.Cell == "h1"));
                            player.Figures[rook].Move("f1");
                        }
                        else
                        {
                            rook = player.Figures.IndexOf(player.Figures.Find(x => x.Type == FigureType.Rook && x.Cell == "a1"));
                            player.Figures[rook].Move("d1");
                        }
                    }
                }
                if (beat)
                {
                    enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == to))].IsActive = false;
                }
                if (holyInquisition)
                {
                    player.Figures[fig].IsActive = false;
                    figures[to] = enemy.Figures[inquisition_id];
                    prev_inq_pos = enemy.Figures[inquisition_id].Cell;
                    enemy.Figures[inquisition_id].Move(to);
                    figures[inquisition_cell] = null;
                }
                if (enPassant)
                {
                    enPassantPawnCell = ColumnOfNumber(a1) + (b1 + (player.Color == Color.White ? -1 : 1)).ToString();
                    figures[enPassantPawnCell] = null;
                    enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == enPassantPawnCell))].IsActive = false;
                }
                if (goneToEnd)
                {
                    Console.WriteLine(new_fig);
                    switch (new_fig)
                    {
                        case "queen":
                            player.Figures[fig].Type = FigureType.Queen;
                            break;
                        case "rook":
                            player.Figures[fig].Type = FigureType.Rook;
                            break;
                        case "knight":
                            player.Figures[fig].Type = FigureType.Knight;
                            Console.WriteLine("knight");
                            break;
                        case "bishop":
                            player.Figures[fig].Type = FigureType.Bishop;
                            break;
                    }
                }
                player_ids = new List<int>();
                enemy_ids = new List<int>();
                if (f == "missile")
                {
                    min_row = b < b1 ? b : b1;
                    max_row = min_row == b ? b1 : b;
                    for (int i = min_row; i <= max_row; i++)
                    {
                        string c = from[0].ToString() + i.ToString();
                        if (figures[c] != null)
                        {
                            if (figures[c].Color == player.Color)
                            {
                                int figure = player.Figures.IndexOf(player.Figures.Find(x => x.Type == figures[c].Type && x.Cell == figures[c].Cell));
                                player.Figures[figure].IsActive = false;
                                player_ids.Add(figure);
                            }
                            else
                            {
                                int figure = enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Type == figures[c].Type && x.Cell == figures[c].Cell));
                                enemy.Figures[figure].IsActive = false;
                                enemy_ids.Add(figure);
                            }
                        }
                    }
                }
            }


            string message_bomb = "";
            if (mines.Keys.Contains(to) && mines[to] != player.Figures[fig].Color)
            {
                message_bomb = $"Bomb {to}";
                figures[to] = null;
                player.Figures[fig].IsActive = false;
            }

            bool check = Check(player.Color == Color.White ? player_black : player_white);

            Send(player, $"Move ok" + (check ? " check" : " nocheck"));
            Console.WriteLine($"Move ok" + (check ? " check" : " nocheck"));
            Send(enemy, "Moved " + words[1] + " " + from + " " + to + (check ? " check" : " nocheck") + " " + words[5] + (enPassant ? " enPassant " + enPassantPawnCell : " noPassant 0"));
            Thread.Sleep(100);
            if (message_bomb != "")
            {
                Send(players, message_bomb);
                Thread.Sleep(50);
            }
            if (check)
            {
                if (CheckmateOrStalemate(enemy, player))
                {
                    gameOver = true;
                    Send(players, $"CHECKMATE {player.Color}");
                    Console.WriteLine($"CHECKMATE {player.Color}");
                }
            }
            else
            {
                if (CheckmateOrStalemate(enemy, player))
                {
                    gameOver = true;
                    Send(players, $"STALEMATE {enemy.Color}");
                    Console.WriteLine($"CHECKMATE {player.Color}");
                }
            }
        }

        #region Мат/Пат
        private bool CheckmateOrStalemate(Player player, Player enemy)
        {
            foreach (Figure f in player.Figures)
            {
                if (!f.IsActive) continue;

                List<string> moves = new List<string>();

                foreach (var s in f.HitCells)
                {
                    moves.Add(s);
                }
                foreach (string to in moves)
                {
                    if (figures[to] != null && figures[to].Color == f.Color) continue;

                    bool beat = figures[to] != null;
                    string from = f.Cell;

                    Dictionary<string, Figure> old_desk = new Dictionary<string, Figure>();

                    foreach (string c in figures.Keys)
                    {
                        old_desk[c] = figures[c] == null ? null : new Figure(figures[c].Type, figures[c].Color, figures[c].X, figures[c].Y);
                    }

                    f.Move(to);
                    figures[to] = f;
                    figures[from] = null;
                    if (beat)
                    {
                        enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == to))].IsActive = false;
                    }

                    if (Check(player))
                    {
                        figures = new Dictionary<string, Figure>();
                        foreach (string c in old_desk.Keys)
                        {
                            figures[c] = old_desk[c] == null ? null : new Figure(old_desk[c].Type, old_desk[c].Color, old_desk[c].X, old_desk[c].Y);
                        }
                        f.Move(from);
                        if (beat)
                        {
                            enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == to))].IsActive = true;
                        }
                        UpdateAllHitCells();
                    }
                    else
                    {
                        figures = new Dictionary<string, Figure>();
                        foreach (string c in old_desk.Keys)
                        {
                            figures[c] = old_desk[c] == null ? null : new Figure(old_desk[c].Type, old_desk[c].Color, old_desk[c].X, old_desk[c].Y);
                        }
                        f.Move(from);
                        if (beat)
                        {
                            enemy.Figures[enemy.Figures.IndexOf(enemy.Figures.Find(x => x.Cell == to))].IsActive = true;
                        }
                        UpdateAllHitCells();
                        Console.WriteLine($"{f.Type} {from} {to}");
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region Шах
        private bool Check(Player loser)
        {
            UpdateAllHitCells();

            if (loser.Color == Color.Black)
            {
                return allHitByWhiteCells.Contains(loser.Figures[15].Cell);
            }
            return allHitByBlackCells.Contains(loser.Figures[15].Cell);
        }
        #endregion

        private void UpdateAllHitCells()
        {
            allHitByBlackCells.Clear();
            allHitByWhiteCells.Clear();
            foreach (Figure figure in player_white.Figures)
            {
                if (!figure.IsActive) continue;
                SetMoves(figure);
            }
            foreach (Figure figure in player_black.Figures)
            {
                if (!figure.IsActive) continue;
                SetMoves(figure);
            }
        }

        private void SetMoves(Figure figure)
        {
            figure.HitCells.Clear();
            switch (figure.Type)
            {
                case FigureType.Pawn:
                    for (int i = 0; i < cells.Count; i++)
                    {
                        if (CheckPawn(figure.Cell, cells[i]))
                        {
                            figure.HitCells.Add(cells[i]);
                        }
                    }
                    break;
                case FigureType.Rook:
                    SetRookHitCells(figure, figure.Cell);
                    break;
                case FigureType.Knight:
                    SetKnightHitCells(figure, figure.Cell);
                    break;
                case FigureType.Bishop:
                    SetBishopHitCells(figure, figure.Cell);
                    break;
                case FigureType.Queen:
                    SetRookHitCells(figure, figure.Cell);
                    SetBishopHitCells(figure, figure.Cell);
                    break;
                case FigureType.King:
                    SetKingHitCells(figure, figure.Cell);
                    break;
                case FigureType.Missile:
                    if (!figure.IsCharged)
                    {
                        SetKingHitCells(figure, figure.Cell);
                    }
                    for (int i = 0; i < cells.Count; i++)
                    {
                        if (CheckMissile(figure.Cell, cells[i]))
                        {
                            figure.HitCells.Add(cells[i]);
                        }
                    }
                    break;
                case FigureType.Tank:
                    break;
            }

            if (figure.Color == Color.White)
            {
                foreach (string cell in figure.HitCells)
                {
                    if (!allHitByWhiteCells.Contains(cell))
                    {
                        allHitByWhiteCells.Add(cell);
                    }
                }
            }
            else
            {
                foreach (string cell in figure.HitCells)
                {
                    if (!allHitByWhiteCells.Contains(cell))
                    {
                        allHitByBlackCells.Add(cell);
                    }
                }
            }
        }

        #region Методы проверки ходов
        private bool CheckPawn(string from, string to)
        {
            Color myColor = figures[from].Color;
            string from_col = from[0].ToString();
            int from_row = Convert.ToInt32(from[1].ToString());
            string to_col = to[0].ToString();
            int to_row = Convert.ToInt32(to[1].ToString());
            if ((NumberOfColumn(from_col) == NumberOfColumn(to_col) - 1 || NumberOfColumn(from_col.ToString()) == NumberOfColumn(to_col.ToString()) + 1) && ((from_row + 1 == to_row && myColor == Color.White) || (from_row - 1 == to_row && myColor == Color.Black)) && figures[to] != null && figures[to].IsActive)
            {
                return true;
            }
            if (figures[to] != null)
            {
                return false;
            }
            if (from_col == to_col && ((from_row == to_row - 1 && myColor == Color.White) || (from_row == to_row + 1 && myColor == Color.Black)))
            {
                return true;
            }
            if ((from_col == to_col && from_row == 2 && to_row == 4 && myColor == Color.White) || (from_col == to_col && from_row == 7 && to_row == 5 && myColor == Color.Black))
            {
                return true;
            }
            return false;
        }

        private bool CheckMissile(string from, string to)
        {
            int from_col = NumberOfColumn(from[0].ToString());
            int to_col = NumberOfColumn(to[0].ToString());

            return to_col == from_col;
        }
        #endregion

        private void SetKnightHitCells(Figure figure, string cell)
        {
            int from_col = NumberOfColumn(cell[0].ToString());
            int from_row = Convert.ToInt32(cell[1].ToString());

            if (from_row < 8)
            {
                if (from_col < 7)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col + 2) + (from_row + 1).ToString());
                }
                if (from_col > 2)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col - 2) + (from_row + 1).ToString());
                }
            }
            if (from_row < 7)
            {
                if (from_col < 8)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col + 1) + (from_row + 2).ToString());
                }
                if (from_col > 1)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col - 1) + (from_row + 2).ToString());
                }
            }
            if (from_row > 1)
            {
                if (from_col < 7)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col + 2) + (from_row - 1).ToString());
                }
                if (from_col > 2)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col - 2) + (from_row - 1).ToString());
                }
            }
            if (from_row > 2)
            {
                if (from_col < 8)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col + 1) + (from_row - 2).ToString());
                }
                if (from_col > 1)
                {
                    figure.HitCells.Add(ColumnOfNumber(from_col - 1) + (from_row - 2).ToString());
                }
            }

            if (figure.Color == Color.White)
            {
                foreach (string c in figure.HitCells)
                {
                    if (!allHitByWhiteCells.Contains(c))
                    {
                        allHitByWhiteCells.Add(c);
                    }
                }
            }
            else
            {
                foreach (string c in figure.HitCells)
                {
                    if (!allHitByBlackCells.Contains(c))
                    {
                        allHitByBlackCells.Add(c);
                    }
                }
            }
        }

        private void SetBishopHitCells(Figure figure, string cell)
        {
            int row = Convert.ToInt32(cell[1].ToString());
            int col = NumberOfColumn(cell[0].ToString());
            string cell1 = "";

            for (int i = 1; i <= 7; i++)
            {
                if (col + i > 8 || row + i > 8)
                {
                    break;
                }
                cell1 = ColumnOfNumber(col + i) + (row + i).ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }

            for (int i = 1; i <= 7; i++)
            {
                if (col + i > 8 || row - i < 1)
                {
                    break;
                }
                cell1 = ColumnOfNumber(col + i) + (row - i).ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }

            for (int i = 1; i <= 7; i++)
            {
                if (col - i < 1 || row + i > 8)
                {
                    break;
                }
                cell1 = ColumnOfNumber(col - i) + (row + i).ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }

            for (int i = 1; i <= 7; i++)
            {
                if (col - i < 1 || row - i < 1)
                {
                    break;
                }
                cell1 = ColumnOfNumber(col - i) + (row - i).ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }
        }

        private void SetRookHitCells(Figure figure, string cell)
        {
            int row = Convert.ToInt32(cell[1].ToString());
            int col = NumberOfColumn(cell[0].ToString());
            string cell1 = "";
            for (int i = row + 1; i <= 8; i++)
            {
                cell1 = ColumnOfNumber(col) + i.ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }

            for (int i = row - 1; i >= 1; i--)
            {
                cell1 = ColumnOfNumber(col) + i.ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }

            for (int i = col - 1; i >= 1; i--)
            {
                cell1 = ColumnOfNumber(i) + row.ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }

            for (int i = col + 1; i <= 8; i--)
            {
                cell1 = ColumnOfNumber(i) + row.ToString();
                if (figures[cell1] == null)
                {
                    figure.HitCells.Add(cell1);
                }
                else
                {
                    figure.HitCells.Add(cell1);
                    break;
                }
            }
        }

        private void SetKingHitCells(Figure figure, string cell)
        {
            int row = Convert.ToInt32(cell[1].ToString());
            int col = NumberOfColumn(cell[0].ToString());

            if (col != 8)
            {
                if (row != 8)
                {
                    figure.HitCells.Add(ColumnOfNumber(col + 1) + (row + 1).ToString());
                }
                if (row != 1)
                {
                    figure.HitCells.Add(ColumnOfNumber(col + 1) + (row - 1).ToString());
                }
                figure.HitCells.Add(ColumnOfNumber(col + 1) + row.ToString());
            }
            if (col != 1)
            {
                if (row != 8)
                {
                    figure.HitCells.Add(ColumnOfNumber(col - 1) + (row + 1).ToString());
                }
                if (row != 1)
                {
                    figure.HitCells.Add(ColumnOfNumber(col - 1) + (row - 1).ToString());
                }
                figure.HitCells.Add(ColumnOfNumber(col - 1) + row.ToString());
            }
        }

        private void SetPawnHitCells(Figure figure, string cell)
        {

        }

        private bool ContainsInquisition(Color color, string cell)
        {
            inquisition_cell = "";
            inquisition_id = -1;

            foreach (string c in figures.Keys)
            {
                if (figures[c] != null && figures[c].Type == FigureType.Inquisition && figures[c].Color == color)
                {
                    inquisition_cell = c;
                    break;
                }
            }

            int to_row = Convert.ToInt32(cell[1].ToString());
            int to_col = NumberOfColumn(cell[0].ToString());
            int inq_row = Convert.ToInt32(inquisition_cell[1].ToString());
            int inq_col = NumberOfColumn(inquisition_cell[0].ToString());

            if (to_row - inq_row < 3 && to_row - inq_row > -3 &&
                to_col - inq_col < 3 && to_col - inq_col > -3)
            {
                Player p = color == Color.White ? player_white : player_black;
                inquisition_id = p.Figures.IndexOf(p.Figures.Find(x => x.Type == FigureType.Inquisition));
                return true;
            }

            return false;
        }

        private int NumberOfColumn(string column)
        {
            return letters.IndexOf(column);
        }

        private string ColumnOfNumber(int column)
        {
            return letters[column];
        }
    }
}
