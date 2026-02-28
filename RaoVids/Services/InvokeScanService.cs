namespace RaoVids.Services
{
    /// <summary>
    /// A singleton service for invoking a channel scan on demand, e.g. from the UI.
    /// </summary>
    public class InvokeScanService
    {
        /// <summary>
        /// The CancellationTokenSource that's used for cancelling the wait time between scans.
        /// </summary>
        private CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Cancel the current wait time to trigger an immediate scan.
        /// </summary>
        public void InvokeScan()
        {
            // Cancel existing cancellation token source to trigger a rescan (if the scan task is waiting).
            _cts.Cancel();

            // Create new cancellation token source for the next time the scan task needs a token.
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Get the CancellationToken associated with the current wait time, which will be
        /// cancelled when InvokeScan is called.
        /// </summary>
        public CancellationToken GetCancellationToken()
        {
            return _cts.Token;
        }
    }
}
