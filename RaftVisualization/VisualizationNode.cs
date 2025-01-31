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
    public int nextValue { get => ((INode)innerNode).nextValue; set => ((INode)innerNode).nextValue = value; }
    public int CommittedIndex { get => ((INode)innerNode).CommittedIndex; set => ((INode)innerNode).CommittedIndex = value; }
    public Dictionary<int, int> StateMachine { get => ((INode)innerNode).StateMachine; set => ((INode)innerNode).StateMachine = value; }
    public List<LogEntries> Log { get => ((INode)innerNode).Log; set => ((INode)innerNode).Log = value; }

    int INode.LeaderId => ((INode)innerNode).LeaderId;

    int INode._id { get => ((INode)innerNode)._id; set => ((INode)innerNode)._id = value; }
    Dictionary<int, int> INode.NextIndexes { get => ((INode)innerNode).NextIndexes; set => ((INode)innerNode).NextIndexes = value; }

    public void AddNode(INode node)
    {
        ((INode)innerNode).AddNode(node);
    }

    public void AppendEntries()
    {
        ((INode)innerNode).AppendEntries();
    }

    public (int TermNumber, int LogIndex, bool valid) AppendEntryResponse(int leaderId, int term, int CommittedIndex, int indexTerm, LogEntries logValue)
    {
        return ((INode)innerNode).AppendEntryResponse(leaderId, term, CommittedIndex, indexTerm, logValue);
    }

    public void BecomeCandidate()
    {
        ((INode)innerNode).BecomeCandidate();
    }

    public void CommandReceived(int setKey, int setValue)
    {
        ((INode)innerNode).CommandReceived(setKey, setValue);
    }

    public void Commit()
    {
        ((INode)innerNode).Commit();
    }

    public Task LeaderCheck(int votes)
    {
        return ((INode)innerNode).LeaderCheck(votes);
    }

    public void RefreshTimer()
    {
        ((INode)innerNode).RefreshTimer();
    }

    public bool RespondVote(int id, int term)
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

    Task INode.StartElection()
    {
        return ((INode)innerNode).StartElection();
    }
}
