using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.MCPServer.Tools;

internal class BookRecommendationTool
{
    [KernelFunction, Description("Gets a book recommendation based on the genre requested.")]
    public static string GetBookRecommendation(string genre)
    {
        return genre switch
        {
            "Thriller" => "Girl on a Train",
            "Horror" => "The Shining",
            "Romantasy" => "Onyx Storm",
            "Fantasy" => "Game of Thrones",
            "Non-Fiction" => "Atomic Habits",
            _ => "A surprise",
        };
    }
}
