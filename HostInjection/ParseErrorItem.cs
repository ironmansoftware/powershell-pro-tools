namespace PowerShellTools.Common.ServiceManagement.IntelliSenseContract
{
    public sealed class ParseErrorItem
    {
        public ParseErrorItem(string message, int startOffset, int endOffset)
        {
            Message = message;
            ExtentStartOffset = startOffset;
            ExtentEndOffset = endOffset;
        }

        public string Message { get; private set; }

        public int ExtentStartOffset { get; private set; }

        public int ExtentEndOffset { get; private set; }

    }
}
