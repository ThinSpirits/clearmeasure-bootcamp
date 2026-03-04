## 1. Core Domain Changes

- [ ] 1.1 Create `CompleteToAssignedCommand` in `src/Core/Model/StateCommands/` — begin status Complete, end status Assigned, Creator-only authorization, sets AssignedDate, clears CompletedDate
- [ ] 1.2 Update `WorkOrder.CanReassign()` in `src/Core/Model/WorkOrder.cs` to return `true` when Status is Complete (in addition to Draft)

## 2. Unit Tests

- [ ] 2.1 `CompleteToAssignedCommand_IsValid_WhenCreatorAndCompleteStatus` — verify IsValid returns true for Creator with Complete work order
- [ ] 2.2 `CompleteToAssignedCommand_IsNotValid_WhenNotCreator` — verify IsValid returns false for non-Creator
- [ ] 2.3 `CompleteToAssignedCommand_IsNotValid_WhenNotCompleteStatus` — verify IsValid returns false for non-Complete status
- [ ] 2.4 `CompleteToAssignedCommand_Execute_ShouldSetStatusToAssigned` — verify status transitions to Assigned
- [ ] 2.5 `CompleteToAssignedCommand_Execute_ShouldUpdateAssignedDate` — verify AssignedDate is set
- [ ] 2.6 `CompleteToAssignedCommand_Execute_ShouldClearCompletedDate` — verify CompletedDate is cleared
- [ ] 2.7 `CanReassign_ShouldReturnTrue_WhenComplete` — verify CanReassign returns true for Complete status
- [ ] 2.8 `CanReassign_ShouldReturnTrue_WhenDraft` — verify existing Draft behavior is preserved

## 3. Integration Tests

- [ ] 3.1 `CompleteToAssignedCommand_ShouldPersistNewAssignee` — execute command through StateCommandHandler and verify database persistence
