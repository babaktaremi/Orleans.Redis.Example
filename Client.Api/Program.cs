using Abstractions.Interfaces;
using Orleans.Configuration;
using Orleans.Runtime;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Host.UseOrleansClient(clientBuilder =>
{
    clientBuilder
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
        });

});


var app = builder.Build();

app.MapPost("/Invoke", async (IClusterClient client, MessageModel model) =>
{
    var messageGrain = client.GetGrain<IMessagingGrain>(model.GrainId);

    await messageGrain.InsertMessageAsync(new Abstractions.MessageModel()
        { MessageContent = model.Content, MessageDate = DateTime.Now });

    return Results.Ok();
});

app.MapGet("/Messages", async (int grainId, IClusterClient client) =>
{
    var messageGrain = client.GetGrain<IMessagingGrain>(grainId);

  var messages=  await messageGrain.GetAllMessages();

    return Results.Ok(messages);
});


app.MapDelete("/Messages/Clear", async (int grainId, IClusterClient client) =>
{
    var messageGrain = client.GetGrain<IMessagingGrain>(grainId);

  await messageGrain.ClearStateAsync();

    return Results.Ok();
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class MessageModel
{
    public string Content { get; set; }
    public int GrainId { get; set; }
}