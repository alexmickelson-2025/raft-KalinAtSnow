
using NSubstitute;
using Raft;

namespace RaftTests;
public class logReplication
{
    //test 1
    [Fact]
    public async Task aCommandToLeader_SendsLogEntryToFollowers_AtRPC()
    {
        Node n = new Node();
        var n1 = Substitute.For<INode>();

        n.AddNode(n1);

        n.State = NodeState.LEADER;
        await n.CommandReceived(new ClientCommandData(0, 5));

        await n1.Received().AppendEntries(new AppendEntriesData(n.Term, n.Id, n.nextValue, n.CommittedIndex, new LogEntries(0,0,5)));
    }

    //test 2
    [Fact]
    public async Task bCommandFromClientAppendsToLeaderLog()
    {
        Node n = new Node();
        n.State = NodeState.LEADER;

        await n.CommandReceived(new ClientCommandData(0, 1));

        Assert.Equal(0, n.nextValue - 1);
        Assert.Equal(1, n.Log[0].value);
    }

    [Fact]
    public async Task cCommandFromClientDoesNotAppendToClientLog()
    {
        Node n = new Node();

        await n.CommandReceived(new ClientCommandData(0, 1));

        Assert.Empty(n.Log);
        Assert.Empty(n.Log);
    }


    //test 3
    [Fact]
    public void dNewNodesDoNotHaveAnyLogs()
    {
        Node n = new Node();

        Assert.Empty(n.Log);
    }

    //test 4
    [Fact]
    public async Task eUponSuccessfulElection_LeaderInitializesNextIndexForEachFollower_IndexOneHigher()
    {
        Node n = new(0);
        Node n1 = new(1);
        var n2 = Substitute.For<INode>();
        n2.Id = 2;
        n1.Id = 1;

        n.AddNode(n1);
        n.AddNode(n2);

        await n.StartElection();

        n.Log.Add(new LogEntries(1, 0, 4));
        n.Log.Add(new LogEntries(2, 0, 5));
        n.nextValue = 2;

        await n.RespondVote(new VoteRequestData(true, 1, 1));
        await n.RespondVote(new VoteRequestData(true, 2, 1));

        Assert.Equal(3, n1.nextValue);
        //Assert.Equal(3, n2.nextValue);
    }

    //test 5
    [Fact]
    public void fLeadersStoreTheNextIndexOfEachFollower()
    {
        Node n = new Node();
        Node n1 = new Node();

        n.AddNode(n1);
        n1.AddNode(n);

        Assert.Single(n.NextIndexes);
    }

    //test 6
    [Fact]
    public async Task gHighestCommittedIndexIncludedInAppendEntries()
    {
        Node n = new Node(0);
        var n1 = Substitute.For<INode>();

        n.AddNode(n1);

        n.State = NodeState.LEADER;
        n.CommittedIndex = 3;

        await n1.AppendEntries(new AppendEntriesData(n.Term, n.Id, n.nextValue, n.CommittedIndex, new LogEntries(n.Term, -1, -1)));
        await n1.Received().AppendEntries(new AppendEntriesData(0, 0, 0, 3, new LogEntries(0, -1, -1)));
    }

    //test 7
    [Fact]
    public async Task hFollowerLearnsOfUncommittedLog_AddToStateMachine()
    {
        Node n = new Node(0);
        Node n1 = new Node(1);

        n.AddNode(n1);
        n.Term = 1;
        n.State = NodeState.LEADER;
        await n.CommandReceived(new ClientCommandData(0, 5));
        n.Commit();

        await n1.AppendEntries(new AppendEntriesData(n.Term, n.Id, n.nextValue, n.CommittedIndex, new LogEntries(n.Term, 0, 5)));
        Assert.Equal(5, n1.StateMachine[0]);
        Assert.Equal(1, n1.CommittedIndex);
    }

    //test 8
    [Fact]
    public async Task iLeaderGetsMajorityConfirmationOfLog_GetsCommitted()
    {
        var n = new Node(0);
        var n1 = Substitute.For<INode>();
        var n2 = Substitute.For<INode>();
        n1.Id = 1;
        n2.Id = 2;

        n.AddNode(n1);
        n.AddNode(n2);
        n.State = NodeState.LEADER;
        await n.CommandReceived(new ClientCommandData(0, 5));

        await n.AppendEntryResponse(new AppendEntriesDTO(true, 1, n1.Term, n1.Id));
        await n.AppendEntryResponse(new AppendEntriesDTO(true, 1, n2.Term, n2.Id));
        Assert.Equal(1, n.CommittedIndex);
    }


