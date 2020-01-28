namespace Wallop.Composer.Services
{
    // NEW NOTES
    // Only other services should call into BridgeService.WriteX.
    // This isolates communication to the engine to services and away from
    // the UI.

    class BridgeService : IService
    {
        public int InitPriority => 0;
        public Bridge.Master Engine { get; private set; }
        public Wallop.Bridge.MessageScheduler Scheduler { get; private set; }

        public void Initialize()
        {
            Engine = new Bridge.Master(App.BaseDir + "Engine.exe");
            Scheduler = new Bridge.MessageScheduler(new Bridge.InputReader<Bridge.Data.IPayload>(Engine));
        }

        public void WriteSetEditMode(bool editModeEnabled)
        {
            Engine.Write(new Bridge.Data.EditModePayload(editModeEnabled));
        }

        public void WriteAddLayer(string module)
        {
            Engine.Write(new Bridge.Data.LayerCreationPayload(module));
        }
    }
}
