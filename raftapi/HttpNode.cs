using Raft;

namespace raftapi;

public class HttpRpcOtherNode : INode
{
    public int Id { get; set; }
    public string Url { get; }

    public int LeaderId { get; }

    public int VotedId { get; set; }
    public int nextValue { get; set; }
    public NodeState State { get; set; }
    public int Term { get; set; }
    public List<LogEntries> Log { get; set; }
    public bool running { get; set; }
    public int electionMultiplier { get; set; }
    public int networkRespondDelay { get; set; }
    public int networkSendDelay { get; set; }

    private HttpClient client = new();

    public HttpRpcOtherNode(int id, string url)
    {
        Id = id;
        Url = url;
    }

    public async Task GetData()
    {
        try
        {
            Console.WriteLine($"Data from {Id}");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");

        }
    }

    public async Task AppendEntries(AppendEntriesData request)
    {
        try
        {
            await client.PostAsJsonAsync(Url + "/request/appendEntries", request);
            Console.WriteLine($"Append Sent from {Id}");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public async Task RequestVote(VoteResponseData voteData)
    {
        try
        {
            await client.PostAsJsonAsync(Url + "/request/vote", voteData);
            Console.WriteLine($"vote request Sent from {Id}");

        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public async Task AppendEntryResponse(AppendEntriesDTO dto)
    {
        try
        {
            Console.WriteLine($"AppendResponse Sent from {Id}");
            await client.PostAsJsonAsync(Url + "/response/appendEntries", dto);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public async Task RespondVote(VoteRequestData voteData)
    {
        try
        {
            Console.WriteLine($"RespondVote Sent from {Id}");
            await client.PostAsJsonAsync(Url + "/response/vote", voteData);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public async Task CommandReceived(ClientCommandData data)
    {
        try
        {
            Console.WriteLine($"Command Sent to {Id}");
            await client.PostAsJsonAsync(Url + "/request/command", data);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($" node {Url} is donw");
        }
    }
}
