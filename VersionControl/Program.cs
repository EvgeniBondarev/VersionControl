using System;
using System.IO.Compression;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Core.FileManager;
using Core.CommentManager;

class Program
{

    public static void Main(string[] args)
    {
        FileTransferServer server = new FileTransferServer(5000, @"D:\test\username\Evgen\TestRepo");
        Task.Run(() => server.Start()); // Запускаем сервер в отдельном потоке


        FileSendServer server1 = new FileSendServer(5001, @"D:\test\username\Evgen\TestRepo");
        Task.Run(() => server1.Start());

        CommentServer server2 = new CommentServer(5002, @"D:\test\username\Evgen\tetst.db");
        Task.Run(() => server2.Start());

        Console.ReadLine();
    }
}
