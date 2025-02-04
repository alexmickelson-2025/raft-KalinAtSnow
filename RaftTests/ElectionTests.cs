using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Raft;
namespace RaftTests;

public class ElectionTests
{
    //test 3
    [Fact]
    public void givenNodeStarts_shouldBeFollower()
    {
        Node n = new();
        Assert.Equal(NodeState.FOLLOWER, n.State);
    }

    //test 11
    [Fact]
    public async Task givenNodeTurnsToCandidate_VotesForItself()
    {
        Node n = new();
        await n.BecomeCandidate();
        Assert.Equal(0, n.VotedId);
    }

    //test 8 single
    [Fact]
    public async Task givenOneNodeElection_TurnLeaderWithOneVote()
    {
        Node n = new();
        await n.StartElection();
        Assert.Equal(NodeState.LEADER, n.State);
    }

    //test 8 multi
    //TODO update this test to not use node.___ for validation
    [Fact]
    public async Task givenThreeNodeElection_TurnLeaderWithThreeVotes()
    {
        Node n = new();
        var n1 = Substitute.For<INode>();
        var n2 = Substitute.For<INode>();

        n.nodes = [n1, n2];

        await n.StartElection();

        Assert.Equal(NodeState.LEADER, n.State);
    }

    //test 19
    [Fact]
    public async Task turnToLeader_SendHeartbeat()
    {
        Node n = new(0);
        var n1 = Substitute.For<INode>();
        var n2 = Substitute.For<INode>();

        n.nodes = [n1, n2];

        await n.StartElection();
        Assert.Equal(NodeState.LEADER, n.State);

        await n1.Received().AppendEntries(Arg.Any<AppendEntriesData>());
        await n2.Received().AppendEntries(Arg.Any<AppendEntriesData>());
    }

    //test 2
    [Fact]
    public async Task givenAppendEntryRecieved_UnderstandTheSenderAsLeaderIfTermIsHigher()
    {
        //node acting as a follower
        Node n = new(1);

        Assert.Equal(0, n.LeaderId);

        //term 3, leader 2
        await n.AppendEntries(new AppendEntriesData(3,2));

        Assert.Equal(2, n.LeaderId);
    }

    //test 6
    [Fact]
    public async Task termsIncrementWhenNewCandidacy()
    {
        Node n = new();

        Assert.Equal(0, n.Term);

        await n.StartElection();

        Assert.Equal(1, n.Term);
    }

    [Fact]
    public async Task termValueAreTakenFromLeaderWhenNewNodeComesOnline()
    {
        Node n = new();
        await n.StartElection();

        Assert.Equal(1, n.Term);

        //new node comes online
        Node n1 = new(1);

        n.nodes.Add(n1);
        n1.nodes.Add(n);

        await n1.AppendEntries(new AppendEntriesData(n.Term,n.Id));
        Assert.Equal(1, n1.Term);
    }

