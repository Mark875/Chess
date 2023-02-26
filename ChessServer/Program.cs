using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ChessServer.Classes;


namespace ChessServer
{
    class Program
    {
        private static Socket socket;
        private static List<Player> players = new List<Player>();
        private static List<Game> games = new List<Game>();
        static void Main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081));
            socket.Listen(100);

            while (true)
            {
                Socket client = socket.Accept();
                Console.WriteLine("User connected");

                Thread t1 = new Thread(() =>
                {
                    Game g = null;
                    Player p = new Player(client);
                    bool stop = false;
                    while (!stop)
                    {
                        byte[] data = new byte[1024];
                        int bytes = 1;
                        string message = "";
                        try
                        {
                            bytes = client.Receive(data);
                            message = Encoding.ASCII.GetString(data, 0, bytes);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }
                        Console.WriteLine(message);
                        if (message.Contains("Bye"))
                        {
                            stop = true;
                            continue;
                        }
                        if (message.Contains("Play"))
                        {
                            string[] words = message.Split();
                            string extra_figures = words[1];
                            bool found = false;
                            for (int i = 0; i < games.Count; i++)
                            {
                                if (games[i].GameState == GameState.Wait && games[i].Extra_figure == extra_figures)
                                {
                                    p.GameIndex = i;
                                    games[i].AddPlayer(p);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                Console.WriteLine("NEW GAME");
                                g = new Game(new List<Player>() { p }, extra_figures);
                                games.Add(g);
                                p.GameIndex = games.Count - 1;
                            }
                        }
                        if (message.Contains("DRAW"))
                        {
                            foreach (Player player in games[p.GameIndex].Players)
                            {
                                if (player.Color != p.Color)
                                {
                                    player.Socket.Send(Encoding.ASCII.GetBytes("DRAW"));
                                }
                            }
                        }
                        if (message.Contains("GiveUp"))
                        {
                            games[p.GameIndex].PlayerGaveUp(p.Color);
                        }
                        if (message.Contains("Draw?"))
                        {
                            foreach (Player player in games[p.GameIndex].Players)
                            {
                                if (player.Color != p.Color)
                                {
                                    player.Socket.Send(Encoding.ASCII.GetBytes("Draw?"));
                                }
                            }
                        }
                        if (message.Contains("Move") || message.Contains("Mines"))
                        {
                            games[p.GameIndex].ReceiveMessageResetEvent.Set();
                            games[p.GameIndex].Message = message;
                        }
                        if (message == "Cancel")
                        {
                            games.RemoveAt(p.GameIndex);
                        }
                    }
                });

                t1.Start();
            }

        }
    }
}
