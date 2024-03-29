﻿using Microsoft.EntityFrameworkCore;
using PowerfulTimer.Api.Entities;
using Timer = PowerfulTimer.Api.Entities.Timer;

namespace PowerfulTimer.Api.Data;

public class PowerfulTimerContext : DbContext
{
    public DbSet<Timer> Timers { get; set; }

    public PowerfulTimerContext() { }

    public PowerfulTimerContext(DbContextOptions<PowerfulTimerContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PowerfulTimerContext).Assembly);
    }
}
