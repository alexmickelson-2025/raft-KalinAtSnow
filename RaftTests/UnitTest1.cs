using Raft;
namespace RaftTests;

public class UnitTest1
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
    public void givenNodeTurnsToCandidate_VotesForItself()
    {
        Node n = new();
        n.BecomeCandidate();
        Assert.Equal(0, n.VotedId);
    }

    //test 8 single
    [Fact]
    public void givenOneNodeElection_TurnLeaderWithOneVote()
    {
        Node n = new();
        n.StartElection();
        Assert.Equal(NodeState.CANDIDATE, n.State);
        n.LeaderCheck();
        Assert.Equal(NodeState.LEADER, n.State);
    }

    //test 8 multi
    [Fact]
    public void givenThreeNodeElection_TurnLeaderWithThreeVotes()
    {
        Node n = new();
        Node n1 = new();
        Node n2 = new();

        n.nodes = [n1, n2];
        n1.nodes = [n, n2];
        n2.nodes = [n, n2];

        n.StartElection();
        Assert.Equal(NodeState.CANDIDATE, n.State);

        n.LeaderCheck();
        Assert.Equal(NodeState.LEADER, n.State);
    }

    //test 19
    [Fact]
    public void turnToLeader_SendHeartbeat()
    {
        Node n = new(0);
        Node n1 = new(1);
        Node n2 = new(2);

        n.nodes = [n1, n2];
        n1.nodes = [n, n2];
        n2.nodes = [n, n1];

        Assert.Null(n.LeaderId);
        Assert.Null(n1.LeaderId);
        Assert.Null(n2.LeaderId);

        n2.StartElection();
        n2.LeaderCheck();

        Assert.Equal(2, n.LeaderId);
        Assert.Equal(2, n1.LeaderId);
        Assert.Equal(2, n2.LeaderId);
    }

    //test 2
    //this test itself might have flaws - by my understanding, we don't want any old appendentry to mark as leader only leaders should be able to. how its phrased in the 19 it sounds like any can send
    [Fact]
    public void givenAppendEntryRecieved_UnderstandTheSenderAsLeader()
    {
        Node n = new(0);
        Node n1 = new(1);

        n.nodes = [n1];
        n1.nodes = [n];

        Assert.Null(n.LeaderId);
        Assert.Null(n1.LeaderId);

        n1.AppendEntries();

        Assert.Equal(1, n.LeaderId);
        Assert.Null(n1.LeaderId);

        n.AppendEntries();

        Assert.Equal(1, n.LeaderId);
        Assert.Equal(0, n1.LeaderId);
    }


    //test 6
    [Fact]
    public void termsIncrementWhenNewCandidacy()
    {
        Node n = new();

        Assert.Equal(0, n.Term);

        n.StartElection();

        Assert.Equal(1, n.Term);
    }

    [Fact]
    public void termValueAreTakenFromLeaderWhenNewNodeComesOnline()
    {
        Node n = new();
        n.StartElection();
        n.LeaderCheck();

        Assert.Equal(1, n.Term);

        //new node comes online
        Node n1 = new(1);

        n.nodes.Add(n1);
        n1.nodes.Add(n);

        //heartbeat to show leader to new node
        n.AppendEntries();
        Assert.Equal(1, n1.Term);
    }

    //test 12
    [Fact]
    public void candidateStartsElectionWtihHigherTermLeaderPresent_RevertsToFollower()
    {
        //Term 1
        Node n = new();
        n.StartElection();
        n.LeaderCheck();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        n.AppendEntries();

        //Term 2
        n1.StartElection();
        n1.LeaderCheck();
        Assert.Equal(NodeState.LEADER, n1.State);
        Assert.Equal(2, n1.Term);

        //restarts or comes online late
        Node n2 = new(2);
        n.nodes.Add(n2);
        n1.nodes.Add(n2);
        n2.nodes = [n, n1];

        //election gives it Term 1 (less than current) before the heartbeat to bring it to speed occurs - shouldn't go through and reverts
        n2.StartElection();
        Assert.Equal(1, n2.Term);
        Assert.Equal(NodeState.FOLLOWER, n2.State);
        Assert.Equal(NodeState.LEADER, n1.State);
    }

    //test 12
    [Fact]
    public void candidateGetsAppendEntryFromHigherTermLeader_RevertsToFollower()
    {
        //Term 1
        Node n = new();
        n.StartElection();
        n.LeaderCheck();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        n.AppendEntries();

        //Term 2
        n1.StartElection();
        n1.LeaderCheck();
        Assert.Equal(NodeState.LEADER, n1.State);
        Assert.Equal(2, n1.Term);

        //restarts or comes online late
        Node n2 = new(2);
        n.nodes.Add(n2);
        n1.nodes.Add(n2);
        n2.nodes = [n, n1];

        //election gives it Term 1 (less than current) heartbeat occurs - shouldn't go through and reverts with current term (2)
        n2.StartElection();
        n1.AppendEntries();
        Assert.Equal(2, n2.Term);
        Assert.Equal(NodeState.FOLLOWER, n2.State);
        Assert.Equal(NodeState.LEADER, n1.State);
    }

    //test 13
    [Fact]
    public void candidateGetsAppendEntryFromHigherTermLeader_revertsToFollower()
    {
        Node n = new();
        n.StartElection();
        n.LeaderCheck();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        //start election followed by heartbeat (election gets term of new node to 1)
        n1.StartElection();
        n.AppendEntries();
        
        //check to see if old election passed
        n1.LeaderCheck();

        Assert.Equal(NodeState.LEADER, n.State);
        Assert.Equal(NodeState.FOLLOWER, n1.State);
        Assert.Equal(1, n1.Term);
    }

    //test 9
    [Fact]
    public void unresponsiveNodeDoesNotCancelVoteIfThereIsMajority()
    {
        Node n = new();
        Node n1 = new(1);
        Node n2 = new(2);

        n2.State = NodeState.CANDIDATE;
        n.VotedId = 2;
        //n1 presumed unresponsive

        n2.LeaderCheck();
        Assert.Equal(NodeState.LEADER, n2.State);
    }

    //test 10
    [Fact]
    public void followerHasNotVoted_isBehindATerm_WhenAskedForVote_RespondsToHighestTermCandidate()
    {
        //Term 1
        Node n = new();
        n.StartElection();
        n.LeaderCheck();

        Node n1 = new(1);
        n.nodes.Add(n1);
        n1.nodes.Add(n);

        n.AppendEntries();

        //Term 2
        n1.StartElection();
        n1.LeaderCheck();

        //restarts or comes online late - defaults 0 term
        Node n2 = new(2);
        n.nodes.Add(n2);
        n1.nodes.Add(n2);
        n2.nodes = [n, n1];

        //asks for vote internally
        n.StartElection();

        Assert.Equal(0, n2.VotedId);
    }
}