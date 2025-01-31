
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
        public List<(int term, int command)> Log { get; set; }
        int nextValue { get; set; }
        Dictionary<int, int> NextIndexes { get; set; }
        public int CommittedIndex { get; set; }
        Dictionary<int, int> StateMachine { get; set; }

        void Commit();
        Task AppendEntries();
        Task AskForVote(int id, int term);
        (int TermNumber, int LogIndex, bool valid) AppendEntryResponse(int leaderId, int term, int CommittedIndex, int indexTerm, (int term, int command) logValue);
        Task BecomeCandidate();
        Task LeaderCheck();
        void RefreshTimer();
        void AddNode(INode node);
        Task RespondVote(int id, int term);
        void CommandReceived(int setKey, int setValue);
        Thread Start();
        Task StartElection();
    }
}