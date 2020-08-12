namespace Wallop.Composer.Services
{
    interface IService
    {
        int InitPriority { get; }
        void Initialize();
    }
}
