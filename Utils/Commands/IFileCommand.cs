using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Commands
{
    // Интерфейс для команд, поддерживающих передачу файлов
    public interface IFileCommand : ICommand
    {
        string Execute(string[] args, Dictionary<string, byte[]> files);
    }
}
