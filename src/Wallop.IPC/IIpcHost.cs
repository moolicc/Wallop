namespace Wallop.IPC
{
    public interface IIpcHost : IIpcEndpoint
    {
        void Listen();
        void Shutdown();
    }
}