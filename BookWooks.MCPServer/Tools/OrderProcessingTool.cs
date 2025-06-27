using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.MCPServer.Tools;

public class OrderProcessingTool
{
    [KernelFunction]
    public string PlaceOrder(string itemName)
    {
        return "success";
    }

    /// <summary>
    /// Executes a refund for the specified item.
    /// </summary>
    /// <param name="itemName">The name of the item to be refunded.</param>
    /// <returns>A string indicating the result of the refund execution.</returns>
    [KernelFunction]
    public string ExecuteRefund(string itemName)
    {
        return "success";
    }
}
