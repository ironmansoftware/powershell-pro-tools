namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// An enum which is synchronized with our image list for the various
    /// kinds of images which are available.  This can be combined with the 
    /// ImageListOverlay to select an image for the appropriate member type
    /// and indicate the appropiate visiblity.  These can be combined with
    /// GetImageListIndex to get the final index.
    /// 
    /// Most of these are unused as we're just using an image list shipped
    /// by the VS SDK.
    /// </summary>
    internal enum ImageListKind
    {
        Class,
        Unknown1,
        Unknown2,
        Enum,
        Unknown3,
        Lightning,
        Unknown4,
        BlueBox,
        Key,
        BlueStripe,
        ThreeDashes,
        TwoBoxes,
        Method,
        StaticMethod,
        Unknown6,
        Namespace,
        Unknown7,
        Property,
        Unknown8,
        Unknown9,
        Unknown10,
        Unknown11,
        Unknown12,
        Unknown13,
        ClassMethod
    }

    /// <summary>
    /// Indicates the overlay kind which should be used for a drop down members
    /// image.  The overlay kind typically indicates visibility.
    /// 
    /// Most of these are unused as we're just using an image list shipped
    /// by the VS SDK.
    /// </summary>
    internal enum ImageListOverlay
    {
        ImageListOverlayNone,
        ImageListOverlayLetter,
        ImageListOverlayBlue,
        ImageListOverlayKey,
        ImageListOverlayPrivate,
        ImageListOverlayArrow,
    }

    internal static class EntryInfoEnumsExtensions
    {
        /// <summary>
        /// Turns an image list kind / overlay into the proper index in the image list.
        /// </summary>
        public static int GetImageListIndex(ImageListKind kind, ImageListOverlay overlay)
        {
            return ((int)kind) * 6 + (int)overlay;
        }
    }
}