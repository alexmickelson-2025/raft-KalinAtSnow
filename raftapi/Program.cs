//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using Raft;
using raftapi;
using System.Text.Json;
 
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
 
 
var nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? throw new Exception("NODE_ID environment variable not set");
var otherNodesRaw = Environment.GetEnvironmentVariable("OTHER_NODES") ?? throw new Exception("OTHER_NODES environment variable not set");
var nodeIntervalScalarRaw = Environment.GetEnvironmentVariable("NODE_INTERVAL_SCALAR") ?? throw new Exception("NODE_INTERVAL_SCALAR environment variable not set");
 
var serviceName = "Node" + nodeId;

var app = builder.Build();
 
var logger = app.Services.GetService<ILogger<Program>>();
logger.LogInformation("Node ID {name}", nodeId);
logger.LogInformation("Other nodes environment config: {}", otherNodesRaw);
 
 
INode[] otherNodes = otherNodesRaw
  .Split(";")
  .Select(s => new HttpRpcOtherNode(int.Parse(s.Split(",")[0]), s.Split(",")[1]))
  .ToArray();
 
 
logger.LogInformation("other nodes {nodes}", JsonSerializer.Serialize(otherNodes));
 
 
var node = new Node(otherNodes)
{
  _id = int.Parse(nodeId),
};
 
Node.NodeIntervalScalar = double.Parse(nodeIntervalScalarRaw);
 
node.Start();
 
app.MapGet("/health", () => "healthy");
 
app.MapGet("/nodeData", () =>
{
  return new NodeData(
    Id: node._id,
    Status: node.State,
    ElectionTimeout: node.ElectionTimeout,
    Term: node.Term,
    CurrentTermLeader: node.LeaderId,
    CommittedEntryIndex: node.CommittedIndex,
    Log: node.Log,
    State: node.State,
    NodeIntervalScalar: Node.NodeIntervalScalar
  );
});
 
app.MapPost("/request/appendEntries", async () => //AppendEntriesData request) =>
{
        logger.LogInformation("received append entries request ");//  {request}", request);
        await node.AppendEntries(); // request);
});
 
app.MapPost("/request/vote", async ()=> //VoteRequestData request) =>
{
  logger.LogInformation("received vote request");// {request}", request);
await node.RequestVote();// request);
});
 
app.MapPost("/response/appendEntries", async (AppendEntriesDTO response) =>
{
  logger.LogInformation("received append entries response {response}", response);
  await node.AppendEntryResponse(response);
});
 
app.MapPost("/response/vote", async (VoteResponseData response) =>
{
  logger.LogInformation("received vote response {response}", response);
  await node.RespondVote(response);
});
 
app.MapPost("/request/command", async (ClientCommandData data) =>
{
  await node.CommandReceived(data);
});
 
app.Run();
