using Core.CommandManager.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public void RegisterCommand(ICommand command)
        {
            _commands[command.Name] = command;
        }

        public void ExecuteCommand(string commandName, string[] args)
        {
            if (_commands.TryGetValue(commandName, out var command))
            {
                command.Execute(args);
            }
            else
            {
                Console.WriteLine($"Command '{commandName}' not found.");
            }
        }
    }
}
