The job of the controller is to send commands to the engine over the IPC.
The controller does not run indefinitely, and only exists long enough to send commands to the Engine.

Invoked in the format:
`controller.exe [connection] {command} {arguments}`
Where connection is an optional connection specifier. If not provided, the command will be
provided with the active connection.

COMMANDS:
 - new
   - connection \
     *Adds a new connection*
   - layout \
     *Creates a new layout*
   - layer \
     *Adds a new layer to the current layout*
 - remove
   - connection \
     *Removes a connection*
   - layout \
     *Removes a layout, resetting the state*
   - layer \
     *Removes a layer from the current layout*
 - view
   - layout \
     *Views info about the current layout*
   - layer \
     *Views info about the specified layer*
   - modules \
     *Views info about installed modules*
   - connections \
     *Views info about living connections*
   - activeconnection \
     *Shows which connection is active*
 - set
   - activeconnection \
     *Sets the currently active connection*
   - ipclib \
     *Sets the active IPC library*
 - import
   - data \
     *Allows the app to import ALL app data from a zip*
   - connection \
     *Allows a connection to be imported*
 - export
   - data \
     *Exports ALL app data*
   - connection \
     *Exports the active connection*
   - layout \
     *Saves the layout to the specified file*