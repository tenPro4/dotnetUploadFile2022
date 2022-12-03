namespace DotnetUploadFile2022.BackgroundTasks
{
    public class QueueService : BackgroundService
    {
        private IBackgroundQueue _queue;

        public QueueService(IBackgroundQueue queue)
        {
            _queue = queue;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var task = await _queue.PopQueue(stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                using (var source = new CancellationTokenSource())
                {
                    source.CancelAfter(TimeSpan.FromMinutes(1));
                    var timeoutToken = source.Token;

                    await task(timeoutToken);
                }
            }
        }
    }
}
