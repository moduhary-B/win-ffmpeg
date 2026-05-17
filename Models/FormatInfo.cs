namespace WinFfmpeg.Models
{
    public class FormatInfo
    {
        public string Extension { get; set; }
        public string DisplayName { get; set; }
        public FormatCategory Category { get; set; }
        public bool IsPopular { get; set; }
        public string DefaultCodec { get; set; }
        public string DefaultAudioCodec { get; set; }
        public string ExtraParams { get; set; }
        public string Description { get; set; }

        public FormatInfo(string extension, string displayName, FormatCategory category,
            bool isPopular = false, string defaultCodec = null, string defaultAudioCodec = null,
            string extraParams = null, string description = null)
        {
            Extension = extension;
            DisplayName = displayName;
            Category = category;
            IsPopular = isPopular;
            DefaultCodec = defaultCodec;
            DefaultAudioCodec = defaultAudioCodec;
            ExtraParams = extraParams;
            Description = description;
        }
    }
}
