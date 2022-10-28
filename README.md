SkyFurry_Visual_Patcher runs through Synthesis and uses Mutagen to identify NPCs that have been visually modified by SkyFurry.esp and/or any mod which inherits from SkyFurry.esp, and forwards those visual changes to Synthesis.esp. The rest of the NPCs data is taken from the winning override for that NPC.

What this does: It removes the need for tedious manual patches when SkyFurry collides with another mod that edits large numbers of non-visual NPC records (stats, items, ai packages, etc.) This was originally made to rectify conflicts with Requiem, Requiem Minor Arcana, etc, but should work for other mods if you want to try it

What this doesen't do: This does not generate *new* SkyFurry visuals for NPCs, meaning is not a replacement for the SkyFurry XEdit patcher which generates visuals for NPCs added by mods. That should still be run on mods that add NPCs.

Do you need this?: If you have mods which are conflicting with large numbers of SkyFurry's NPC records, and hand-made patches that incorporate SkyFurry visuals with those mods either don't exist or are incomplete, this can save you many hours of patching. In my case, this was the combination of SkyFurry, AI Overhaul, Requiem, and several other mods. 

How to use: This patcher allows for more load-order flexibility. As long as you put the output of this patch after everything which modifies NPC records, SkyFurry.esp and SkyFurry patches containing NPC records can be placed *before* other mods that edit NPCs. The winning overrides will have their visuals erased and replaced with SkyFurry visuals.

Notes: This patcher always prioritizes visuals from the latest override that inherits from SkyFurry, so if you have patches which intentionally overwrite SkyFurry visual data, they will be included in the generated patch, as long as no more than one edits the same NPC, as this script does not attempt to merge any visual changes.

This is very much beta code, and may be expanded upon in the future.