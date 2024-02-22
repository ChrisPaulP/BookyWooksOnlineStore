using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.TestContainersIntegrationTests;

[CollectionDefinition("Order Test Collection")]
public class SharedTestCollection : ICollectionFixture<OrderApiApplicationFactory<Program>> //, IAsyncLifetime
{
    /// This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
