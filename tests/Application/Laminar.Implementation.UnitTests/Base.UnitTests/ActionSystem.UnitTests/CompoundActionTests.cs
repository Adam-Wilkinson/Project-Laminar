using Laminar.Contracts.Base.ActionSystem;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.UnitTests.Base.UnitTests.ActionSystem.UnitTests;

public class CompoundActionTests
{
    public class CanExecute
    {
        [Fact]
        public void ShouldBeTrueWhenAllActionsCanExecute()
        {
            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();

            action1.CanExecute.Returns(true);
            action2.CanExecute.Returns(true);

            var sut = new CompoundAction(action1, action2);

            sut.CanExecute.Should().BeTrue();
        }

        [Fact]
        public void ShouldBeFalseWhenAnyActionCannotExecute()
        {
            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();

            action1.CanExecute.Returns(true);
            action2.CanExecute.Returns(false);

            var sut = new CompoundAction(action1, action2);

            sut.CanExecute.Should().BeFalse();
        }
    }

    public class Execute
    {
        [Fact]
        public async Task ShouldReturnInvalidWhenActionCannotExecute()
        {
            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();

            action1.CanExecute.Returns(true);
            action2.CanExecute.Returns(false);

            var sut = new CompoundAction(action1, action2);

            var result = await sut.Execute();

            result.Should().BeOfType<UserActionInvalid>();

            await action1.DidNotReceive().Execute();
            await action2.DidNotReceive().Execute();
        }

        [Fact]
        public async Task ShouldExecuteAllActions()
        {
            var inverse1 = Substitute.For<IUserAction>();
            var inverse2 = Substitute.For<IUserAction>();
            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();
            action1.CanExecute.Returns(true);
            action2.CanExecute.Returns(true);
            action1.Execute().Returns(Task.FromResult(IUserActionResult.Success(inverse1)));
            action2.Execute().Returns(Task.FromResult(IUserActionResult.Success(inverse2)));

            var sut = new CompoundAction(action1, action2);

            await sut.Execute();
            await action1.Received(1).Execute();
            await action2.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldReturnCompoundInverseActionOnSuccess()
        {
            var inverse1 = Substitute.For<IUserAction>();
            var inverse2 = Substitute.For<IUserAction>();
            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();
            action1.CanExecute.Returns(true);
            action2.CanExecute.Returns(true);
            action1.Execute().Returns(Task.FromResult(IUserActionResult.Success(inverse1)));
            action2.Execute().Returns(Task.FromResult(IUserActionResult.Success(inverse2)));

            var sut = new CompoundAction(action1, action2);
            var result = await sut.Execute();
            var success = result.Should().BeOfType<UserActionSuccess>().Subject;
            success.InverseAction.Should().BeOfType<CompoundAction>();
            var compoundInverse = (CompoundAction)success.InverseAction;

            compoundInverse.Actions.Should().ContainInOrder(inverse2, inverse1);
        }

        [Fact]
        public async Task ShouldUndoPreviouslyExecutedActionsWhenActionFails()
        {
            var inverse1 = Substitute.For<IUserAction>();
            inverse1.Execute().Returns(Task.FromResult(IUserActionResult.Success(Substitute.For<IUserAction>())));
            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();
            action1.CanExecute.Returns(true);
            action2.CanExecute.Returns(true);
            action1.Execute().Returns(Task.FromResult(IUserActionResult.Success(inverse1)));
            var failure = IUserActionResult.Invalid();

            action2.Execute().Returns(Task.FromResult(failure));
            var sut = new CompoundAction(action1, action2);
            var result = await sut.Execute();
            result.Should().BeSameAs(failure);

            await inverse1.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldUndoActionsInReverseOrderWhenMultipleActionsExecuted()
        {
            var inverse1 = Substitute.For<IUserAction>();
            var inverse2 = Substitute.For<IUserAction>();
            
            var undoOrder = new List<int>();
            inverse1.Execute().Returns(_ =>
            {
                undoOrder.Add(1);
                return Task.FromResult(IUserActionResult.Success(Substitute.For<IUserAction>()));
            });
            inverse2.Execute().Returns(_ =>
            {
                undoOrder.Add(2);
                return Task.FromResult(IUserActionResult.Success(Substitute.For<IUserAction>()));
            });

            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();
            var action3 = Substitute.For<IUserAction>();
            action1.CanExecute.Returns(true);
            action2.CanExecute.Returns(true);
            action3.CanExecute.Returns(true);
            action1.Execute().Returns(Task.FromResult(IUserActionResult.Success(inverse1)));
            action2.Execute().Returns(Task.FromResult(IUserActionResult.Success(inverse2)));
            action3.Execute().Returns(Task.FromResult(IUserActionResult.Invalid()));

            var sut = new CompoundAction(action1, action2, action3);

            await sut.Execute();

            undoOrder.Should().Equal(2, 1);
        }
    }
}