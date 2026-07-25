using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Implementation.Storage.FileExplorer.Synchronization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Synchronization.UnitTests;

public class FileSystemSynchronizerTests
{
    public class OnFileSystemEvent
    {
        [Fact]
        public void ShouldForwardEventToMutationComputer()
        {
            var mutationComputer = Substitute.For<IFileSystemMutationComputer>();
            var sut = CreateSynchronizer(mutationComputer: mutationComputer);

            var fileSystemEvent = FileSystemEvent.Created(MockFactory.CreatePath());

            sut.OnFileSystemEvent(fileSystemEvent);

            mutationComputer.Received(1).AddEvent(fileSystemEvent);
        }
    }

    public class ReconcileAndReset
    {
        [Fact]
        public void ShouldApplyComputedMutations()
        {
            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Creation, null, MockFactory.CreatePath());

            var mutationComputer = Substitute.For<IFileSystemMutationComputer>();
            mutationComputer
                .ComputeMutationsAndClear(Arg.Any<IOutdatedItemsBuffer>())
                .Returns([mutation]);

            var graphMutationApplier = Substitute.For<IGraphMutationApplier>();
            var sut = CreateSynchronizer(mutationComputer: mutationComputer, graphMutationApplier: graphMutationApplier);

            sut.ReconcileAndReset([]);

            graphMutationApplier.Received(1).Apply(mutation, Arg.Any<IFileSystemGraph>());
        }

        [Fact]
        public void ShouldClearOutdatedItemsAfterReconciliation()
        {
            var repository = Substitute.For<IFileSystemItemRepository>();
            var sut = CreateSynchronizer(repository: repository);

            sut.ReconcileAndReset([]);

            repository.Received(2).DetachOutdatedItems();
        }

        [Fact]
        public void ShouldComputeDiscrepanciesForTargetFolders()
        {
            var folder = MockFactory.CreateFolder();
            var discrepancyComputer = Substitute.For<IFileSystemDiscrepancyComputer>();

            var sut = CreateSynchronizer(discrepancyComputer: discrepancyComputer);

            sut.ReconcileAndReset([folder]);

            discrepancyComputer.Received(1).ComputeFolderDiscrepancies(folder);
        }

        [Fact]
        public void ShouldNotFallbackWhenNoDifferencesExist()
        {
            var mutationComputer = Substitute.For<IFileSystemMutationComputer>();
            var discrepancyComputer = Substitute.For<IFileSystemDiscrepancyComputer>();

            var sut = CreateSynchronizer(mutationComputer: mutationComputer, discrepancyComputer: discrepancyComputer);

            sut.ReconcileAndReset([MockFactory.CreateFolder()]);

            mutationComputer.DidNotReceive().AddEvent(Arg.Any<FileSystemEvent>());
        }

        [Fact]
        public void ShouldReconcileDifferencesWhenIncrementalSyncFails()
        {
            var discrepancy = FileSystemEvent.Created(MockFactory.CreatePath());

            var discrepancyComputer = Substitute.For<IFileSystemDiscrepancyComputer>();
            discrepancyComputer
                .ComputeFolderDiscrepancies(Arg.Any<IFileSystemFolder>())
                .Returns([discrepancy], []);

            var mutationComputer = Substitute.For<IFileSystemMutationComputer>();
            mutationComputer
                .ComputeMutationsAndClear(Arg.Any<IOutdatedItemsBuffer>())
                .Returns([], 
                    [new FileSystemGraphMutation(FileSystemGraphMutationType.Creation, null, discrepancy.NewPath)]);

            var graphMutationApplier = Substitute.For<IGraphMutationApplier>();

            var sut = CreateSynchronizer(
                mutationComputer: mutationComputer,
                discrepancyComputer: discrepancyComputer,
                graphMutationApplier: graphMutationApplier);

            sut.ReconcileAndReset([MockFactory.CreateFolder()]);

            mutationComputer.Received(1).AddEvent(discrepancy);

            graphMutationApplier.Received(1).Apply(Arg.Any<FileSystemGraphMutation>(), Arg.Any<IFileSystemGraph>());
        }

        [Fact]
        public void ShouldRefreshFolderWhenFallbackFails()
        {
            var folder = MockFactory.CreateFolder();
            var discrepancy = FileSystemEvent.Created(MockFactory.CreatePath());

            var discrepancyComputer = Substitute.For<IFileSystemDiscrepancyComputer>();
            discrepancyComputer
                .ComputeFolderDiscrepancies(folder)
                .Returns([discrepancy], [discrepancy]);

            var mutationComputer = Substitute.For<IFileSystemMutationComputer>();

            var sut = CreateSynchronizer(mutationComputer: mutationComputer, discrepancyComputer: discrepancyComputer);

            sut.ReconcileAndReset([folder]);

            folder.Received(1).Refresh();
        }

        [Fact]
        public void ShouldLogWarningWhenUsingDifferenceFallback()
        {
            var logger = Substitute.For<ILogger<FileSystemSynchronizer>>();
            var folder = MockFactory.CreateFolder();

            var discrepancyComputer = Substitute.For<IFileSystemDiscrepancyComputer>();
            discrepancyComputer.ComputeFolderDiscrepancies(folder)
                .Returns([FileSystemEvent.Created(MockFactory.CreatePath())]);

            var sut = CreateSynchronizer(
                discrepancyComputer: discrepancyComputer,
                logger: logger);

            sut.ReconcileAndReset([folder]);

            logger.Received()
                .Log(
                    LogLevel.Warning,
                    Arg.Any<EventId>(),
                    Arg.Any<object>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>());
        }
    }

    private static FileSystemSynchronizer CreateSynchronizer(
        IFileSystemGraph? targetGraph = null,
        IFileSystemMutationComputer? mutationComputer = null,
        IGraphMutationApplier? graphMutationApplier = null,
        IFileSystemItemRepository? repository = null,
        IFileSystemDiscrepancyComputer? discrepancyComputer = null,
        ILogger<FileSystemSynchronizer>? logger = null)
    {
        return new FileSystemSynchronizer(
            targetGraph ?? Substitute.For<IFileSystemGraph>(),
            mutationComputer ?? Substitute.For<IFileSystemMutationComputer>(),
            graphMutationApplier ?? Substitute.For<IGraphMutationApplier>(),
            repository ?? Substitute.For<IFileSystemItemRepository>(),
            discrepancyComputer ?? Substitute.For<IFileSystemDiscrepancyComputer>(),
            logger ?? Substitute.For<ILogger<FileSystemSynchronizer>>());
    }
}