using Domain.Entities;
using Domain.Enumerations;
using MediatR;
using System.Collections.Generic;
using System.IO;

namespace Application.GitVersioning.Queries
{
    /// <summary>
    /// The <see cref="IRequest{TResponse}"/> object responsible for retrieving a <see cref="VersionIncrement"/>.
    /// </summary>
    public class GetVersionIncrementQuery : IRequest<VersionIncrement>
    {
        /// <summary>
        /// The directory containing the .git folder.
        /// </summary>
        public string GitDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The git remote target. Defaults to 'origin'.
        /// </summary>
        public string RemoteTarget { get; set; } = "origin";

        /// <summary>
        /// The name of the branch to determine the <see cref="VersionIncrement"/>.
        /// </summary>
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// The directory to target for file versioning.
        /// </summary>
        public string TargetDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Search option to use with the <see cref="TargetDirectory"/>.
        /// </summary>
        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;
    }
}
