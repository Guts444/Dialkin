using System.Windows;
using Dialkin.App.Infrastructure;
using Xunit;

namespace Dialkin.Tests;

public sealed class WindowPlacementTests
{
    [Fact]
    public void ClampMovesWindowOutOfGapBetweenMonitors()
    {
        var workAreas = new[]
        {
            new Rect(0, 0, 1920, 1040),
            new Rect(1920, 300, 1280, 720)
        };

        var result = WindowPlacement.ClampToVisibleWorkArea(
            new Rect(2200, 100, 200, 150),
            workAreas);

        Assert.Equal(300, result.Top);
        Assert.Equal(2200, result.Left);
    }

    [Fact]
    public void ClampKeepsWindowInsideMonitorWorkArea()
    {
        var result = WindowPlacement.ClampToVisibleWorkArea(
            new Rect(1850, 1000, 200, 150),
            [new Rect(0, 0, 1920, 1040)]);

        Assert.Equal(1720, result.Left);
        Assert.Equal(890, result.Top);
    }
}