    //test 12
    //----------------------------------------------------------------------------------------------------------
    [Fact]
    public async Task candidateStartsElectionWtihHigherTermLeaderPresent_RevertsToFollower()
    {
        //Term 1
        Node n = new();
        await n.StartElection();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1,-1));

        //Term 2
        await n1.StartElection();
        Assert.Equal(NodeState.LEADER, n1.State);
        Assert.Equal(2, n1.Term);

        //restarts or comes online late
        Node n2 = new(2);
        n.nodes.Add(n2);
        n1.nodes.Add(n2);
        n2.nodes = [n, n1];

        //election gives it Term 1 (less than current) before the heartbeat to bring it to speed occurs - shouldn't go through and reverts
        await n2.StartElection();
        Assert.Equal(1, n2.Term);
        Assert.Equal(NodeState.FOLLOWER, n2.State);
        Assert.Equal(NodeState.LEADER, n1.State);
    }

    //test 12
    [Fact]
    public async Task candidateGetsAppendEntryFromHigherTermLeader_RevertsToFollower()
    {
        //Term 1
        Node n = new();
        await n.StartElection();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1,-1));

        //Term 2
        await n1.StartElection();
        Assert.Equal(NodeState.LEADER, n1.State);
        Assert.Equal(2, n1.Term);

        //restarts or comes online late
        Node n2 = new(2);
        n.nodes.Add(n2);
        n1.nodes.Add(n2);
        n2.nodes = [n, n1];

        //election gives it Term 1 (less than current) heartbeat occurs - shouldn't go through and reverts with current term (2)
        await n2.StartElection();
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1,-1));
        Assert.Equal(2, n2.Term);
        Assert.Equal(NodeState.FOLLOWER, n2.State);
        Assert.Equal(NodeState.LEADER, n1.State);
    }

    //test 13
    [Fact]
    public async Task candidateGetsAppendEntryFromHigherTermLeader_revertsToFollower()
    {
        //Term 1
        Node n = new();
        await n.StartElection();
        
        //force it to be higher
        n.Term = 2;

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        //Election for n1 term 1
        await n1.StartElection();

        //Heartbeat from n at term 2
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1,-1));

        Assert.Equal(NodeState.LEADER, n.State);
        Assert.Equal(NodeState.FOLLOWER, n1.State);
        Assert.Equal(2, n1.Term);
    }

    //test 9
    [Fact]
    public async Task unresponsiveNodeDoesNotCancelVoteIfThereIsMajority()
    {
        Node n = new();
        Node n1 = new(1);
        Node n2 = new(2);

        n2.State = NodeState.CANDIDATE;
        n.VotedId = 2;
        //n1 presumed unresponsive

        await n2.LeaderCheck(1);
        Assert.Equal(NodeState.LEADER, n2.State);
    }

    //test 10
    [Fact]
    public async Task followerHasNotVoted_isBehindATerm_WhenAskedForVote_RespondsToHighestTermCandidate()
    {
        //Term 1
        Node n = new();
        await n.StartElection();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));

        //Term 2
        await n1.StartElection();

        //restarts or comes online late - defaults 0 term
        Node n2 = new(2);
        n.nodes.Add(n2);
        n1.nodes.Add(n2);
        n2.nodes = [n, n1];

        await n.StartElection();

        Assert.Equal(0, n2.VotedId);
    }

    //test 5
    [Fact]
    public void RandomValuesForElectionTimeouts()
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
    public async Task VotingForTheSameTermShouldReturnFalse()
    {
        Node n = new();
        Node n1 = new(1);
        Node n2 = new(2);

        n1.nodes = [n, n2];
        n2.nodes = [n, n1];
        n.nodes = [n1, n2];

        await n.StartElection();

        Assert.Equal(n.Id, n2.VotedId);
        Assert.Equal(1, n2.VotedTerm);

        n1.Term = 0;
        await n1.StartElection();

        Assert.Equal(n.Id, n2.VotedId);
        Assert.Equal(1, n2.VotedTerm);
    }

    //test 15
    [Fact]
    public async Task GivenNodeRecievesVotesForFutureElectionAtTimeOfYounger_VotesForTheOldest()
    {
        Node n = new();
        Node n1 = new(1);
        Node n2 = new(2);

        n1.nodes = [n, n2];
        n2.nodes = [n, n1];
        n.nodes = [n1, n2];

        //term 1
        await n.StartElection();

        Assert.Equal(n.Id, n2.VotedId);
        Assert.Equal(1, n2.VotedTerm);

        //term 1
        await n1.StartElection();

        Assert.Equal(n.Id, n2.VotedId);
        Assert.Equal(1, n2.VotedTerm);

        //term 2
        await n.StartElection();

        Assert.Equal(n.Id, n2.VotedId);
        Assert.Equal(2, n2.VotedTerm);
    }

    //test 15 possible edge?
    [Fact]
    public async Task GivenNodeRecievesVotesForFutureElectionAtTimeOfYounger_VotesForTheOldest_TwoHappenBeforeTheYounger()
    {
        Node n = new();
        Node n1 = new(1);
        Node n2 = new(2);

        n1.nodes = [n, n2];
        n2.nodes = [n, n1];
        n.nodes = [n1, n2];

        //term 1
        await n.StartElection();

        Assert.Equal(0, n2.VotedId);
        Assert.Equal(1, n2.VotedTerm);

        //term 2
        await n.StartElection();

        Assert.Equal(0, n2.VotedId);
        Assert.Equal(2, n2.VotedTerm);
 
        //term 1, should be white noise
        await n1.StartElection();

        Assert.Equal(0, n2.VotedId);
        Assert.Equal(2, n2.VotedTerm);
    }

    //test 18
    [Fact]
    public async Task givenCandidateRecievesAppendEntryFromPreviousTerm_RejectAndContinueWithCandidacy()
    {
        Node n = new();
        await n.StartElection();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        n1.State = NodeState.LEADER;
        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));

        //current term that its applying for
        Assert.Equal(1, n.Term);
        //n doesn't know about a leader with the manual set
        Assert.Equal(-1, n.LeaderId);
    }

    //test 17
    [Fact]
    public async Task GetAResponseFromAppendEntry()
    {
        Node n = new();
        var n1 = Substitute.For<Node>();

        n.nodes.Add(n1);

        await n1.AppendEntries(new AppendEntriesData(-1, -1));

        await n1.Received().AppendEntryResponse(Arg.Any<AppendEntriesDTO>());
    }

    //test 7
    [Fact]
    public async Task ElectionTimerResetsWhenAppendEntryCalled()
    {
        Node n = new();
        var n1 = Substitute.For<Node>();

        n.nodes.Add(n1);

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));

        n1.Received().RefreshTimer();
    }

    //test 4
    [Fact]
    public void FollowerDoesNotGetCommunicationFor300ms_StartsElection()
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
    public async Task LeadersSendAppendEntriesEvery10ms()
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

        //TODO: update this
        await n1.AppendEntries(new AppendEntriesData(-1, -1));
    }

    //test 16
    [Fact]
    public async Task CandidatesStartNewElectionWhenTimerRunsOut()
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