using System;

namespace RealmCore.Server.Modules.Server;

internal sealed class ExecuteTaskJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var innerJob = (Func<Task>)context.JobDetail.JobDataMap["job"];
        await innerJob();
    }
}

public sealed class ScheduledJob
{
    private readonly IScheduler _scheduler;
    private readonly string _jobId;
    private readonly CancellationToken _cancellationToken;

    internal ScheduledJob(IScheduler scheduler, string jobId, CancellationToken cancellationToken)
    {
        _scheduler = scheduler;
        _jobId = jobId;
        _cancellationToken = cancellationToken;
    }

    public void Reschedule(TimeSpan every)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(_jobId)
            .WithSimpleSchedule(x => x
                .WithInterval(every)
                .RepeatForever())
            .Build();

        _scheduler.RescheduleJob(new TriggerKey(_jobId), trigger).Wait();
    }
}

public sealed class ScheduledJobAt
{
    private readonly IScheduler _scheduler;
    private readonly string _jobId;
    private readonly CancellationToken _cancellationToken;

    internal ScheduledJobAt(IScheduler scheduler, string jobId, CancellationToken cancellationToken)
    {
        _scheduler = scheduler;
        _jobId = jobId;
        _cancellationToken = cancellationToken;
    }

    public void Reschedule(DateTime at)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(_jobId)
            .StartAt(at)
            .Build();

        _scheduler.RescheduleJob(new TriggerKey(_jobId), trigger).Wait();
    }
}

public sealed class CronScheduledJob
{
    private readonly IScheduler _scheduler;
    private readonly string _jobId;
    private readonly CancellationToken _cancellationToken;

    internal CronScheduledJob(IScheduler scheduler, string jobId, CancellationToken cancellationToken)
    {
        _scheduler = scheduler;
        _jobId = jobId;
        _cancellationToken = cancellationToken;
    }

    public void Reschedule(string cronExpression)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(_jobId)
            .WithCronSchedule(cronExpression)
            .Build();

        _scheduler.RescheduleJob(new TriggerKey(_jobId), trigger).Wait();
    }
}

public interface ISchedulerService
{
    ScheduledJob ScheduleJob(Func<Task> job, TimeSpan every, CancellationToken cancellationToken = default);
    ScheduledJobAt ScheduleJobAt(Func<Task> job, DateTime dateTime, CancellationToken cancellationToken = default);
    CronScheduledJob ScheduleJobAt(Func<Task> job, string cronExpression, CancellationToken cancellationToken = default);
    void ScheduleJobOnce(Func<Task> job, TimeSpan delay, CancellationToken cancellationToken = default);
    internal Task StartAsync();
}

internal sealed class SchedulerService : ISchedulerService
{
    private IScheduler? _scheduler;
    private readonly StdSchedulerFactory _stdSchedulerFactory;
    public SchedulerService()
    {
        _stdSchedulerFactory = new();
    }

    public async Task StartAsync()
    {
        _scheduler = await _stdSchedulerFactory.GetScheduler();
        await _scheduler.Start();
    }

    public ScheduledJob ScheduleJob(Func<Task> job, TimeSpan every, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null)
            throw new InvalidOperationException();

        var jobId = Guid.NewGuid().ToString();
        IJobDetail jobDetail = JobBuilder.Create<ExecuteTaskJob>()
            .SetJobData(new JobDataMap
            {
                ["job"] = job,
            })
            .WithIdentity(jobId)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(jobId)
            .WithSimpleSchedule(x => x
                .WithInterval(every)
                .RepeatForever())
            .Build();

        _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).Wait(cancellationToken);

        cancellationToken.Register(() =>
        {
            _scheduler.UnscheduleJob(trigger.Key).Wait();
        });

        return new ScheduledJob(_scheduler, jobId, cancellationToken);
    }
    
    public void ScheduleJobOnce(Func<Task> job, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null)
            throw new InvalidOperationException();

        var jobId = Guid.NewGuid().ToString();
        IJobDetail jobDetail = JobBuilder.Create<ExecuteTaskJob>()
            .SetJobData(new JobDataMap
            {
                ["job"] = job,
            })
            .WithIdentity(jobId)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(jobId)
            .StartAt(DateTimeOffset.Now + delay)
            .Build();

        _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).Wait(cancellationToken);

        cancellationToken.Register(() =>
        {
            _scheduler.UnscheduleJob(trigger.Key).Wait();
        });
    }

    public ScheduledJobAt ScheduleJobAt(Func<Task> job, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null)
            throw new InvalidOperationException();

        var jobId = Guid.NewGuid().ToString();
        IJobDetail jobDetail = JobBuilder.Create<ExecuteTaskJob>()
            .SetJobData(new JobDataMap
            {
                ["job"] = job,
            })
            .WithIdentity(jobId)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(jobId)
            .StartAt(dateTime)
            .Build();

        _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).Wait(cancellationToken);

        cancellationToken.Register(() =>
        {
            _scheduler.UnscheduleJob(trigger.Key).Wait();
        });

        return new ScheduledJobAt(_scheduler, jobId, cancellationToken);
    }

    public CronScheduledJob ScheduleJobAt(Func<Task> job, string cronExpression, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null)
            throw new InvalidOperationException();

        var jobId = Guid.NewGuid().ToString();
        IJobDetail jobDetail = JobBuilder.Create<ExecuteTaskJob>()
            .SetJobData(new JobDataMap
            {
                ["job"] = job,
            })
            .WithIdentity(jobId)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(jobId)
            .WithCronSchedule(cronExpression)
            .Build();

        _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).Wait(cancellationToken);

        cancellationToken.Register(() =>
        {
            _scheduler.UnscheduleJob(trigger.Key).Wait();
        });

        return new CronScheduledJob(_scheduler, jobId, cancellationToken);
    }
}
