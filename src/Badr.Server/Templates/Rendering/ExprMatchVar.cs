//
// ExprMatchVar.cs
//
// Author: najmeddine nouri
//
// Copyright (c) 2013 najmeddine nouri, amine gassem
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Except as contained in this notice, the name(s) of the above copyright holders
// shall not be used in advertising or otherwise to promote the sale, use or other
// dealings in this Software without prior written authorization.
//
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Badr.Server.Templates.Rendering
{
    public class ExprMatchVar
    {
        public ExprMatchVar(Match variableMatch)
        {
            Variable = new TemplateVar(variableMatch.Groups[BadrGrammar.GROUP_VARIABLE_VALUE].Value);
            Group filterGroup = variableMatch.Groups[BadrGrammar.GROUP_VARIABLE_FILTER];
            if (filterGroup != null)
            {
                int capCount = filterGroup.Captures.Count;
                if (capCount > 0)
                {
                    Filters = new KeyValuePair<string, TemplateVar>[capCount];
                    for (int j = 0; j < capCount; j++)
                    {
                        string[] filterNameAndArg = filterGroup.Captures[j].Value.Split(':');
                        string filterName = filterNameAndArg[0];
                        TemplateVar filterArg = filterNameAndArg.Length > 1 ? new TemplateVar(filterNameAndArg[1]) : null;
                        Filters[j] = new KeyValuePair<string, TemplateVar>(filterName, filterArg);
                    }
                }
            }
        }

        protected internal TemplateVar Variable { get; private set; }
        protected internal KeyValuePair<string, TemplateVar>[] Filters { get; private set; }
    }
}