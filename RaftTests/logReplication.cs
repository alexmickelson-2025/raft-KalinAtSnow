
using NSubstitute;
using Raft;

namespace RaftTests;
public class logReplication
{
    //test 1
    [Fact]
    public async Task CommandToLeader_SendsLogEntryToFollowers_AtRPC()
    {
        Node n = new Node();
        Node n1 = new Node();

        n.AddNode(n1);

        n.State = NodeState.LEADER;
        await n.CommandReceived(new ClientCommandData(0, 5));

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        Assert.Equal(5, n1.Log[0].value);
    }

    //test 2
    [Fact]
    public async Task CommandFromClientAppendsToLeaderLog()
    {
        Node n = new Node();
        n.State = NodeState.LEADER;

        await n.CommandReceived(new ClientCommandData(0, 1));

        Assert.Equal(0, n.nextValue - 1);
        Assert.Equal(1, n.Log[0].value);
    }

    [Fact]
    public async Task CommandFromClientDoesNotAppendToClientLog()
    {
        Node n = new Node();

        await n.CommandReceived(new ClientCommandData(0, 1));

        Assert.Empty(n.Log);
        Assert.Empty(n.Log);
    }


    //test 3
    [Fact]
    public void NewNodesDoNotHaveAnyLogs()
    {
        Node n = new Node();

        Assert.Empty(n.Log);
    }

    //test 4
    [Fact]
    public async Task UponSuccessfulElection_LeaderInitializesNextIndexForEachFollower_IndexOneHigher()
    {
        Node n = new Node();
        Node n1 = new Node();
        Node n2 = new Node();

        n.nodes = [n1, n2];
        n1.nodes = [n, n2];
        n2.nodes = [n1, n];

        n.Log.Add(new LogEntries(1, 0, 4));
        n.Log.Add(new LogEntries(2, 0, 5));
        n.nextValue = 2;

        Assert.Equal(2, n.nextValue);

        await n.StartElection();

        Assert.Equal(3, n1.nextValue);
        Assert.Equal(3, n2.nextValue);
    }

    //test 5
    [Fact]
    public void LeadersStoreTheNextIndexOfEachFollower()
    {
        Node n = new Node();
        Node n1 = new Node();

        n.AddNode(n1);
        n1.AddNode(n);

        Assert.Single(n.NextIndexes);
    }

    //test 6
    [Fact]
    public async Task HighestCommittedIndexIncludedInAppendEntries()
    {
        Node n = new Node(10);
        var n1 = Substitute.For<INode>();

        n.AddNode(n1);

        n.State = NodeState.LEADER;
        await n.CommandReceived(new ClientCommandData(0, 5));
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        await n1.Received().AppendEntryResponse(Arg.Is<AppendEntriesDTO>(dto => dto.CommittedIndex == 0));
    }

    //test 7
    [Fact]
    public async Task FollowerLearnsOfUncommittedLog_AddToStateMachine()
    {
        Node n = new Node(0);
        Node n1 = new Node(1);

        n.AddNode(n1);
        n.Term = 1;
        n.State = NodeState.LEADER;
        await n.CommandReceived(new ClientCommandData(0, 5));
        n.Commit();

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        Assert.Equal(5, n1.StateMachine[0]);
    }

    //test 8
    [Fact]
    public async Task LeaderGetsMajorityConfirmationOfLog_GetsCommitted()
    {
        var n = new Node(0);
        var n1 = new Node(1);

        n.AddNode(n1);
        n.Term = 1;
        n.State = NodeState.LEADER;
        await n.CommandReceived(new ClientCommandData(0, 5));

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        Assert.Equal(1, n.CommittedIndex);
    }


    //test 9
    [Fact]
    public async Task LeaderCommitsByincrementingCommittedInded()
    {
        Node n = new Node(0);
        n.State = NodeState.LEADER;

        Assert.Equal(0, n.CommittedIndex);

        await n.CommandReceived(new ClientCommandData(0, 5));
        n.Commit();

        Assert.Equal(1, n.CommittedIndex);
    }

    [Fact]
    public void FollowerOrCandidateCanNotCommit()
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
    public async Task FollowerGetsAppendEntryWithLogs_AddsToPersonalLog()
    {
        Node n = new Node();
        Node n1 = new Node();

        n.AddNode(n1);
        n1.AddNode(n);
        n.State = NodeState.LEADER;

        n.Log.Add(new LogEntries(1, 0, 4));
        n.nextValue++;
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        n.Log.Add(new LogEntries(2, 0, 5));
        n.nextValue++;
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        Assert.Equal(4, n1.Log[0].value);
        Assert.Equal(5, n1.Log[1].value);
    }

