using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager.Commands
{
    public interface ICommand
    {
        string Name { get; }
        void Execute(string[] args);
    }

}
