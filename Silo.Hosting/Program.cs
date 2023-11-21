using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence;
using Orleans.Runtime;
using StackExchange.Redis;
using StackExchange.Redis.Configuration;
using Orleans.Statistics;
using Silo.Hosting;

try
{
    using IHost host = await StartSiloAsync(args);
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

static async Task<IHost> StartSiloAsync(string[] args)
{
    var builder = Host
        .CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddOptions();
        })
        .UseOrleans((context, silo) =>
        {
            (int siloPort, int gatewayPort) = DetermineHostPortsBasedOnArgs(args);

            silo
                .UseRedisClustering(options =>
                {
                    options.ConfigurationOptions = new ConfigurationOptions()
                    {
                        EndPoints = new EndPointCollection { "localhost:2020" }
                    };
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
                .ConfigureLogging(logging => logging.AddConsole())
                .Configure<GrainCollectionOptions>(options =>
                {
                    options.ActivationTimeout=TimeSpan.FromSeconds(30);
                    options.DeactivationTimeout=TimeSpan.FromSeconds(30);
                    options.CollectionAge=TimeSpan.FromHours(2);
                });
            silo.ConfigureEndpoints(IPAddress.Loopback, siloPort,gatewayPort);

        }).ConfigureServices(services =>
        {
            services.AddSingleton<PlacementStrategy, HashBasedPlacement>();
           // services.AddSingleton<IHostEnvironmentStatistics, HostEnvironmentStatistics>();
        });
    var host = builder.Build();
    await host.StartAsync();
    return host;
}

static (int siloPort, int gatewayPort) DetermineHostPortsBasedOnArgs(string[] args)
{
    string port = null;
    string gateway = null;

    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "-port" && i + 1 < args.Length)
        {
            port = args[i + 1];
        }
        else if (args[i] == "-gateway" && i + 1 < args.Length)
        {
            gateway = args[i + 1];
        }
    }

    var siloPort = 11111;
    var gatewayPort = 33333;

    if(!string.IsNullOrEmpty(port)&&!int.TryParse(port,out siloPort))
        Console.WriteLine("Invalid port declaration. setting default port to 11111");

    if(!string.IsNullOrEmpty(gateway) && !int.TryParse(gateway,out gatewayPort))
        Console.WriteLine("Invalid gateway declaration. setting default gateway to 33333");

    return (siloPort, gatewayPort);
}


//static int GetActiveSilos()
//{
//    var connectionString = "localhost:2020";
//    var redis = ConnectionMultiplexer.Connect(connectionString);
//    var db = redis.GetDatabase();

//    // Replace 'orleans_membership_key' with the actual key used by Orleans
//    var membershipKey = "orleans.Service/members/orleans.Redis";
//    var members =  db.HashGetAll(membershipKey);

//    var activeSilos = members
//        .Count(IsSiloActive);

//    Console.WriteLine($"Number of active silos: {activeSilos}");

//    return activeSilos;
//}


//static bool IsSiloActive(HashEntry siloData)
//{

//    if (siloData.Equals(default(HashEntry)))
//    {
//        return false; 
//    }

//    try
//    {
//        var siloDataModel = JsonSerializer.Deserialize<RedisSiloModel>(siloData.Value.ToString());

//        if (siloDataModel is null)
//            return false;

//        return siloDataModel.Status == 3;
//    }
//    catch (Exception e)
//    {
//        return false;
//    }
//}


//public class RedisSiloModel
//{
//    public SiloAddressModel SiloAddress { get; set; }
//    public int Status { get; set; }
//    public List<object> SuspectTimes { get; set; }
//    public int ProxyPort { get; set; }
//    public string HostName { get; set; }
//    public string SiloName { get; set; }
//    public string RoleName { get; set; }
//    public int UpdateZone { get; set; }
//    public int FaultZone { get; set; }
//    public DateTime StartTime { get; set; }
//    public DateTime IAmAliveTime { get; set; }
//}

//public class SiloAddressModel
//{
//    public string SiloAddress { get; set; }
//}