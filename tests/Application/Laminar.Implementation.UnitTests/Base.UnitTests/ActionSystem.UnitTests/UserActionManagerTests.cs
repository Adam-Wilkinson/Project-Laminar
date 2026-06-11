using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Implementation.Base.ActionSystem;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UnitTests.Base.UnitTests.ActionSystem.UnitTests;

public class UserActionManagerTests
{
    public class ExecuteAction
    {
        [Fact]
        public async Task ShouldExecuteAction()
        {
            var action = Substitute.For<IUserAction>();
            var inverse = Substitute.For<IUserAction>();
            action.CanExecute.Returns(true);
            action.Execute().Returns(IUserActionResult.Success(inverse));

            var sut = CreateManager();
            await sut.ExecuteAction(action);

            await action.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldRegisterInverseActionWhenSuccessful()
        {
            var inverse = Substitute.For<IUserAction>();
            var action = Substitute.For<IUserAction>();
            action.CanExecute.Returns(true);
            action.Execute().Returns(IUserActionResult.Success(inverse));
            inverse.CanExecute.Returns(true);
            inverse.Execute().Returns(IUserActionResult.Success(action));
            var sut = CreateManager();

            await sut.ExecuteAction(action);
            await sut.Undo();

            await inverse.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldNotRegisterUndoActionWhenExecutionFails()
        {
            var action = Substitute.For<IUserAction>();
            action.CanExecute.Returns(true);
            action.Execute().Returns(IUserActionResult.Invalid());
            var sut = CreateManager();
            
            await sut.ExecuteAction(action);
            var result = await sut.Undo();

            result.Should().BeOfType<UserActionInvalid>();
        }
    }

    public class Undo
    {
        [Fact]
        public async Task ShouldReturnInvalidWhenUndoListEmpty()
        {
            var sut = CreateManager();

            var result = await sut.Undo();

            result.Should().BeOfType<UserActionInvalid>();
        }

        [Fact]
        public async Task ShouldExecuteLastUndoAction()
        {
            var undoAction = Substitute.For<IUserAction>();
            var inverse = Substitute.For<IUserAction>();
            undoAction.CanExecute.Returns(true);
            undoAction.Execute().Returns(IUserActionResult.Success(inverse));
            var sut = CreateManager();

            sut.RegisterUndoAction(undoAction);
            await sut.Undo();

            await undoAction.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldRegisterRedoActionWhenUndoSucceeds()
        {
            var redoAction = Substitute.For<IUserAction>();
            var undoAction = Substitute.For<IUserAction>();
            undoAction.CanExecute.Returns(true);
            undoAction.Execute().Returns(IUserActionResult.Success(redoAction));
            redoAction.CanExecute.Returns(true);
            redoAction.Execute().Returns(IUserActionResult.Success(undoAction));
            var sut = CreateManager();

            sut.RegisterUndoAction(undoAction);
            await sut.Undo();
            await sut.Redo();

            await redoAction.Received(1).Execute();
        }
    }

    public class Redo
    {
        [Fact]
        public async Task ShouldReturnInvalidWhenRedoListEmpty()
        {
            var sut = CreateManager();

            var result = await sut.Redo();

            result.Should().BeOfType<UserActionInvalid>();
        }

        [Fact]
        public async Task ShouldRegisterUndoActionWhenRedoSucceeds()
        {
            var undo = Substitute.For<IUserAction>();
            var redo = Substitute.For<IUserAction>();
            undo.CanExecute.Returns(true);
            undo.Execute().Returns(IUserActionResult.Success(redo));
            redo.CanExecute.Returns(true);
            redo.Execute().Returns(IUserActionResult.Success(undo));
            var sut = CreateManager();
            sut.RegisterUndoAction(undo);

            await sut.Undo();
            await sut.Redo();
            await sut.Undo();

            await undo.Received(2).Execute();
        }
    }

    public class ResolveExecutionAsync
    {
        [Fact]
        public async Task ShouldReturnInvalidWhenActionCannotExecute()
        {
            var action = Substitute.For<IUserAction>();
            action.CanExecute.Returns(false);
            var sut = CreateManager();

            var result = await sut.ResolveExecutionAsync(action);

            
            result.Should().BeOfType<UserActionInvalid>();
            await action.DidNotReceive().Execute();
        }

        [Fact]
        public async Task ShouldReturnSuccessWhenActionSucceeds()
        {
            var action = Substitute.For<IUserAction>();
            var inverse = Substitute.For<IUserAction>();
            var success = IUserActionResult.Success(inverse);

            action.CanExecute.Returns(true);
            action.Execute().Returns(success);

            var sut = CreateManager();

            var result = await sut.ResolveExecutionAsync(action);
            result.Should().BeSameAs(success);
        }

        [Fact]
        public async Task ShouldReturnCancelledWhenResolverCancels()
        {
            var action = Substitute.For<IUserAction>();
            var resolver = Substitute.For<IUserActionErrorResolver>();
            resolver.TryResolve(Arg.Any<IUserActionResult>()).Returns(new UserActionCancelledResolution());
            Action onCancelled = Substitute.For<Action>();
            var resolvableError = new ResolvableError<bool>
            {
                Exception = new Exception(),
                Resolve = _ => throw new InvalidOperationException("A cancelled operations should not go via resolve"),
                OnCancelled = onCancelled,
            };
            action.CanExecute.Returns(true);
            action.Execute().Returns(resolvableError);
            
            var sut = CreateManager(errorResolvers: [resolver]);

            var result = await sut.ResolveExecutionAsync(action);
            result.Should().BeOfType<UserActionCancelled>();
            onCancelled.Received(1).Invoke();
        }

        [Fact]
        public async Task ShouldExecuteAlternativeActionWhenResolverProvidesOne()
        {
            var original = Substitute.For<IUserAction>();
            var alternative = Substitute.For<IUserAction>();
            var alternativeInverse = Substitute.For<IUserAction>();
            var alternativeResult = IUserActionResult.Success(alternativeInverse);
            var resolver = Substitute.For<IUserActionErrorResolver>();
            var resolvableError = new ResolvableError<bool>
            {
                Exception = new Exception(),
                Resolve = _ => new AlternativeActionFound(alternative),
            };
            original.CanExecute.Returns(true);
            original.Execute().Returns(resolvableError);
            alternative.CanExecute.Returns(true);
            alternative.Execute().Returns(alternativeResult);
            resolver.TryResolve(resolvableError).Returns(resolvableError.Resolve(true));

            var sut = CreateManager(errorResolvers: [resolver]);

            var resolved = await sut.ResolveExecutionAsync(original);
            await alternative.Received(1).Execute();
            resolved.Should().BeSameAs(alternativeResult);
        }

        [Fact]
        public async Task ShouldIgnoreAlternativeActionThatCannotExecute()
        {
            var original = Substitute.For<IUserAction>();
            var alternative = Substitute.For<IUserAction>();
            alternative.CanExecute.Returns(false);
            var resolver = Substitute.For<IUserActionErrorResolver>();
            var exception = new InvalidOperationException();
            var resolvableError = new ResolvableError<bool>
            {
                Exception = exception,
                Resolve = _ => new AlternativeActionFound(alternative),
            };
            original.CanExecute.Returns(true);
            original.Execute().Returns(resolvableError);
            resolver.TryResolve(Arg.Any<IUserActionResult>()).Returns(new AlternativeActionFound(alternative));

            var sut = CreateManager(errorResolvers: [resolver]);
            var result = await sut.ResolveExecutionAsync(original);

            result.Should().BeOfType<UserActionError>().Which.Exception.Should().BeSameAs(exception);
        }

        [Fact]
        public async Task ShouldReportUserActionErrorToExceptionHandler()
        {
            var exception = new InvalidOperationException();
            var action = Substitute.For<IUserAction>();
            action.CanExecute.Returns(true);
            action.Execute().Returns(IUserActionResult.Error(exception));
            var exceptionHandler = Substitute.For<IExceptionHandler>();
            var sut = CreateManager(exceptionHandler: exceptionHandler);

            await sut.ResolveExecutionAsync(action);
            
            await exceptionHandler.Received(1).OnExceptionAsync(exception);
        }

        [Fact]
        public async Task ShouldPromoteResolvableErrorToUserActionError()
        {
            var exception = new InvalidOperationException();
            var action = Substitute.For<IUserAction>();
            action.CanExecute.Returns(true);
            action.Execute().Returns(new ResolvableError<bool>
            {
                Exception = exception,
                Resolve = _ => throw new InvalidOperationException("There should not be any action resolvers here")
            });
            var exceptionHandler = Substitute.For<IExceptionHandler>();
            var sut = CreateManager(exceptionHandler: exceptionHandler);

            await sut.ResolveExecutionAsync(action);
            
            await exceptionHandler.Received(1).OnExceptionAsync(exception);
        }
    }

    public class Simplify
    {
        [Fact]
        public void ShouldForwardActionsToChainSimplifier()
        {
            var chainSimplifier = Substitute.For<IUserActionChainSimplifier>();
            var actions = new List<IUserAction>();
            var sut = CreateManager(chainSimplifier: chainSimplifier);

            sut.Simplify(actions);

            chainSimplifier.Received(1).Simplify(actions, Arg.Any<ICollection<IUserActionSimplifier>>());
        }

        [Fact]
        public void ShouldPassRegisteredSimplifiers()
        {
            var simplifier = Substitute.For<IUserActionSimplifier>();
            var chainSimplifier = Substitute.For<IUserActionChainSimplifier>();
            var sut = CreateManager(chainSimplifier: chainSimplifier);
            sut.RegisterSimplifier(simplifier);

            sut.Simplify([]);

            chainSimplifier.Received(1)
                .Simplify(
                    Arg.Any<List<IUserAction>>(), 
                    Arg.Is<ICollection<IUserActionSimplifier>>(x => x.Contains(simplifier)));
        }
    }

    private static UserActionManager CreateManager(
        IEnumerable<IUserActionErrorResolver>? errorResolvers = null,
        IExceptionHandler? exceptionHandler = null,
        IUserActionChainSimplifier? chainSimplifier = null)
    {
        return new UserActionManager(
            errorResolvers ?? [],
            exceptionHandler ?? Substitute.For<IExceptionHandler>(),
            chainSimplifier ?? Substitute.For<IUserActionChainSimplifier>(),
            Substitute.For<ILogger<UserActionManager>>());
    }
}