using Raft;

namespace RaftVisualization;

public class VisualizationNode : INode
{

    public Node innerNode;

    public VisualizationNode(Node node)
    {
        innerNode = node;
    }

    public int _id => innerNode.Id;

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
    public double NodeIntervalScalar { get => ((INode)innerNode).NodeIntervalScalar; set => ((INode)innerNode).NodeIntervalScalar = value; }

    int INode.LeaderId => ((INode)innerNode).LeaderId;

    int INode.Id { get => ((INode)innerNode).Id; set => ((INode)innerNode).Id = value; }
    Dictionary<int, int> INode.NextIndexes { get => ((INode)innerNode).NextIndexes; set => ((INode)innerNode).NextIndexes = value; }

    public void AddNode(INode node)
    {
        ((INode)innerNode).AddNode(node);
    }

    public Task AppendEntries(AppendEntriesData data)
    {
        return ((INode)innerNode).AppendEntries(data);
    }

    public Task AppendEntryResponse(AppendEntriesDTO dto)
    {
        return ((INode)innerNode).AppendEntryResponse(dto);
    }

    public void BecomeCandidate()
    {
        ((INode)innerNode).BecomeCandidate();
    }

    public Task CommandReceived(ClientCommandData commandData)
    {
        return ((INode)innerNode).CommandReceived(commandData);
    }

    public void Commit()
    {
        ((INode)innerNode).Commit();
    }

    public void RefreshTimer()
    {
        ((INode)innerNode).RefreshTimer();
    }

    public Task RequestVote(VoteResponseData voteData)
    {
        return ((INode)innerNode).RequestVote(voteData);
    }

    public Task RespondVote(VoteRequestData voteData)
    {
        return ((INode)innerNode).RespondVote(voteData);
    }

    public Thread Start()
    {
        return ((INode)innerNode).Start();
    }

    public void StartElection()
    {
        ((INode)innerNode).StartElection();
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
