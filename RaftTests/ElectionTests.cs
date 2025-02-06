using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Raft;
namespace RaftTests;

public class ElectionTests
{
    //test 3
    [Fact]
    public void agivenNodeStarts_shouldBeFollower()
    {
        Node n = new();
        Assert.Equal(NodeState.FOLLOWER, n.State);
    }

    //test 11
    [Fact]
    public async Task bgivenNodeTurnsToCandidate_VotesForItself()
    {
        Node n = new();
        await n.BecomeCandidate();
        Assert.Equal(n.Id, n.VotedId);
    }

    //test 8 single
    [Fact]
    public async Task cgivenOneNodeElection_TurnLeaderWithOneVote()
    {
        Node n = new();
        await n.StartElection();
        await n.LeaderCheck();
        Assert.Equal(NodeState.LEADER, n.State);
    }

    //test 8 multi
    //TODO update this test to not use node.___ for validation
    [Fact]
    public async Task dgivenThreeNodeElection_TurnLeaderWithThreeVotes()
    {
        Node n = new(0);
        var n1 = Substitute.For<INode>();
        var n2 = Substitute.For<INode>();
        n2.Id = 2;
        n1.Id = 1;

        n.AddNode(n1);
        n.AddNode(n2);

        await n.StartElection();

        await n.RespondVote(new VoteRequestData(true, 1, 1));
        await n.RespondVote(new VoteRequestData(true, 2, 1));

        Assert.Equal(NodeState.LEADER, n.State);
        Assert.Equal(n.Id, n1.LeaderId);
        Assert.Equal(n.Id, n1.VotedId);
    }

    //test 19
    //how do you get this to pass as a substitute?
    [Fact]
    public async Task eturnToLeader_SendHeartbeat()
    {
        Node n = new(0);
        var n1 = Substitute.For<INode>();
        var n2 = Substitute.For<INode>();
        n2.Id = 2;
        n1.Id = 1;

        n.AddNode(n1);
        n.AddNode(n2);

        await n.StartElection();

        await n.RespondVote(new VoteRequestData(true, 1, 1));
        await n.RespondVote(new VoteRequestData(true, 2, 1));


        Assert.Equal(NodeState.LEADER, n.State);

        await n1.Received().AppendEntries(Arg.Any<AppendEntriesData>());
        await n2.Received().AppendEntries(Arg.Any<AppendEntriesData>());
    }

    //test 2
    [Fact]
    public async Task fgivenAppendEntryRecieved_UnderstandTheSenderAsLeaderIfTermIsHigher()
    {
        //node acting as a follower
        Node n = new(1);

        Assert.Equal(0, n.LeaderId);

        await n.AppendEntries(new AppendEntriesData(3,2,n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));

        Assert.Equal(2, n.LeaderId);
    }

    //test 6
    [Fact]
    public async Task gtermsIncrementWhenNewCandidacy()
    {
        Node n = new();

        Assert.Equal(0, n.Term);

        await n.StartElection();

        Assert.Equal(1, n.Term);
    }

    [Fact]
    public async Task htermValueAreTakenFromLeaderWhenNewNodeComesOnline()
    {
        Node n = new();
        await n.StartElection();

        Assert.Equal(1, n.Term);

        //new node comes online
        Node n1 = new(1);

        n.nodes.Add(n1);
        n1.nodes.Add(n);

        await n1.AppendEntries(new AppendEntriesData(n.Term,n.Id,n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));
        Assert.Equal(1, n1.Term);
    }

    //test 12
    [Fact]
    public async Task icandidateStartsElectionWtihHigherTermLeaderPresent_RevertsToFollower()
    {
        Node n = new();
        n.Term = 3;
        n.State = NodeState.LEADER;

        Node n1 = new(2);
        
        n.AddNode(n1);
        n1.AddNode(n);

        await n1.BecomeCandidate();

        Assert.Equal(NodeState.FOLLOWER, n1.State);
    }

    //test 12
    [Fact]
    public async Task jcandidateGetsAppendEntryFromHigherTermLeader_RevertsToFollower()
    {   
        //Term 1 - n leader
        Node n = new();
        await n.StartElection();

        Node n1 = new(1);
        n.AddNode(n1);
        n1.AddNode(n);
        await n1.AppendEntries(new AppendEntriesData(n.Term,n.Id,n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));

        //Term 2 - n1 leader
        await n1.StartElection();
        Assert.Equal(NodeState.LEADER, n1.State);
        Assert.Equal(2, n1.Term);

        //restarts or comes online late
        Node n2 = new(2);
        n.AddNode(n2);
        n1.AddNode(n2);
        n2.AddNode(n);
        n2.AddNode(n1);

        //election gives it Term 1 (less than current) heartbeat occurs - shouldn't go through and reverts with current term (2)
        await n2.StartElection();
        await n2.AppendEntries(new AppendEntriesData(n1.Term,n1.Id, n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));
        Assert.Equal(2, n2.Term);
        Assert.Equal(NodeState.FOLLOWER, n2.State);
        Assert.Equal(NodeState.LEADER, n1.State);
    }

