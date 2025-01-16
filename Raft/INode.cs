
namespace Raft
{
    public interface INode
    {
        int LeaderId { get; }
        NodeState State { get; set; }
        int VotedId { get; set; }

        Task AppendEntries();
        Task BecomeCandidate();
        Task LeaderCheck();
        Task RefreshTimer();
        Task StartElection();
    }
}