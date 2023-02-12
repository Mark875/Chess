using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessServer.Classes
{
    public class Figure
    {
        private int x;
        private int y;
        private string cell;
        private Color color;
        private List<string> letters = new List<string>() { "", "a", "b", "c", "d", "e", "f", "g", "h" };
        private bool isActive = true;
        private FigureType type;

        public FigureType Type { get { return type; } set { type = value; } }
        public bool IsActive { get { return isActive; } set { isActive = value; } }
        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }
        public string Cell { get { return cell; } }
        public Color Color { get { return color; } }
        public List<string> HitCells { get; set; } = new List<string>();
        public bool IsCharged { get; set; } = false;

        public Figure(FigureType type, Color color, int x, int y)
        {
            this.type = type;
            this.color = color;
            this.x = x;
            this.y = y;
            cell = letters[x] + y.ToString();
        }

        public void Move(int x, int y)
        {
            this.x = x;
            this.y = y;
            cell = letters[x] + y.ToString();
        }

        public void Move(string cell)
        {
            this.cell = cell;
            x = letters.IndexOf(cell[0].ToString());
            y = Convert.ToInt32(cell[1].ToString());
        }
    }
}
