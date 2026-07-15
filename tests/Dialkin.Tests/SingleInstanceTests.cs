using Dialkin.App.Infrastructure;
using Xunit;

namespace Dialkin.Tests;

public sealed class SingleInstanceTests
{
    [Fact]
    public void OnlyFirstInstanceOwnsTheApplicationName()
    {
        var name = $"Dialkin.Tests.{Guid.NewGuid():N}";

        using var first = new SingleInstanceService(name);
        using var second = new SingleInstanceService(name);

        Assert.True(first.IsPrimaryInstance);
        Assert.False(second.IsPrimaryInstance);
    }
}
