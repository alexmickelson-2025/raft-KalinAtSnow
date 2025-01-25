using Raft;

namespace RaftVisualization;

public class VisualizationNode : INode
{

    public Node innerNode;

    public VisualizationNode(Node node)
    {
        innerNode = node;
    }

    public int _id => innerNode._id;

    public int? LeaderId => ((INode)innerNode).LeaderId;

    public NodeState State { get => ((INode)innerNode).State; set => ((INode)innerNode).State = value; }
    public int VotedId { get => ((INode)innerNode).VotedId; set => ((INode)innerNode).VotedId = value; }
    public int VotedTerm { get => ((INode)innerNode).VotedTerm; set => ((INode)innerNode).VotedTerm = value; }
    public int Term { get => ((INode)innerNode).Term; set => ((INode)innerNode).Term = value; }
    public int ElectionTimeout { get => ((INode)innerNode).ElectionTimeout; set => ((INode)innerNode).ElectionTimeout = value; }
    public bool running { get => ((INode)innerNode).running; set => ((INode)innerNode).running = value; }
    public int electionMultiplier { get => ((INode)innerNode).electionMultiplier; set => ((INode)innerNode).electionMultiplier = value; }
    public int networkRespondDelay { get => ((INode)innerNode).networkRespondDelay; set => ((INode)innerNode).networkRespondDelay = value; }
    public int networkSendDelay { get => ((INode)innerNode).networkSendDelay; set => ((INode)innerNode).networkSendDelay = value; }
    public Dictionary<int, int> Log { get => ((INode)innerNode).Log; set => ((INode)innerNode).Log = value; }
    public int nextValue { get => ((INode)innerNode).nextValue; set => ((INode)innerNode).nextValue = value; }
    public List<int> NextIndexes { get => ((INode)innerNode).NextIndexes; set => ((INode)innerNode).NextIndexes = value; }

    int INode.LeaderId => ((INode)innerNode).LeaderId;

    int INode._id { get => ((INode)innerNode)._id; set => ((INode)innerNode)._id = value; }

    public void AppendEntries()
    {
        ((INode)innerNode).AppendEntries();
    }

    public Task AppendEntryResponse(int id, int term)
    {
        return ((INode)innerNode).AppendEntryResponse(id, term);
    }

    public Task AskForVote(int id, int term)
    {
        return ((INode)innerNode).AskForVote(id, term);
    }

    public void BecomeCandidate()
    {
        ((INode)innerNode).BecomeCandidate();
    }

    public void CommandReceived(int setValue)
    {
       ((INode)innerNode).CommandReceived(setValue);
    }

    public void LeaderCheck()
    {
        ((INode)innerNode).LeaderCheck();
    }

    public Task RefreshTimer()
    {
        return ((INode)innerNode).RefreshTimer();
    }

    public Task RespondVote(int id, int term)
    {
        return ((INode)innerNode).RespondVote(id, term);
    }

    public Thread Start()
    {
        return ((INode)innerNode).Start();
    }

    public void StartElection()
    {
        ((INode)innerNode).StartElection();
    }

    Task INode.AppendEntries()
    {
        return ((INode)innerNode).AppendEntries();
    }

    Task INode.BecomeCandidate()
    {
        return ((INode)innerNode).BecomeCandidate();
    }

    Task INode.LeaderCheck()
    {
        return ((INode)innerNode).LeaderCheck();
    }

    Task INode.StartElection()
    {
        return ((INode)innerNode).StartElection();
    }
}
