// <copyright file="Overwrite.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class Overwrite
    {
        public static bool? Parse(string overwrite)
        {
            if (string.IsNullOrWhiteSpace(overwrite))
                return null;
            overwrite = overwrite.Trim();
            if (overwrite == "T")
                return true;
            if (overwrite == "F")
                return false;
            throw new NotSupportedException($"Overwrite value '{overwrite}' isn't supported");
        }
    }
}
