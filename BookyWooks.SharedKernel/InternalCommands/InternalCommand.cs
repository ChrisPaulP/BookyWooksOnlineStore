using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.SharedKernel.InternalCommands;

public class InternalCommand
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
    public DateTime EnqueueDate { get; set; }

    public DateTime? ProcessedDate { get; set; }
    public string Error { get; set; } = string.Empty;
}
