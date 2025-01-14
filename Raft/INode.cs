using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft;

public interface INode
{
    public int CountVotes();
    public void BecomeCandidate();
    public void StartElection();
}
