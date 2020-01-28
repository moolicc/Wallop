# Wallop
Wallop is a wallpaper engine/runtime based on the idea of "layouts" (commonly referred to as "scenes") being composed of multiple, scripted layers.

Wallop is crossplatform, though I'll only personally be trying it on linux and Windows 10.

## Current State
Wallop is undergoing **huge** refactoring.

To understand, here's a rough timeline. (It's very rough, though the years should be correct. Going on memory here, folks.)
 * June 2016
   * I being work on *Wall Apparatus*, using a single project and Windows Forms.
 * January 2017
   * Proof of concept complete - It really worked!
   * Begin first refactoring - I threw everything out except the rendering.
   * Create first modules. *Image*, *Youtube* - which did exactly what they sound like; one displayed an image, and the other streamed a youtube video.
 * July 2017
   * First refactoring complete. Code is much cleaner now, and it still works.
   * First git commit.
 * December 2017
   * Made the UI functional and responsive.
   * Implemented necessary features (layout editing through the UI mostly, if memory serves.).
 * January 2018
   * Added the first Effect, *Black and White*.
   * Added the Gif layer, to support animated gifs.
 * February 2019
   * UI conversion to WPF finished.
 * May 2019
   * UI rewrite complete.
   * Live updating of layouts.
 * June 2019
   * UI tweaking.
   * Layout editing previewing.
 * October 2019
   * The Great Refactoring started. Separating the app into three separate projects, the UI, Bridge, and Engine.
   * The UI now uses Mahapps.MetroUI for prettiness.
* January 2020
  * Added Cake build system.
  * Without even having finished the Great Refactoring, the second large refactoring task has been planned. This one with the goal of further modularizing the app to make it better (tm).
  * Renamed to Wallop.
  

So, the point of all that is to explain that right now I'm in the process of performing the extremely large task of refactoring and reorganizing the entire project's structure.

We're going from 3 projects (wasn't done with the refactor from 1 to 3 projects, either), to 7. This will improve productivity in the long-run, make cross-platform easier, and make testing easier.
   

## Roadmap
In the future, I'd like to finish the second Great Refactoring.
It'd also be nice to make the app more extensible via Runtime and Client plugins.
We're gonna need a bunch of modules. I would like to make a bunch of modules for user consumption.
Help the app grow from user-feedback.


## Building
WallApp is built on .net Core 3.0 and from with a Cake build script.
Invoke `./build.ps1` to build the project.

> Note that we use MonoGame 3.5 because 3.7+ had strange sharpdx problems I encountered that I'll look into and report accordingly in the future