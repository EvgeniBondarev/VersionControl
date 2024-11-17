using Core.CommentManager.Repository;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Core.CommentManager
{
    public class CommentServer
    {
        private readonly int _port;
        private readonly CommentRepository _repository;

        public CommentServer(int port, string databasePath)
        {
            _port = port;
            _repository = new CommentRepository(databasePath);
        }

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            Console.WriteLine($"Сервер запущен и слушает порт {_port}...");

            while (true)
            {
                using TcpClient client = listener.AcceptTcpClient();
                using NetworkStream stream = client.GetStream();

                try
                {
                    byte command = (byte)stream.ReadByte();
                    if (command == 1)
                    {
                        ReceiveComment(stream);
                    }
                    else if (command == 2)
                    {
                        SendComments(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
            }
        }

        private void ReceiveComment(NetworkStream stream)
        {
            string repositoryName = ReceiveString(stream);
            string comment = ReceiveString(stream);
            string username = ReceiveString(stream);

            _repository.InsertComment(repositoryName, comment, username);
            Console.WriteLine($"Комментарий сохранён для репозитория '{repositoryName}' пользователем '{username}'.");
        }

        private void SendComments(NetworkStream stream)
        {
            string repositoryName = ReceiveString(stream);
            List<(string Comment, string Username, string Date)> comments = _repository.GetComments(repositoryName);

            // Отправка количества комментариев
            stream.Write(BitConverter.GetBytes(comments.Count), 0, 4);

            // Отправка каждого комментария
            foreach (var (comment, username, date) in comments)
            {
                SendString(stream, date);
                SendString(stream, username);
                SendString(stream, comment);
            }
        }

        private void SendString(NetworkStream stream, string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            stream.Write(BitConverter.GetBytes(dataBytes.Length), 0, 4);
            stream.Write(dataBytes, 0, dataBytes.Length);
        }

        private string ReceiveString(NetworkStream stream)
        {
            byte[] sizeBytes = new byte[4];
            stream.Read(sizeBytes, 0, 4);
            int size = BitConverter.ToInt32(sizeBytes, 0);

            byte[] dataBytes = new byte[size];
            stream.Read(dataBytes, 0, size);
            return Encoding.UTF8.GetString(dataBytes);
        }
    }
}
