namespace SagaOrchestration.StateMap;

public class StateMachineMap : SagaClassMap<OrderStateInstance>
{
    protected override void Configure(EntityTypeBuilder<OrderStateInstance> entity, ModelBuilder model)
    {
        // entity.Property(x => x.CustomerId).HasMaxLength(256);
    }
}
