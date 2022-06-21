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
* December 2021
  * Wallop Redux Started with an actual, concrete plan in mind.
  * Hopeully will get a working implementation by the end of the upcoming Spring 2022 Semester at the University.


## Roadmap
* Get the Engine component complete (that's what gets stuff on the screen) (IN PROGRESS)
* Get the Composition component complete (that's what talks to the engine)
* Write an array of useful plugins and modules
* Get the builder component complete (that's the UI that talks to the compositor)

## Building
Just build it from within visual studio - no fancy cake scripts anymore (the cake was thus indeed a lie).

