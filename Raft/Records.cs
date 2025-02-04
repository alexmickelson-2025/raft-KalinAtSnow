

namespace Raft;

public record LogEntries(int term, int key, int value);
public record AppendEntriesData(int Term, int LeaderId);
public record AppendEntriesDTO(int leaderId, int term, int CommittedIndex, int indexTerm, LogEntries? logValue);
public record VoteResponseData(int id, int term);
public record ClientCommandData(int setKey, int setValue);
public record NodeData(
    int Id,
    bool Status,
    int ElectionTimeout,
    int Term,
    int CurrentTermLeader,
    int CommittedEntryIndex,
    List<LogEntries> Log,
    NodeState State,
    double NodeIntervalScalar
);