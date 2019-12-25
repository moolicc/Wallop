namespace WallApp.App.Layout
{
    interface ILayoutScript
    {
        void Execute(string source);
        void Cleanup();
    }
}
