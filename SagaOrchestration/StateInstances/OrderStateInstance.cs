namespace SagaOrchestration.StateInstances;

public class OrderStateInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal OrderTotal { get; set; }
    public DateTime CreatedDate { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        GetType().GetProperties().ToList().ForEach(p =>
        {
            var value = p.GetValue(this, null);
            sb.AppendLine($"{p.Name}: {value}");
        });

        sb.Append("------------------------");
        return sb.ToString();
    }
}