    //test 9
    [Fact]
    public async Task jLeaderCommitsByincrementingCommittedInded()
    {
        Node n = new Node(0);
        n.State = NodeState.LEADER;

        Assert.Equal(0, n.CommittedIndex);

        await n.CommandReceived(new ClientCommandData(0, 5));
        n.Commit();

        Assert.Equal(1, n.CommittedIndex);
    }

    [Fact]
    public void kFollowerOrCandidateCanNotCommit()
    {
        Node n = new Node(0);
        Node n1 = new Node(1);

        n1.State = NodeState.CANDIDATE;

        n.Commit();
        n1.Commit();

        Assert.Equal(0, n.CommittedIndex);
        Assert.Equal(0, n1.CommittedIndex);
    }


    //test 10
    [Fact]
    public async Task lFollowerGetsAppendEntryWithLogs_AddsToPersonalLog()
    {
        Node n = new Node();
        Node n1 = new Node();

        n.AddNode(n1);
        n1.AddNode(n);
        n.State = NodeState.LEADER;

        await n.CommandReceived(new ClientCommandData(0, 4));
        //await n.CommandReceived(new ClientCommandData(1, 5));
        
        await n1.AppendEntries(new AppendEntriesData(n.Term, n.Id, n.nextValue, n.CommittedIndex, n.Log[n.nextValue-1]));
        
        Assert.Equal(4, n1.Log[0].value);
    }

    //test 11
    [Fact]
    public async Task mfollowersResponseIncludesTermNumberAndLogEntryIndex()
    {
        Node n = new Node();

        await n.AppendEntryResponse(new AppendEntriesDTO(true, n.nextValue, n.Term, n.Id));
    }


    //when a leader receives a majority responses from the clients after a log replication heartbeat, the leader sends a confirmation response to the client



    //test 13
    [Fact]
    public async Task nWhenCommitting_ApplyToInternalStateMachine()
    {
        Node n = new Node(0);
        n.State = NodeState.LEADER;

        await n.CommandReceived(new ClientCommandData(0, 3));
        n.Commit();

        Assert.Equal(3, n.StateMachine[0]);
    }


    //test 14
    [Fact]
    public async Task oWhenFollowerGetsHeartbeat_increasesCommitIndexToMatchIndexOfLeader()
    {
        Node n = new Node();
        var n1 = new Node();
        n.AddNode(n1);

        n.State = NodeState.LEADER;
        n.Term = 1;
        await n.CommandReceived(new ClientCommandData(0, 3));
        n.Commit();

        Assert.Equal(1, n.CommittedIndex);

        await n1.AppendEntries(new AppendEntriesData(n.Term, n.Id, n.nextValue, n.CommittedIndex, n.Log[n.nextValue-1]));
        Assert.Equal(1, n1.CommittedIndex);
    }

    //14.b
    [Fact]
    public async Task pRejectHeartbeatWhenPreviousCommittedIndexDoesNotMatchLog()
    {
        Node n = new Node();

        n.Log.Add(new LogEntries(5, 0, 2));
        n.CommittedIndex = 1;
        
       // await n.AppendEntryResponse(new AppendEntriesDTO(1, 1, 0, Arg.Any<int>(), Arg.Any<LogEntries>()));

        //TODO: fix this test
        //Assert.False(response.isValid);
    }

    [Fact]
    public async Task qRejectHeartbeatWhenPreviousLogTermDoesNotMatchLog()
    {
        Node n = new Node();

        n.Log.Add(new LogEntries(5, 0, 2));
        n.Term = 5;

        //await n.AppendEntryResponse(new AppendEntriesDTO(1, 1, 0, Arg.Any<int>(), Arg.Any<LogEntries>()));

        //TODO: fix this test
        //Assert.False(response.isValid);
    }

