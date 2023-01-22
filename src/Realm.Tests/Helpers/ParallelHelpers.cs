namespace Realm.Tests.Helpers;

public static class ParallelHelpers
{
    public static async Task Run(Action action, int threads = 8, int times = 100)
    {
        var tasks = Enumerable.Range(0, threads).Select(x =>
            Task.Run(() =>
            {
                for (int i = 0; i < times; i++)
                {
                    action();
                }
            })
        );

        await Task.WhenAll(tasks);
    }

    public static async Task Run(Action<int, int> action, int threads = 8, int times = 100)
    {
        var tasks = Enumerable.Range(0, threads).Select(x =>
            Task.Run(() =>
            {
                for (int i = 0; i < times; i++)
                {
                    action(x, i);
                }
            })
        );

        await Task.WhenAll(tasks);
    }
}
