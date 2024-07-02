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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellTools.Repl {
    public static class ReplConstants {
        public const string ReplContentTypeName = "PowerShellREPLCode";
        public const string ReplOutputContentTypeName = "PowerShellREPLOutput";

        /// <summary>
        /// The additional role found in any PowerShell REPL editor window.
        /// </summary>
        public const string ReplTextViewRole = "PowerShellREPL";

        internal const string ReplMouseProcessor = "PowerShellReplWindowMouseProcessor";

        // These are used for Registry serialization.
        internal const string ActiveReplsKey = "PowerShellActiveRepls";
        internal const string ContentTypeKey = "PowerShellContentType";
        internal const string RolesKey = "PowerShellRoles";
        internal const string TitleKey = "PowerShellTitle";
        internal const string ReplIdKey = "PowerShellReplId";
        internal const string LanguageServiceGuidKey = "PowerShellLanguageServiceGuid";
    }
}