    // test 15
    [Fact]
    public async Task rLeaderIncludesPreviousIndexAndCurrentTermInAppendRPC()
    {
        var n = new Node();
        var n1 = Substitute.For<INode>();
        n.AddNode(n1);
        n.State = NodeState.LEADER;
        n.Term = 2;

        //needed with commit being implemented
        n.Log.Add(new LogEntries(2, 0, 99));
        n.nextValue++;
        //TODO: update this
       // await n1.AppendEntries(new AppendEntriesData(-1, -1));
        n.Log.Add(new LogEntries(2, 0, 98));
        n.nextValue++;
        //TODO: update this
        //await n1.AppendEntries(new AppendEntriesData(-1, -1));
        //await n1.Received().AppendEntryResponse(Arg.Is<AppendEntriesDTO>(dto => dto.term == 2 && dto.indexTerm == 1));
    }


    // 15 1
    [Fact]
    public async Task sFollowerDoesNotFindEntry_RefusesRPC()
    {
        var n = new Node();
        n.Log.Add(new LogEntries(2, 0, 5));
        n.Log.Add(new LogEntries(2, 0, 6));
        n.Term = 2;

        //await n.AppendEntryResponse(new AppendEntriesDTO(0, 1, 0, 1, new LogEntries(0, 0, 0)));
        //TODO: Fix this test
        //Assert.False(badTerm.valid);
    }

    // 15 1 a
    [Fact]
    public async Task tFollowerAddsLogWhenTermIsHigher()
    {
        var n = new Node();
        var n1 = Substitute.For<INode>();
        n.AddNode(n1);

        n.Log.Add(new LogEntries(3, 0, 5));
        n.nextValue++;
        n.Term = 3;
        n.State = NodeState.LEADER;
        //TODO: update this
        //await n1.AppendEntries(new AppendEntriesData(-1, -1));
        Assert.Single(n1.Log);
    }


    //15 1 b
    [Fact]
    public async Task uFollowerIndexIsGreaterThanLeader_LeaderDecreasesItsLocalCheck()
    {
        var n = new Node();
        var n1 = Substitute.For<INode>();

        n.Log.Add(new LogEntries(2, 0, 5));
        n.nextValue++;

        n1.Log = [new LogEntries(2, 0, 6), new LogEntries(2, 0, 4), new LogEntries(2, 0, 5)];
        n1.nextValue = 3;

        n.AddNode(n1);

        //TODO: update this
        //await n1.AppendEntries(new AppendEntriesData(-1, -1)); 

        Assert.Equal(2, n.NextIndexes[0]);
        Assert.Equal(2, n1.nextValue);
    }


    //if leader index is less, followers delete
    //if a follower rejects the AppendEntries RPC, the leader decrements nextIndex and retries the AppendEntries RPC


    //test 16
    //this test isn't testing non responses - its testing failing responses. I'm not sure how I can test non response with my current implementation
    [Fact]
    public async Task vLeaderSendsHeartbeat_DoesNotRecieveResponseFromMajority_RemainsUncommitted()
    {
        var n = new Node();
        var n1 = new Node(1);
        var n2 = new Node(2);

        n.State = NodeState.LEADER;
        n.AddNode(n1);
        n.AddNode(n2);

        //will return false as shown in 14.b
        n1.Log.Add(new LogEntries(5, 0, 2));
        n1.Term = 5;
        n2.Log.Add(new LogEntries(5, 0, 2));
        n2.Term = 5;
        n.Log.Add(new LogEntries(1, 0, 1));
        n.nextValue++;

        //TODO: update this
        //await n1.AppendEntries(new AppendEntriesData(-1, -1));

        Assert.Equal(0, n.CommittedIndex);
    }

    //test 17
    // same as above, not sure how to test unresponsive
    [Fact]
    public void wLeaderDoesNotGetResponse_ContinuesToSend()
    {
        var n = new Node();
        var n1 = Substitute.For<INode>();

        n.State = NodeState.LEADER;
        n1.Term = 5;

        n.AddNode(n1);
        Thread t = n.Start();

        n1.ClearReceivedCalls();

        Thread.Sleep(50);

        n.running = false;
        t.Join();

        n1.Received().AppendEntryResponse(Arg.Any<AppendEntriesDTO>());
    }


    //if a leader cannot commit an entry, it does not send a response to the client
    //if a node receives an appendentries with a logs that are too far in the future from your local state, you should reject the appendentries
    //if a node receives and appendentries with a term and index that do not match, you will reject the appendentry until you find a matching log
}
