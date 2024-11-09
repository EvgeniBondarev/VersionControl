using Core.FileManager;


class Program
{
    public static void Main(string[] args)
    {
        FileTransferClient client = new FileTransferClient("127.0.0.1", 5000);
        client.SendDirectory(@"D:\test\username\repo_name", "repo1");

        Thread.Sleep(1000);

        FileReceiveClient client2 = new FileReceiveClient("127.0.0.1", 5001);
        client2.ReceiveDirectory(@"D:\test\username", "repo1", 1);
    }
}