    //test 13
    [Fact]
    public async Task kcandidateGetsAppendEntryFromHigherTermLeader_revertsToFollower()
    {
        //Term 1
        Node n = new();
        await n.StartElection();
        await n.LeaderCheck();
        
        //force it to be higher
        n.Term = 2;

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        //Election for n1 term 1
        await n1.StartElection();

        //Heartbeat from n at term 2
        await n1.AppendEntries(new AppendEntriesData(n.Term,n.Id, n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));

        Assert.Equal(NodeState.LEADER, n.State);
        Assert.Equal(NodeState.FOLLOWER, n1.State);
        Assert.Equal(2, n1.Term);
    }

    //test 9
    [Fact]
    public async Task lunresponsiveNodeDoesNotCancelVoteIfThereIsMajority()
    {
        Node n = new();
        Node n1 = new(1);
        Node n2 = new(2);

        n.AddNode(n2);
        n2.AddNode(n);

        await n.BecomeCandidate();
  
        //n1 presumed unresponsive - not sure how do this better

        await n.LeaderCheck();
        Assert.Equal(NodeState.LEADER, n.State);
    }

    //test 10
    [Fact]
    public async Task mfollowerHasNotVoted_isBehindATerm_WhenAskedForVote_RespondsToHighestTermCandidate()
    {
        //Term 1
        Node n = new();
        n.Term = 1;

        Node n1 = new(1);
        n1.Term = 2;

        //restarts or comes online late 
        Node n2 = new(2);
        n.AddNode(n2);
        n1.AddNode(n2);
        n2.AddNode(n);
        n2.AddNode(n1);

        //term 3
        await n1.StartElection();
        //term 2
        await n.StartElection();

        Assert.Equal(n1.Id, n2.VotedId);
        Assert.Equal(n1.Term, n2.Term);
    }

    //test 5
    [Fact]
    public void nRandomValuesForElectionTimeouts()
    {
        Node n = new();

        for (int i = 0; i < 10; i++)
        {
            n.RefreshTimer();
            int oldValue = n.ElectionTimeout;
            n.RefreshTimer();
            Assert.NotEqual(oldValue, n.ElectionTimeout);
        }
    }

    //test 14
    [Fact]
    public async Task oVotingForTheSameTermShouldReturnFalse()
    {
        Node n = new(1);
        var n1 = Substitute.For<INode>();
        n1.Term = 1;

        n.AddNode(n1);

        await n.RequestVote(new VoteResponseData(1,1));
        await n1.Received().RespondVote(new VoteRequestData(false, n1.Id,1));
    }

    //test 15
    //----------------------------------------------------------------------------------------------------------
    [Fact]
    public async Task pGivenNodeEntersElectionsTwice_ShouldVoteForBoth_EndingWithStatsOfSecond()
    {
        Node node = new(1);
        Node n1 = new(2);

        node.AddNode(n1);

        await node.StartElection();
        await node.StartElection();

        Assert.Equal(1, n1.VotedId);
        Assert.Equal(2, n1.Term);
    }

    //test 18
    [Fact]
    public async Task rgivenCandidateRecievesAppendEntryFromPreviousTerm_RejectAndContinueWithCandidacy()
    {
        Node node = new(1);
        var n1 = Substitute.For<INode>();

        node.AddNode(n1);

        node.Term = 3;

        await node.StartElection();
        await n1.AppendEntries(new AppendEntriesData(1,1, node.nextValue, node.CommittedIndex, new LogEntries(-1, -1, -1)));

        Assert.Equal(NodeState.CANDIDATE, node.State);
    }

    //test 17
    [Fact]
    public async Task sGetAResponseFromAppendEntry()
    {
        Node n = new();
        var n1 = Substitute.For<Node>();

        n.nodes.Add(n1);

        await n1.AppendEntries(new AppendEntriesData(-1, -1, n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));

        await n1.Received().AppendEntryResponse(Arg.Any<AppendEntriesDTO>());
    }

    //test 7
    [Fact]
    public async Task tElectionTimerResetsWhenAppendEntryCalled()
    {
        Node n = new();
        var n1 = Substitute.For<Node>();

        n.nodes.Add(n1);

        await n1.AppendEntries(new AppendEntriesData(-1, -1, n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));

        n1.Received().RefreshTimer();
    }

    //test 4
    [Fact]
    public void uFollowerDoesNotGetCommunicationFor300ms_StartsElection()
    {
        Node n = new();
        Thread t = n.Start();

        Thread.Sleep(300);

        n.running = false;
        t.Join();

        Assert.True(n.Term >= 1);
    }


    //test 1
    [Fact]
    public async Task vLeadersSendAppendEntriesEvery10ms()
    {
        Node n = new();
        await n.StartElection();
        Thread t = n.Start();

        var n1 = Substitute.For<Node>();
        n1.nodes.Add(n);
        n.nodes.Add(n1);

        Thread.Sleep(25);

        n.running = false;
        t.Join();

        await n1.AppendEntries(new AppendEntriesData(-1, -1, n.nextValue, n.CommittedIndex, new LogEntries(-1, -1, -1)));
        Assert.False(true);
    }

    //test 16
    [Fact]
    public async Task wCandidatesStartNewElectionWhenTimerRunsOut()
    {
        Node n = new();
        await n.StartElection();

        Thread t = n.Start();

        Thread.Sleep(301);

        n.running = false;
        t.Join();

        Assert.True(n.Term > 1);
    }
}