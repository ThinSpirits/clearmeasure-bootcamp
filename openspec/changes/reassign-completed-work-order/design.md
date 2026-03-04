## Context

The ChurchBulletin system uses a state command pattern for work order status transitions. Each `IStateCommand` implementation defines a begin status, end status, execution logic, and authorization rules. The `StateCommandHandler` in DataAccess persists changes via EF Core. The `WorkOrder.CanReassign()` method currently only returns `true` for Draft status, gating UI reassignment controls.

## Goals / Non-Goals

**Goals:**
- Allow a Creator to reassign a completed work order to a new Employee
- Transition the work order from Complete back to Assigned
- Update AssignedDate and clear CompletedDate on reassignment
- Follow the existing state command pattern exactly

**Non-Goals:**
- UI changes (the existing reassign UI controls are driven by `CanReassign()` which will be updated)
- Reassignment from Cancelled status
- Allowing non-Creators to reassign

## Decisions

### Decision 1: Create `CompleteToAssignedCommand` following the state command pattern

**Rationale:** Consistent with existing commands like `DraftToAssignedCommand` and `InProgressToAssignedCommand`. The `StateCommandHandler` already handles all `StateCommandBase` subclasses, so no handler changes are needed.

### Decision 2: Update `CanReassign()` to include Complete status

**Rationale:** The `CanReassign()` method gates reassignment UI controls. Adding Complete status enables reassignment for completed work orders without modifying UI component logic.

### Decision 3: Clear CompletedDate on reassignment from Complete

**Rationale:** When a completed work order is reassigned, it re-enters the active workflow. The CompletedDate should be cleared to accurately reflect the work order is no longer complete.

## Risks / Trade-offs

- **[Workflow clarity]** Reopening completed work could cause confusion if audit trails are expected. → Mitigation: The status transition is explicit (Complete→Assigned) and logged by `StateCommandHandler`.
