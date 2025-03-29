﻿using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Tasks
{
    public class EnrollmentStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public EnrollmentStatusUpdater(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ECourseContext>();

                    var enrollments = await context.Enrollments
                        .Where(e => e.ExpiredDate.HasValue &&
                                    e.ExpiredDate.Value < DateTime.Now &&
                                    e.Progress < 100)
                        .ToListAsync(stoppingToken);

                    foreach (var enrollment in enrollments)
                    {
                        enrollment.Status = 2; // Update status to 2
                    }

                    if (enrollments.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                // Wait for a certain time before checking again (e.g., 1 day)
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
