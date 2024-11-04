using Utils.Commands;
using System.Collections.Generic;

namespace Utils
{
    public class CommandProcessor
    {
        private readonly Dictionary<string, ICommand> _commands = new();
        private readonly Dictionary<string, IFileCommand> _fileCommands = new();

        public void RegisterCommand(ICommand command)
        {
            if (!_commands.ContainsKey(command.Name))
            {
                _commands[command.Name] = command;
            }
        }

        // Регистрация команд, поддерживающих файлы
        public void RegisterFileCommand(IFileCommand fileCommand)
        {
            if (!_fileCommands.ContainsKey(fileCommand.Name))
            {
                _fileCommands[fileCommand.Name] = fileCommand;
            }
        }

        public string ProcessCommand(string commandInput)
        {
            var parts = commandInput.Split(' ', 2);
            var commandName = parts[0];
            var args = parts.Length > 1 ? parts[1].Split(' ') : Array.Empty<string>();

            if (_commands.TryGetValue(commandName, out var command))
            {
                return command.Execute(args);
            }
            return $"Unknown command: {commandName}";
        }

        // Перегруженный метод для команд с файлами
        public string ProcessFileCommand(string commandName, string[] args, Dictionary<string, byte[]> files)
        {
            if (_fileCommands.TryGetValue(commandName, out var fileCommand))
            {
                return fileCommand.Execute(args, files);
            }
            return $"Unknown file command: {commandName}";
        }
    }
}
