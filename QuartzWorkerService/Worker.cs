using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using QuartzWorkerService.Jobs;

namespace QuartzWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration configuration;
        private IScheduler scheduler;
        private ISchedulerFactory schedulerFactory;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            configuration = builder.Build();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var container = services.BuildServiceProvider();
            var jobFactory = new JobFactory(container);
            schedulerFactory = new StdSchedulerFactory();
            scheduler = await schedulerFactory.GetScheduler();
            scheduler.JobFactory = jobFactory;
            await scheduler.Start();

            await ConfigureJobs();
            await WaitForCancelAsync(stoppingToken);

        }

        // What is a better way to implement this?
        private async Task WaitForCancelAsync(CancellationToken stoppingToken)
        {
            
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    await Task.Delay(1000, stoppingToken);
            //}
            try 
            {
                await Task.Delay(-1, stoppingToken);
            }
            catch(TaskCanceledException ex)
            {
                // Start shutdown process
            }
            
        }

        private async Task ConfigureJobs()
        {
            IJobDetail dataRetrievalJob = JobBuilder
                .Create<DataRetrievalJob>()
                .WithIdentity("MyDataRetrievalJob", "MyJobGroup")
                .Build();

            ITrigger dataRetrievalJobTrigger = TriggerBuilder.Create()
                .WithIdentity("MyTrigger", "MyJobGroup")
                .WithCronSchedule(configuration.GetValue<string>("myCron"))
                .Build();

            await scheduler.ScheduleJob(dataRetrievalJob, dataRetrievalJobTrigger);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine(configuration.GetValue<string>("testString"));
            services.AddSingleton<RestApiEmployeeRepository>();
            services.AddTransient<DataRetrievalJob>();
            //services.AddTransient<EFEmployeeRepository>();
        }
    }
}
