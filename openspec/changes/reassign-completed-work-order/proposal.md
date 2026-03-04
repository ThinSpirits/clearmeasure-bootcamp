## Why

Currently, once a work order reaches Complete status, it cannot be reassigned. If a Creator discovers the work needs further attention by a different team member, the only option is to create a new work order. This feature allows the Creator to reassign a completed work order to a different Employee, transitioning it back to Assigned status and preserving the work order's history and continuity.

## What Changes

- Add `CompleteToAssignedCommand` state command in `src/Core/Model/StateCommands/` that transitions a work order from Complete to Assigned with a new assignee
- Update `WorkOrder.CanReassign()` to also return `true` when status is `Complete`
- Validate that only the Creator can perform this action
- Update `AssignedDate` when reassigning
- Clear `CompletedDate` when transitioning out of Complete status

## Capabilities

### New Capabilities
- Creators can reassign a Complete work order to a different employee, transitioning status back to Assigned

### Modified Capabilities
- `WorkOrder.CanReassign()` returns `true` for Complete status in addition to Draft

## Impact

- **Core** — New `CompleteToAssignedCommand` state command; updated `CanReassign()` method
- **DataAccess** — No handler changes needed; existing `StateCommandHandler` handles all `StateCommandBase` subclasses
- **Database** — No schema changes
- **UI** — No UI changes required for this issue (reassign button visibility already driven by `CanReassign()`)
