using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forms = System.Windows.Forms;
using WallApp.UI.Models;
using WallApp.UI.ViewModels;
using WallApp.Scripting;
using System.Windows;
using System.IO;
using System.Windows.Markup;
using System.Windows.Controls;

namespace WallApp.UI.Interop
{
    class LayerSettingsModel : Models.LayerSettingsModel
    {
        private List<Models.LayerSettings> _layers;
        private int _currentLayer;

        public LayerSettingsModel()
        {
            Layout.EnterPreview();
            _layers = new List<Models.LayerSettings>();

            var screens = GetScreens();
            var effects = GetEffects();
            var modules = GetModules();

            _layers.AddRange(Layout.Layers.Select(l =>
            {
                return new Models.LayerSettings()
                {
                    Name = l.Name,
                    Description = l.Description,
                    ScreenIndex = GetIndexFromName(l.Dimensions.MonitorName, screens),
                    AbsToggle = l.Dimensions.AbsoluteValues,
                    MarginsToggle = l.Dimensions.MarginValues,
                    CurrentPosition = (l.Dimensions.XValue, l.Dimensions.YValue, l.Dimensions.ZValue, l.Dimensions.WValue),
                    TintColor = (l.TintColor.A, l.TintColor.R, l.TintColor.G, l.TintColor.B),
                    EffectIndex = GetIndexFromName(l.Effect, effects),

                    Enabled = l.Enabled,
                    ModuleIndex = GetIndexFromName(l.Module, modules),
                    ModuleName = l.Module,
                    ID = l.LayerId,
                };
            }));

            _currentLayer = 0;
        }

        public override LayerItemViewModel AddNewLayerItem()
        {
            var newLayer = new Models.LayerSettings()
            {
                Name = "New Layer",
                Description = "",
                ScreenIndex = 0,
                AbsToggle = true,
                MarginsToggle = false,
                CurrentPosition = (0, 0, 100, 100),
                TintColor = (255, 255, 255, 255),
                EffectIndex = 0,
                ModuleIndex = 0,
                Enabled = true,
                ID = _layers.DefaultIfEmpty(new Models.LayerSettings() { ID = -1 }).Max(l => l.ID) + 1,
                ModuleName = GetModules().ElementAt(0),
            };
            _layers.Add(newLayer);

            Layout.Layers.Add(new LayerSettings()
            {
                Description = newLayer.Description,
                Effect = GetEffects().ElementAt(newLayer.EffectIndex),
                Dimensions = new LayerDimensions()
                {
                    AbsoluteValues = true,
                    MarginValues = false,
                    XValue = 0,
                    YValue = 0,
                    ZValue = 100,
                    WValue = 100,
                },
                Enabled = newLayer.Enabled,
                LayerId = newLayer.ID,
                Module = GetModules().ElementAt(0),
                Name = newLayer.Name,
                Opacity = 1.0F,
                TintColor = Microsoft.Xna.Framework.Color.White,
            });

            _currentLayer = _layers.Count - 1;
            return new LayerItemViewModel()
            {
                Name = newLayer.Name,
                Description = newLayer.Description,
                ID = newLayer.ID.ToString(),
                Enabled = newLayer.Enabled,
                Module = GetModules().ElementAt(newLayer.ModuleIndex),
            };

        }

        public override Models.LayerSettings GetCurrentLayer()
        {
            return _layers[_currentLayer];
        }

        public override string[] GetEffects()
        {
            return Effects.GetEffects();
        }

        public override LayerItemViewModel[] GetLayerItems()
        {
            return _layers.Select(l => new LayerItemViewModel()
                {
                    Name = l.Name,
                    Description = l.Description,
                    ID = l.ID.ToString(),
                    Enabled = l.Enabled,
                    Module = GetModules().ElementAt(l.ModuleIndex),
                }).ToArray();
        }

        public override string[] GetScreens()
        {
            return Forms.Screen.AllScreens.Select(s => s.DeviceName).ToArray();
        }

        public override string[] GetModules()
        {
            Module[] modules = Resolver.Cache.Values.ToArray();
            return modules.Select(m => m.Name).ToArray();
        }

