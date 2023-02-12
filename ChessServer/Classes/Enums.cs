namespace ChessServer.Classes
{
    public enum GameMode { Classic, Missile, Tank, Mines, Police };
    public enum Color { Black, White };
    public enum FigureType { Pawn, Rook, Knight, Bishop, Queen, King, Missile, Tank, Inquisition }
    public enum GameState { Wait, MoveWhite, MoveBlack }
}