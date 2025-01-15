namespace Raft
{
    public interface INode
    {
        int? LeaderId { get; }
        NodeState State { get; set; }
        int VotedId { get; set; }

        void AppendEntries();
        void BecomeCandidate();
        void LeaderCheck();
        void StartElection();
    }
}