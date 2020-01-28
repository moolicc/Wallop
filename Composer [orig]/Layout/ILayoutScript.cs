namespace Wallop.Composer.Layout
{
    interface ILayoutScript
    {
        void Execute(string source);
        void Cleanup();
    }
}
