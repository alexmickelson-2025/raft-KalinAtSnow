using Raft;

namespace raftapi;

public class HttpRpcOtherNode// : INode
{
    public int _id { get; }
    public string Url { get; }
    private HttpClient client = new();

    public HttpRpcOtherNode(int id, string url)
    {
        _id = id;
        Url = url;
    }

    public async Task AppendEntries(AppendEntriesData request)
    {
        try
        {
            await client.PostAsJsonAsync(Url + "/request/appendEntries", request);
            Console.WriteLine($"Append Sent from {_id}");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public async Task RequestVote()//VoteRequestData request)
    {
        try
        {
            //await client.PostAsJsonAsync(Url + "/request/vote");//, request);
            Console.WriteLine($"vote request Sent from {_id}");

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
            Console.WriteLine($"AppendResponse Sent from {_id}");
            await client.PostAsJsonAsync(Url + "/response/appendEntries", dto);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public async Task ResponseVote(VoteResponseData response)
    {
        try
        {
            Console.WriteLine($"RespondVote Sent from {_id}");
            await client.PostAsJsonAsync(Url + "/response/vote", response);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"node {Url} is down");
        }
    }

    public async Task SendCommand(ClientCommandData data)
    {
        Console.WriteLine($"Command Sent to {_id}");
        await client.PostAsJsonAsync(Url + "/request/command", data);
    }
}
