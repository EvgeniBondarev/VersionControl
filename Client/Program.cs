using Core.CommandManager;
using Core.CommandManager.Commands;
using Core.CommandManager.Commands.Implementation;
using Core.FileManager;


class Program
{
    public static void Main(string[] args)
    {
        FileTransferClient client = new FileTransferClient("127.0.0.1", 5000);
        FileReceiveClient client2 = new FileReceiveClient("127.0.0.1", 5001);

        CommandRegistry commandRegistry = new CommandRegistry();

        string userName = GetUserInput();

        while (true)
        {
            RegisterCommands(commandRegistry, client, client2, userName);
            PrintInstructions();

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

                if (commandName.Equals("User", StringComparison.OrdinalIgnoreCase))
                {
                    if (parts.Length > 1)
                    {
                        userName = parts[1];
                        Console.Clear();
                        break; 
                    }
                    else
                    {
                        Console.WriteLine(">> Please provide a new user name.");
                        continue;
                    }
                }

                if (commandName.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.Exit(0);
                }

                string[] argsArray = parts.Length > 1 ? parts[1].Split(' ') : Array.Empty<string>();
                commandRegistry.ExecuteCommand(commandName, argsArray);
            }
        }
    }

    private static string GetUserInput()
    {
        Console.Write(">> User: ");
        return Console.ReadLine();
    }

    private static void RegisterCommands(CommandRegistry commandRegistry,
                                         FileTransferClient client, 
                                         FileReceiveClient client2, 
                                         string userName)
    {
        commandRegistry.ClearCommands(); 
        commandRegistry.RegisterCommand(new CreateRepoCommand(client, userName));
        commandRegistry.RegisterCommand(new AddFileCommand(client, userName));
        commandRegistry.RegisterCommand(new GetFileCommand(client2, userName));
    }

    private static void PrintInstructions()
    {
        Console.WriteLine(">> Available commands:");
        Console.WriteLine(">> create `repo name`");
        Console.WriteLine(">> add `user directory` `repo name`");
        Console.WriteLine(">> get `user directory` `repo name`");
        Console.WriteLine(">> get `user directory` `repo name` `version(int)`");
        Console.WriteLine(">> Change user: User `new user name`");
        Console.WriteLine(">> Exit: exit");
    }

}
