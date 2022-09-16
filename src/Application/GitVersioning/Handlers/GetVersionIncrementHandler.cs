using Application.AssemblyVersioning.Commands;
using Application.Extensions;
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
    public class GetVersionIncrementHandler : IRequestHandler<GetVersionIncrementQuery, VersionIncrement>
    {
        private readonly IMediator _mediator;
        private readonly IGitVersioningService _gitVersioningService;
        private readonly IAssemblyVersioningService _assemblyVersioningService;
        private readonly ILogger<GetVersionIncrementHandler> _logger;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="mediator">An abstraction for accessing application behaviors.</param>
        /// <param name="gitVersioningService">An abstraction for retrieving version hint info from git commit messages.</param>
        /// <param name="assemblyVersioningService">An abstraction for working with assembly versions.</param>
        /// <param name="logger">A generic interface for logging.</param>
        public GetVersionIncrementHandler(
            IMediator mediator,
            IGitVersioningService gitVersioningService,
            IAssemblyVersioningService assemblyVersioningService,
            ILogger<GetVersionIncrementHandler> logger)
        {
            _mediator = mediator;
            _gitVersioningService = gitVersioningService;
            _assemblyVersioningService = assemblyVersioningService;
            _logger = logger;
        }

        /// <summary>Handles the request to increment the version with git integration.</summary>
        /// <param name="request">The <see cref="IRequest{TResponse}"/> object responsible for incrementing the version with git integration.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        public async Task<VersionIncrement> Handle(GetVersionIncrementQuery request, CancellationToken cancellationToken)
        {
            var query = new GetCommitVersionInfosQuery { GitDirectory = request.GitDirectory, RemoteTarget = request.RemoteTarget, TipBranchName = request.BranchName };
            List<GitCommitVersionInfo> versionInfos = (await _mediator.Send(query, cancellationToken)).ToList();
            VersionIncrement increment = _gitVersioningService.DeterminePriorityIncrement(versionInfos.Select(x => x.VersionIncrement));
            _logger.LogInformation($"Increment '{increment}' was determined from the commits.");

            if (string.IsNullOrWhiteSpace(request.TargetDirectory))
            {
                request.TargetDirectory = request.GitDirectory;
            }

            SemVersion assemblyVersion = _assemblyVersioningService.GetLatestAssemblyVersion(request.TargetDirectory, request.SearchOption);

            if (!versionInfos.Any(x => x.ExitBeta) && assemblyVersion < new SemVersion(1))
            {
                _logger.LogInformation($"Assembly currently in beta. Lowering increment: {increment}.");
                increment = increment.ToBeta();
                _logger.LogInformation($"Increment lowered to: {increment}");
            }

            return increment;
        }
    }
}
