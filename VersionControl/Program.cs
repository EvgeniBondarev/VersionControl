using System;
using System.IO.Compression;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Core.FileManager;

class Program
{

    public static void Main(string[] args)
    {
        FileTransferServer server = new FileTransferServer(5000, @"D:\test\username\Evgen\TestRepo");
        Task.Run(() => server.Start()); // Запускаем сервер в отдельном потоке

        Thread.Sleep(1000);

        FileSendServer server1 = new FileSendServer(5001, @"D:\test\username\Evgen\TestRepo");
        Task.Run(() => server1.Start());

        Console.ReadLine();
    }
}
