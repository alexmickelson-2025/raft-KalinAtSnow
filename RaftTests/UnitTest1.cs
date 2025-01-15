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
    public void givenTurnToLeader_SendHeartbeat()
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

}