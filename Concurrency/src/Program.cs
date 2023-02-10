using System.Diagnostics;
using System.Numerics;

namespace concurrency
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Scenario_CaptureAndAssessSyncrhonizationContext();

            //Task<int> taskStatuses = AssessStatusesAsync();
            //await taskStatuses;
            //Console.WriteLine(taskStatuses.Status);

            //await Scenario_ExceptionHandlingBehaviorAsync();

            //await Scenario_MultipleConcurrentTasksAsync();

            //await Scenario_WaitingTheFirstTaskCompleted();

            //await Scenario_WaitingTheFirstTaskCompleted_SimpleCancellationToken();

            //await Scenario_WaitingTheFirstTaskCompleted_CancellationViaTimeout();

            //await Scenario_CapturingContext();
            //await Scenario_PoorlyWrittenRetryAsync_From_Origence();

            Console.WriteLine("Program completed, press ENTER to finish...");
            Console.ReadLine();
        }

        private static void Scenario_CaptureAndAssessSyncrhonizationContext()
        {
            var sctxGlobal = SynchronizationContext.Current;

            // What if, Winform or WPF or Xamarin, or MAUI?
            var sc1 = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private static async Task<int> AssessStatusesAsync()
        {
            //simulating cancellation using a token
            await Task.Delay(TimeSpan.FromSeconds(3), new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

            return await Task.FromResult(10);
            //return 10;
        }

        private static async Task Scenario_MultipleConcurrentTasksAsync()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // scenario 1: naive concurrent approach, really? Assess total time elapsed
            //await DoSomethingAsync(1);
            //await DoSomethingAsync(5);
            //await DoSomethingAsync(3);



            // scenario 2: real concurrent processing deferring to tasks (awaitables), Assess total time elapsed
            //Task<int> t1 = DoSomethingAsync(1);
            //Task<int> t2 = DoSomethingAsync(5);
            //Task<int> t3 = DoSomethingAsync(3);


            // scenario 2a: WHEN awaiting all individually, what task takes the total time?
            //await Task.WhenAll(t1,t2,t3);


            // scenario 2b: WHEN using an array of tasks (awaitables), Assess total time elapsed
            //Task[] allTasks = { t1, t2, t3 };
            //await Task.WhenAll(allTasks);


            // observation between 2a and 2b
            // Comment: Avoid using LINQ when creating an array of tasks, may loose context info such has exception info due to a sync unwrapping
            // I.e. using Select(x=> ...) and then .ToArray()

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
        }

        private static async Task<int> DoSomethingAsync(int delay)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            Console.WriteLine($"Completed: {delay}");
            return delay;
        }

        private static async Task Scenario_PoorlyWrittenRetryAsync_From_Origence()
        {
            await Task.Delay(1000);

            int retryCounter = 0;
            int retryMax = 3;
            bool status = await FakeDownloadAsync(retryCounter); ;
            while (retryCounter < retryMax && status.Equals(false))
            {
                await Task.Delay(1000);
                Console.WriteLine($"===> Retry #{retryCounter}");
                try
                {
                    status = await FakeDownloadAsync(retryCounter);
                }
                catch (Exception ex)
                {
                    var s = ex.Message;
                }
                retryCounter++;
            }
        }

        private static async Task<bool> FakeDownloadAsync(int someParam)
        {
            Console.WriteLine("===> FakeDownloadAsync: started");
            await Task.Delay(3000);

            if (someParam == 1) throw new InvalidOperationException("Fake ex");

            Console.WriteLine("===> FakeDownloadAsync: completed");
            return someParam == 1;
        }

        private static async Task Scenario_CapturingContext()
        {
            var sctxLocal = SynchronizationContext.Current;
            var sc2 = TaskScheduler.FromCurrentSynchronizationContext();

            // Code here runs in the original context.
            var t = Task.FromResult(1);
            await t;
            // Code here runs in the original context.
            await Task.FromResult(1).ConfigureAwait(continueOnCapturedContext: false);
            // Code here runs in the original context.
            var random = new Random();
            int delay = random.Next(2); // Delay is either 0 or 1
            await Task.Delay(delay).ConfigureAwait(continueOnCapturedContext: false);
            // Code here might or might not run in the original context.
            // The same is true when you await any Task
            // that might complete very quickly.
        }

        private static async Task Scenario_WaitingTheFirstTaskCompleted()
        {
            // set breakpoint below
            Task<int> t1 = FakeMethodAsync(5);
            Task<int> t2 = FakeMethodAsync(2);
            Task<int> t3 = FakeMethodAsync(4);


            // first: await the task
            Task<int> whichTask = await Task.WhenAny(t1, t2, t3);

            int theResult = await whichTask;

            // second: inspect the result... or, results? WTF?
            Console.WriteLine($"This is the result: {theResult}");

            // Discussion: Task.WhenAny() when this method returns the first completed task,
            // the other tasks <will continue running until completion>,
            // even any of them completed in the Canceled or Faulted state.
            // If that behavior is not desired you may want to cancel all the remaining tasks once the first task complete
        }

        private static async Task<int> FakeMethodAsync(int delay)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            Console.WriteLine($"Completed: {delay}");
            return delay;
        }


        private static async Task Scenario_WaitingTheFirstTaskCompleted_SimpleCancellationToken()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // set breakpoint below
            Task<int> t1 = FakeMethodAsync_Token(5, cancellationTokenSource.Token);
            Task<int> t2 = FakeMethodAsync_Token(2, cancellationTokenSource.Token);
            Task<int> t3 = FakeMethodAsync_Token(4, cancellationTokenSource.Token);

            // first: await the task
            Task<int> whichTask = await Task.WhenAny(t1, t2, t3);

            // second: issue a token cancellation
            cancellationTokenSource.Cancel();

            // inspect the result
            int theResult = await whichTask;
            Console.WriteLine($"This is the result: {theResult}");
        }

        private static async Task Scenario_WaitingTheFirstTaskCompleted_CancellationViaTimeout()
        {
            // using the constructor
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // or, declaratively
            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            //cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));
            //CancellationToken cancellationToken = cancellationTokenSource.Token;

            // set breakpoint below
            Task<int> t1 = FakeMethodAsync_Token(5, cancellationToken);
            Task<int> t2 = FakeMethodAsync_Token(2, cancellationToken); //test with 3 secs and observe the result
            Task<int> t3 = FakeMethodAsync_Token(4, cancellationToken);

            // first: await the task
            Task<int> whichTask = await Task.WhenAny(t1, t2, t3);

            // inspect the result
            int theResult = await whichTask;
            Console.WriteLine($"This is the result: {theResult}");
        }

        private static async Task<int> FakeMethodAsync_Token(int delay, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
            Console.WriteLine($"Completed: {delay}");
            return delay;
        }

        private static async Task Scenario_ExceptionHandlingBehaviorAsync()
        {
            // Simple try/catch and exception handling
            try
            {
                await FakeMethodAsync();
                await ThrowFakeExceptionAsync();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // task maintains the exception
            Task<int> fakeTask = FakeMethodAsync();
            Task fakeExTask = ThrowFakeExceptionAsync();
            try
            {
                await fakeTask; // ranToCompletion
                await fakeExTask; // awaiting a faulted task, might be useful depending on the scenario
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            // Experiment with an aggregate exception= t1 ex + t2 ex
            //catch (AggregateException aggEx)
            //{
            //    Debug.WriteLine(aggEx.Message);
            //}
        }

        private static async Task ThrowFakeExceptionAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            throw new InvalidOperationException("***my custom exception ***");
        }

        private static async Task<int> FakeMethodAsync() //observe here the 'error' hint
        {
            return 10;
            //return await Task.FromResult(10);
        }

    }
}