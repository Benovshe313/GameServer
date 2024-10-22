
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameServer
{
    internal class Program
    {
        static void InvitePlayer(TcpClient client1, TcpClient client2)
        {
            var networkStream1 = client1.GetStream();
            var sw1 = new StreamWriter(networkStream1) { AutoFlush = true };
            var sr1 = new StreamReader(networkStream1);

            var networkStream2 = client2.GetStream();
            var sw2 = new StreamWriter(networkStream2) { AutoFlush = true };
            var sr2 = new StreamReader(networkStream2);

            sw1.WriteLine("Press 1 accept invitation else press 2 decline:");
            sw2.WriteLine("Press 1 accept invitation else press 2 decline:");

            bool player1Accept = false;
            bool player2Accept = false;

            while (true)
            {
                var msg1 = sr1.ReadLine();
                Console.WriteLine($"{client1.Client.RemoteEndPoint}: {msg1}");

                if (msg1 != null && int.TryParse(msg1, out int choice1))
                {
                    if (choice1 == 1)
                    {
                        sw1.WriteLine("You are Player X");
                        player1Accept = true;
                    }
                    else if (choice1 == 2)
                    {
                        sw1.WriteLine("You rejected invitation");
                        sw2.WriteLine("Opponent rejected invitation");
                        EndGame(client1, client2);
                        return;
                    }
                }

                var msg2 = sr2.ReadLine();
                Console.WriteLine($"{client2.Client.RemoteEndPoint}: {msg2}");

                if (msg2 != null && int.TryParse(msg2, out int choice2))
                {
                    if (choice2 == 1)
                    {
                        sw2.WriteLine("You are Player O");
                        player2Accept = true;
                    }
                    else if (choice2 == 2)
                    {
                        sw2.WriteLine("You rejected invitation");
                        sw1.WriteLine("Opponent rejected invitation");
                        EndGame(client1, client2);
                        return;
                    }
                }

                if (player1Accept && player2Accept)
                {
                    StartGame(client1, client2);
                    break;
                }
            }
        }

        static void EndGame(TcpClient client1, TcpClient client2)
        {
            client1.Close();
            client2.Close();
            Console.WriteLine("Game ended");
        }

        static void StartGame(TcpClient client1, TcpClient client2)
        {
            char[] board = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
            char currentPlayer = 'X';

            var networkStream1 = client1.GetStream();
            var sw1 = new StreamWriter(networkStream1) { AutoFlush = true };
            var sr1 = new StreamReader(networkStream1);

            var networkStream2 = client2.GetStream();
            var sw2 = new StreamWriter(networkStream2) { AutoFlush = true };
            var sr2 = new StreamReader(networkStream2);

            while (true)
            {
                Console.Clear();
                GameBoard(board);

                if (currentPlayer == 'X')
                {
                    sw1.WriteLine($"Player {currentPlayer} your turn enter your move (1-9): ");
                    int value = int.Parse(sr1.ReadLine()!);
                    if (PlayerMove(value, board))
                    {
                        board[value] = currentPlayer;
                        if (WinCond(board, currentPlayer))
                        {
                            Console.Clear();
                            GameBoard(board);
                            sw1.WriteLine($"Player {currentPlayer} wins. Game over!");
                            sw2.WriteLine("You lost. Game over!");
                            break;
                        }
                        currentPlayer = 'O';
                    }
                    else
                    {
                        sw1.WriteLine("Invalid move");
                    }
                }
                else
                {
                    sw2.WriteLine($"Player {currentPlayer} your turn enter your move (1-9): ");
                    int value = int.Parse(sr2.ReadLine()!);
                    if (PlayerMove(value, board))
                    {
                        board[value] = currentPlayer;
                        if (WinCond(board, currentPlayer))
                        {
                            Console.Clear();
                            GameBoard(board);
                            sw2.WriteLine($"Player {currentPlayer} win. Game over!");
                            sw1.WriteLine("You lost! Game over!");
                            break;
                        }
                        currentPlayer = 'X';
                    }
                    else
                    {
                        sw2.WriteLine("Invalid move");
                    }
                }
            }
            EndGame(client1, client2);
        }

        static void GameBoard(char[] board)
        {
            Console.WriteLine($" {board[1]} | {board[2]} | {board[3]} ");
            Console.WriteLine("---|---|---");
            Console.WriteLine($" {board[4]} | {board[5]} | {board[6]} ");
            Console.WriteLine("---|---|---");
            Console.WriteLine($" {board[7]} | {board[8]} | {board[9]} ");
        }

        static bool WinCond(char[] board, char player)
        {
            return (board[1] == player && board[2] == player && board[3] == player) ||
                   (board[4] == player && board[5] == player && board[6] == player) ||
                   (board[7] == player && board[8] == player && board[9] == player) ||
                   (board[1] == player && board[5] == player && board[9] == player) ||
                   (board[3] == player && board[5] == player && board[7] == player) ||
                   (board[2] == player && board[5] == player && board[8] == player) ||
                   (board[1] == player && board[4] == player && board[7] == player) ||
                   (board[3] == player && board[6] == player && board[9] == player);
        }

        static bool PlayerMove(int move, char[] board)
        {
            return move >= 1 && move <= 9 && board[move] == ' ';
        }
        static void Main(string[] args)
        {
            var ip = IPAddress.Parse("192.168.100.115");
            var port = 27001;
            var endPoint = new IPEndPoint(ip, port);
            var listener = new TcpListener(endPoint);

            try
            {
                listener.Start();
                Console.WriteLine("Server started");

                while (true)
                {
                    var client1 = listener.AcceptTcpClient();
                    Console.WriteLine($"User1 {client1.Client.RemoteEndPoint} connected");

                    var client2 = listener.AcceptTcpClient();
                    Console.WriteLine($"User2 {client2.Client.RemoteEndPoint} connected");

                    _ = Task.Run(() => InvitePlayer(client1, client2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}

