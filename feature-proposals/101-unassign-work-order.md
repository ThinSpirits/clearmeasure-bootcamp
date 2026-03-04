## Why
Currently, once a work order is assigned to an Employee, there is no way to remove the assignment without reassigning to another Employee or cancelling the work order entirely. Cancelling destroys the work order's lifecycle. A dedicated unassign capability lets the Creator clear the assignment and return the work order to Draft status, allowing a fresh assignment decision without losing the work order.

## What Changes
- Add `AssignedToDraftCommand` state command in `src/Core/Model/StateCommands/` that transitions from Assigned to Draft
- Add `InProgressToDraftCommand` state command in `src/Core/Model/StateCommands/` that transitions from InProgress to Draft
- Both commands clear the Assignee, AssignedDate, and set status back to Draft
- Only the Creator of the work order can execute these commands
- Register both commands in `StateCommandList.GetAllStateCommands()`
- The UI already dynamically renders valid command buttons, so "Unassign" will appear automatically when valid

## Capabilities
### New Capabilities
- Creators can unassign a work order that is in Assigned status, returning it to Draft
- Creators can unassign a work order that is in InProgress status, returning it to Draft
- Unassignment clears the Assignee and AssignedDate fields

### Modified Capabilities
- `StateCommandList` includes the two new unassign commands in its command registry
- WorkOrderManage page will automatically show "Unassign" button for Assigned/InProgress work orders when the current user is the Creator (no UI code changes needed due to dynamic button rendering)

## Impact
- **Core** — Two new state commands (`AssignedToDraftCommand`, `InProgressToDraftCommand`) following the existing `StateCommandBase` pattern
- **Core** — `StateCommandList` updated to register the new commands
- **DataAccess** — No changes needed; the existing `StateCommandHandler` handles all `StateCommandBase` subclasses via MediatR
- **UI.Shared** — No changes needed; the `WorkOrderManage.razor` page dynamically renders buttons for all valid commands
- **Database** — No schema changes needed; Assignee and AssignedDate are already nullable columns

## Acceptance Criteria
### Unit Tests
- `AssignedToDraftCommand_ShouldNotBeValidInWrongStatus` — verify command is invalid when work order is not in Assigned status
- `AssignedToDraftCommand_ShouldNotBeValidWithWrongEmployee` — verify command is invalid when current user is not the Creator
- `AssignedToDraftCommand_ShouldBeValid` — verify command is valid when status is Assigned and user is Creator
- `AssignedToDraftCommand_ShouldTransitionStateProperly` — verify status changes to Draft, Assignee is null, AssignedDate is null
- `InProgressToDraftCommand_ShouldNotBeValidInWrongStatus` — verify command is invalid when work order is not in InProgress status
- `InProgressToDraftCommand_ShouldNotBeValidWithWrongEmployee` — verify command is invalid when current user is not the Creator
- `InProgressToDraftCommand_ShouldBeValid` — verify command is valid when status is InProgress and user is Creator
- `InProgressToDraftCommand_ShouldTransitionStateProperly` — verify status changes to Draft, Assignee is null, AssignedDate is null
- `StateCommandList_ShouldReturnAllStateCommandsIncludingUnassign` — verify the command list includes the new unassign commands

### Integration Tests
- None required for this change; the existing `StateCommandHandler` integration test pattern already covers the persistence pathway for all `StateCommandBase` subclasses

### Acceptance Tests
- Navigate to an Assigned work order as the Creator, click "Unassign", and verify the status returns to Draft with no Assignee
- Navigate to an InProgress work order as the Creator, click "Unassign", and verify the status returns to Draft with no Assignee
