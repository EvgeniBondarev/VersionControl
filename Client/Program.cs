using Core.CommandManager;
using Core.CommandManager.Commands;
using Core.FileManager;


class Program
{
    public static void Main(string[] args)
    {
        FileTransferClient client = new FileTransferClient("127.0.0.1", 5000);
        //client.SendDirectory(@"D:\test\username\repo_name", "repo111");

        //Thread.Sleep(1000);

        FileReceiveClient client2 = new FileReceiveClient("127.0.0.1", 5001);
        //client2.ReceiveDirectory(@"D:\test\username", "repo111", 1);

        CommandRegistry commandRegistry = new CommandRegistry();

        commandRegistry.RegisterCommand(new CreateRepoCommand(client));
        commandRegistry.RegisterCommand(new AddFileCommand(client));
        commandRegistry.RegisterCommand(new GetFileCommand(client2));

        Console.WriteLine(">> create `repo name`");
        Console.WriteLine(">> add `user directory` `repo name`");
        Console.WriteLine(">> get `user directory` `repo name`");
        Console.WriteLine(">> get `user directory` `repo name` `version(int)`");

        while (true)
        {
            Console.Write(">> ");
            string commandInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(commandInput))
            {
                continue;
            }

            var parts = commandInput.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string commandName = parts[0];
            if (commandName == "exit")
            {
                Environment.Exit(0);
            }
            string[] arg = parts.Length > 1 ? parts[1].Split(' ') : Array.Empty<string>();
            commandRegistry.ExecuteCommand(commandName, arg);
        }
    }
}
