namespace Dialkin.App.Infrastructure;

public sealed class SingleInstanceService : IDisposable
{
    private readonly Mutex? _mutex;
    private bool _ownsMutex;

    public SingleInstanceService(string applicationName)
    {
        try
        {
            _mutex = new Mutex(initiallyOwned: true, applicationName, out var createdNew);
            _ownsMutex = createdNew;
            IsPrimaryInstance = createdNew;
        }
        catch (UnauthorizedAccessException)
        {
            // Failing open is safer than preventing the app from launching entirely.
            IsPrimaryInstance = true;
        }
    }

    public bool IsPrimaryInstance { get; }

    public void Dispose()
    {
        if (_ownsMutex)
        {
            try
            {
                _mutex?.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // The mutex was already released or ownership changed during shutdown.
            }

            _ownsMutex = false;
        }

        _mutex?.Dispose();
    }
}
