using System.Diagnostics;
using System.Transactions;

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
        _random = new Random();
        ElectionTimeout = _random.Next(151, 300) * electionMultiplier;
    }


    //global
    private Random _random = new Random();
    public List<INode> nodes = new List<INode>();
    public int _id { get; set; }
    public NodeState State { get; set; } = NodeState.FOLLOWER;
    public bool running { get; set; } = true;

    //election
    public int Term { get; set; } = 0;
    public int VotedId { get; set; } = -1;
    public int VotedTerm { get; set; } = -1;
    public int LeaderId { get; private set; } = -1;
    public int ElectionTimeout { get; set; } = 0;

    //running
    public int electionMultiplier { get; set; } = 1;
    public int networkSendDelay { get; set; } = 0;
    public int networkRespondDelay { get; set; } = 0;

    //logs
    public Dictionary<int, int> Log { get; set; } = new Dictionary<int, int>();
    public int nextValue { get; set; } = 0;
    public Dictionary<int, int> NextIndexes { get; set; } = new Dictionary<int, int>();
    public int CommittedIndex { get; set; } = 0;


    public void AddNode(INode node)
    {
        nodes.Add(node);
        NextIndexes.Add(node._id, node.nextValue);
    }

    public Thread Start()
    {
        Thread t = new(async () =>
        {
            while (running)
            {
                if (LeaderId != _id)
                {
                    State = NodeState.FOLLOWER;
                }
                ElectionTimeout -= 10;
                Thread.Sleep(10);
                if (ElectionTimeout <= 0)
                {
                    if (State != NodeState.LEADER)
                    {
                        await StartElection();
                    }
                    await RefreshTimer();
                    if (State == NodeState.CANDIDATE)
                    {
                        await StartElection();
                    }
                }
                if (State == NodeState.LEADER)
                {
                    await AppendEntries();
                }

            }
        });

        t.Start();
        return t;
    }

    public async Task LeaderCheck()
    {

        foreach (INode node in nodes)
        {
            if (node._id == LeaderId && node.Term >= Term)
            {
                State = NodeState.FOLLOWER;
                return;
            }
        }


        var _majority = Math.Ceiling((double)nodes.Count / 2);

        int votes = 1;
        foreach (INode node in nodes)
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
            
            foreach (Node node  in nodes)
            {
                node.nextValue = nextValue+1;
            }

            await AppendEntries();
        }
    }

    public async Task BecomeCandidate()
    {
        State = NodeState.CANDIDATE;
        VotedId = _id;
        foreach (INode node in nodes)
        {
            await node.AskForVote(_id, Term);
        }
        await LeaderCheck();
    }

    public async Task RespondVote(int id, int term)
    {
        Thread.Sleep(networkRespondDelay);
        if (term > VotedTerm)
        {
            VotedId = id;
            VotedTerm = term;
        }
        await Task.CompletedTask;
    }

    public async Task AskForVote(int id, int term)
    {
        Thread.Sleep(networkSendDelay);
        foreach (INode node in nodes)
        {
            await node.RespondVote(id, term);
        }
    }

    public async Task StartElection()
    {
        Term++;
        bool ValidForPromotion = true;
        foreach (INode node in nodes)
        {
            if (node.Term > Term)
                ValidForPromotion = false;
        }
        if (ValidForPromotion)
            await BecomeCandidate();
    }

    public async Task AppendEntries()
    {
        Thread.Sleep(networkSendDelay);
        foreach (INode node in nodes)
        {
            await node.RefreshTimer();
            await node.AppendEntryResponse(_id, Term, CommittedIndex);

            node.Log = Log;
        }
    }

    public void Commit()
    {
        if (State == NodeState.LEADER)
        {
            CommittedIndex++;
        }
    }

    public async Task AppendEntryResponse(int id, int term, int CommittedIndex)
    {
        Thread.Sleep(networkRespondDelay);
        if (term > Term)
        {
            LeaderId = id;
            Term = term;
        }
        await Task.CompletedTask;
    }

    public async Task RefreshTimer()
    {
        ElectionTimeout = _random.Next(150, 300) * electionMultiplier;
        await Task.CompletedTask;
    }

    public void CommandReceived(int setValue)
    {
        if (State == NodeState.LEADER)
        {
            Log.Add(nextValue, setValue);
            nextValue++;
        } 
    }
}

