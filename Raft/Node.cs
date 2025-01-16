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

    private Random _random = new Random();
    public List<Node> nodes = new List<Node>();
    public int _id;
    public int VotedId { get; set; } = -1;
    public int VotedTerm { get; set; } = -1;
    public int LeaderId { get; private set; } = -1;
    public NodeState State { get; set; } = NodeState.FOLLOWER;
    public int Term = 0;
    public int ElectionTimeout = 0;

    public async Task LeaderCheck()
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
            if (node.VotedTerm >= Term)
            {
                break;
            }

            if (node.VotedId == _id)
            {
                votes++;
            }
        }

        if (votes >= _majority)
        {
            State = NodeState.LEADER;
            LeaderId = _id;
            await AppendEntries();
        }

    }

    public async Task BecomeCandidate()
    {
        State = NodeState.CANDIDATE;
        VotedId = _id;
        foreach (Node node in nodes)
        {
            await node.AskForVote(_id, Term);
        }
    }

    public async Task RespondVote(int id, int term)
    {
        if (term > VotedTerm)
        {
            VotedId = id;
            VotedTerm = term;
        }
    }

    private async Task AskForVote(int id, int term)
    {
        foreach (Node node in nodes)
        {
             await node.RespondVote(id, term);
        }
    }

    public async Task StartElection()
    {
        Term++;
        bool ValidForPromotion = true;
        foreach (Node node in nodes)
        {
            if (node.Term > Term)
                ValidForPromotion = false;
        }
        if (ValidForPromotion)
            await BecomeCandidate();
    }

    public async Task AppendEntries()
    {
        foreach (Node node in nodes)
        {
            node.LeaderId = _id;
            node.Term = Term;
        }
    }

    public async Task RefreshTimer()
    {
        ElectionTimeout = _random.Next(150, 300);
    }
}

