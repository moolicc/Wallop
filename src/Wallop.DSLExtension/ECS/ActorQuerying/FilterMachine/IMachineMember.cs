namespace Wallop.Shared.ECS.ActorQuerying.FilterMachine
{
    public interface IMachineMember
    {
        string Tag { get; }
        string Name { get; }
        bool TryExecute(Machine machine, string[] qualifiers, int argCount);
    }
}