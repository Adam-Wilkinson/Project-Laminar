using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Implementation.Base.ActionSystem;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UnitTests.Base.UnitTests.ActionSystem.UnitTests;

public class UserActionScopeTests
{
    public class ExecuteAction
    {
        [Fact]
        public async Task ShouldExecuteAction()
        {
            var action = Substitute.For<IUserAction>();
            var inverse = Substitute.For<IUserAction>();
            action.Execute().Returns(IUserActionExecutionOutcome.Success(inverse));

            var sut = CreateScope();
            await sut.ExecuteAction(action);

            await action.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldRegisterInverseActionWhenSuccessful()
        {
            var inverse = Substitute.For<IUserAction>();
            var action = Substitute.For<IUserAction>();
            action.Execute().Returns(IUserActionExecutionOutcome.Success(inverse));
            inverse.Execute().Returns(IUserActionExecutionOutcome.Success(action));
            var sut = CreateScope();

            await sut.ExecuteAction(action);
            await sut.Undo();

            await inverse.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldNotRegisterUndoActionWhenExecutionFails()
        {
            var action = Substitute.For<IUserAction>();
            action.Execute().Returns(IUserActionExecutionOutcome.Ineffectual());
            var sut = CreateScope();
            
            await sut.ExecuteAction(action);
            var result = await sut.Undo();

            result.Should().BeOfType<UserActionIneffectual>();
        }
    }

    public class Undo
    {
        [Fact]
        public async Task ShouldReturnInvalidWhenUndoListEmpty()
        {
            var sut = CreateScope();

            var result = await sut.Undo();

            result.Should().BeOfType<UserActionIneffectual>();
        }

        [Fact]
        public async Task ShouldExecuteLastUndoAction()
        {
            var undoAction = Substitute.For<IUserAction>();
            var inverse = Substitute.For<IUserAction>();
            undoAction.Execute().Returns(IUserActionExecutionOutcome.Success(inverse));
            var sut = CreateScope();

            sut.RegisterUndoAction(undoAction);
            await sut.Undo();

            await undoAction.Received(1).Execute();
        }

        [Fact]
        public async Task ShouldRegisterRedoActionWhenUndoSucceeds()
        {
            var redoAction = Substitute.For<IUserAction>();
            var undoAction = Substitute.For<IUserAction>();
            undoAction.Execute().Returns(IUserActionExecutionOutcome.Success(redoAction));
            redoAction.Execute().Returns(IUserActionExecutionOutcome.Success(undoAction));
            var sut = CreateScope();

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
            var sut = CreateScope();

            var result = await sut.Redo();

            result.Should().BeOfType<UserActionIneffectual>();
        }

        [Fact]
        public async Task ShouldRegisterUndoActionWhenRedoSucceeds()
        {
            var undo = Substitute.For<IUserAction>();
            var redo = Substitute.For<IUserAction>();
            undo.Execute().Returns(IUserActionExecutionOutcome.Success(redo));
            redo.Execute().Returns(IUserActionExecutionOutcome.Success(undo));
            var sut = CreateScope();
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
            var sut = CreateScope();

            var result = await sut.ResolveExecutionAsync(action);

            
            result.Should().BeOfType<UserActionIneffectual>();
            await action.DidNotReceive().Execute();
        }

        [Fact]
        public async Task ShouldReturnSuccessWhenActionSucceeds()
        {
            var action = Substitute.For<IUserAction>();
            var inverse = Substitute.For<IUserAction>();
            var success = IUserActionExecutionOutcome.Success(inverse);

            action.Execute().Returns(success);

            var sut = CreateScope();

            var result = await sut.ResolveExecutionAsync(action);
            result.Should().BeSameAs(success);
        }

        [Fact]
        public async Task ShouldReturnCancelledWhenResolverCancels()
        {
            var action = Substitute.For<IUserAction>();
            var resolver = Substitute.For<IUserActionErrorResolver>();
            resolver.TryResolve(Arg.Any<IUserActionExecutionOutcome>()).Returns(new UserActionCancelledResolution());
            Action onCancelled = Substitute.For<Action>();
            var resolvableError = new ResolvableError<bool>
            {
                Exception = new Exception(),
                Resolve = _ => throw new InvalidOperationException("A cancelled operations should not go via resolve"),
                OnCancelled = onCancelled,
            };
            action.Execute().Returns(resolvableError);
            
            var sut = CreateScope(errorResolvers: [resolver]);

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
            var alternativeResult = IUserActionExecutionOutcome.Success(alternativeInverse);
            var resolver = Substitute.For<IUserActionErrorResolver>();
            var resolvableError = new ResolvableError<bool>
            {
                Exception = new Exception(),
                Resolve = _ => new AlternativeActionFound(alternative),
            };
            original.Execute().Returns(resolvableError);
            alternative.Execute().Returns(alternativeResult);
            resolver.TryResolve(resolvableError).Returns(resolvableError.Resolve(true));

            var sut = CreateScope(errorResolvers: [resolver]);

            var resolved = await sut.ResolveExecutionAsync(original);
            await alternative.Received(1).Execute();
            resolved.Should().BeSameAs(alternativeResult);
        }

        [Fact]
        public async Task ShouldIgnoreAlternativeActionThatCannotExecute()
        {
            var original = Substitute.For<IUserAction>();
            var alternative = Substitute.For<IUserAction>();
            var resolver = Substitute.For<IUserActionErrorResolver>();
            var exception = new InvalidOperationException();
            var resolvableError = new ResolvableError<bool>
            {
                Exception = exception,
                Resolve = _ => new AlternativeActionFound(alternative),
            };
            original.Execute().Returns(resolvableError);
            resolver.TryResolve(Arg.Any<IUserActionExecutionOutcome>()).Returns(new AlternativeActionFound(alternative));

            var sut = CreateScope(errorResolvers: [resolver]);
            var result = await sut.ResolveExecutionAsync(original);

            result.Should().BeOfType<UserActionError>().Which.Exception.Should().BeSameAs(exception);
        }

        [Fact]
        public async Task ShouldReportUserActionErrorToExceptionHandler()
        {
            var exception = new InvalidOperationException();
            var action = Substitute.For<IUserAction>();
            action.Execute().Returns(IUserActionExecutionOutcome.Error(exception));
            var exceptionHandler = Substitute.For<IExceptionHandler>();
            var sut = CreateScope(exceptionHandler: exceptionHandler);

            await sut.ResolveExecutionAsync(action);
            
            await exceptionHandler.Received(1).OnExceptionAsync(exception);
        }

        [Fact]
        public async Task ShouldPromoteResolvableErrorToUserActionError()
        {
            var exception = new InvalidOperationException();
            var action = Substitute.For<IUserAction>();
            action.Execute().Returns(new ResolvableError<bool>
            {
                Exception = exception,
                Resolve = _ => throw new InvalidOperationException("There should not be any action resolvers here")
            });
            var exceptionHandler = Substitute.For<IExceptionHandler>();
            var sut = CreateScope(exceptionHandler: exceptionHandler);

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
            var sut = CreateScope(chainSimplifier: chainSimplifier);

            sut.Simplify(actions);

            chainSimplifier.Received(1).Simplify(actions, Arg.Any<ICollection<IUserActionSimplifier>>());
        }

        [Fact]
        public void ShouldPassRegisteredSimplifiers()
        {
            var simplifier = Substitute.For<IUserActionSimplifier>();
            var chainSimplifier = Substitute.For<IUserActionChainSimplifier>();
            var sut = CreateScope(chainSimplifier: chainSimplifier, simplifiers: [simplifier]);

            sut.Simplify([]);

            chainSimplifier.Received(1)
                .Simplify(
                    Arg.Any<List<IUserAction>>(), 
                    Arg.Is<ICollection<IUserActionSimplifier>>(x => x != null && x.Contains(simplifier)));
        }
    }

    private static UserActionScope CreateScope(
        IUserActionSimplifier[]? simplifiers = null,
        IEnumerable<IUserActionErrorResolver>? errorResolvers = null,
        IExceptionHandler? exceptionHandler = null,
        IUserActionChainSimplifier? chainSimplifier = null)
    {
        return new UserActionScope(
            simplifiers ?? [],
            errorResolvers ?? [],
            exceptionHandler ?? Substitute.For<IExceptionHandler>(),
            chainSimplifier ?? Substitute.For<IUserActionChainSimplifier>(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserActionScope>>());
    }
}