using Core.FileManager;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class FileTransferServer : IFileTransfer
{
    private readonly int _port;
    private readonly string _destinationDirectory;

    public FileTransferServer(int port, string destinationDirectory)
    {
        _port = port;
        _destinationDirectory = destinationDirectory;
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
                ReceiveFile(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении файла: " + ex.Message);
            }
        }
    }

    private void ReceiveFile(NetworkStream stream)
    {
        // Получаем имя репозитория
        byte[] repoNameSizeBytes = new byte[4];
        stream.Read(repoNameSizeBytes, 0, 4);
        int repoNameSize = BitConverter.ToInt32(repoNameSizeBytes, 0);

        byte[] repoNameBytes = new byte[repoNameSize];
        stream.Read(repoNameBytes, 0, repoNameSize);
        string repositoryName = Encoding.UTF8.GetString(repoNameBytes);

        // Получаем имя файла
        byte[] nameSizeBytes = new byte[4];
        stream.Read(nameSizeBytes, 0, 4);
        int nameSize = BitConverter.ToInt32(nameSizeBytes, 0);

        byte[] nameBytes = new byte[nameSize];
        stream.Read(nameBytes, 0, nameSize);
        string fileName = Encoding.UTF8.GetString(nameBytes);

        // Получаем содержимое файла
        byte[] fileSizeBytes = new byte[4];
        stream.Read(fileSizeBytes, 0, 4);
        int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

        byte[] fileData = new byte[fileSize];
        int totalRead = 0;
        while (totalRead < fileSize)
        {
            totalRead += stream.Read(fileData, totalRead, fileSize - totalRead);
        }

        // Сохраняем и разархивируем файл
        string repoDirectory = Path.Combine(_destinationDirectory, repositoryName);
        Directory.CreateDirectory(repoDirectory);

        string receivedFilePath = Path.Combine(repoDirectory, fileName);
        File.WriteAllBytes(receivedFilePath, fileData);
        Console.WriteLine($"Файл {fileName} получен и сохранен в {repoDirectory}");

        ExtractFile(receivedFilePath, repoDirectory);
    }

    private void ExtractFile(string zipFilePath, string destinationDirectory)
    {
        string tempDirectory = Path.Combine(destinationDirectory, "TempExtract");
        Directory.CreateDirectory(tempDirectory);

        ZipFile.ExtractToDirectory(zipFilePath, tempDirectory);
        Console.WriteLine($"Файл {zipFilePath} успешно разархивирован во временную директорию {tempDirectory}");

        foreach (string filePath in Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(tempDirectory, filePath);
            string destinationPath = Path.Combine(destinationDirectory, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            File.Copy(filePath, destinationPath, overwrite: true);
        }

        Directory.Delete(tempDirectory, recursive: true);
        File.Delete(zipFilePath);
        Console.WriteLine($"Файл {zipFilePath} успешно разархивирован и заменён в {destinationDirectory}");
    }
}
