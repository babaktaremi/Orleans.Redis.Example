using Orleans.Statistics;
using System.Diagnostics;

namespace Silo.Hosting;

public class HostEnvironmentStatistics:IHostEnvironmentStatistics
{
    public long? TotalPhysicalMemory => 24000000000;

    public float? CpuUsage =>
        (float)Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;

    public long? AvailableMemory => 24000000000 - 8000000000 - Process.GetCurrentProcess().WorkingSet64;
}