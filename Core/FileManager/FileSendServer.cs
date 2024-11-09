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
    private static readonly object fileLock = new object();

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
                string repositoryName = ReceiveRepositoryName(stream);

                // Получаем номер версии
                byte[] versionBytes = new byte[4];
                stream.Read(versionBytes, 0, 4);
                int versionNumber = BitConverter.ToInt32(versionBytes, 0);

                string repositoryPath = Path.Combine(_baseDirectory, repositoryName);

                if (!Directory.Exists(repositoryPath))
                {
                    Console.WriteLine($"Репозиторий '{repositoryName}' не найден.");
                    continue;
                }

                lock (fileLock)
                {
                    SendDirectory(stream, repositoryPath, versionNumber != -1 ? versionNumber : (int?)null);
                }
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
        return Encoding.UTF8.GetString(nameBytes);
    }

    private void SendDirectory(NetworkStream stream, string sourceDirectory, int? versionNumber)
    {
        string zipFilePath = Path.Combine(Path.GetTempPath(), "archive.zip");

        try
        {
            if (versionNumber.HasValue && versionNumber > 0)
            {
                // Если указан номер версии, ищем архив в папке "version"
                string versionPath = Path.Combine(sourceDirectory, "versions", $"v.{versionNumber}.zip");
                if (File.Exists(versionPath))
                {
                    SendFile(versionPath, $"v{versionNumber}.zip", stream);
                    return;
                }
                else
                {
                    Console.WriteLine($"Архив версии v{versionNumber} не найден.");
                }
            }

            // Если версия не указана или архив версии не найден, отправляем основной репозиторий
            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (string filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
                {
                    if (filePath.Contains(Path.Combine(sourceDirectory, "version")))
                        continue;

                    string relativePath = Path.GetRelativePath(sourceDirectory, filePath);
                    zipArchive.CreateEntryFromFile(filePath, relativePath);
                }
            }

            SendFile(zipFilePath, "archive.zip", stream);
        }
        finally
        {
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
