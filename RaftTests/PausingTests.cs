using NSubstitute;
using NSubstitute.Core;
using Raft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftTests;
public class PausingTests
{
    /*
     1. when node is a leader with an election loop, then they get paused, other nodes do not get heartbeast for 400 ms
2. when node is a leader with an election loop, then they get paused, other nodes do not get heartbeast for 400 ms, then they get un-paused and heartbeats resume
3. When a follower gets paused, it does not time out to become a candidate
4. When a follower gets unpaused, it will eventually become a candidate.
    */
    [Fact]
    public async Task WhenLeaderInElection_GetsPaused_OtherNodesDoNotGetHeartbeat()
    {
        Node n = new Node();
        var n1 = Substitute.For<INode>();

        n.AddNode(n1);

        n.State = NodeState.LEADER;
        //await n.LeaderCheck();

        n.Start();
        n.PauseToggle();

        Thread.Sleep(400);

        n1.ClearReceivedCalls();

        Thread.Sleep(400);

        await n1.DidNotReceive().AppendEntryResponse(Arg.Any<AppendEntriesDTO>());
    }
}
