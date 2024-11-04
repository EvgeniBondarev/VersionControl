using Core.FileManager;
using System;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

class Program
{
    public static void Main(string[] args)
    {
        FileTransferClient client = new FileTransferClient("127.0.0.1", 5000);
        client.SendDirectory(@"D:\test\username\repo_name");

        client.RequestDirectory(@"D:\test\username");
    }
}
