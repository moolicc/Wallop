namespace WallApp.App.Services
{
    interface IService
    {
        int InitPriority { get; }
        void Initialize();
    }
}
