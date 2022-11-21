# MHotkeyCommands

Unturned plugin for binding commands/messages to various keys/ingame actions.

Main command: `/hotkey`
Syntax: `/Hotkey \<delete> \<key> | \<add/set> \<key> \<command or msg> | \<list> \<keys/bound> (key)`

Each key supports multiple commands/messages to be run at once. Max number of actions per bind is configurable. There is a delay between actions due to unturned limitations. 

Saves binds between sessions for players with the `Binds.Save` permission. Players without the permission will have to rebind their keys every time the server reboots.

Plugin Hotkeys 1-5 are assigned in your controls menu under "Mods/Plugins"

Key names are dynamic components are case sensitive. `facepalm` will not work, you must use `Facepalm`!

### Available Keys/actions
- Jump
- Crouch
- Prone
- Sprint
- LeanLeft
- LeanRight
- PluginKey1
- PluginKey2
- PluginKey3
- PluginKey4
- PluginKey5
- SteadyAim
- InventoryOpen
- InventoryClose
- Pickup
- PunchLeft
- PunchRight
- SurrenderStart
- SurrenderStop
- Point
- Wave
- Salute
- Arrest_Start
- Arrest_Stop
- Rest_Start
- Rest_Stop
- Facepalm

### Dynamic Components
Commands/messages support adding dynamic components that will change depending on the circumstances of when the action is ran. Place the component identifier (is a short string surrounded by { }) when you use `/hotkey add <key>`

Each component has its own permission, or you can give all of them with `Binds.Dynamic.*`
Add one of the following behind `Binds.Dynamic.` to give permissions for just that component: `GestureName`, `Bearing`, `Caller`, `Target`, `Chatmode`

- GestureName
	- {G} | the name of the gesture/action that called this
- Bearing
	- {B} | Your compass Bearing
- Caller
	- {C.ID} | Your id
	- {C.Pos} | Your position
	- {C.X} | Your x coordinate
	- {C.Y} | Your Y coordinate
	- {C.Z} | Your Z coordinate
	- {C.Name} | Your displayname
- Target
	- {T.Name} | The name of the player you are looking at 
	- {T.ID} | The id of the player you are looking at 
- Chatmode 
	- {AREA} | Sets the chat mode to area
	- {GROUP} | Sets the chat mode to group

### Examples
- Kill the player you are looking at and tell them to get lost when you facepalm at them
	- /hotkey set Facepalm Get lost {T.Name}
	- /hotkey add Facepalm /kill {T.ID}
- Tell your bearing to your group when you point
	- /hotkey set Point {GROUP}There is a guy at {B}
- Use a kit when you wave
	- /hotkey set Wave /kit eaglefire

### Known Issues
- Pickup gesture doesn't work
- All chat messages are global by default. Will fix as soon as I can figure out how to. 