using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using UpdateExchangeV4.Models;

namespace UpdateExchangeV4.Services
{
    public class WorkerService : BackgroundService
    {
        private static ILogger<WorkerService>? _logger;
        private static IOptions<Scheduled>? _scheduled;
        private static ICoreService? _coreService;

        private StdSchedulerFactory? SchedulerFactory;
        private IScheduler? Scheduler;


        public WorkerService(ILogger<WorkerService> logger, IOptions<Scheduled> scheduled, ICoreService coreService)
        {
            _logger = logger;
            _scheduled = scheduled;
            _coreService = coreService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SchedulerFactory = new StdSchedulerFactory();
            Scheduler = await SchedulerFactory.GetScheduler();
            await Scheduler.Start();

            string message = "UpdateExchange V4 Started ...";
            await _coreService!.WriteTextFile(message);
            _logger!.LogInformation(message);

            CreateTaskScheduler(Scheduler);
        }

        private void CreateTaskScheduler(IScheduler Scheduler)
        {
            IJobDetail FirstRunJob = JobBuilder.Create<GetExchange>().WithIdentity(string.Format("Task First Run")).Build();
            ITrigger FirstRunTrigger = TriggerBuilder.Create()
                                                        .WithIdentity(string.Format("First Run Trigger"))
                                                        .StartNow()
                                                        .Build();
            Scheduler.ScheduleJob(FirstRunJob, FirstRunTrigger);

            IJobDetail Job1 = JobBuilder.Create<GetExchange>().WithIdentity(string.Format("Task 1")).Build();
            ITrigger Trigger1 = TriggerBuilder
                                        .Create()
                                        .WithIdentity(string.Format("Task 1 Trigger"))
                                        .StartNow()
                                        .WithCronSchedule($"0 {_scheduled!.Value.ScheduledTime1.Minute} {_scheduled.Value.ScheduledTime1.Hour} ? * MON-FRI")
                                        .Build();

            IJobDetail Job2 = JobBuilder.Create<GetExchange>().WithIdentity(string.Format("Task 2")).Build();
            ITrigger Trigger2 = TriggerBuilder.Create()
                                                        .WithIdentity(string.Format("Task 2 Trigger"))
                                                        .StartNow()
                                                        .WithCronSchedule($"0 {_scheduled.Value.ScheduledTime2.Minute} {_scheduled.Value.ScheduledTime2.Hour} ? * MON-FRI")
                                                        .Build();

            IJobDetail Job3 = JobBuilder.Create<GetExchange>().WithIdentity(string.Format("Task 3")).Build();
            ITrigger Trigger3 = TriggerBuilder.Create()
                                                        .WithIdentity(string.Format("Task 3 Trigger"))
                                                        .StartNow()
                                                        .WithCronSchedule($"0 {_scheduled.Value.ScheduledTime3.Minute} {_scheduled.Value.ScheduledTime3.Hour} ? * MON-FRI")
                                                        .Build();


            Scheduler.ScheduleJob(Job1, Trigger1);
            Scheduler.ScheduleJob(Job2, Trigger2);
            Scheduler.ScheduleJob(Job3, Trigger3);

            _logger!.LogInformation("CreateTaskScheduler Success");
        }

        public class GetExchange : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                return Task.Run(async () =>
                {
                    string message = $"GetExchabge Trigger Name : {context.Trigger.Key.Name}";
                    await _coreService!.WriteTextFile(message);
                    _logger!.LogInformation(message);

                    try
                    {
                        Exchcurr lsExch = _coreService!.GetExchangeRateFromAPI();
                        GoldPrice lsGold = _coreService.GetGoldPriceFromAPI((double)lsExch.Exchangerate);
                        SilverPrice lsSilver = _coreService.GetSilverPriceFromAPI((double)lsExch.Exchangerate);

                        //
                        // for log check only
                        //
                        _logger!.LogInformation($"BankUpdate : {lsExch.Bankupdate} ");
                        _logger!.LogInformation($"{lsExch.Currency} : {lsExch.Exchangerate:N2}");
                        _logger!.LogInformation($"Gold : {lsGold.GOLD_SELL_PRICE_THB:N2} THB");
                        _logger!.LogInformation($"SilverPrice : {lsSilver.SILVER_SELL_PRICE_THB:N2} THB");

                        // update data to Database
                        await _coreService.UpdateDB(lsExch, lsGold, lsSilver);
                    }
                    catch (Exception ex)
                    {
                        await _coreService!.WriteTextFile(ex.Message);
                        _logger!.LogError(ex.Message.ToString());
                    }
                });
            }
        }
    }
}