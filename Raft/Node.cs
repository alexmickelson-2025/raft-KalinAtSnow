namespace Raft;

public class Node
{
    public Node()
    {
        _id = 0;
    }

    public Node(int id)
    {
        _id = id;
    }

    public List<Node> nodes = new List<Node>();
    private int _id;
    public int VotedId { get; private set; }
    public int? LeaderId { get; private set; }
    public NodeState State { get; set; } = NodeState.FOLLOWER;

    public void LeaderCheck()
    {
        var _majority = Math.Ceiling((double) nodes.Count / 2);

        int votes = 1;
        foreach (Node node in nodes)
        {
            if (node.VotedId == votes)
            {
                votes++;
            }
        }

        if (votes >= _majority) {
            State = NodeState.LEADER;
            LeaderId = _id;
            AppendEntries();
        }     
    }

    public void BecomeCandidate()
    {
        State = NodeState.CANDIDATE;
        VotedId = _id;
        foreach (Node node in nodes) {
            AskForVote(_id);
        }
    }

    private void AskForVote(int id)
    {
        VotedId = id;
    }

    public void StartElection()
    {
        BecomeCandidate();
    }

    public void AppendEntries()
    {
        foreach (Node node in nodes)
        {
            node.LeaderId = _id;
        }
    }
}

