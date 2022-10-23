﻿using Application.AssemblyVersioning.Commands;
using Application.GitVersioning.Commands;
using Application.GitVersioning.Handlers;
using Application.GitVersioning.Queries;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Moq;
using Semver;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enumerations;
using Xunit;
using System.Linq;

namespace Business.Tests.GitVersioning
{
    public class IncrementVersionWithGitHandlerTests
    {
        private List<GitCommit> Commits {get; } = new()
        {
            new GitCommit("27d8879","fake(Build): Updated build.yml to target the template solution file [skip hint]", new List<GitCommitFileInfo>()),
            new GitCommit("27d8880","build(Pipeline): Downgraded to .net core 3.1 from 5 [skip ci]", new List<GitCommitFileInfo>()),
            new GitCommit("27d8881","ci(Versioning): Increment version 0.0.0 -> 0.0.0 [skip ci] [skip hint]", new List<GitCommitFileInfo>())
        };
        
        [Fact]
        public async Task Handler_ExitsWhen_IncrementIsNone()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gitService = new Mock<IGitService>();
            var assemblyVersioningService = new Mock<IAssemblyVersioningService>();

            mediator.Setup(x => x.Send(It.IsAny<IncrementAssemblyVersionCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            var sut = new IncrementVersionWithGitHandler(mediator.Object, gitService.Object, assemblyVersioningService.Object);

            // Act
            await sut.Handle(new IncrementVersionWithGitCommand(), CancellationToken.None);

            // Assert
            mediator.Verify(x => x.Send(It.IsAny<IncrementAssemblyVersionCommand>(), CancellationToken.None), Times.Never);
            gitService.Verify(x => x.GetCommits(It.IsAny<string>()), Times.Never);
            assemblyVersioningService.Verify(x => x.GetLatestAssemblyVersion(It.IsAny<string>(), It.IsAny<SearchOption>()), Times.Never);
        }

        [Fact]
        public async Task Handler_ExitsWhen_IncrementIsUnknown()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var gitService = new Mock<IGitService>();
            var assemblyVersioningService = new Mock<IAssemblyVersioningService>();

            mediator.Setup(x => x.Send(It.IsAny<IncrementAssemblyVersionCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            var sut = new IncrementVersionWithGitHandler(mediator.Object, gitService.Object, assemblyVersioningService.Object);

            // Act
            await sut.Handle(new IncrementVersionWithGitCommand(), CancellationToken.None);

            // Assert
            mediator.Verify(x => x.Send(It.IsAny<IncrementAssemblyVersionCommand>(), CancellationToken.None), Times.Never);
            gitService.Verify(x => x.GetCommits(It.IsAny<string>()), Times.Never);
            assemblyVersioningService.Verify(x => x.GetLatestAssemblyVersion(It.IsAny<string>(), It.IsAny<SearchOption>()), Times.Never);
        }

        [Fact]
        public async Task TargetDirectory_SetToGitDirectory_WhenEmpty()
        {
            // Arrange
            var request = new IncrementVersionWithGitCommand
            {
                GitDirectory = "C:\\Temp",
                TargetDirectory = null,
                SearchOption = SearchOption.AllDirectories,
                CommitAuthorEmail = "support@versioning.net",
                BranchName = "test",
                RemoteTarget = "origin",
                VersionIncrement = VersionIncrement.Minor
            };
            var assemblyVersion = new SemVersion(0);
            var mediator = new Mock<IMediator>();
            var gitService = new Mock<IGitService>();
            var assemblyVersioningService = new Mock<IAssemblyVersioningService>();

            mediator.Setup(x => x.Send(It.IsAny<IncrementAssemblyVersionCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            gitService.Setup(x => x.GetCommits(It.IsAny<string>())).Returns(Commits);

            assemblyVersioningService.Setup(x => x.GetLatestAssemblyVersion(request.GitDirectory, request.SearchOption)).Returns(assemblyVersion);
            var sut = new IncrementVersionWithGitHandler(mediator.Object, gitService.Object, assemblyVersioningService.Object);

            // Act
            await sut.Handle(request, CancellationToken.None);

            // Assert
            assemblyVersioningService.Verify(x => x.GetLatestAssemblyVersion(request.TargetDirectory, request.SearchOption), Times.Exactly(2));
        }

        [Fact]
        public async Task Handler_CallsDependencies()
        {
            // Arrange
            var request = new IncrementVersionWithGitCommand
            {
                GitDirectory = "C:\\Temp",
                TargetDirectory = "C:\\Temp\\Sub",
                SearchOption = SearchOption.AllDirectories,
                CommitAuthorEmail = "support@versioning.net",
                BranchName = "test",
                RemoteTarget = "origin",
                VersionIncrement = VersionIncrement.Minor,
                TagPrefix = "b",
                TagSuffix = "c"
            };
            var commit = Commits.First(x => x.Subject == "ci(Versioning): Increment version 0.0.0 -> 0.0.0 [skip ci] [skip hint]");
            var assemblyVersion = new SemVersion(0);
            var mediator = new Mock<IMediator>();
            var gitService = new Mock<IGitService>();
            var assemblyVersioningService = new Mock<IAssemblyVersioningService>();

            mediator.Setup(x => x.Send(It.IsAny<IncrementAssemblyVersionCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            gitService.Setup(x => x.GetCommits(It.IsAny<string>())).Returns(Commits);

            assemblyVersioningService.Setup(x => x.GetLatestAssemblyVersion(request.TargetDirectory, request.SearchOption)).Returns(assemblyVersion);
            var sut = new IncrementVersionWithGitHandler(mediator.Object, gitService.Object, assemblyVersioningService.Object);

            // Act
            await sut.Handle(request, CancellationToken.None);

            // Assert
            mediator.Verify(x => x.Send(It.IsAny<IncrementAssemblyVersionCommand>(), CancellationToken.None), Times.Once);
            assemblyVersioningService.Verify(x => x.GetLatestAssemblyVersion(request.TargetDirectory, request.SearchOption), Times.Exactly(2));
            gitService.Verify(x => x.CommitChanges(request.GitDirectory, commit.Subject, request.CommitAuthorEmail), Times.Once);
            gitService.Verify(x => x.GetCommits(request.GitDirectory), Times.Once);
            gitService.Verify(x => x.CreateTag(request.GitDirectory, $"{request.TagPrefix}{assemblyVersion}{request.TagSuffix}", commit.Id), Times.Once);
            gitService.Verify(x => x.PushRemote(request.GitDirectory, request.RemoteTarget, $"refs/heads/{request.BranchName}"), Times.Once);
            gitService.Verify(x => x.PushRemote(request.GitDirectory, request.RemoteTarget, $"refs/tags/{request.TagPrefix}{assemblyVersion}{request.TagSuffix}"), Times.Once);
        }
    }
}
