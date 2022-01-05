// <copyright file="IfHeaderMatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Utils;

public class IfHeaderMatcher
{
    private readonly IReadOnlyCollection<IfHeader> _ifHeaders;
    private readonly IWebDavContext _context;
    private readonly IEqualityComparer<EntityTag> _entityTagComparer;

    public IfHeaderMatcher(
        IReadOnlyCollection<IfHeader> ifHeaders,
        IWebDavContext context,
        IEqualityComparer<EntityTag>? entityTagComparer = null)
    {
        _ifHeaders = ifHeaders;
        _context = context;
        _entityTagComparer = entityTagComparer ?? EntityTagComparer.Strong;
    }

    public IEnumerable<IfHeader> Find(
        string path,
        IReadOnlyCollection<Uri> stateTokens,
        EntityTag? entityTag)
    {
        var compareInformation = new CompareInformation(path, stateTokens, entityTag);
        return _ifHeaders.Where(ifHeader => IsMatch(compareInformation, ifHeader));
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IfHeader ifHeader)
    {
        return ifHeader.IsTaggedList
            ? IsMatch(compareInformation, ifHeader.TaggedLists)
            : IsMatch(compareInformation, ifHeader.NoTagLists);
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IEnumerable<IfNoTagList> noTagLists)
    {
        return noTagLists.Any(noTagList => IsMatch(compareInformation, noTagList));
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IEnumerable<IfTaggedList> taggedLists)
    {
        return taggedLists.Any(taggedList => IsMatch(compareInformation, taggedList));
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IfNoTagList noTagList)
    {
        return IsMatch(compareInformation, noTagList.List);
    }

    private bool IsMatch(
        CompareInformation compareInformation,
        IfTaggedList taggedList)
    {
        if (!taggedList.TryGetPath(_context, out var path))
        {
            return false;
        }

        if (!StringComparer.OrdinalIgnoreCase.Equals(path, compareInformation.Path))
        {
            return false;
        }

        return taggedList.Lists
            .Any(list => IsMatch(compareInformation, list));
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
        string Path,
        IReadOnlyCollection<Uri> StateTokens,
        EntityTag? EntityTag);
}
