namespace WallApp.UI.Models
{
    public abstract class LayerSettingsModel
    {
        public abstract string[] GetScreens();
        public abstract string[] GetEffects();

        public abstract string[] GetModules();

        public abstract ViewModels.LayerItemViewModel[] GetLayerItems();

        public abstract ViewModels.LayerItemViewModel AddNewLayerItem();

        public abstract void SetActiveLayer(int index);

        public abstract LayerSettings GetCurrentLayer();
        public abstract ViewModels.LayerItemViewModel GetCurrentLayerUI();

        public abstract (bool Result, string Message)  UpdateCurrentLayer();

        public abstract int RemoveSelectedLayer();

        public abstract void Exit(bool accept);

        public abstract object GetLayerSettingsView();

        public abstract void UpdateLayout();
    }
}
