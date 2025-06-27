using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.MCPServer.Tools;

public class ReservationTool
{
    [KernelFunction, Description("Reserves stock for the requested book based on the genere selected")]
    public static string ReserveStock(string bookTitle)
    {
        return $"Thank you for your order. {bookTitle} has been reserved";
    }
}
