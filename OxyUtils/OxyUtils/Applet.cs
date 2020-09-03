using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyUtils
{
    public class Applet
    {
        private string _name;
        public string Name { get => char.ToUpper(_name[0]) + _name.Substring(1); set => _name = value; }
        public string AppExe { get; set; }
        public string Arguments { get; set; }
        public bool Is64Bits { get; set; }

        public Applet(string name, string appexe, string arguments = "", bool is64bits = false)
        {
            _name = name;
            AppExe = appexe;
            Arguments = arguments;
            Is64Bits = is64bits;
        }

        public override string ToString() => Name;
    }

    internal class MyAppletList : List<Applet>
    { }
}