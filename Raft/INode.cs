﻿
namespace Raft
{
    public interface INode
    {
        int LeaderId { get; }
        NodeState State { get; set; }
        int VotedId { get; set; }
        //int VotedTerm { get; set; }
        int Term { get; set; }
        //int ElectionTimeout { get; set; }
        bool running { get; set; }
        int Id { get; set; }
        int electionMultiplier { get; set; }
         int networkRespondDelay { get; set; }
         int networkSendDelay { get; set; }
       public List<LogEntries> Log { get; set; }
        int nextValue { get; set; }
       // Dictionary<int, int> NextIndexes { get; set; }
        //public int CommittedIndex { get; set; }
        //Dictionary<int, int> StateMachine { get; set; }
       // double NodeIntervalScalar { get; set; }

        //void Commit();
        Task AppendEntries(AppendEntriesData data);
        Task AppendEntryResponse(AppendEntriesDTO dto);
        //Task BecomeCandidate();
        //void RefreshTimer();
        //void AddNode(INode node);
        Task RespondVote(VoteRequestData voteData);
        Task CommandReceived(ClientCommandData commandData);
        //Thread Start();
        Task RequestVote(VoteResponseData voteData);
        //Task StartElection();
    }
}