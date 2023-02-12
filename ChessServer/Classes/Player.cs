using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ChessServer.Classes
{
    class Player
    {
        public List<Figure> Figures = new List<Figure>();
        public Socket Socket { get; set; }
        public Color Color { get; set; }
        public int GameIndex { get; set; }

        public Player(Socket socket)
        {
            Socket = socket;
        }
    }
}
