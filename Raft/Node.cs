using System.Xml.Linq;

namespace Raft;

public class Node : INode
{
    public Node()
    {
        Id = 0;
    }

    public Node(INode[] nodes)
    {
        foreach (INode node in nodes)
        {
            AddNode(node);
        }
    }

    public Node(int id)
    {
        Id = id;
        _random = new Random();
        ElectionTimeout = _random.Next(151, 300) * electionMultiplier;
    }

    //global
    private Random _random = new Random();
    public List<INode> nodes = new List<INode>();
    public int validVotes = 0;
    public int Id { get; set; }
    public NodeState State { get; set; } = NodeState.FOLLOWER;
    public bool running { get; set; } = true;
    public double NodeIntervalScalar { get; set; } = 1;

    //election
    public int Term { get; set; } = 0;
    public int VotedId { get; set; } = -1;
    public int VotedTerm { get; set; } = -1;
    public int LeaderId { get; set; }
    public int ElectionTimeout { get; set; } = 0;

    //running
    public int electionMultiplier { get; set; } = 1;
    public int networkSendDelay { get; set; } = 0;
    public int networkRespondDelay { get; set; } = 0;

    //logs
    public List<LogEntries> Log { get; set; } = new List<LogEntries>();
    public int nextValue { get; set; } = 0;
    public Dictionary<int, int> NextIndexes { get; set; } = new Dictionary<int, int>();
    public int CommittedIndex { get; set; } = 0;
    public Dictionary<int, int> StateMachine { get; set; } = new Dictionary<int, int>();

    public void PauseToggle()
    {
        running = false;
    }

    public void AddNode(INode node)
    {
        nodes.Add(node);
        NextIndexes.Add(node.Id, node.nextValue);
    }

    public Thread Start()
    {
        Thread t = new(async () =>
        {
            while (running)
            {
                if (LeaderId != Id)
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
                    RefreshTimer();
                    if (State == NodeState.CANDIDATE)
                    {
                        await StartElection();
                    }
                }
                if (State == NodeState.LEADER)
                {
                    foreach (INode node in nodes)
                    {
                        if (Log.Count > 1)
                        {
                            await node.AppendEntries(new AppendEntriesData(Term, Id, nextValue, CommittedIndex, Log[nextValue - 1]));
                        }
                        else
                        {
                            await node.AppendEntries(new AppendEntriesData(Term, Id, nextValue, CommittedIndex, new LogEntries(Term, -1, -1)));
                        }
                    }
                }

            }
        });

        t.Start();
        return t;
    }

    //follower implement
    public async Task RequestVote(VoteResponseData voteRequestData)
    {
        if (voteRequestData.Term > Term)
        {
            VotedId = voteRequestData.LeaderId;
            VotedTerm = voteRequestData.Term;
            LeaderId = voteRequestData.LeaderId;
            Term = voteRequestData.Term;
            foreach (INode node in nodes)
            {
                if (node.Id == LeaderId)
                    await node.RespondVote(new VoteRequestData(true, Id, Term));
            }
        }
        foreach (INode node in nodes)
        {
            if (node.Id == voteRequestData.LeaderId)
                await node.RespondVote(new VoteRequestData(false, Id, Term));
        }
    }

    public async Task LeaderCheck()
    {
        var majority = Math.Ceiling(((double)nodes.Count + 1) / 2);
        if (validVotes >= majority)
        {
            State = NodeState.LEADER;
            LeaderId = Id;
            foreach (INode n in nodes)
            {
                if (Log.Count > 1)
                {
                    await n.AppendEntries(new AppendEntriesData(Term, Id, nextValue, CommittedIndex, Log[nextValue - 1]));
                }
                else
                {
                    await n.AppendEntries(new AppendEntriesData(Term, Id, nextValue, CommittedIndex, new LogEntries(Term, -1, -1)));
                }
            }
        }
    }

    public async Task BecomeCandidate()
    {
        validVotes = 1;
        State = NodeState.CANDIDATE;
        VotedId = Id;

        foreach (INode node in nodes)
        {
            await node.RequestVote(new VoteResponseData(Id, Term));
        }
    }

    //leader implement
    public async Task RespondVote(VoteRequestData voteData)
    {
        if (voteData.Term > Term)
        {
            State = NodeState.FOLLOWER;
            return;
        }

        if (voteData.VoteStatus)
            validVotes++;

        await LeaderCheck();
    }

    public async Task StartElection()
    {
        Term++;
        await BecomeCandidate();
    }


    // implemented as a follower - leader send this to a follower
    public async Task AppendEntries(AppendEntriesData appendEntriesData)
    {
        if (appendEntriesData.log.key != -1 && appendEntriesData.log.value != -1)
        {
            Term = appendEntriesData.Term;
            LeaderId = appendEntriesData.LeaderId;
            if (appendEntriesData.nextValue > Log.Count)
            {
                Log.Add(appendEntriesData.log);
                nextValue = appendEntriesData.nextValue;
            }
            if (CommittedIndex < appendEntriesData.CommittedIndex)
            {
                CommittedIndex = appendEntriesData.CommittedIndex;
                StateMachine[appendEntriesData.log.key] = appendEntriesData.log.value;
            }
        }
        RefreshTimer();

        foreach (INode n in nodes)
        {
            if (n.Id == LeaderId)
            {
                await n.AppendEntryResponse(new AppendEntriesDTO(true, nextValue,Term, Id));
            }
        }
    }

    public void Commit()
    {
        if (State == NodeState.LEADER)
        {
            if (Log.Count < 1)
            {
                return;
            }
            CommittedIndex++;
            StateMachine[Log[nextValue-1].key] = Log[nextValue - 1].value;
        }
    }

    //implemnt this as the leader
    public async Task AppendEntryResponse(AppendEntriesDTO dto)
    {
        await Task.CompletedTask;

        NextIndexes[dto.id] = dto.index;
        int readyForCommit = 1;
        var majority = Math.Ceiling(((double)nodes.Count + 1) / 2);
        foreach (INode node in nodes)
        {
            if (NextIndexes[node.Id] > CommittedIndex)
            {
                readyForCommit++;
            }
        }
        if (readyForCommit >= majority)
        {
            Commit();
        }
    }

    public void RefreshTimer()
    {
        ElectionTimeout = _random.Next(150, 300) * electionMultiplier;

    }

    public async Task CommandReceived(ClientCommandData commandData)
    {
        if (State == NodeState.LEADER)
        {
            Log.Add(new LogEntries(Term, commandData.setKey, commandData.setValue));
            nextValue++;
            foreach (INode node in nodes)
            {
                if (Log.Count > 1)
                {
                    await node.AppendEntries(new AppendEntriesData(Term, Id, nextValue, CommittedIndex, Log[nextValue - 1]));
                }
                else
                {
                    await node.AppendEntries(new AppendEntriesData(Term, Id, nextValue, CommittedIndex, new LogEntries(Term, commandData.setKey, commandData.setValue)));
                }
            }
        }
    }
}




