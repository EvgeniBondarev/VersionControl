using Core.FileManager;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class FileTransferServer : IFileTransfer
{
    private readonly int _port;
    private readonly string _destinationDirectory;
    private static readonly object fileLock = new object();

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

        string repoDirectory = Path.Combine(_destinationDirectory, repositoryName);

        if (fileSize == 0)
        {
            // Если файл пустой, просто создаем папку
            Directory.CreateDirectory(repoDirectory);
            Console.WriteLine($"Создана пустая директория {repoDirectory} для репозитория {repositoryName}");
        }
        else
        {
            // Если файл не пустой, продолжаем обычную обработку
            Directory.CreateDirectory(repoDirectory);

            // Создаём папку versions, если её нет
            string versionsDir = Path.Combine(repoDirectory, "versions");
            Directory.CreateDirectory(versionsDir);

            // Определяем следующую версию архива
            string newVersionName = GetNextVersion(versionsDir);
            string receivedFilePath = Path.Combine(versionsDir, newVersionName);

            lock (fileLock)
            {
                File.WriteAllBytes(receivedFilePath, fileData);
            }

            Console.WriteLine($"Файл {fileName} сохранён как {newVersionName} в {versionsDir}");

            ExtractFile(receivedFilePath, repoDirectory);
        }
    }

    // Функция для определения следующего версионного имени
    private string GetNextVersion(string versionsDir)
    {
        int version = 1;
        while (File.Exists(Path.Combine(versionsDir, $"v.{version}.zip")))
        {
            version++;
        }
        return $"v.{version}.zip";
    }

    private void ExtractFile(string zipFilePath, string destinationDirectory)
    {
        string tempDirectory = Path.Combine(destinationDirectory, "TempExtract");
        Directory.CreateDirectory(tempDirectory);

        lock (fileLock)
        {
            ZipFile.ExtractToDirectory(zipFilePath, tempDirectory);
        }
        Console.WriteLine($"Файл {zipFilePath} успешно разархивирован во временную директорию {tempDirectory}");

        foreach (string filePath in Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(tempDirectory, filePath);
            string destinationPath = Path.Combine(destinationDirectory, relativePath);

            lock (fileLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                File.Copy(filePath, destinationPath, overwrite: true);
            }
        }

        lock (fileLock)
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
        Console.WriteLine($"Архив {zipFilePath} успешно разархивирован и заменён в {destinationDirectory}");
    }
}
