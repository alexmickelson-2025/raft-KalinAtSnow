
using NSubstitute;
using Raft;

namespace RaftTests;
public class logReplication
{
    //when a leader receives a client command the leader sends the log entry in the next appendentries RPC to all nodes
    [Fact]
    public async Task CommandToLeader_SendsLogEntryToFollowers_AtRPC()
    {
        Node n = new Node();
        Node n1 = new Node();

        n.nodes.Add(n1);
        
        n.State = NodeState.LEADER;
        n.Command(5);

        await n.AppendEntries();

        Assert.Equal(5, n1.Log[0]);
    }

    //when a leader receives a command from the client, it is appended to its log
    [Fact]
    public void CommandFromClientAppendsToLeaderLog()
    {
        Node n = new Node();
        n.State = NodeState.LEADER;

        n.Command(1);

        Assert.Equal(0, n.nextValue-1);
        Assert.Equal(1, n.Log[0]);
    }

    [Fact]
    public void CommandFromClientDoesNotAppendToClientLog()
    {
        Node n = new Node();

        n.Command(1);

        Assert.Empty(n.Log);
        Assert.Empty(n.Log);
    }


    //when a node is new, its log is empty
    [Fact] 
    public void NewNodesDoNotHaveAnyLogs()
    {
        Node n = new Node();

        Assert.Empty(n.Log);
    }

    //when a leader wins an election, it initializes the nextIndex for each follower to the index just after the last one it its log
    [Fact]
    public async Task UponSuccessfulElection_LeaderInitializesNextIndexForEachFollower_IndexOneHigher()
    {
        Node n = new Node();
        Node n1 = new Node();
        Node n2 = new Node();
    
        n.nodes = [n1, n2];
        n1.nodes = [n, n2];
        n2.nodes = [n1, n];

        n.nextValue = 3;

        await n.StartElection();

        Assert.Equal(4, n1.nextValue);
        Assert.Equal(4, n2.nextValue);
    }
    //leaders maintain an "nextIndex" for each follower that is the index of the next log entry the leader will send to that follower
    //Highest committed index from the leader is included in AppendEntries RPC's
    //When a follower learns that a log entry is committed, it applies the entry to its local state machine
    //when the leader has received a majority confirmation of a log, it commits it
    //the leader commits logs by incrementing its committed log index
    //given a follower receives an appendentries with log(s) it will add those entries to its personal log
    //a followers response to an appendentries includes the followers term number and log entry index
    //when a leader receives a majority responses from the clients after a log replication heartbeat, the leader sends a confirmation response to the client
    //given a leader node, when a log is committed, it applies it to its internal state machine
    //when a follower receives a valid heartbeat, it increases its commitIndex to match the commit index of the heartbeat
        //reject the heartbeat if the previous log index / term number does not match your log
    //When sending an AppendEntries RPC, the leader includes the index and term of the entry in its log that immediately precedes the new entries
        //If the follower does not find an entry in its log with the same index and term, then it refuses the new entries
            //term must be same or newer
            //if index is greater, it will be decreased by leader
            //if index is less, we delete what we have
        //if a follower rejects the AppendEntries RPC, the leader decrements nextIndex and retries the AppendEntries RPC
    //when a leader sends a heartbeat with a log, but does not receive responses from a majority of nodes, the entry is uncommitted
    //if a leader does not response from a follower, the leader continues to send the log entries in subsequent heartbeats  
    //if a leader cannot commit an entry, it does not send a response to the client
    //if a node receives an appendentries with a logs that are too far in the future from your local state, you should reject the appendentries
    //if a node receives and appendentries with a term and index that do not match, you will reject the appendentry until you find a matching log
}
