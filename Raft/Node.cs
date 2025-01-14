namespace Raft;

public class Node : INode
{
    public Node()
    {
        State = NodeState.FOLLOWER;
        _id = 0;
    }

    private int _id;
    public int VotedId;
    private int _majority;
    public NodeState State { get; private set; }

    public int CountVotes()
    {
        State = NodeState.LEADER;
        return 1;
    }

    public void BecomeCandidate()
    {
        State = NodeState.CANDIDATE;
        VotedId = _id;
    }

    public void StartElection()
    {
        BecomeCandidate();
    }
}

