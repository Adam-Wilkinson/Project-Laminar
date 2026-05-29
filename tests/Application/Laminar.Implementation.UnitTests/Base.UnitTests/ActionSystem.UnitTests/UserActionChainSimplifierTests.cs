using Laminar.Contracts.Base.ActionSystem;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.UnitTests.Base.UnitTests.ActionSystem.UnitTests;

public class UserActionChainSimplifierTests
{
    [Fact]
    public void ShouldReturnFalseWhenChainIsEmpty()
    {
        var chain = new List<IUserAction>();
        var sut = new UserActionChainSimplifier();
        var result = sut.Simplify(chain, []);
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldFlattenCompoundActions()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var action3 = Substitute.For<IUserAction>();
        var chain = new List<IUserAction>
        {
            new CompoundAction(action1, action2),
            action3
        };

        var sut = new UserActionChainSimplifier();
        sut.Simplify(chain, []);
        
        chain.Should().Equal(action1, action2, action3);
    }

    [Fact]
    public void ShouldFlattenNestedCompoundActions()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var action3 = Substitute.For<IUserAction>();
        var chain = new List<IUserAction>
        {
            new CompoundAction(action1, new CompoundAction(action2, action3))
        };

        var sut = new UserActionChainSimplifier();
        sut.Simplify(chain, []);

        chain.Should().Equal(action1, action2, action3);
    }

    [Fact]
    public void ShouldReturnTrueWhenFlatteningResultsInSingleAction()
    {
        var action = Substitute.For<IUserAction>();
        var chain = new List<IUserAction>
        {
            new CompoundAction(action)
        };

        var sut = new UserActionChainSimplifier();

        var result = sut.Simplify(chain, []);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldRemoveFirstActionWhenOverridesActionReturned()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var simplifier = Substitute.For<IUserActionSimplifier>();
        simplifier.Simplify(action1, action2).Returns(new OverridesAction());

        var chain = new List<IUserAction>
        {
            action1,
            action2
        };
        var sut = new UserActionChainSimplifier();
        var result = sut.Simplify(chain, [simplifier]);

        result.Should().BeTrue();
        chain.Should().Equal(action2);
    }

    [Fact]
    public void ShouldRemoveBothActionsWhenReversesActionReturned()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var simplifier = Substitute.For<IUserActionSimplifier>();
        simplifier.Simplify(action1, action2).Returns(new ReversesAction());

        var chain = new List<IUserAction>
        {
            action1,
            action2
        };
        var sut = new UserActionChainSimplifier();
        var result = sut.Simplify(chain, [simplifier]);

        result.Should().BeTrue();
        chain.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReplaceActionsWhenNewEffectiveActionReturned()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var replacement = Substitute.For<IUserAction>();
        var simplifier = Substitute.For<IUserActionSimplifier>();

        simplifier.Simplify(action1, action2).Returns(new NewEffectiveAction(replacement));
        var chain = new List<IUserAction>
        {
            action1,
            action2
        };
        var sut = new UserActionChainSimplifier();

        var result = sut.Simplify(chain, [simplifier]);

        result.Should().BeTrue();
        chain.Should().Equal(replacement);
    }

    [Fact]
    public void ShouldUseFirstMatchingSimplifier()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var simplifier1 = Substitute.For<IUserActionSimplifier>();
        var simplifier2 = Substitute.For<IUserActionSimplifier>();

        simplifier1.Simplify(action1, action2).Returns(new OverridesAction());

        var chain = new List<IUserAction>
        {
            action1,
            action2
        };
        var sut = new UserActionChainSimplifier();
        sut.Simplify(chain, [simplifier1, simplifier2]);

        simplifier2.DidNotReceive().Simplify(Arg.Any<IUserAction>(), Arg.Any<IUserAction>());
    }

    [Fact]
    public void ShouldReturnFalseWhenNoSimplificationsOccur()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var simplifier = Substitute.For<IUserActionSimplifier>();
        simplifier.Simplify(action1, action2).Returns(IUserActionSimplification.None());

        var chain = new List<IUserAction>
        {
            action1,
            action2
        };
        var sut = new UserActionChainSimplifier();
        var result = sut.Simplify(chain, [simplifier]);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldContinueSimplifyingUntilStable()
    {
        var action1 = Substitute.For<IUserAction>();
        var action2 = Substitute.For<IUserAction>();
        var action3 = Substitute.For<IUserAction>();
        var merged = Substitute.For<IUserAction>();
        var simplifier = Substitute.For<IUserActionSimplifier>();

        simplifier.Simplify(action1, action2).Returns(new NewEffectiveAction(merged));

        simplifier.Simplify(merged, action3).Returns(new ReversesAction());

        var chain = new List<IUserAction>
        {
            action1,
            action2,
            action3
        };

        var sut = new UserActionChainSimplifier();

        var result = sut.Simplify(chain, [simplifier]);

        result.Should().BeTrue();
        chain.Should().BeEmpty();
    }
}
