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

    public int? LeaderId => (innerNode).LeaderId;

    public NodeState State { get => (innerNode).State; set => (innerNode).State = value; }
    public int VotedId { get => (innerNode).VotedId; set => (innerNode).VotedId = value; }
    public int VotedTerm { get => (innerNode).VotedTerm; set => (innerNode).VotedTerm = value; }
    public int Term { get => (innerNode).Term; set => (innerNode).Term = value; }
    public int ElectionTimeout { get => (innerNode).ElectionTimeout; set => (innerNode).ElectionTimeout = value; }
    public bool running { get => (innerNode).running; set => (innerNode).running = value; }
    public int electionMultiplier { get => (innerNode).electionMultiplier; set => (innerNode).electionMultiplier = value; }
    public int networkRespondDelay { get => (innerNode).networkRespondDelay; set => (innerNode).networkRespondDelay = value; }
    public int networkSendDelay { get => (innerNode).networkSendDelay; set => (innerNode).networkSendDelay = value; }
    public int nextValue { get => (innerNode).nextValue; set => (innerNode).nextValue = value; }
    public int CommittedIndex { get => (innerNode).CommittedIndex; set => (  innerNode).CommittedIndex = value; }
    public Dictionary<int, int> StateMachine { get => (innerNode).StateMachine; set => ( innerNode).StateMachine = value; }
    public List<LogEntries> Log { get => (  innerNode).Log; set => (innerNode).Log = value; }
    public double NodeIntervalScalar { get => (innerNode).NodeIntervalScalar; set => (innerNode).NodeIntervalScalar = value; }
    int Id { get => (innerNode).Id; set => (innerNode).Id = value; }
    int INode.Id { get => ((INode)innerNode).Id; set => ((INode)innerNode).Id = value; }
    Dictionary<int, int> NextIndexes { get => (innerNode).NextIndexes; set => (innerNode).NextIndexes = value; }

    int INode.LeaderId => ((INode)innerNode).LeaderId;

    public void AddNode(INode node)
    {
        (innerNode).AddNode(node);
    }

    public Task AppendEntries(AppendEntriesData data)
    {
        return (innerNode).AppendEntries(data);
    }

    public Task AppendEntryResponse(AppendEntriesDTO dto)
    {
        return (innerNode).AppendEntryResponse(dto);
    }

    public Task CommandReceived(ClientCommandData commandData)
    {
        return (innerNode).CommandReceived(commandData);
    }

    public void Commit()
    {
        (innerNode).Commit();
    }

    public void RefreshTimer()
    {
        (   innerNode).RefreshTimer();
    }

    public Task RequestVote(VoteResponseData voteData)
    {
        return (innerNode).RequestVote(voteData);
    }

    public Task RespondVote(VoteRequestData voteData)
    {
        return (innerNode).RespondVote(voteData);
    }

    public Thread Start()
    {
        return (innerNode).Start();
    }
}
