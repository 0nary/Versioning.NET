﻿using Application.GitVersioning.Commands;
using FluentValidation;
using System.IO;

namespace Application.GitVersioning.Validators
{
    /// <summary>
    /// The validator responsible for enforcing business rules for the <see cref=" IncrementVersionByWithGitIntegrationCommand"/>.
    /// </summary>
    public class  IncrementVersionByWithGitIntegrationValidator : AbstractValidator< IncrementVersionByWithGitIntegrationCommand>
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public  IncrementVersionByWithGitIntegrationValidator()
        {
            RuleFor(x => x.GitDirectory).Must(Directory.Exists).WithMessage("Must be a valid directory.");
            RuleFor(x => x.GitDirectory).Must(x => Directory.Exists(Path.Join(x, ".git"))).WithMessage("Must be a valid .git directory.");
            RuleFor(x => x.CommitAuthorEmail).NotNull().NotEmpty();
            RuleFor(x => x.BranchName).NotNull().NotEmpty();

            RuleFor(x => x.TargetDirectory).Must(x => Directory.Exists(x)).WithMessage("Must be a valid directory.")
                .When(x => !string.IsNullOrWhiteSpace(x.TargetDirectory));
        }
    }
}
