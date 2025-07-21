using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracing;

public class JaegerSettings
{
    public string Protocol { get; init; } = "http";
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 4317;

    public static JaegerSettings Default => new JaegerSettings();
}
