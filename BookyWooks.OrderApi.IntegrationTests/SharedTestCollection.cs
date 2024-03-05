using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.OrderApi.IntegrationTests;

[CollectionDefinition("Order Test Collection")]
public class SharedTestCollection : ICollectionFixture<OrderApiApplicationTestFactory<Program>> //, IAsyncLifetime
{
    /// This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
