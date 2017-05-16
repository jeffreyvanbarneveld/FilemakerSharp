using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilemakerSharp
{
    public class FilemakerException : Exception
    {

        public FilemakerException(string message)
            : base(message) { }
    }
}
