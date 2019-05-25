# WallApp

WallApp is an open-source live-wallpaper engine for Windows 10.
Using WallApp, you create multiple background layers that are controlled via external scripts.

WallApp uses MonoGame as the driving force behind rendering and the familiar update/draw loop.

## Current State
Technically, WallApp is in a usable state. However, there's little-to-no graceful error handling in place. This means that if you enter invalid settings, it's all going to come tumbling down.

I'm working towards applying the necessary polish to WallApp to make me feel comfortable enough to consider a beta-level release.

## Roadmap
TODO: Explain plans

## Building
WallApp is built on .net Framework 4.7.2 and from within Visual Studio 2019
Additionally, there are various dependencies taken from nuget:
 * MonoGame 3.5 (3.7+ had strange sharpdx problems I encountered that I'll look into and report accordingly in the future)
 * Newtonsoft.Json
 * Roslyn's CSharp Scripting (Microsoft.CodeAnalysis.CSharp.Scripting, 3.0)
 * MahApps.Metro 1.6.5
 * WPF
 * Winforms + System.Drawing
 
## Docs
TODO: Further explain how it internally works.

## Showcase/Examples
TODO: Take the time to make some, then put them here.