        public override void SetActiveLayer(int index)
        {
            _currentLayer = index;
        }

        private int GetIndexFromName(string name, string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if(values[i] == name)
                {
                    return i;
                }
            }
            return 0;
        }

        public override (bool Result, string Message) UpdateCurrentLayer()
        {
            var curLayer = _layers[_currentLayer];
            if(curLayer.ModuleIndex < 0 || curLayer.ModuleIndex >= GetModules().Length)
            {
                return (false, "Invalid module specified");
            }
            if (curLayer.ScreenIndex < 0 || curLayer.ScreenIndex >= GetScreens().Length)
            {
                return (false, "Invalid screen specified");
            }
            if (curLayer.EffectIndex < 0 || curLayer.EffectIndex >= GetEffects().Length)
            {
                return (false, "Invalid effect specified");
            }

            var layer = Layout.Layers[_currentLayer];
            layer.Description = curLayer.Description;
            layer.Name = curLayer.Name;
            layer.Dimensions.AbsoluteValues = curLayer.AbsToggle;
            layer.Dimensions.MarginValues = curLayer.MarginsToggle;
            layer.Dimensions.MonitorName = GetScreens().ElementAt(curLayer.ScreenIndex);
            layer.Dimensions.XValue = curLayer.CurrentPosition.X;
            layer.Dimensions.YValue = curLayer.CurrentPosition.Y;
            layer.Dimensions.ZValue = curLayer.CurrentPosition.Z;
            layer.Dimensions.WValue = curLayer.CurrentPosition.W;
            layer.TintColor = new Microsoft.Xna.Framework.Color(curLayer.TintColor.R, curLayer.TintColor.G, curLayer.TintColor.B, curLayer.TintColor.A);
            layer.Effect = GetEffects().ElementAt(curLayer.EffectIndex);
            layer.Enabled = curLayer.Enabled;
            layer.Module = GetModules().ElementAt(curLayer.ModuleIndex);
            layer.LayerId = curLayer.ID;

            return (true, "");
        }

        public override void Exit(bool accept)
        {
            Layout.ExitPreview(accept);
            UpdateLayout();
        }

        public override int RemoveSelectedLayer()
        {
            if (_currentLayer < 0 || _currentLayer >= _layers.Count)
            {
                return -1;
            }
            _layers.RemoveAt(_currentLayer);
            Layout.Layers.RemoveAt(_currentLayer);
            _currentLayer--;
            return _currentLayer;
        }

        public override LayerItemViewModel GetCurrentLayerUI()
        {
            var curLayer = GetCurrentLayer();
            return new LayerItemViewModel()
            {
                Name = curLayer.Name,
                Description = curLayer.Description,
                ID = curLayer.ID.ToString(),
                Enabled = curLayer.Enabled,
                Module = GetModules().ElementAt(curLayer.ModuleIndex),
            };
        }

        public override object GetLayerSettingsView()
        {
            //TODO: Refactor thie view's creation out of here. It should be a Type in the new manifestreader.
            var module = Resolver.GetCachedModuleFromName(_layers[_currentLayer].ModuleName);
            var xamlFile = module.ViewSourceFile;
            if(string.IsNullOrWhiteSpace(xamlFile) || !File.Exists(xamlFile))
            {
                return "There are no settings for this module.";
            }
            UserControl rootObject = null;
            try
            {
                using (var reader = new StreamReader(xamlFile))
                {
                    rootObject = XamlReader.Load(reader.BaseStream) as UserControl;
                }
                if(rootObject == null)
                {
                    return "The settings view must have be of type UserControl.";
                }
                rootObject.DataContext = module.CreateViewModel(Layout.Layers[_currentLayer]);
            }
            catch (Exception ex)
            {
                return "An error has ocurred in the creation of the settings view.\r\n"
                    + ex.Message;
            }
            return rootObject;
        }

        public override void UpdateLayout()
        {
            if(UI.ModelProvider.Instance is ModelProvider provider)
            {
                provider.GameInsance.InitNewLayout();
            }
        }

        public override void TickDraw()
        {
            if (UI.ModelProvider.Instance is ModelProvider provider)
            {
                provider.GameInsance.Tick();
            }
        }
    }
}
