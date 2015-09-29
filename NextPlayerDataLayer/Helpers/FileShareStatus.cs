
namespace NextPlayerDataLayer.Helpers
{
    public enum FileShareStatus : byte
    {
        None,
        Waiting,
        Connecting,
        Sharing,
        Completed,
        Error,
        Cancelled
    }
}
