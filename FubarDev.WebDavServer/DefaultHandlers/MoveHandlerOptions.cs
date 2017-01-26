namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class MoveHandlerOptions
    {
        public RecursiveProcessingMode Mode { get; set; }
        public bool OverwriteAsDefault { get; set; } = true;
    }
}
