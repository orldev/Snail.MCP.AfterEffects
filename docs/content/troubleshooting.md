# Troubleshooting

Start with `ae_diagnose`: it reports whether After Effects answers, whether the installed bridge is the build this server ships, the port
and token the bridge uses, the announcements folder, the project folder, and which client and scopes the call came with.

## "After Effects is not reachable"

- After Effects is not running, or the bridge is not installed: `ae_install_bridge` installs it and starts it in the running After Effects.
- *Allow Scripts to Write Files and Access Network* is off. The bridge cannot open a socket without it; switch it on in Preferences →
  Scripting & Expressions and run `ae_install_bridge` again.
- After Effects shows a modal dialog. Scripts do not run while one is open; close it. After a forced restart a recovery or crash dialog
  can sit behind other windows.
- The bridge log is at `~/.s3l-mcp-aftereffects/state/bridge.log`, the loader's at `loader.log` beside it.

## "The bridge is older than this server" or "differs"

The files in After Effects' Startup folder are not the ones this server bundles; `ae_install_bridge` replaces them and restarts the
bridge.

## "After Effects did not answer within N s" (`Timeout`)

After Effects was busy: a render in place, a heavy import, a dialog. Finish it; the next call continues. Pass a longer `timeoutSeconds`
where the tool takes one, and move long renders to the farm, which does not hold After Effects.

## Undo does nothing, or a tool hangs after undo

Undo and redo are run as a script handed to After Effects from outside the bridge, because `executeCommand` inside the bridge's tick
wedges it. `ae_undo` answers at once and says whether the request was accepted; `ae_project_info` shows the result.

## Names in another language

After Effects' interface language decides display names of properties, effects and menu commands. Paths take match names
(`ADBE Position`), which never change; `ae_property_tree` and `ae_effects_catalog` give both.

## The farm says aerender was not found

`aerender` lives beside After Effects: `/Applications/Adobe After Effects <year>/aerender` on macOS, `Support Files\aerender.exe` on
Windows. The farm looks there for the newest version; `ae_render` renders inside After Effects instead.
