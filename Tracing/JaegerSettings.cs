using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracing;

public class JaegerSettings
{
    public string Protocol { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
}
