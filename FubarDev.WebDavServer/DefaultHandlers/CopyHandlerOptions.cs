namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class CopyHandlerOptions
    {
        public RecursiveProcessingMode Mode { get; set; }
        public bool OverwriteAsDefault { get; set; } = true;
    }
}
