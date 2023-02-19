using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Classes
{
    public enum Color { White, Black, NoColor }
    public class Client
    {
        private Socket socket;
        private string ip = "127.0.0.1";
        private int port = 8081;
        private bool isMyMove = false;
        private GameWindow window;
        private GameWindowBlack windowBlack;
        private Color myColor = Color.NoColor;
        private string chosenCell = "";
        private bool hasKingMoved = false;
        private bool hasARookMoved = false;
        private bool hasHRookMoved = false;
        private bool castle = false;
        private bool check = false;
        private bool transform = false;
        private bool enemy_check = false;
        private string cell1 = "";
        private int move = 0;
        private string chosenFigure = "";
        private bool canMoveEnPassant = false;
        private bool moveEnPassant = false;
        private string enPassantColumn = "0";
        private bool isMissileCharged = false;
        private string inquisition_cell = "";
        private List<string> letters = new List<string>() { "", "a", "b", "c", "d", "e", "f", "g", "h" };
        // (f == "knight" ? "К" : (f == "bishop" ? "С" : (f == "queen" ? "Ф" : (f == "rook" ? "Л" : (f == "king" ? "Кр" : (f == "missile" ? "Р" : (f == "tank" ? "Т" : (f == "inquisition" ? "П" : ""))))))))
        private ManualResetEvent moveResetEvent = new ManualResetEvent(false);
        private Dictionary<string, string> lettersToWrite = new Dictionary<string, string>()
        {
            { "knight", "К" },
            { "bishop", "С" },
            { "queen", "Ф" },
            { "rook", "Л" },
            { "king", "Кр" },
            { "missile", "Р" },
            { "tank", "Т" },
            { "inquisition", "П" },
            { "pawn", "" }
        };
        private Dictionary<string, string> figures = new Dictionary<string, string>()
        {
            { "a1", "rook_white" }, { "b1", "knight_white" }, { "c1", "bishop_white" }, { "d1", "queen_white" }, { "e1", "king_white" }, { "f1", "bishop_white" }, { "g1", "knight_white" }, { "h1", "rook_white" },
            { "a2", "pawn_white" }, { "b2", "pawn_white" },   { "c2", "pawn_white" },   { "d2", "pawn_white" },  { "e2", "pawn_white" }, { "f2", "pawn_white" },   { "g2", "pawn_white" },   { "h2", "pawn_white" },
            { "a3", "" },           { "b3", "" },             { "c3", "" },             { "d3", "" },            { "e3", "" },           { "f3", "" },             { "g3", "" },             { "h3", "" },
            { "a4", "" },           { "b4", "" },             { "c4", "" },             { "d4", "" },            { "e4", "" },           { "f4", "" },             { "g4", "" },             { "h4", "" },
            { "a5", "" },           { "b5", "" },             { "c5", "" },             { "d5", "" },            { "e5", "" },           { "f5", "" },             { "g5", "" },             { "h5", "" },
            { "a6", "" },           { "b6", "" },             { "c6", "" },             { "d6", "" },            { "e6", "" },           { "f6", "" },             { "g6", "" },             { "h6", "" },
            { "a7", "pawn_black" }, { "b7", "pawn_black" },   { "c7", "pawn_black" },   { "d7", "pawn_black" },  { "e7", "pawn_black" }, { "f7", "pawn_black" },   { "g7", "pawn_black" },   { "h7", "pawn_black" },
            { "a8", "rook_black" }, { "b8", "knight_black" }, { "c8", "bishop_black" }, { "d8", "queen_black" }, { "e8", "king_black" }, { "f8", "bishop_black" }, { "g8", "knight_black" }, { "h8", "rook_black" },
        };
        private List<string> enemyMines = new List<string>();

        public ManualResetEvent AddMineResetEvent = new ManualResetEvent(false);
        public ManualResetEvent LoadWindowResetEvent = new ManualResetEvent(false);
        public bool CanChooseMines { get; set; }
        public string Extra_figure { get; set; }
        public bool IsInquisitionInGame { get; set; }
        public List<string> Mines { get; set; } = new List<string>();
        public GameWindow Window { get { return window; } set { window = value; } }
        public GameWindowBlack WindowBlack { get { return windowBlack; } set { windowBlack = value; } }
        public Color MyColor { get { return myColor; } }
        public Dictionary<string, string> Figures { get { return figures; } }
        public Client()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Pick(string cell)
        {
            bool missile = chosenCell != "" && figures[chosenCell].Split("_")[0] == "missile" && !isMissileCharged;
            if (!isMyMove) return;
            if (transform) return;
            if (chosenCell == "" && figures[cell] != "" && figures[cell].Split("_")[1] == (myColor == Color.White ? "white" : "black"))
            {
                chosenCell = cell;
                if (myColor == Color.White) window.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => window.SetCellState(a, b)), chosenCell, "chosen");
                else windowBlack.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => windowBlack.SetCellState(a, b)), chosenCell, "chosen");
                return;
            }
            if (chosenCell == "") return;
            if (cell == chosenCell && !missile) return;
            if (figures[cell] != "" && figures[cell].Split("_")[1] == figures[chosenCell].Split("_")[1] && !missile)
            {
                chosenCell = cell;
                return;
            }

            bool flag = castle;
            if (!CanMove(chosenCell, cell)) return;
            moveResetEvent.Reset();

            chosenFigure = figures[chosenCell];
            if (figures[chosenCell].Split('_')[0] == "pawn" && (cell[1].ToString() == "8" || cell[1].ToString() == "1"))
            {
                //figures[cell] = "queen_" + figures[chosenCell].Split('_')[1];
                transform = true;
                cell1 = cell;
                if (myColor == Color.White)
                {
                    window.LetChooseFigure();
                    return;
                }
                else
                {
                    windowBlack.LetChooseFigure();
                    return;
                }
            }

            bool beat = !(figures[cell] == "" || (missile && !isMissileCharged));
            if (beat && figures[cell].Split("_")[0] == "king")
            {
                return;
            }
            Send("Move " + $"{figures[chosenCell]} {chosenCell} {cell} {(beat ? "beat" : "nothing")} noChange {(moveEnPassant ? "enPassant" : "noPassant")}");
            moveResetEvent.WaitOne();

            if (isMyMove)
            {
                cell = "";
                moveEnPassant = false;
                enPassantColumn = "0";
                return;
            }
            bool short_castle = false;
            bool long_castle = false;
            //window.Dispatcher.BeginInvoke((Action<string>)(a => window.MbShow(a)), "Move " + $"{figures[chosenCell]} {chosenCell} {cell} {myColor.ToString()} {isMyMove}");

            // Рокировка
            if (castle != flag)
            {
                if (myColor == Color.White)
                {
                    short_castle = cell == "g1";
                    long_castle = !short_castle;
                    figures[short_castle ? "f1" : "d1"] = "rook_white";
                    figures[short_castle ? "h1" : "a1"] = "";
                }
                else
                {
                    short_castle = cell == "g8";
                    long_castle = !short_castle;
                    figures[short_castle ? "f8" : "d8"] = "rook_black";
                    figures[short_castle ? "h8" : "a8"] = "";
                }
            }
            string f = figures[chosenCell].Split('_')[0];
            if (myColor == Color.White)
            {
                move++;
                string m = move.ToString() + "." + lettersToWrite[f] + chosenCell + "-" + cell + (enemy_check ? "+" : "") + " ";
                if (short_castle) m = move.ToString() + "." + "O-O ";
                if (long_castle) m = move.ToString() + "." + "O-O-O ";
                window.Dispatcher.BeginInvoke((Action<string>)(message => window.AddMove(message)), m);
            }
            else
            {
                string m = lettersToWrite[f] + chosenCell + "-" + cell + (enemy_check ? "+" : "") + " ";
                if (short_castle) m = "O-O ";
                if (long_castle) m = "O-O-O ";
                windowBlack.Dispatcher.BeginInvoke((Action<string>)(message => windowBlack.AddMove(message)), m);
            }
            check = false;
            // Танк
            if (f == "tank" && beat)
            {
                figures[cell] = "";
            }
            else
            {
                figures[cell] = chosenFigure;
                if (cell != chosenCell)
                {
                    figures[chosenCell] = "";
                }
            }

            // Взятие на проходе
            if (moveEnPassant)
            {
                figures[enPassantColumn + (Convert.ToInt32(cell[1].ToString()) + (myColor == Color.White ? -1 : 1)).ToString()] = "";
            }

            // Ракета
            if (f == "missile" && isMissileCharged && cell != chosenCell)
            {
                if (!beat)
                {
                    isMissileCharged = false;
                }
                else
                {
                    int from_row = Convert.ToInt32(chosenCell[1].ToString());
                    int to_row = Convert.ToInt32(cell[1].ToString());
                    if (from_row < to_row)
                    {
                        for (int i = from_row; i <= to_row; i++)
                        {
                            if (!figures[chosenCell[0].ToString() + i.ToString()].Contains("king"))
                            {
                                figures[chosenCell[0].ToString() + i.ToString()] = "";
                            }
                        }
                    }
                    else
                    {
                        for (int i = to_row; i <= from_row; i++)
                        {
                            if (!figures[chosenCell[0].ToString() + i.ToString()].Contains("king"))
                            {
                                figures[chosenCell[0].ToString() + i.ToString()] = "";
                            }
                        }
                    }
                }
            }

            // Инквизиция
            if (IsInquisitionInGame && beat && ContainsInquisition(myColor == Color.White ? "black" : "white", cell))
            {
                figures[cell] = "inquisition_" + (myColor == Color.White ? "black" : "white");
                figures[inquisition_cell] = "";
            }

            if (myColor == Color.White)
            {
                window.Dispatcher.BeginInvoke((Action)(() => window.UpdateDesk()));
                window.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => window.SetCellState(a, b)), chosenCell, "usual");
                if (enemy_check)
                {
                    string c = "";
                    foreach (string item in figures.Keys)
                    {
                        if (figures[item] == "king_black")
                        {
                            c = item;
                            break;
                        }
                    }
                    window.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => window.SetCellState(a, b)), c, "check");
                }
            }
            else
            {
                windowBlack.Dispatcher.BeginInvoke((Action)(() => windowBlack.UpdateDesk()));
                windowBlack.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => windowBlack.SetCellState(a, b)), chosenCell, "usual");
                if (enemy_check)
                {
                    string c = "";
                    foreach (string item in figures.Keys)
                    {
                        if (figures[item] == "king_white")
                        {
                            c = item;
                            break;
                        }
                    }
                    windowBlack.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => windowBlack.SetCellState(a, b)), c, "check");
                }
            }
            if (f == "missile" && cell == chosenCell)
            {
                isMissileCharged = true;
            }
            canMoveEnPassant = false;
            moveEnPassant = false;
            chosenCell = "";
            isMyMove = false;
        }

        public void ChooseFigure(string figure)
        {
            chosenFigure = figure;
            string a = figure.Split('_')[0];
            Send("Move " + $"{figures[chosenCell]} {chosenCell} {cell1} {(figures[cell1] == "" ? "nothing" : "beat")} {chosenFigure} noPassant");
            moveResetEvent.WaitOne();

            if (isMyMove)
            {
                cell1 = "";
                return;
            }
            canMoveEnPassant = false;
            string f = figures[chosenCell].Split('_')[0];
            if (myColor == Color.White)
            {
                move++;
                window.Dispatcher.BeginInvoke((Action<string>)(a => window.AddMove(a)), move.ToString() + "." + chosenCell + "-" + cell1 + (enemy_check ? "+" : "") + (a == "knight" ? " К" : (a == "bishop" ? " С" : (a == "queen" ? " Ф" : (a == "rook" ? " Л" : (a == "king" ? " Кр" : ""))))) + " ");
            }
            else
            {
                windowBlack.Dispatcher.BeginInvoke((Action<string>)(a => windowBlack.AddMove(a)), chosenCell + "-" + cell1 + (enemy_check ? "+" : "") + (a == "knight" ? " К" : (a == "bishop" ? " С" : (a == "queen" ? " Ф" : (a == "rook" ? " Л" : (a == "king" ? " Кр" : ""))))) + " ");
            }
            check = false;
            figures[cell1] = chosenFigure;
            figures[chosenCell] = "";
            if (myColor == Color.White)
            {
                window.Dispatcher.BeginInvoke((Action)(() => window.UpdateDesk()));
                window.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => window.SetCellState(a, b)), chosenCell, "usual");
                if (enemy_check)
                {
                    string c = "";
                    foreach (string item in figures.Keys)
                    {
                        if (figures[item] == "king_black")
                        {
                            c = item;
                            break;
                        }
                    }
                    window.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => window.SetCellState(a, b)), c, "check");
                }
            }
            else
            {
                windowBlack.Dispatcher.BeginInvoke((Action)(() => windowBlack.UpdateDesk()));
                windowBlack.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => windowBlack.SetCellState(a, b)), chosenCell, "usual");
                if (enemy_check)
                {
                    string c = "";
                    foreach (string item in figures.Keys)
                    {
                        if (figures[item] == "king_white")
                        {
                            c = item;
                            break;
                        }
                    }
                    windowBlack.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => windowBlack.SetCellState(a, b)), c, "check");
                }
            }
            chosenCell = "";
            isMyMove = false;
            transform = false;
        }

        private bool ContainsInquisition(string color, string cell)
        {
            inquisition_cell = "";

            foreach (string c in figures.Keys)
            {
                if (figures[c] == "inquisition_" + color)
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
                return true;
            }

            #region 3x3
            /*
            if (to_row != 8)
            {
                if (to_col != 8)
                {
                    if (figures[ColumnOfNumber(to_col + 1) + (to_row + 1).ToString()] == inquisition)
                    {
                        return true;
                    }
                }
                if (to_col != 0)
                {
                    if (figures[ColumnOfNumber(to_col - 1) + (to_row + 1).ToString()] == inquisition)
                    {
                        return true;
                    }
                }
                if (figures[ColumnOfNumber(to_col) + (to_row + 1).ToString()] == inquisition)
                {
                    return true;
                }
            }
            if (to_row != 0)
            {
                if (to_col != 8)
                {
                    if (figures[ColumnOfNumber(to_col + 1) + (to_row - 1).ToString()] == inquisition)
                    {
                        return true;
                    }
                }
                if (to_col != 0)
                {
                    if (figures[ColumnOfNumber(to_col - 1) + (to_row - 1).ToString()] == inquisition)
                    {
                        return true;
                    }
                }
                if (figures[ColumnOfNumber(to_col) + (to_row - 1).ToString()] == inquisition)
                {
                    return true;
                }
            }
            if (to_col != 8)
            {
                if (figures[ColumnOfNumber(to_col + 1) + to_row.ToString()] == inquisition)
                {
                    return true;
                }
            }
            if (to_col != 0)
            {
                if (figures[ColumnOfNumber(to_col - 1) + (to_row).ToString()] == inquisition)
                {
                    return true;
                }
            }
            */
            #endregion

            return false;
        }

        public void ResetGame()
        {
            figures = new Dictionary<string, string>()
            {
            { "a1", "rook_white" }, { "b1", "knight_white" }, { "c1", "bishop_white" }, { "d1", "queen_white" }, { "e1", "king_white" }, { "f1", "bishop_white" }, { "g1", "knight_white" }, { "h1", "rook_white" },
            { "a2", "pawn_white" }, { "b2", "pawn_white" },   { "c2", "pawn_white" },   { "d2", "pawn_white" },  { "e2", "pawn_white" }, { "f2", "pawn_white" },   { "g2", "pawn_white" },   { "h2", "pawn_white" },
            { "a3", "" },           { "b3", "" },             { "c3", "" },             { "d3", "" },            { "e3", "" },           { "f3", "" },             { "g3", "" },             { "h3", "" },
            { "a4", "" },           { "b4", "" },             { "c4", "" },             { "d4", "" },            { "e4", "" },           { "f4", "" },             { "g4", "" },             { "h4", "" },
            { "a5", "" },           { "b5", "" },             { "c5", "" },             { "d5", "" },            { "e5", "" },           { "f5", "" },             { "g5", "" },             { "h5", "" },
            { "a6", "" },           { "b6", "" },             { "c6", "" },             { "d6", "" },            { "e6", "" },           { "f6", "" },             { "g6", "" },             { "h6", "" },
            { "a7", "pawn_black" }, { "b7", "pawn_black" },   { "c7", "pawn_black" },   { "d7", "pawn_black" },  { "e7", "pawn_black" }, { "f7", "pawn_black" },   { "g7", "pawn_black" },   { "h7", "pawn_black" },
            { "a8", "rook_black" }, { "b8", "knight_black" }, { "c8", "bishop_black" }, { "d8", "queen_black" }, { "e8", "king_black" }, { "f8", "bishop_black" }, { "g8", "knight_black" }, { "h8", "rook_black" },
            };

            isMyMove = false;
            windowBlack = null;
            window = null;
            myColor = Color.NoColor;
            chosenCell = "";
            hasKingMoved = false;
            hasARookMoved = false;
            hasHRookMoved = false;
            castle = false;
            check = false;
            transform = false;
            enemy_check = false;
            cell1 = "";
            move = 0;
            chosenFigure = "";
            canMoveEnPassant = false;
            moveEnPassant = false;
            enPassantColumn = "0";
            isMissileCharged = false;
            inquisition_cell = "";
            moveResetEvent = new ManualResetEvent(false);
            IsInquisitionInGame = false;
        }

        #region Проверка хода
        private bool CanMove(string from, string to)
        {
            switch (figures[from].Split("_")[0])
            {
                case "pawn":
                    return CheckPawn(from, to);
                case "king":
                    return CheckKing(from, to);
                case "queen":
                    return CheckQueen(from, to);
                case "bishop":
                    return CheckBishop(from, to);
                case "knight":
                    return CheckKnight(from, to);
                case "rook":
                    return CheckRook(from, to);
                case "missile":
                    return (isMissileCharged && (to[0] == from[0] || to[1] == from[1])) || CheckKing(from, to);
                case "inquisition":
                    return CheckBishop(from, to);
                case "tank":
                    return CheckTank(from, to);
            }
            return true;
        }


        private bool CheckTank(string from, string to)
        {
            if (figures[to] == "" && CheckKing(from, to))
            {
                return true;
            }

            int from_col = NumberOfColumn(from[0].ToString());
            int from_row = Convert.ToInt32(from[1].ToString());
            int to_col = NumberOfColumn(to[0].ToString());
            int to_row = Convert.ToInt32(to[1].ToString());

            if (figures[to] != "" && figures[to].Split("_")[1] == (myColor == Color.White ? "black" : "white") && ((to_col == from_col && to_row - from_row < 6 && to_row - from_row > -6 && CheckRook(from, to)) || (to_row == from_row && to_col - from_col < 6 && to_col - from_col > -6 && CheckRook(from, to))))
            {
                return true;
            }
            return false;
        }

        private bool CheckPawn(string from, string to)
        {
            string from_col = from[0].ToString();
            int from_row = Convert.ToInt32(from[1].ToString());
            string to_col = to[0].ToString();
            int to_row = Convert.ToInt32(to[1].ToString());
            if ((NumberOfColumn(from_col) == NumberOfColumn(to_col) - 1 || NumberOfColumn(from_col.ToString()) == NumberOfColumn(to_col.ToString()) + 1) && 
                ((from_row + 1 == to_row && myColor == Color.White) || (from_row - 1 == to_row && myColor == Color.Black)) && figures[to] != "" && figures[from].Split("_")[1] != figures[to].Split("_")[1])
            {
                return true;
            }
            if (figures[to] != "")
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
            if (canMoveEnPassant && to_col == enPassantColumn && to_row == (myColor == Color.White ? 6 : 3) && figures[to_col + (to_row + (myColor == Color.White ? -1 : 1)).ToString()].Split('_')[0] == "pawn")
            {
                moveEnPassant = true;
                return true;
            }
            return false;
        }

        private bool CheckKnight(string from, string to)
        {
            int from_col = NumberOfColumn(from[0].ToString());
            int from_row = Convert.ToInt32(from[1].ToString());
            int to_col = NumberOfColumn(to[0].ToString());
            int to_row = Convert.ToInt32(to[1].ToString());

            if ((from_col == to_col + 2 && from_row == to_row + 1) ||
                (from_col == to_col + 2 && from_row == to_row - 1) ||
                (from_col == to_col - 2 && from_row == to_row + 1) ||
                (from_col == to_col - 2 && from_row == to_row - 1) ||
                (from_col == to_col + 1 && from_row == to_row + 2) ||
                (from_col == to_col + 1 && from_row == to_row - 2) ||
                (from_col == to_col - 1 && from_row == to_row + 2) ||
                (from_col == to_col - 1 && from_row == to_row - 2))
            {
                return true;
            }

            return false;
        }

        private bool CheckBishop(string from, string to)
        {
            int from_col = NumberOfColumn(from[0].ToString());
            int from_row = Convert.ToInt32(from[1].ToString());
            int to_col = NumberOfColumn(to[0].ToString());
            int to_row = Convert.ToInt32(to[1].ToString());

            if (to_col - from_col != to_row - from_row && to_col - from_col != from_row - to_row)
            {
                return false;
            }
            else
            {
                if (from_col > to_col)
                {
                    for (int i = 1; i < from_col - to_col; i++)
                    {
                        if (figures[ColumnOfNumber(to_col + i) + (to_row + (to_row > from_row ? -i : i)).ToString()] != "")
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < to_col - from_col; i++)
                    {
                        if (figures[ColumnOfNumber(from_col + i) + (from_row + (from_row > to_row ? -i : i)).ToString()] != "")
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool CheckQueen(string from, string to)
        {
            int from_col = NumberOfColumn(from[0].ToString());
            int from_row = Convert.ToInt32(from[1].ToString());
            int to_col = NumberOfColumn(to[0].ToString());
            int to_row = Convert.ToInt32(to[1].ToString());

            if ((from_col > to_col ? from_col - to_col : to_col - from_col) != (from_row > to_row ? from_row - to_row : to_row - from_row) && !(from_row == to_row || from_col == to_col))
            {
                return false;
            }

            if (from_col == to_col)
            {
                for (int i = 1; i < (from_row > to_row ? from_row - to_row : to_row - from_row); i++)
                {
                    if (figures[ColumnOfNumber(from_col) + (to_row + (to_row > from_row ? -i : i)).ToString()] != "")
                    {
                        return false;
                    }
                }
            }
            else if (from_row == to_row)
            {
                for (int i = 1; i < (from_col > to_col ? from_col - to_col : to_col - from_col); i++)
                {
                    if (figures[ColumnOfNumber(from_col + (from_col < to_col ? i : -i)) + from_row.ToString()] != "")
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (from_col > to_col)
                {
                    for (int i = 1; i < from_col - to_col; i++)
                    {
                        if (figures[ColumnOfNumber(to_col + i) + (to_row + (to_row > from_row ? -i : i)).ToString()] != "")
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < to_col - from_col; i++)
                    {
                        if (figures[ColumnOfNumber(from_col + i) + (from_row + (from_row > to_row ? -i : i)).ToString()] != "")
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        private bool CheckKing(string from, string to)
        {
            int from_col = NumberOfColumn(from[0].ToString());
            int from_row = Convert.ToInt32(from[1].ToString());
            int to_col = NumberOfColumn(to[0].ToString());
            int to_row = Convert.ToInt32(to[1].ToString());

            if ((from_col > to_col ? from_col - to_col : to_col - from_col) <= 1 && (from_row > to_row ? from_row - to_row : to_row - from_row) <= 1)
            {
                if (!hasKingMoved) hasKingMoved = true;
                return true;
            }
            if (!hasKingMoved && !hasHRookMoved && !castle && to_col == 7 && figures["f" + from_row.ToString()] == "" && !check)
            {
                hasKingMoved = true;
                hasHRookMoved = true;
                castle = true;
                return true;
            }
            if (!hasKingMoved && !hasARookMoved && !castle && to_col == 3 && figures["d" + from_row.ToString()] == "" && !check)
            {
                hasKingMoved = true;
                castle = true;
                hasARookMoved = true;
                return true;
            }

            return false;
        }

        private bool CheckRook(string from, string to)
        {
            int from_col = NumberOfColumn(from[0].ToString());
            int from_row = Convert.ToInt32(from[1].ToString());
            int to_col = NumberOfColumn(to[0].ToString());
            int to_row = Convert.ToInt32(to[1].ToString());

            if (from_col != to_col && from_row != to_row)
            {
                return false;
            }

            if (from_col == to_col)
            {
                for (int i = 1; i < (from_row > to_row ? from_row - to_row : to_row - from_row); i++)
                {
                    if (figures[ColumnOfNumber(from_col) + (to_row + (to_row > from_row ? -i : i)).ToString()] != "")
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 1; i < (from_col > to_col ? from_col - to_col : to_col - from_col); i++)
                {
                    if (figures[ColumnOfNumber(from_col + (from_col < to_col ? i : -i)) + from_row.ToString()] != "")
                    {
                        return false;
                    }
                }
            }

            if (!hasARookMoved && from_col == 1) hasARookMoved = true;
            if (!hasHRookMoved && from_col == 8) hasHRookMoved = true;
            return true;
        }

        private int NumberOfColumn(string column)
        {
            return letters.IndexOf(column);
        }

        private string ColumnOfNumber(int column)
        {
            return letters[column];
        }

        #endregion

        public void StartPlaying(string mode)
        {
            Extra_figure = mode;
            socket.Send(Encoding.ASCII.GetBytes("Play " + mode));
            Thread receiverThread = new Thread(() =>
            {
                bool stop = false;
                while (!stop)
                {
                    string message = Receive();
                    if (message.Contains("CHECKMATE"))
                    {
                        Checkmate(message.Split()[1] == "White" ? Color.White : Color.Black);
                        stop = true;
                    }
                    else if (message.Contains("GaveUp"))
                    {
                        PlayerGaveUp();
                        stop = true;
                    }
                    else if (message.Contains("Draw?"))
                    {
                        PlayerWantsDraw();
                    }
                    else if (message.Contains("DRAW"))
                    {
                        DRAW();
                        stop = true;
                    }
                    else if (message.Contains("Color"))
                    {
                        ReceiveColor(message);
                    }
                    else if (message == "Move again")
                    {
                        moveResetEvent.Set();
                    }
                    else if (message.Contains("Move ok"))
                    {
                        isMyMove = false;
                        enemy_check = message.Split()[2] == "check";
                        moveResetEvent.Set();
                    }
                    else if (message.Contains("Move "))
                    {
                        ReceiveMove(message);
                    }
                    else if (message.Contains("Moved"))
                    {
                        ReceiveMoved(message);
                    }
                    else if (message.Contains("Figure"))
                    {
                        ReceiveNewFigure(message);
                    }
                    else if (message.Contains("Traitors"))
                    {
                        ReceiveTraitors(message);
                    }
                    else if (message == "CHOOSE MINES")
                    {
                        ChooseMines();
                    }
                    else if (message == "Wait mines")
                    {
                        WaitMines();
                    }
                    else if (message.Contains("EnemyMines"))
                    {
                        ReceiveMines(message);
                    }
                }
            });

            receiverThread.Start();
        }

        #region Receive
        private void WaitMines()
        {
            LoadWindowResetEvent.WaitOne();
            if (myColor == Color.White)
            {
                window.Dispatcher.BeginInvoke((Action)(() => window.WaitMines()));
            }
            else
            {
                windowBlack.Dispatcher.BeginInvoke((Action)(() => windowBlack.WaitMines()));
            }
        }
        private void ReceiveMines(string message)
        {
            string[] words = message.Split();
            enemyMines.Add(words[1]);
            enemyMines.Add(words[2]);
        }
        private void ChooseMines()
        {
            CanChooseMines = true;
            AddMineResetEvent.WaitOne();
            AddMineResetEvent.Reset();
            AddMineResetEvent.WaitOne();
            AddMineResetEvent.Reset();
            CanChooseMines = false;
            Send($"Mines {Mines[0]} {Mines[1]}");
        }
        private void Checkmate(Color color)
        {
            if (myColor == Color.White)
            {
                window.Dispatcher.Invoke(() => window.EndGame("Checkmate", color, color == myColor));
            }
            else
            {
                windowBlack.Dispatcher.Invoke(() => windowBlack.EndGame("Checkmate", color, color == myColor));
            }
            ResetGame();
        }
        private void PlayerWantsDraw()
        {
            bool draw = false;
            if (myColor == Color.White)
            {
                draw = window.Dispatcher.Invoke(() => window.AskForDraw());
            }
            else
            {
                draw = windowBlack.Dispatcher.Invoke(() => windowBlack.AskForDraw());
            }
            if (draw)
            {
                Send("DRAW");
                DRAW();
            }
        }
        private void DRAW()
        {
            if (myColor == Color.White)
            {
                window.Dispatcher.Invoke(() => window.EndGame("Draw"));
            }
            else
            {
                windowBlack.Dispatcher.Invoke(() => windowBlack.EndGame("Draw"));
            }
            ResetGame();
        }
        private void PlayerGaveUp()
        {
            if (myColor == Color.White)
            {
                window.Dispatcher.Invoke(() => window.EndGame("GaveUp"));
            }
            else
            {
                windowBlack.Dispatcher.Invoke(() => windowBlack.EndGame("GaveUp"));
            }
            ResetGame();
        }
        private void ReceiveColor(string message)
        {
            myColor = message.Split()[1] == "white" ? Color.White : Color.Black;
        }
        private void ReceiveMove(string message)
        {
            isMyMove = message.Split()[1] == (myColor == Color.White ? "white" : "black");
            if (isMyMove)
            {
                if (myColor == Color.White && move == 1 && Mines.Count > 0)
                {
                    window.Dispatcher.BeginInvoke((Action)(() => window.CanMove()));
                }
                moveResetEvent.Set();
            }
        }
        private void ReceiveTraitors(string message)
        {
            string[] words = message.Split();
            string white_pos = words[1];
            string black_pos = words[2];
            string type = figures[white_pos].Split("_")[0];


            figures[white_pos] = type + "_white";
            figures[black_pos] = type + "_black";

            if (myColor == Color.White)
            {
                window.Dispatcher.BeginInvoke((Action)(() => window.UpdateDesk()));
                window.Dispatcher.BeginInvoke((Action<string>)(a => window.MbShow(a)), "Предатели!");
            }
            else
            {
                windowBlack.Dispatcher.BeginInvoke((Action)(() => windowBlack.UpdateDesk()));
                windowBlack.Dispatcher.BeginInvoke((Action<string>)(a => windowBlack.MbShow(a)), "Предатели!");
            }
        }
        private void ReceiveNewFigure(string message)
        {
            string[] words = message.Split();
            string new_figure = words[1];
            string white_new_figure = words[2];
            string black_new_figure = words[3];

            figures[white_new_figure] = new_figure + "_white";
            figures[black_new_figure] = new_figure + "_black";
            if (myColor == Color.White)
            {
                window.Dispatcher.BeginInvoke((Action)(() => window.UpdateDesk()));
                window.Dispatcher.BeginInvoke((Action<string>)(a => window.MbShow(a)), "Новая фигура: " + (new_figure == "missile" ? "ракета" : (new_figure == "tank" ? "танк" : (new_figure == "inquisition" ? "военная полиция" : ""))));
            }
            else
            {
                windowBlack.Dispatcher.BeginInvoke((Action)(() => windowBlack.UpdateDesk()));
                windowBlack.Dispatcher.BeginInvoke((Action<string>)(a => windowBlack.MbShow(a)), "Новая фигура: " + (new_figure == "missile" ? "ракета" : (new_figure == "tank" ? "танк" : (new_figure == "inquisition" ? "военная полиция" : ""))));
            }
        }
        private void ReceiveMoved(string message)
        {
            string[] words = message.Split();
            string figure = words[1];
            string from = words[2];
            string to = words[3];
            string new_fig = words[5];
            string a = "";
            bool enPassant = words[6] == "enPassant";
            string enPassantCell = words[7];

            if (new_fig != "noChange")
            {
                figure = new_fig;
                a = new_fig.Split('_')[0];
            }

            bool beat = figures[to] != "" && to != from;
            if (figure.Split("_")[0] == "tank" && beat)
            {
                figures[to] = "";
            }
            else
            {
                figures[from] = "";
                figures[to] = figure;
            }

            isMyMove = true;
            enemy_check = false;

            check = words[4] == "check";

            string f = figure.Split('_')[0];
            string from_y = from[1].ToString();
            string to_y = to[1].ToString();
            if (f == "pawn" && ((from_y == "7" && to_y == "5") || (from_y == "2" && to_y == "4")))
            {
                canMoveEnPassant = true;
                enPassantColumn = to[0].ToString();
            }
            else if (canMoveEnPassant)
            {
                canMoveEnPassant = false;
                enPassantColumn = "0";
            }
            // Взятие на проходе
            if (enPassant)
            {
                figures[enPassantCell] = "";
            }
            // Ракета уничтожает все
            if (figure.Split("_")[0] == "missile" && to != from)
            {
                int from_row = Convert.ToInt32(from[1].ToString());
                int to_row = Convert.ToInt32(to[1].ToString());
                if (from_row < to_row)
                {
                    for (int i = from_row; i <= to_row; i++)
                    {
                        figures[from[0].ToString() + i.ToString()] = "";
                    }
                }
                else
                {
                    for (int i = to_row; i <= from_row; i++)
                    {
                        figures[from[0].ToString() + i.ToString()] = "";
                    }
                }
            }
            // Инквизиция карает грешников
            if (IsInquisitionInGame && beat && ContainsInquisition(myColor == Color.White ? "white" : "black", to))
            {
                figures[to] = "inquisition_" + (myColor == Color.White ? "white" : "black");
                figures[inquisition_cell] = "";
            }
            // Запись ходов
            if (myColor == Color.Black)
            {
                bool short_castle = f == "king" && from == "e1" && to == "g1";
                bool long_castle = f == "king" && from == "e1" && to == "c1";
                move++;
                string m = lettersToWrite[f] + from + "-" + to + (check ? "+" : "") + (new_fig != "noChange" ? lettersToWrite[f] : "") + " ";
                if (long_castle) m = "O-O-O ";
                if (short_castle) m = "O-O ";
                windowBlack.Dispatcher.BeginInvoke((Action<string>)(a => windowBlack.AddMove(a)), move.ToString() + "." + m);
            }
            else
            {
                bool short_castle = f == "king" && from == "e8" && to == "g8";
                bool long_castle = f == "king" && from == "e8" && to == "c8";
                string m = (new_fig == "noChange" ? lettersToWrite[f] : "") + from + "-" + to + (check ? "+" : "") + (new_fig != "noChange" ? lettersToWrite[f] : "") + " ";
                if (long_castle) m = "O-O-O ";
                if (short_castle) m = "O-O ";
                window.Dispatcher.BeginInvoke((Action<string>)(a => window.AddMove(a)), m);
            }

            // Обновление доски
            if (myColor == Color.White)
            {
                window.Dispatcher.BeginInvoke((Action)(() => window.UpdateDesk()));
                if (check)
                {
                    string c = "";
                    foreach (string item in figures.Keys)
                    {
                        if (figures[item] == "king_white")
                        {
                            c = item;
                            break;
                        }
                    }
                    window.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => window.SetCellState(a, b)), c, "check");
                }
            }
            else
            {
                windowBlack.Dispatcher.BeginInvoke((Action)(() => windowBlack.UpdateDesk()));
                if (check)
                {
                    string c = "";
                    foreach (string item in figures.Keys)
                    {
                        if (figures[item] == "king_black")
                        {
                            c = item;
                            break;
                        }
                    }
                    windowBlack.Dispatcher.BeginInvoke((Action<string, string>)((a, b) => windowBlack.SetCellState(a, b)), c, "check");
                }
            }
        }
        #endregion

        public void Connect()
        {
            if (!socket.Connected)
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            }
        }

        public void Send(string message)
        {
            try
            {
                socket.Send(Encoding.ASCII.GetBytes(message));
            }
            catch (Exception e)
            {

            }
        }

        public string Receive()
        {
            try
            {
                byte[] bytes = new byte[1024];
                int number = socket.Receive(bytes);
                return Encoding.ASCII.GetString(bytes, 0, number);
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
