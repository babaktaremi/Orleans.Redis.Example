// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Persistence;
using StackExchange.Redis;
using StackExchange.Redis.Configuration;

try
{
    using IHost host = await StartSiloAsync();
    Console.WriteLine("\n\n Press Enter to terminate...\n\n");
    Console.ReadLine();

    await host.StopAsync();

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return 1;
}

static async Task<IHost> StartSiloAsync()
{
    var builder = Host
        .CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddOptions();
        })
        .UseOrleans((context, silo) =>
        {
            var activeSilos = GetActiveSilos();

            silo
                .UseRedisClustering(options =>
                {
                    options.ConfigurationOptions = new ConfigurationOptions()
                    {
                        EndPoints = new EndPointCollection()
                    };

                    options.ConfigurationOptions.EndPoints.Add("localhost:2020");
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = $"orleans.Redis";
                    options.ServiceId = $"orleans.Service";
                })
                .AddRedisGrainStorage("Redis", options =>
                {
                    options.ConnectionString = "localhost:2020";
                    options.DatabaseNumber = 1;
                })
                .ConfigureLogging(logging => logging.AddConsole());
               

            silo.ConfigureEndpoints(IPAddress.Loopback, 11111 + activeSilos, 30000 + activeSilos);

        }).ConfigureServices(services =>
        {
        });

    var host = builder.Build();
    await host.StartAsync();

    return host;
}

static int GetActiveSilos()
{
    var connectionString = "localhost:2020";
    var redis = ConnectionMultiplexer.Connect(connectionString);
    var db = redis.GetDatabase();

    // Replace 'orleans_membership_key' with the actual key used by Orleans
    var membershipKey = "orleans.Service/members/orleans.Redis";
    var members =  db.HashGetAll(membershipKey);

    var activeSilos = members
        .Count(IsSiloActive);

    Console.WriteLine($"Number of active silos: {activeSilos}");

    return activeSilos;
}


static bool IsSiloActive(HashEntry siloData)
{

    if (siloData.Equals(default(HashEntry)))
    {
        return false; 
    }

    try
    {
        var siloDataModel = JsonSerializer.Deserialize<RedisSiloModel>(siloData.Value.ToString());

        if (siloDataModel is null)
            return false;

        return siloDataModel.Status == 3;
    }
    catch (Exception e)
    {
        return false;
    }
}


public class RedisSiloModel
{
    public SiloAddressModel SiloAddress { get; set; }
    public int Status { get; set; }
    public List<object> SuspectTimes { get; set; }
    public int ProxyPort { get; set; }
    public string HostName { get; set; }
    public string SiloName { get; set; }
    public string RoleName { get; set; }
    public int UpdateZone { get; set; }
    public int FaultZone { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime IAmAliveTime { get; set; }
}

public class SiloAddressModel
{
    public string SiloAddress { get; set; }
}