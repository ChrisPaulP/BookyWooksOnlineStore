using System.Collections.Specialized;
using BookWooks.OrderApi.Infrastructure.Common.Processing.InternalCommands;
using BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;

using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using TriggerBuilder = Quartz.TriggerBuilder;

namespace BookWooks.OrderApi.Infrastructure.Quartz;
internal static class QuartzStartup
{
  private static IScheduler? _scheduler;

  internal static void Initialize(ILogger logger, long? internalProcessingPoolingInterval)
  {
    logger.LogInformation("Quartz starting...");

    var schedulerConfiguration = new NameValueCollection();
    schedulerConfiguration.Add("quartz.scheduler.instanceName", "Meetings");

    ISchedulerFactory schedulerFactory = new StdSchedulerFactory(schedulerConfiguration);
    _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

    LogProvider.SetCurrentLogProvider(new SerilogLogProvider(logger));

    _scheduler.Start().GetAwaiter().GetResult();

    var processOutboxJob = JobBuilder.Create<ProcessOutboxJob>().Build();

    ITrigger trigger;
    if (internalProcessingPoolingInterval.HasValue)
    {
      trigger =
          TriggerBuilder
              .Create()
              .StartNow()
              .WithSimpleSchedule(x =>
                  x.WithInterval(TimeSpan.FromMilliseconds(internalProcessingPoolingInterval.Value))
                      .RepeatForever())
              .Build();
    }
    else
    {
      trigger =
      TriggerBuilder
              .Create()
              .StartNow()
              .WithCronSchedule("0/2 * * ? * *")
              .Build();
    }

    _scheduler
        .ScheduleJob(processOutboxJob, trigger)
        .GetAwaiter().GetResult();

  

    var processInternalCommandsJob = JobBuilder.Create<ProcessInternalCommandJob>().Build();

    ITrigger triggerCommandsProcessing;
    if (internalProcessingPoolingInterval.HasValue)
    {
      triggerCommandsProcessing =
          TriggerBuilder
              .Create()
              .StartNow()
              .WithSimpleSchedule(x =>
                  x.WithInterval(TimeSpan.FromMilliseconds(internalProcessingPoolingInterval.Value))
                      .RepeatForever())
              .Build();
    }
    else
    {
      triggerCommandsProcessing =
          TriggerBuilder
              .Create()
              .StartNow()
              .WithCronSchedule("0/2 * * ? * *")
              .Build();
    }

    _scheduler.ScheduleJob(processInternalCommandsJob, triggerCommandsProcessing).GetAwaiter().GetResult();

    logger.LogInformation("Quartz started.");
  }

  internal static void StopQuartz()
  {
    _scheduler?.Shutdown();
  }
}
