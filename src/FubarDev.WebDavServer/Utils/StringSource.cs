﻿// <copyright file="StringSource.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Utils
{
    internal class StringSource
    {
        private readonly string _s;

        private int _currentIndex;

        public StringSource(string s)
        {
            _s = s;
        }

        public bool Empty => _currentIndex >= _s.Length;

        public string Remaining => _s.Substring(_currentIndex);

        public char Get()
        {
            return _s[_currentIndex++];
        }

        public void Back()
        {
            _currentIndex -= 1;
        }

        public bool AdvanceIf(string text)
        {
            return AdvanceIf(text, StringComparison.Ordinal);
        }

        public bool AdvanceIf(string text, StringComparison comparer)
        {
            if (!_s.Substring(_currentIndex).StartsWith(text, comparer))
                return false;

            _currentIndex += text.Length;
            return true;
        }

        public StringSource Advance(int count)
        {
            _currentIndex += count;
            return this;
        }

        public bool SkipWhiteSpace()
        {
            while (!Empty)
            {
                if (!char.IsWhiteSpace(_s, _currentIndex))
                    break;
                _currentIndex += 1;
            }

            return Empty;
        }

        public string? GetUntil(char ch)
        {
            var index = _s.IndexOf(ch, _currentIndex);
            if (index == -1)
                return null;
            var result = _s.Substring(_currentIndex, index - _currentIndex);
            _currentIndex = index;
            return result;
        }
    }
}