    //test 11
    [Fact]
    public async Task followersResponseIncludesTermNumberAndLogEntryIndex()
    {
        Node n = new Node();

        await n.AppendEntryResponse(Arg.Any<AppendEntriesDTO>());

        //TODO: fix this test
        //Assert.Equal(0, result.term);
        //Assert.Equal(0, result.log);
    }


    //when a leader receives a majority responses from the clients after a log replication heartbeat, the leader sends a confirmation response to the client



    //test 13
    [Fact]
    public async Task WhenCommitting_ApplyToInternalStateMachine()
    {
        Node n = new Node(0);
        n.State = NodeState.LEADER;

        await n.CommandReceived(new ClientCommandData(0, 3));
        n.Commit();

        Assert.Equal(3, n.StateMachine[0]);
    }


    //test 14
    [Fact]
    public async Task WhenFollowerGetsHeartbeat_increasesCommitIndexToMatchIndexOfLeader()
    {
        Node n = new Node();
        var n1 = new Node();
        n.AddNode(n1);

        n.State = NodeState.LEADER;
        n.Term = 1;
        await n.CommandReceived(new ClientCommandData(0, 3));
        n.Commit();

        Assert.Equal(1, n.CommittedIndex);

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        Assert.Equal(1, n1.CommittedIndex);
    }

    //14.b
    [Fact]
    public async Task RejectHeartbeatWhenPreviousCommittedIndexDoesNotMatchLog()
    {
        Node n = new Node();

        n.Log.Add(new LogEntries(5, 0, 2));
        n.CommittedIndex = 1;
        
        await n.AppendEntryResponse(new AppendEntriesDTO(1, 1, 0, Arg.Any<int>(), Arg.Any<LogEntries>()));

        //TODO: fix this test
        //Assert.False(response.isValid);
    }

    [Fact]
    public async Task RejectHeartbeatWhenPreviousLogTermDoesNotMatchLog()
    {
        Node n = new Node();

        n.Log.Add(new LogEntries(5, 0, 2));
        n.Term = 5;

        await n.AppendEntryResponse(new AppendEntriesDTO(1, 1, 0, Arg.Any<int>(), Arg.Any<LogEntries>()));

        //TODO: fix this test
        //Assert.False(response.isValid);
    }

    // test 15
    [Fact]
    public async Task LeaderIncludesPreviousIndexAndCurrentTermInAppendRPC()
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
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        n.Log.Add(new LogEntries(2, 0, 98));
        n.nextValue++;
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        await n1.Received().AppendEntryResponse(Arg.Is<AppendEntriesDTO>(dto => dto.term == 2 && dto.indexTerm == 1));
    }


    // 15 1
    [Fact]
    public async Task FollowerDoesNotFindEntry_RefusesRPC()
    {
        var n = new Node();
        n.Log.Add(new LogEntries(2, 0, 5));
        n.Log.Add(new LogEntries(2, 0, 6));
        n.Term = 2;

        await n.AppendEntryResponse(new AppendEntriesDTO(0, 1, 0, 1, new LogEntries(0, 0, 0)));
        //TODO: Fix this test
        //Assert.False(badTerm.valid);
    }

    // 15 1 a
    [Fact]
    public async Task FollowerAddsLogWhenTermIsHigher()
    {
        var n = new Node();
        var n1 = Substitute.For<INode>();
        n.AddNode(n1);

        n.Log.Add(new LogEntries(3, 0, 5));
        n.nextValue++;
        n.Term = 3;
        n.State = NodeState.LEADER;
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
        Assert.Single(n1.Log);
    }


    //15 1 b
    [Fact]
    public async Task FollowerIndexIsGreaterThanLeader_LeaderDecreasesItsLocalCheck()
    {
        var n = new Node();
        var n1 = Substitute.For<INode>();

        n.Log.Add(new LogEntries(2, 0, 5));
        n.nextValue++;

        n1.Log = [new LogEntries(2, 0, 6), new LogEntries(2, 0, 4), new LogEntries(2, 0, 5)];
        n1.nextValue = 3;

        n.AddNode(n1);

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1)); 

        Assert.Equal(2, n.NextIndexes[0]);
        Assert.Equal(2, n1.nextValue);
    }


    //if leader index is less, followers delete
    //if a follower rejects the AppendEntries RPC, the leader decrements nextIndex and retries the AppendEntries RPC


    //test 16
    //this test isn't testing non responses - its testing failing responses. I'm not sure how I can test non response with my current implementation
    [Fact]
    public async Task LeaderSendsHeartbeat_DoesNotRecieveResponseFromMajority_RemainsUncommitted()
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
        await n1.AppendEntries(new AppendEntriesData(-1, -1));

        Assert.Equal(0, n.CommittedIndex);
    }

    //test 17
    // same as above, not sure how to test unresponsive
    [Fact]
    public void LeaderDoesNotGetResponse_ContinuesToSend()
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
