# Wall Apparatus
## Also known as: WallApp, just so I can pronounce it "wallop".
WallApp is an engine for "living" desktop wallpapers. It uses Monogame at its core to drive things. Built around extensions called *modules*, wallapp allows you to write-in new things using the familiar update/draw loop in c# script files. Backgrounds are built using one or more *layers*. Each layer is associated with a module and a specific set of settings.

### The following gif represents the creation of a layer.
![newlayer](https://github.com/IcecreamBurglar/icecreamburglar.github.io/tree/master/images/introducing-wallapparatus/newlayer.gif)
*Creating a new layer by selecting a module*

### This gif showcases the layer properties that can be modified by the user.
![layersettings](https://github.com/IcecreamBurglar/icecreamburglar.github.io/tree/master/images/introducing-wallapparatus/layersettings.gif)
*Modifying layer properties*

Each layer resides on a single display monitor only and won't exceed the bounds of that display.

## Back buffer size factor
Under the "General" tab of the first gif, there is a setting called "Back-buffer size factor". There is an obvious memory limitation when you try to set the backbuffer's size to the entire width/height of multiple combined monitors. So to compensate, we set the backbuffer's size to a fraction of the total screen real-estate and multiply each coordinate input by the user by the same factor.  The fields `GraphicsDeviceManager.PreferredBackBufferWidth` and `GraphicsDeviceManager.PreferredBackBufferHeight` in monogame allow you to set the size of the back buffer.

So, in my initialize overloaded function, I have essentially done
```cs
...

//Set the game window's size and border style.
_form = (Form)Control.FromHandle(Window.Handle);
_form.FormBorderStyle = FormBorderStyle.None;
_form.SetDesktopLocation(0, 0);
_form.Width = SystemInformation.VirtualScreen.Width;
_form.Height = SystemInformation.VirtualScreen.Height;

//Set the back buffer's size using our user-defined factor.
_graphicsManager.PreferredBackBufferWidth = (int)(SystemInformation.VirtualScreen.Width * Settings.Instance.BackBufferWidthFactor);
_graphicsManager.PreferredBackBufferHeight = (int)(SystemInformation.VirtualScreen.Height * Settings.Instance.BackBufferHeightFactor);

...
```

## Drawing each layer
Each layer has a rendertarget whose width/height is determined by the dimensions the user specifies in the settings window. Each layer draws to its own rendertarget, then all rendertargets are drawn to the screen using the backbuffer factor applied to their coordinates.

```cs
//Get the dimensions of the layer, applying the scale factor.
var rect = controller.Settings.Dimensions.GetBoundsRectangle();
rect.X = (int) (rect.X * Settings.Instance.BackBufferWidthFactor);
rect.Y = (int)(rect.Y * Settings.Instance.BackBufferHeightFactor);
rect.Width = (int)(rect.Width * Settings.Instance.BackBufferWidthFactor);
rect.Height = (int)(rect.Height * Settings.Instance.BackBufferHeightFactor);

//Draw the rendertarget. 
_spriteBatch.Draw(controller.RenderTarget, rect, controller.Settings.TintColor);
```

In the future, I'll cache the rectangle for optimization.

## Scripting
Currently, the only scripting environment supported is c# via [Roslyn's scripting API](<<https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples>). I sometimes refer an instance of a script as a *controller*, which makes a *layer* the bundle of both a controller and a set of settings. New scripting environments are made available by implementing the *[Module](<https://github.com/IcecreamBurglar/WallApp/blob/master/WallApp/Scripting/Module.cs>)* class.

Each module contains a manifest.xml file that specifies various things.
Here's an example:
```xml
<root>
  <source>image.csx</source>
  <name>Image</name>
  <description>Displays an image</description>
  <version>1.0</version>
  <customeffects>true</customeffects>
</root>
```

## Placing the game window behind the desktop
There's already an article about this. Check it out here: <https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows-plus>
