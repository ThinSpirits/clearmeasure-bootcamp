using ClearMeasure.Bootcamp.Core.Model;
using ClearMeasure.Bootcamp.Core.Model.StateCommands;
using ClearMeasure.Bootcamp.Core.Services;
using Shouldly;

namespace ClearMeasure.Bootcamp.UnitTests.Core.Model.StateCommands;

[TestFixture]
public class CompleteToAssignedCommandTests : StateCommandBaseTests
{
    [Test]
    public void ShouldNotBeValidInWrongStatus()
    {
        var order = new WorkOrder();
        order.Status = WorkOrderStatus.Draft;
        var employee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, employee);
        command.IsValid().ShouldBeFalse();
    }

    [Test]
    public void ShouldNotBeValidWithWrongEmployee()
    {
        var order = new WorkOrder();
        order.Status = WorkOrderStatus.Complete;
        var employee = new Employee();
        var differentEmployee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, differentEmployee);
        command.IsValid().ShouldBeFalse();
    }

    [Test]
    public void ShouldBeValid()
    {
        var order = new WorkOrder();
        order.Status = WorkOrderStatus.Complete;
        var employee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, employee);
        command.IsValid().ShouldBeTrue();
    }

    [Test]
    public void ShouldTransitionStateProperly()
    {
        var order = new WorkOrder();
        order.Number = "123";
        order.Status = WorkOrderStatus.Complete;
        order.CompletedDate = DateTime.UtcNow;
        var employee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, employee);
        var context = new StateCommandContext { CurrentDateTime = new DateTime(2026, 3, 4) };
        command.Execute(context);

        order.Status.ShouldBe(WorkOrderStatus.Assigned);
        order.AssignedDate.ShouldBe(new DateTime(2026, 3, 4));
        order.CompletedDate.ShouldBeNull();
    }

    protected override StateCommandBase GetStateCommand(WorkOrder order, Employee employee)
    {
        return new CompleteToAssignedCommand(order, employee);
    }
}
