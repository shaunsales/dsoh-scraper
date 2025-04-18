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

    public void AddTask(string trackTitle)
    {
        if (_context == null || _tasks.ContainsKey(trackTitle))
            return;
        
        var task = _context.AddTask($"[yellow]{trackTitle}[/]");
        
        _tasks[trackTitle] = task;
    }

    public void UpdateProgress(string trackTitle, double progress)
    {
        if (_tasks.TryGetValue(trackTitle, out var task))
        {
            task.Value(progress * 100);
        }
    }

    public void CompleteTask(string trackTitle)
    {
        if (_tasks.TryGetValue(trackTitle, out var task))
        {
            task.StopTask();
        }
    }
}
