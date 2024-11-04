using Core.FileManager;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class FileSendServer : IFileTransfer
{
    private readonly int _port;
    private readonly string _baseDirectory;

    public FileSendServer(int port, string baseDirectory)
    {
        _port = port;
        _baseDirectory = baseDirectory;
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
                // Получаем имя репозитория от клиента
                string repositoryName = ReceiveRepositoryName(stream);

                // Проверяем существование директории репозитория
                string repositoryPath = Path.Combine(_baseDirectory, repositoryName);
                if (!Directory.Exists(repositoryPath))
                {
                    Console.WriteLine($"Репозиторий '{repositoryName}' не найден.");
                    return;
                }

                SendDirectory(stream, repositoryPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке файла: " + ex.Message);
            }
        }
    }

    private string ReceiveRepositoryName(NetworkStream stream)
    {
        byte[] nameSizeBytes = new byte[4];
        stream.Read(nameSizeBytes, 0, 4);
        int nameSize = BitConverter.ToInt32(nameSizeBytes, 0);

        byte[] nameBytes = new byte[nameSize];
        stream.Read(nameBytes, 0, nameSize);
        string repositoryName = Encoding.UTF8.GetString(nameBytes);

        Console.WriteLine($"Получено имя репозитория: {repositoryName}");
        return repositoryName;
    }

    private void SendDirectory(NetworkStream stream, string sourceDirectory)
    {
        string zipFilePath = Path.Combine(Path.GetTempPath(), "archive.zip");

        try
        {
            // Архивируем директорию перед отправкой
            ZipFile.CreateFromDirectory(sourceDirectory, zipFilePath);
            string fileName = "archive.zip";
            Console.WriteLine($"Папка {sourceDirectory} заархивирована в {zipFilePath}");

            SendFile(zipFilePath, fileName, stream);
        }
        finally
        {
            // Удаляем временный архив после отправки
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
        }
    }

    private void SendFile(string filePath, string fileName, NetworkStream stream)
    {
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
        stream.Write(BitConverter.GetBytes(fileNameBytes.Length), 0, 4);
        stream.Write(fileNameBytes, 0, fileNameBytes.Length);

        byte[] fileData = File.ReadAllBytes(filePath);
        stream.Write(BitConverter.GetBytes(fileData.Length), 0, 4);
        stream.Write(fileData, 0, fileData.Length);

        Console.WriteLine("Файл успешно отправлен клиенту.");
    }
}
