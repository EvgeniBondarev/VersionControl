using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string Execute(string[] args);
    }

}
