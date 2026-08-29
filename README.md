Modified Version of Vision`s Vector 1.4.4 Decompilation that feature small tweaks and ported features from Vector 2.

## Current Release: Build 1.0

### Changes
- Added Directional Filters for Models that interact with Areas: `Direction="1"` Facing Right only, `Direction="-1"` facing Left only.
- Added a `ResetOnReload` Flag to Music (In Levels) `<Music Name="Music" ResetOnReload="1">`.
- Added a Track Description to Tracks `<Track Name="DOWNTOWN_STORY_01" Description="Your Text Here">` This shows up before you play the level.
- Ported Impulse Trigger Action from Vector 2: `<Impulse R="float" Model="Player" Impulse="float" Absorption="float">` Produces an impulse effect similar to ForceBlasters. Origin is at the Center of the Trigger. Only works if models are on physics simulation.
- Added a Frames Attribute to Camera Zooms `<Camera Zoom="0.5" Frames="30">`. Default is 30 Frames.
- Added a StopX and StopY Attribute to Camera Actions. Input is Coordinates.
- Added Materials to Platforms `<Platform Material="">`. (Edit `sound_manager.xml` to make material presets)
- Added Voice to Models `<Model Voice="Male">`. (Edit `sound_manager.xml` to make voice presets)
- Music and Sounds are now paused when pausing the game.
- Small UI Tweaks

## Currently Working but Subject to Change

- Voice and Material XML Tree
- Modular Move Parsing (Unfinished at the Moment, I highly advise not using the config.xml as it is going under heavy changes).

## Note About Vectorier

Vectorier may not support the underlying features (e.g added attributes to objects, models) in the editor and may need modification in order to use them.

## DISCLAIMER

Vision and I do not own ANY of the games code, all credit goes to Nekki for making this incredible game.
This should not be redistributed comercially. I do not own any of the assets so neither I nor anyone else has the right to sell them.

My code is in no way good, if there is some way to improve them, do a PR! or let me know of a better way to do something!
