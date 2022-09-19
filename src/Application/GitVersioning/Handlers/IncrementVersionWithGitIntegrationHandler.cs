using Application.AssemblyVersioning.Commands;
using Application.GitVersioning.Commands;
using Application.GitVersioning.Queries;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enumerations;
using MediatR;
using Microsoft.Extensions.Logging;
using Semver;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.GitVersioning.Handlers
{
    /// <summary>
    /// The <see cref="IRequestHandler{TRequest,TResponse}"/> responsible for incrementing the version with git integration.
    /// </summary>
    public class IncrementVersionWithGitIntegrationHandler : IRequestHandler<IncrementVersionWithGitIntegrationCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly IGitService _gitService;
        private readonly IGitVersioningService _gitVersioningService;
        private readonly IAssemblyVersioningService _assemblyVersioningService;
        private readonly ILogger<IncrementVersionWithGitIntegrationHandler> _logger;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="mediator">An abstraction for accessing application behaviors.</param>
        /// <param name="gitService">An abstraction to facilitate testing without using the git integration.</param>
        /// <param name="gitVersioningService">An abstraction for retrieving version hint info from git commit messages.</param>
        /// <param name="assemblyVersioningService">An abstraction for working with assembly versions.</param>
        /// <param name="logger">A generic interface for logging.</param>
        public IncrementVersionWithGitIntegrationHandler(
            IMediator mediator,
            IGitService gitService,
            IGitVersioningService gitVersioningService,
            IAssemblyVersioningService assemblyVersioningService,
            ILogger<IncrementVersionWithGitIntegrationHandler> logger)
        {
            _mediator = mediator;
            _gitService = gitService;
            _gitVersioningService = gitVersioningService;
            _assemblyVersioningService = assemblyVersioningService;
            _logger = logger;
        }

        /// <summary>Handles the request to increment the version with git integration.</summary>
        /// <param name="request">The <see cref="IRequest{TResponse}"/> object responsible for incrementing the version with git integration.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        public async Task<Unit> Handle(IncrementVersionWithGitIntegrationCommand request, CancellationToken cancellationToken)
        {
            GetVersionIncrementQuery getVersionIncrementQuery = new GetVersionIncrementQuery { GitDirectory = request.GitDirectory, RemoteTarget = request.RemoteTarget, BranchName = request.BranchName, SearchOption = request.SearchOption, TargetDirectory = request.TargetDirectory };
            VersionIncrement increment = await _mediator.Send(getVersionIncrementQuery, cancellationToken);

            IncrementVersionByWithGitIntegrationCommand incrementVersionByWithGitIntegrationCommand = new IncrementVersionByWithGitIntegrationCommand { BranchName = request.BranchName, CommitAuthorEmail = request.CommitAuthorEmail, GitDirectory = request.GitDirectory, RemoteTarget = request.RemoteTarget, SearchOption = request.SearchOption, TargetDirectory = request.TargetDirectory, Increment = increment };
            return await _mediator.Send(incrementVersionByWithGitIntegrationCommand, cancellationToken);
        }
    }
}
