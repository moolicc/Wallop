using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Wallop.Presenter.Scripting;

namespace Wallop.Presenter.Services
{
    class ControllerService
    {
        public Controller[] Controllers => _controllersCached;

        private LayoutTrackingService _layoutTracker;
        private GraphicsDevice _graphicsDevice;

        private Dictionary<int, Controller> _controllers;
        private Controller[] _controllersCached;

        public ControllerService()
        {
            _controllers = new Dictionary<int, Controller>();
            _controllersCached = new Controller[0];
            _layoutTracker = ServiceProvider.GetService<LayoutTrackingService>();
            _graphicsDevice = ServiceProvider.GetService<GraphicsDevice>();
            _layoutTracker.LayerAdded += LayerAdded;
            _layoutTracker.LayerResized += LayerResized;
            _layoutTracker.LayerRemoved += LayerRemoved;
        }

        public void Reset()
        {
            if (_controllers != null)
            {
                foreach (var controller in _controllers.Values)
                {
                    controller.Rendering.RenderTarget.Dispose();
                    controller.Dispose();
                }

                _controllers.Clear();
            }

            foreach (var item in _layoutTracker.Layout.Layers)
            {
                LayerAdded(item);
            }
        }

        private void LayerAdded(LayerSettings layerSettings)
        {
            _controllers.Add(layerSettings.LayerId, GetNewController(layerSettings));
            _controllersCached = _controllers.Values.ToArray();
        }

        private void LayerResized(LayerSettings layerSettings)
        {
            var controller = _controllers[layerSettings.LayerId];

            (float x, float y, float width, float height) = layerSettings.Dimensions;

            var scaledLayerBounds = new RectangleF(
                (x * Settings.Instance.BackBufferWidthFactor),
                (y * Settings.Instance.BackBufferHeightFactor),
                (width * Settings.Instance.BackBufferWidthFactor),
                (height * Settings.Instance.BackBufferHeightFactor));

            var renderTarget = new RenderTarget2D(_graphicsDevice, (int)scaledLayerBounds.Width, (int)scaledLayerBounds.Height);

            controller.Rendering.RenderTarget.Dispose();
            controller.Rendering = new Rendering(_graphicsDevice, renderTarget);
            controller.Rendering.ActualX = (int)scaledLayerBounds.X;
            controller.Rendering.ActualY = (int)scaledLayerBounds.Y;
            controller.Rendering.ActualWidth = (int)scaledLayerBounds.Width;
            controller.Rendering.ActualHeight = (int)scaledLayerBounds.Height;
        }

        private void LayerRemoved(int layerId)
        {
            _controllers[layerId].Dispose();
            _controllers.Remove(layerId);
        }

        private Controller GetNewController(LayerSettings settings)
        {
            //Get the current layer's module out of the cache. This creates a clone instead of referencing
            //the module in the layer.
            var module = ModuleCache.GetCachedModuleFromName(settings.Module);

            //Create the controller to be used.
            var controller = CsModule.CreateController(module);

            //Get the user-specified dimensions of the layer.
            (float x, float y, float width, float height) = settings.Dimensions;

            //Pass the layer's configuration to the controller.
            controller.Settings = settings;
            controller.Module = module;
            var scaledLayerBounds = new RectangleF(
                (x * Settings.Instance.BackBufferWidthFactor),
                (y * Settings.Instance.BackBufferHeightFactor),
                (width * Settings.Instance.BackBufferWidthFactor),
                (height * Settings.Instance.BackBufferHeightFactor));

            //We setup the rendertarget that the controller will draw to.
            var renderTarget = new RenderTarget2D(_graphicsDevice, (int)scaledLayerBounds.Width, (int)scaledLayerBounds.Height);

            //Init the controller's rendering parameters.
            controller.Rendering = new Rendering(_graphicsDevice, renderTarget);
            controller.Rendering.ActualX = (int)scaledLayerBounds.X;
            controller.Rendering.ActualY = (int)scaledLayerBounds.Y;
            controller.Rendering.ActualWidth = (int)scaledLayerBounds.Width;
            controller.Rendering.ActualHeight = (int)scaledLayerBounds.Height;

            //Allow the controller to handle any initialization is needs.
            try
            {
                controller.Setup();
            }
            catch (Exception ex)
            {
            }

            /* TODO
            //Cache any effects.
            if (!string.IsNullOrWhiteSpace(settings.Effect))
            {
                if (!_effectsCache.ContainsKey(settings.Effect))
                {
                    _effectsCache.Add(settings.Effect, Effects.CreateEffect(settings.Effect, Content));
                }
            }
            */

            return controller;
        }
    }
}
