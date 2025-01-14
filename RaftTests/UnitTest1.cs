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
        int votes = n.CountVotes();
        Assert.Equal(1, votes);
        Assert.Equal(NodeState.LEADER, n.State);
    }

    //test 8 multi
    [Fact]
    public void givenThreeNodeElection_TurnLeaderWithThreeVotes()
    {

        Assert.False(true);
    }

    //test 12
    //test 13
    //test 14
    //test 15
    //test 17
    //test 18
    //test 1
}