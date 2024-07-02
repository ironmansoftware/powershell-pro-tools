/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

// Guids.cs
using System;

namespace PowerShellTools.Repl
{
    public static class GuidList
    {
        public const string guidReplWindowPkgString = "702AA1B3-556E-4AEE-AFB6-DCFA7FC420D3";
        public const string guidReplWindowCmdSetString = "A82B8BE2-8794-40EB-B77A-7B4D2FCF69EB";

        public static readonly Guid guidReplWindowCmdSet = new Guid(guidReplWindowCmdSetString);
    };
}