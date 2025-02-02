using System.Xml.Linq;

namespace Raft;

public class Node : INode
{
    public Node()
    {
        _id = 0;
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
    public double NodeIntervalScalar { get; set; } = 1;

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
                    RefreshTimer();
                    if (State == NodeState.CANDIDATE)
                    {
                        await StartElection();
                    }
                }
                if (State == NodeState.LEADER)
                {
                    //TODO: fix this
                    await AppendEntries(new AppendEntriesData(0,0,false));
                }

            }
        });

        t.Start();
        return t;
    }

    public async Task RequestVote()
    {

    }

    public async Task LeaderCheck(int votes)
    {
        foreach (INode node in nodes)
        {
            if (node._id == LeaderId && node.Term >= Term)
            {
                State = NodeState.FOLLOWER;
                return;
            }
        }

        var _majority = Math.Ceiling(((double)nodes.Count + 1) / 2);

        foreach (INode node in nodes)
        {
            if (node.VotedTerm >= Term)
            {
                continue;
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

            foreach (INode node in nodes)
            {
                node.nextValue = nextValue + 1;
            }
        }
    }

    public async Task BecomeCandidate()
    {
        State = NodeState.CANDIDATE;
        VotedId = _id;

        Thread.Sleep(networkSendDelay);
        int yea = 1;
        foreach (INode node in nodes)
        {
            if (await node.RespondVote(new VoteResponseData(_id, Term)))
            {
                yea++;
            }
        }

        await LeaderCheck(yea);
    }

    public async Task<bool> RespondVote(VoteResponseData voteData)
    {
        Thread.Sleep(networkRespondDelay);
        if (voteData.term > VotedTerm)
        {
            VotedId = voteData.id;
            VotedTerm = voteData.term;
            return true;
        }
        return false;
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

    public async Task AppendEntries(AppendEntriesData appendEntriesData)
    {
        Thread.Sleep(networkSendDelay);
        var _majority = Math.Ceiling(((double)nodes.Count + 1) / 2);
        int success = 1;
        foreach (INode node in nodes)
        {
            await node.AppendEntryResponse(new AppendEntriesDTO(_id, Term, CommittedIndex, nextValue - 1, new LogEntries(-1, -1, -1)));

            if (appendEntriesData.isValid == true)
            {
                success++;
            }
            else
            {
                NextIndexes[node._id]--;
                node.nextValue--;
            }
        }
        if (success >= _majority)
        {
            Commit();
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
            if (StateMachine.ContainsKey(nextValue - 1))
            {
                StateMachine[nextValue - 1] = Log[nextValue - 1].value;
            }
            else
            {
                StateMachine.Add(nextValue - 1, Log[nextValue - 1].value);
            }
        }
    }

    public async Task AppendEntryResponse(AppendEntriesDTO dto)
    {
        Thread.Sleep(networkRespondDelay);
        RefreshTimer();
        foreach (INode node in nodes)
        {
            if (node._id != dto.leaderId)
                break;

            if (Log is null)
                Log = new List<LogEntries>();

            if (dto.logValue is null)
                await node.AppendEntries(new AppendEntriesData(Term, nextValue, false));


            if (dto.logValue.term != -1)
                Log.Add(dto.logValue);

            if (dto.term > Term)
            {
                LeaderId = dto.leaderId;
                Term = dto.term;
            }
            else
            {
                await node.AppendEntries(new AppendEntriesData(Term, nextValue, false));
            }

            if (CommittedIndex == 0 && this.CommittedIndex <= CommittedIndex)
            {
                await node.AppendEntries(new AppendEntriesData(Term, nextValue, true));
            }

            if (CommittedIndex > this.CommittedIndex)
            {
                this.CommittedIndex = CommittedIndex;
                StateMachine.Add(CommittedIndex - 1, Log[CommittedIndex - 1].value);
                await node.AppendEntries(new AppendEntriesData(Term, nextValue, true));
            }

            await node.AppendEntries(new AppendEntriesData(Term, nextValue, false));
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
        }
    }
}




