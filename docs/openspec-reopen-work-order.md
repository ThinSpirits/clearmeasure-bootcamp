# OpenSpec: Add Ability to Reopen a Work Order by Assignee

## GitHub Issue
Issue #6 - Add ability to reopen a work order by assignee

## Problem Statement
Once a work order reaches a terminal status (Complete or Cancelled), it cannot be reopened. The Assignee needs the ability to reopen a work order, transitioning it back to InProgress.

## Current State Machine
```
Draft --[Assign]--> Assigned --[Begin]--> InProgress --[Complete]--> Complete
                       |                      |
                       |                      +--[Shelve]--> Assigned
                       +--[Cancel]--> Cancelled
```

## Proposed State Machine (additions in **)
```
Draft --[Assign]--> Assigned --[Begin]--> InProgress --[Complete]--> Complete
                       |                      |                         |
                       |                      +--[Shelve]--> Assigned   |
                       +--[Cancel]--> Cancelled                         |
                                        |                               |
                                        +------**[Reopen]**------>  InProgress
                                                                    <--**[Reopen]**--+
```

Two new transitions:
- Complete --> InProgress (via Reopen, by Assignee)
- Cancelled --> InProgress (via Reopen, by Assignee)

## Acceptance Criteria
1. An Assignee can reopen a work order that is in Complete status
2. An Assignee can reopen a work order that is in Cancelled status
3. The work order status transitions to InProgress when reopened
4. Only the Assignee of the work order can perform this action
5. The CompletedDate is cleared when a work order is reopened
6. Unit tests cover the new reopen logic

## Design Decisions

### Two State Commands
Since the state command pattern validates that `WorkOrder.Status == GetBeginStatus()`, a single command can only match one starting status. We need two commands:
- `CompleteToInProgressCommand` - transitions Complete --> InProgress
- `CancelledToInProgressCommand` - transitions Cancelled --> InProgress

Both share the same verb "Reopen" and authorization rule (Assignee only).

### Side Effects on Execute
- **CompletedDate**: Cleared (set to null) on both reopen transitions. The Complete state sets it; reopening should clear it.
- **Assignee**: Preserved. Unlike Cancel (which clears Assignee), reopening keeps the existing Assignee since they are the one initiating the reopen.
- **AssignedDate**: Preserved for Complete-->InProgress (it was set during the original Assign). For Cancelled-->InProgress, the Assignee may have been cleared by the Cancel command, but the issue states "only the Assignee can reopen" which implies the Assignee reference must still exist. We will re-set AssignedDate for the Cancelled-->InProgress path since Cancel clears it.

### Note on Cancelled State
The `AssignedToCancelledCommand` clears both `Assignee` and `AssignedDate`. For a Cancelled work order to be reopened by its Assignee, the Assignee must still be known. This creates a design tension. Since the issue explicitly states the Assignee can reopen a Cancelled work order, we need to handle this. The `CancelledToInProgressCommand` will need to accept the current user as the new Assignee reference and restore AssignedDate.

However, looking at the existing Cancel command: it sets `Assignee = null`. This means after cancellation, `WorkOrder.Assignee` is null, and the `UserCanExecute` check `currentUser == WorkOrder.Assignee` would always fail. 

**Resolution**: For the `CancelledToInProgressCommand`, we'll use `currentUser == WorkOrder.Creator` as the authorization check instead, since the Creator is the one who cancelled and it's the only known reference. The Execute method will need to restore the Assignee to the current user and set AssignedDate.

Actually, re-reading the issue more carefully: "Only the Assignee of the work order can perform this action". For Complete-->InProgress this works since the Assignee is preserved. For Cancelled, we have a problem since Assignee is null after cancellation.

**Final Resolution**: For `CancelledToInProgressCommand`, we'll authorize based on `currentUser == WorkOrder.Creator` (since that's the only party that can cancel, and the Assignee was cleared). During Execute, we'll set the Assignee to the current user. This is the most pragmatic interpretation that preserves the spirit of the feature.

## Files to Create/Modify

### New Files
1. `src/Core/Model/StateCommands/CompleteToInProgressCommand.cs` - State command for Complete --> InProgress
2. `src/Core/Model/StateCommands/CancelledToInProgressCommand.cs` - State command for Cancelled --> InProgress
3. `src/UnitTests/Core/Model/StateCommands/CompleteToInProgressCommandTests.cs` - Tests for Complete reopen
4. `src/UnitTests/Core/Model/StateCommands/CancelledToInProgressCommandTests.cs` - Tests for Cancelled reopen

### Modified Files
5. `src/Core/Services/Impl/StateCommandList.cs` - Register the two new commands
6. `src/UnitTests/Core/Services/StateCommandListTests.cs` - Update command count assertion and add type checks

## Test Plan
Each new command gets 4 tests following the existing pattern:
- `ShouldNotBeValidInWrongStatus` - wrong starting status
- `ShouldNotBeValidWithWrongEmployee` - wrong user
- `ShouldBeValid` - valid scenario
- `ShouldTransitionStateProperly` - execute and verify end state + side effects

StateCommandList tests updated to verify 8 total commands (was 6).
