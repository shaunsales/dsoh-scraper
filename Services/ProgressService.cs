using System.Collections.Concurrent;
using Spectre.Console;

namespace DsohScraper.Services;

public class ProgressService
{
    private readonly ConcurrentDictionary<string, ProgressTask> _tasks = new();
    private ProgressContext? _context;

    // Progress Service instance for managing progress across the app
    public static ProgressService Instance { get; } = new();

    public async Task RunAsync(Func<Task> action)
    {
        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn(), new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                _context = ctx;
                await action();
            });
        _context = null;
        _tasks.Clear();
    }

    public void AddTask(string taskKey, string displayName)
    {
        if (_context == null)
            return;

        var task = _context.AddTask($"[yellow]{Markup.Escape(displayName)}[/]");

        if (!_tasks.TryAdd(taskKey, task))
            task.StopTask();
    }

    public void UpdateProgress(string taskKey, double progress)
    {
        if (_tasks.TryGetValue(taskKey, out var task))
        {
            task.Value(progress * 100);
        }
    }

    public void CompleteTask(string taskKey)
    {
        if (_tasks.TryGetValue(taskKey, out var task))
        {
            task.StopTask();
        }
    }
}
