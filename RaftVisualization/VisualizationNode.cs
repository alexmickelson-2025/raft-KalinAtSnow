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

    int INode.LeaderId => ((INode)innerNode).LeaderId;

    public void AppendEntries()
    {
        ((INode)innerNode).AppendEntries();
    }

    public Task AppendEntryResponse(int id, int term)
    {
        return ((INode)innerNode).AppendEntryResponse(id, term);
    }

    public void BecomeCandidate()
    {
        ((INode)innerNode).BecomeCandidate();
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
