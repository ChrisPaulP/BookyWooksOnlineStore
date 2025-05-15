

namespace BookyWooks.SharedKernel.Messages;

public interface IInboxDbContext
{
    public DbSet<InboxMessage> InboxMessages { get; set; }
}
