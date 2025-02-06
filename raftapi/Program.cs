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
 
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
 
 
var nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? throw new Exception("NODE_ID environment variable not set");
var otherNodesRaw = Environment.GetEnvironmentVariable("OTHER_NODES") ?? throw new Exception("OTHER_NODES environment variable not set");
var nodeIntervalScalarRaw = Environment.GetEnvironmentVariable("NODE_INTERVAL_SCALAR") ?? throw new Exception("NODE_INTERVAL_SCALAR environment variable not set");
 
var serviceName = "Node" + nodeId;

var app = builder.Build();

//INode[] otherNodes = (INode[])otherNodesRaw
//  .Split(";")
//  .Select(s => new HttpRpcOtherNode(int.Parse(s.Split(",")[0]), s.Split(",")[1]))
//  .ToArray();

INode[] otherNodes = [];
 
 
var node = new Node(otherNodes)
{
  Id = int.Parse(nodeId),
};
 
node.NodeIntervalScalar = double.Parse(nodeIntervalScalarRaw);
 
node.Start();
 
app.MapGet("/health", () => "healthy");
 
app.MapGet("/nodeData", () =>
{
  return new NodeData(
    Id: node.Id,
    Status: node.running,
    ElectionTimeout: node.ElectionTimeout,
    Term: node.Term,
    CurrentTermLeader: node.LeaderId,
    CommittedEntryIndex: node.CommittedIndex,
    Log: node.Log,
    State: node.State
  );
});
 
app.MapPost("/request/appendEntries", async (AppendEntriesData request) =>
{
        await node.AppendEntries(request);
});
 
app.MapPost("/request/vote", async (VoteResponseData request) =>
{
await node.RequestVote( request);
});
 
app.MapPost("/response/appendEntries", async (AppendEntriesDTO response) =>
{
  await node.AppendEntryResponse(response);
});
 
app.MapPost("/response/vote", async (VoteRequestData response) =>
{
  await node.RespondVote(response);
});
 
app.MapPost("/request/command", async (ClientCommandData data) =>
{
  await node.CommandReceived(data);
});
 
app.Run();
