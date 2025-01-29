using Raft;

namespace raftapi;

public class HttpNode: INode
{
    public int _id { get; }
    public string Url { get; }
    private HttpClient client = new();

    public HttpNode(int id, string url)
    {
        _id = id;
        Url = url;
    }
}
