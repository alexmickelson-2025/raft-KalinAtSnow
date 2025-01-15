using Raft;

namespace RaftVisualization;

public class VisualizationNode : INode
{

    public required Node innerNode;

    public VisualizationNode(Node node)
    {
        innerNode = node;
    }

    public int? LeaderId => ((INode)innerNode).LeaderId;

    public NodeState State { get => ((INode)innerNode).State; set => ((INode)innerNode).State = value; }
    public int VotedId { get => ((INode)innerNode).VotedId; set => ((INode)innerNode).VotedId = value; }

    public void AppendEntries()
    {
        ((INode)innerNode).AppendEntries();
    }

    public void BecomeCandidate()
    {
        ((INode)innerNode).BecomeCandidate();
    }

    public void LeaderCheck()
    {
        ((INode)innerNode).LeaderCheck();
    }

    public void StartElection()
    {
        ((INode)innerNode).StartElection();
    }
}
