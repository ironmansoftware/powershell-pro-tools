using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Classifies tokens for syntax highlighting.
    /// </summary>
    internal class ClassifierService
    {
        /// <summary>
        /// Classifies the specified tokens.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="spanStart"></param>
        /// <returns></returns>
        internal IEnumerable<ClassificationInfo> ClassifyTokens(IEnumerable<Token> tokens, int spanStart)
        {
            var info = new List<ClassificationInfo>();
            foreach (var token in tokens)
            {
                AddSpanForToken(token, spanStart, info);
            }

            return info;
        }

        private void AddSpanForToken(Token token, int spanStart, List<ClassificationInfo> classificationInfo)
        {
            var stringExpandableToken = token as StringExpandableToken;
            if (stringExpandableToken != null && stringExpandableToken.NestedTokens != null)
            {
                AddSpansForStringToken(stringExpandableToken, spanStart, classificationInfo);
            }

            ToClassificationInfo(token, token.Extent.StartOffset + spanStart, token.Extent.EndOffset - token.Extent.StartOffset, classificationInfo);
        }

        private IClassificationType GetClassificationType(Token token)
        {
            var pSTokenType = PSToken.GetPSTokenType(token);
            var classificationType = PowerShellClassifier.GetClassificationType(pSTokenType);
            return classificationType;
        }

        private void ToClassificationInfo(Token token, int start, int length, ICollection<ClassificationInfo> classificationInfo)
        {
            if (token != null && length > 0)
            {
                classificationInfo.Add(new ClassificationInfo(start, length, GetClassificationType(token)));
            }
        }

        private void AddSpansForStringToken(StringExpandableToken stringToken, int spanStart, List<ClassificationInfo> classificationInfo)
        {
            var startOffset = stringToken.Extent.StartOffset;
            foreach (var current in stringToken.NestedTokens)
            {
                ToClassificationInfo(stringToken, startOffset + spanStart, current.Extent.StartOffset - startOffset, classificationInfo);
                AddSpanForToken(current, spanStart, classificationInfo);
                startOffset = current.Extent.EndOffset;
            }

            ToClassificationInfo(stringToken, startOffset + spanStart, stringToken.Extent.EndOffset - startOffset, classificationInfo);
        }
    }

    /// <summary>
    /// Matches braces and regions for code folding.
    /// </summary>
    internal class RegionAndBraceMatchingService
    {
        private static char[] OpenChars { get; set; }

        static RegionAndBraceMatchingService()
        {
            OpenChars = new char[255];
            OpenChars[125] = '{';
            OpenChars[41] = '(';
            OpenChars[93] = '[';
        }

        internal void GetRegionsAndBraceMatchingInformation(string spanText,
                                    int spanStart,
                                    IList<Token> generatedTokens,
                                    out Dictionary<int, int> startBraces,
                                    out Dictionary<int, int> endBraces,
                                    out List<TagInformation<IOutliningRegionTag>> regions)
        {
            endBraces = new Dictionary<int, int>();
            startBraces = new Dictionary<int, int>();
            regions = new List<TagInformation<IOutliningRegionTag>>();
            var braceInformations = new List<BraceInformation>();
            var poundRegionStart = new Stack<Token>();
            var tokenOffset = 0;
            var tokenIndex = 0;
            while (tokenOffset < spanText.Length && tokenIndex < generatedTokens.Count)
            {
                var token = generatedTokens[tokenIndex];
                if (token.Kind == TokenKind.Comment)
                {
                    var text = token.Text;
                    if (text.Length >= 2 && text[0] == '<' && text[text.Length - 1] == '>')
                    {
                        var startOffset = token.Extent.StartOffset;
                        var endOffset = token.Extent.EndOffset;
                        AddMatch(spanStart, startBraces, endBraces, startOffset, endOffset - 1);
                    }
                    AddOutlinesForComment(spanStart, regions, spanText, poundRegionStart, token);
                    tokenOffset = token.Extent.EndOffset;
                    tokenIndex++;
                }
                else
                {
                    var stringToken = token as StringToken;
                    if (stringToken != null)
                    {
                        AddBraceMatchingAndOutlinesForString(spanStart, startBraces, endBraces, regions, spanText, stringToken);
                        tokenOffset = token.Extent.EndOffset;
                        tokenIndex++;
                    }
                    else
                    {
                        var c = spanText[tokenOffset];

                        if (c == '(' || c == '[' || c == '{')
                        {
                            OpenBrace(braceInformations, c, tokenOffset);
                            NextCharacter(ref tokenOffset, token, ref tokenIndex);
                            continue;
                        }

                        if (c == ')' || c == ']' || c == '}')
                        {
                            CloseBrace(spanText, spanStart, startBraces, endBraces, regions, c, braceInformations, tokenOffset);
                            NextCharacter(ref tokenOffset, token, ref tokenIndex);
                            continue;
                        }

                        if (c == '\\')
                        {
                            break;
                        }

                        NextCharacter(ref tokenOffset, token, ref tokenIndex);
                    }
                }
            }
        }

        private static void CloseBrace(string spanText,
                           int spanStart,
                           IDictionary<int, int> startBraces,
                           IDictionary<int, int> endBraces,
                           ICollection<TagInformation<IOutliningRegionTag>> regions,
                           char c,
                           IList<BraceInformation> braceInformations,
                           int tokenOffset)
        {
            var braceInformation = FindAndRemove(OpenChars[c], braceInformations);
            if (!braceInformation.HasValue) return;

            AddMatch(spanStart, startBraces, endBraces, braceInformation.Value.Position, tokenOffset);
            AddRegion(spanStart, spanText, regions, braceInformation.Value.Position + 1, tokenOffset);
        }

        private static void OpenBrace(ICollection<BraceInformation> braceInformations, char c, int tokenOffset)
        {
            braceInformations.Add(new BraceInformation(c, tokenOffset));
        }

        private static void NextCharacter(ref int tokenOffset, Token token, ref int tokenIndex)
        {
            tokenOffset++;
            if (tokenOffset > token.Extent.EndOffset)
            {
                tokenIndex++;
            }
        }

        private static void AddBraceMatchingAndOutlinesForString(int spanStart,
                                     IDictionary<int, int> startBraces,
                                     IDictionary<int, int> endBraces,
                                     ICollection<TagInformation<IOutliningRegionTag>> regions,
                                     string text,
                                     Token stringToken)
        {
            if (stringToken.Extent.StartLineNumber == stringToken.Extent.EndLineNumber)
            {
                return;
            }

            var startOffset = stringToken.Extent.StartOffset;
            var endOffset = stringToken.Extent.EndOffset;
            var text2 = text.Substring(startOffset, endOffset - startOffset);

            if (text2.StartsWith("\"", StringComparison.Ordinal) ||
            text2.StartsWith("\'", StringComparison.Ordinal))
            {
                startOffset++;
            }
            else
            {
                if (text2.StartsWith("@\"", StringComparison.Ordinal) ||
                    text2.StartsWith("@\'", StringComparison.Ordinal))
                {
                    startOffset += 2;
                }
            }
            if (text2.EndsWith("\"", StringComparison.Ordinal) ||
            text2.EndsWith("\'", StringComparison.Ordinal))
            {
                endOffset--;
            }
            else
            {
                if (text2.EndsWith("\"@", StringComparison.Ordinal) ||
                    text2.EndsWith("\'@", StringComparison.Ordinal))
                {
                    endOffset -= 2;
                }
            }
            int num3 = startOffset;
            int num4 = endOffset;
            if (text2.StartsWith("@\"\r\n", StringComparison.Ordinal) ||
            text2.StartsWith("@\'\r\n", StringComparison.Ordinal))
            {
                num3 += 2;
            }
            else
            {
                if (text2.StartsWith("@\"\n", StringComparison.Ordinal) ||
                    text2.StartsWith("@\'\n", StringComparison.Ordinal))
                {
                    num3++;
                }
            }
            if (text2.EndsWith("\r\n\"@", StringComparison.Ordinal) ||
            text2.EndsWith("\r\n\'@", StringComparison.Ordinal))
            {
                num4 -= 2;
            }
            else
            {
                if (text2.EndsWith("\n\"@", StringComparison.Ordinal) ||
                    text2.EndsWith("\n\'@", StringComparison.Ordinal))
                {
                    num4--;
                }
            }
            if (num4 < num3)
            {
                num4 = num3;
            }

            var collapsedTooltip = text.Substring(num3, num4 - num3);
            AddRegion(spanStart, text, regions, startOffset, endOffset, null, collapsedTooltip);
        }


        private static BraceInformation? FindAndRemove(char c, IList<BraceInformation> braces)
        {
            if (!braces.Any()) return null;

            for (var i = braces.Count - 1; i >= 0; i--)
            {
                var value = braces[i];
                if (value.Character != c) continue;

                braces.RemoveAt(i);
                return value;
            }
            return null;
        }

        private static void AddRegion(int spanStart,
                          string text,
                          ICollection<TagInformation<IOutliningRegionTag>> regions,
                          int start,
                          int end)
        {
            AddRegion(spanStart, text, regions, start, end, null, null);
        }

        private static void AddRegion(int spanStart,
                          string text,
                          ICollection<TagInformation<IOutliningRegionTag>> regions,
                          int start,
                          int end,
                          string collapsedText)
        {
            AddRegion(spanStart, text, regions, start, end, collapsedText, null);
        }

        private static void AddRegion(int spanStart,
                          string text,
                          ICollection<TagInformation<IOutliningRegionTag>> regions,
                          int start,
                          int end,
                          string collapsedText,
                          string collapsedTooltip)
        {
            if (collapsedText == null)
            {
                collapsedText = "...";
            }

            var length = end - start;
            if (collapsedTooltip == null)
            {
                collapsedTooltip = text.Substring(start, length);
            }
            if (text.IndexOf('\n', start, end - start) == -1)
            {
                return;
            }
            var tag = new OutliningRegionTag(false, false, collapsedText, collapsedTooltip);
            regions.Add(new TagInformation<IOutliningRegionTag>(start + spanStart, length, tag));
        }

        private static void AddMatch(int spanStart,
                         IDictionary<int, int> startBraces,
                         IDictionary<int, int> endBraces,
                         int start,
                         int end)
        {
            start += spanStart;
            end += spanStart;
            endBraces[end] = start;
            startBraces[start] = end;
        }

        private static void AddOutlinesForComment(int spanStart,
                              ICollection<TagInformation<IOutliningRegionTag>> regions,
                              string text,
                              Stack<Token> poundRegionStart,
                              Token commentToken)
        {
            var commentText = commentToken.Text;
            if (commentText.IndexOf('\n') != -1)
            {
                int endOffset = commentToken.Extent.EndOffset;
                int startOffset = commentToken.Extent.StartOffset;
                if (commentText.StartsWith("<#", StringComparison.Ordinal))
                {
                    startOffset += 2;
                }
                if (commentText.EndsWith("#>", StringComparison.Ordinal))
                {
                    endOffset -= 2;
                }
                AddRegion(spanStart, text, regions, startOffset, endOffset);
                return;
            }
            if (commentText.StartsWith("#region", StringComparison.Ordinal))
            {
                poundRegionStart.Push(commentToken);
                return;
            }
            if (commentText.StartsWith("#endregion", StringComparison.Ordinal) && poundRegionStart.Count != 0)
            {
                var token = poundRegionStart.Pop();
                var regionText = token.Text;
                var startOffset = token.Extent.StartOffset;
                var endOffset = commentToken.Extent.EndOffset;
                AddRegion(spanStart, text, regions, startOffset, endOffset, regionText + "...");
            }
        }

    }
}

