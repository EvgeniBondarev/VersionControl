using Core.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager.Commands
{
    public class GetFileCommand : ICommand
    {
        private FileReceiveClient _fileReceiveClient;
        public string Name => "get";

        public GetFileCommand(FileReceiveClient fileReceiveClient)
        {
            _fileReceiveClient = fileReceiveClient;
        }

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Repository name is required.");
                return;
            }

            string userDirectory = args[0];
            string repoName = args[1];
            int? version = null;
            if (args.Length > 2)
            {
                version = Int32.Parse(args[2]);
            }
            
            try
            {
                _fileReceiveClient.ReceiveDirectory(userDirectory, repoName, version);
                Console.WriteLine($"Files were copied to the directory `{userDirectory}` from {repoName}" + 
                                    version != null ? $" v.{version}" : "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository created error: {ex.Message}.");
            }

        }
    }
}
