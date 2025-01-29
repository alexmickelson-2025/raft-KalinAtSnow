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
    [Fact]
    public async Task WhenLeaderInElection_GetsPaused_OtherNodesDoNotGetHeartbeat()
    {
        Node n = new Node();
        var n1 = Substitute.For<INode>();

        n.AddNode(n1);

        n.State = NodeState.LEADER;
        await n.LeaderCheck();

        n.Start();
        n.PauseToggle();

        Thread.Sleep(400);

        n1.ClearReceivedCalls();

        Thread.Sleep(400);

        n1.DidNotReceive().AppendEntryResponse(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<(int, int)>());
    }
}
