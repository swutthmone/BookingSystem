using ASPNETCoreScheduler.BackgroundService;
using BookingSystem.Operational;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASPNETCoreScheduler.Scheduler
{
    public abstract class ScheduledProcessor : ScopedProcessor
    {
        private CrontabSchedule _schedule;
        private DateTime _nextRun;
        protected abstract string Schedule { get; }
        public ScheduledProcessor(IServiceScopeFactory serviceScopeFactory, ILoggerFactory DepLoggerFactory) : base(serviceScopeFactory)
        {
            try
            {
                var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                var Configuration = appsettingbuilder.Build();
                var CronSchedule =  Configuration[Schedule];

                Log._logger = DepLoggerFactory.CreateLogger(Schedule);

                CrontabSchedule.ParseOptions opt = new CrontabSchedule.ParseOptions();
                opt.IncludingSeconds = true;
                _schedule = CrontabSchedule.Parse(CronSchedule, opt);
                _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ScheduledProcessor "+DateTime.Now+ex.Message + " " + ex.StackTrace);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                do
                {
                    var now = DateTime.Now;
                    var nextrun = _schedule.GetNextOccurrence(now);
                    if (now > _nextRun)
                    {
                        await Process();
                        _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                    }
                    await Task.Delay(100, stoppingToken); //.. seconds delay
                    //Thread.Sleep(100);
                }
                while (!stoppingToken.IsCancellationRequested);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ExecuteAsync "+DateTime.Now+ex.Message+" "+ex.StackTrace);
            }  
        }
    }
}
