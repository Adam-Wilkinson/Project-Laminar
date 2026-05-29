using Laminar.Contracts.Base.ActionSystem;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.UnitTests.Base.UnitTests.ActionSystem.UnitTests;

public class UserActionSessionTests
{
    public class ExecuteAction
    {
        [Fact]
        public async Task ShouldExecuteActionThroughOwner()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var action = Substitute.For<IUserAction>();
            var session = new UserActionSession(owner);
            owner.ResolveExecutionAsync(action).Returns(IUserActionResult.Success(Substitute.For<IUserAction>()));
            
            await session.ExecuteAction(action);

            await owner.Received(1).ResolveExecutionAsync(action);
        }

        [Fact]
        public async Task ShouldStoreInverseActionOnSuccess()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var inverse = Substitute.For<IUserAction>();
            var action = Substitute.For<IUserAction>();
            var success = IUserActionResult.Success(inverse);

            owner.ResolveExecutionAsync(action).Returns(success);

            var session = new UserActionSession(owner);

            await session.ExecuteAction(action);
            await session.Reset();

            await owner.Received(1).ResolveExecutionAsync(inverse);
        }

        [Fact]
        public async Task ShouldNotStoreInverseWhenActionFails()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var action = Substitute.For<IUserAction>();
            owner.ResolveExecutionAsync(action).Returns(IUserActionResult.Invalid());
            var session = new UserActionSession(owner);

            await session.ExecuteAction(action);
            await session.Reset();

            await owner.Received(1).ResolveExecutionAsync(Arg.Any<IUserAction>());
        }
    }

    public class Reset
    {
        [Fact]
        public async Task ShouldReplayUndoActionsInLifoOrder()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var undo1 = Substitute.For<IUserAction>();
            var undo2 = Substitute.For<IUserAction>();
            var session = new UserActionSession(owner);

            owner.ResolveExecutionAsync(Arg.Any<IUserAction>()).Returns(IUserActionResult.Success(undo1));
            await session.ExecuteAction(Substitute.For<IUserAction>());
            owner.ResolveExecutionAsync(Arg.Any<IUserAction>()).Returns(IUserActionResult.Success(undo2));
            await session.ExecuteAction(Substitute.For<IUserAction>());

            await session.Reset();

            Received.InOrder(async () =>
            {
                await owner.ResolveExecutionAsync(undo2);
                await owner.ResolveExecutionAsync(undo1);
            });
        }

        [Fact]
        public async Task ShouldDoNothingWhenNoUndoActions()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var session = new UserActionSession(owner);

            await session.Reset();

            await owner.DidNotReceive().ResolveExecutionAsync(Arg.Any<IUserAction>());
        }
    }

    public class Dispose
    {
        [Fact]
        public async Task ShouldRegisterCompoundUndoActionWhenStackNotEmpty()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var action1 = Substitute.For<IUserAction>();
            var action2 = Substitute.For<IUserAction>();
            var session = new UserActionSession(owner);

            owner.ResolveExecutionAsync(Arg.Any<IUserAction>()).Returns(IUserActionResult.Success(action1));
            await session.ExecuteAction(Substitute.For<IUserAction>());
            owner.ResolveExecutionAsync(Arg.Any<IUserAction>()).Returns(IUserActionResult.Success(action2));
            await session.ExecuteAction(Substitute.For<IUserAction>());

            session.Dispose();

            owner.Received(1).Simplify(Arg.Any<List<IUserAction>>());
            owner.Received(1).RegisterUndoAction(Arg.Is<IUserAction>(a => a is CompoundAction));
        }

        [Fact]
        public void ShouldNotRegisterUndoActionWhenStackEmpty()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var session = new UserActionSession(owner);

            session.Dispose();

            owner.DidNotReceive().RegisterUndoAction(Arg.Any<IUserAction>());
        }

        [Fact]
        public async Task ShouldRegisterSingleCompoundUndoActionAfterSimplification()
        {
            var owner = Substitute.For<IUserActionSessionHost>();
            var action = Substitute.For<IUserAction>();
            var session = new UserActionSession(owner);

            owner.ResolveExecutionAsync(action).Returns(IUserActionResult.Success(Substitute.For<IUserAction>()));

            await session.ExecuteAction(action);

            session.Dispose();

            owner.Received(1).Simplify(Arg.Any<List<IUserAction>>());
        }
    }
}