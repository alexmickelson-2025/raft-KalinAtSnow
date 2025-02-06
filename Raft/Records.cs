

namespace Raft;

public record LogEntries(int term, int key, int value);
public record AppendEntriesData(int Term, int LeaderId, int nextValue, int CommittedIndex, LogEntries log);
public record AppendEntriesDTO(int leaderId, int term, int CommittedIndex, int indexTerm, LogEntries? logValue);
public record VoteResponseData(int LeaderId, int Term);
public record VoteRequestData(bool VoteStatus, int NodeId, int Term);
public record ClientCommandData(int setKey, int setValue);
public record NodeData(
    int Id,
    bool Status,
    int ElectionTimeout,
    int Term,
    int CurrentTermLeader,
    int CommittedEntryIndex,
    List<LogEntries> Log,
    NodeState State
);