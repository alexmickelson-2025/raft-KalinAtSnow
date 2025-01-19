
namespace Raft
{
    public interface INode
    {
        int LeaderId { get; }
        NodeState State { get; set; }
        int VotedId { get; set; }
        int VotedTerm { get; set; }
        int Term { get; set; }
        int ElectionTimeout { get; set; }
        bool running { get; set; }
        int _id { get; set; }
        int electionMultiplier { get; set; }
         int networkRespondDelay { get; set; }
         int networkSendDelay { get; set; }

        Task AppendEntries();
        Task AskForVote(int id, int term);
        Task AppendEntryResponse(int id, int term);
        Task BecomeCandidate();
        Task LeaderCheck();
        Task RefreshTimer();
        Task RespondVote(int id, int term);
        Thread Start();
        Task StartElection();
    }
}