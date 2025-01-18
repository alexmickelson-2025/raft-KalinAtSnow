
namespace Raft
{
    public interface INode
    {
        int LeaderId { get; }
        NodeState State { get; set; }
        int VotedId { get; set; }
        int VotedTerm { get; set; }

        Task AppendEntries();
        Task AppendEntryResponse(int id, int term);
        Task BecomeCandidate();
        Task LeaderCheck();
        Task RefreshTimer();
        Task RespondVote(int id, int term);
        Thread Start();
        Task StartElection();
    }
}