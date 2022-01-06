// <copyright file="IfHeaderMatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Utils;

/// <summary>
/// Finds matching If headers.
/// </summary>
public class IfHeaderMatcher
{
    private readonly IWebDavContext _context;
    private readonly IFileSystem _fileSystem;
    private readonly string _targetPath;
    private readonly string? _owner;
    private readonly IReadOnlyCollection<IfHeader> _ifHeaders;
    private readonly IEqualityComparer<EntityTag> _entityTagComparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="IfHeaderMatcher"/> class.
    /// </summary>
    /// <param name="context">The WebDAV context.</param>
    /// <param name="fileSystem">The file system to get the ETags from.</param>
    /// <param name="targetPath">The path of the target of the operation.</param>
    /// <param name="ifHeaders">The <c>If</c> headers provided by the client.</param>
    /// <param name="entityTagComparer">Comparer for entity tags.</param>
    public IfHeaderMatcher(
        IWebDavContext context,
        IFileSystem fileSystem,
        string targetPath,
        IReadOnlyCollection<IfHeader> ifHeaders,
        IEqualityComparer<EntityTag>? entityTagComparer = null)
    {
        _context = context;
        _fileSystem = fileSystem;
        _targetPath = targetPath;
        _ifHeaders = ifHeaders;
        _entityTagComparer = entityTagComparer ?? EntityTagComparer.Strong;
    }

    /// <summary>
    /// Finds the <c>If</c> headers for the provided state tokens.
    /// </summary>
    /// <param name="stateTokens">The state tokens.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The matched <c>If</c> headers.</returns>
    public IAsyncEnumerable<IfHeaderMatch> FindAsync(
        IReadOnlyCollection<Uri> stateTokens,
        CancellationToken cancellationToken = default)
    {
        return _ifHeaders
            .ToAsyncEnumerable()
            .SelectMany(ifHeader => FindAsync(stateTokens, ifHeader, cancellationToken));
    }

    private async IAsyncEnumerable<IfHeaderMatch> FindAsync(
        IReadOnlyCollection<Uri> stateTokens,
        IfHeader ifHeader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (ifHeader.IsNoTagList)
        {
            EntityTag? entityTag = null;
            var requiresEntityTag = ifHeader.NoTagLists
                .Any(x => x.List.RequiresEntityTag);
            if (requiresEntityTag)
            {
                var result = await _fileSystem.SelectAsync(_targetPath, cancellationToken);
                if (!result.IsMissing)
                {
                    entityTag = await result.TargetEntry.GetEntityTagAsync(cancellationToken);
                }
            }

            var compareInformation = new CompareInformation(stateTokens, entityTag);
            foreach (var noTagList in ifHeader.NoTagLists)
            {
                if (IsMatch(compareInformation, noTagList))
                {
                    yield return new IfHeaderMatch(ifHeader, noTagList);
                }
            }
        }
        else
        {
            foreach (var taggedList in ifHeader.TaggedLists)
            {
                EntityTag? entityTag = null;
                var requiresEntityTag = ifHeader.TaggedLists
                    .Any(x => x.Lists.Any(l => l.RequiresEntityTag));
                if (requiresEntityTag)
                {
                    if (_context.TryGetPathFor(taggedList, out var path))
                    {
                        var result = await _fileSystem.SelectAsync(path, cancellationToken);
                        if (!result.IsMissing)
                        {
                            entityTag = await result.TargetEntry.GetEntityTagAsync(cancellationToken);
                        }
                    }
                }

                var compareInformation = new CompareInformation(
                    stateTokens,
                    entityTag);
                foreach (var list in taggedList.Lists)
                {
                    if (IsMatch(compareInformation, list))
                    {
                        // Return only one result per <c>Tagged-list</c>
                        yield return new IfHeaderMatch(ifHeader, taggedList, list);
                        break;
                    }
                }
            }
        }
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IfNoTagList noTagList)
    {
        return IsMatch(compareInformation, noTagList.List);
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IEnumerable<IfCondition> conditions)
    {
        return conditions.All(condition => IsMatch(compareInformation, condition));
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IfCondition condition)
    {
        bool result;

        if (condition.EntityTag.HasValue)
        {
            if (compareInformation.EntityTag == null)
            {
                result = false;
            }
            else
            {
                result = _entityTagComparer.Equals(
                    compareInformation.EntityTag.Value,
                    condition.EntityTag.Value);
            }
        }
        else if (condition.StateToken != null)
        {
            result = compareInformation.StateTokens.Any(x => x.Equals(condition.StateToken));
        }
        else
        {
            // Invalid (empty) condition
            return false;
        }

        return condition.Not ? !result : result;
    }

    private record CompareInformation(
        IReadOnlyCollection<Uri> StateTokens,
        EntityTag? EntityTag);
}
