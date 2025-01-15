namespace Raft;

public class Node : INode
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
    public int _id;
    public int VotedId { get; set; }
    public int? LeaderId { get; private set; }
    public NodeState State { get; set; } = NodeState.FOLLOWER;
    public int Term = 0;

    public void LeaderCheck()
    {

        foreach (Node node in nodes)
        {
            if (node._id == LeaderId && node.Term >= Term)
            {
                State = NodeState.FOLLOWER;
                return;
            }
        }


        var _majority = Math.Ceiling((double)nodes.Count / 2);

        int votes = 1;
        foreach (Node node in nodes)
        {
            if (node.VotedId == _id)
            {
                votes++;
            }
        }

        if (votes >= _majority)
        {
            State = NodeState.LEADER;
            LeaderId = _id;
            AppendEntries();
        }

    }

    public void BecomeCandidate()
    {
        State = NodeState.CANDIDATE;
        VotedId = _id;
        foreach (Node node in nodes)
        {
            node.AskForVote(_id);
        }
    }

    private void AskForVote(int id)
    {
        VotedId = id;
    }

    public void StartElection()
    {
        Term++;
        bool ValidForPromotion = true;
        foreach (Node node in nodes)
        {
            if (node.Term > Term)
                ValidForPromotion = false;
        }
        if (ValidForPromotion)
            BecomeCandidate();
    }

    public void AppendEntries()
    {
        foreach (Node node in nodes)
        {
            node.LeaderId = _id;
            node.Term = Term;
        }
    }

}

