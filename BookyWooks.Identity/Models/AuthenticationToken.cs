namespace BookyWooks.Identity.Models;

public record AuthenticationToken(string Token, int ExpiresIn);
