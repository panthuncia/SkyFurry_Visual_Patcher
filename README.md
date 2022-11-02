SkyFurry_Visual_Patcher runs through Synthesis and uses Mutagen to identify records that have been modified by SkyFurry's main files, and/or any mod which inherits from SkyFurry's main files, and forwards those changes to Synthesis.esp. The rest of the record's data is taken from the winning override for that record. A focus is placed on compatibility- Only records that are required for visual consistency are hard overwritten, while less essential SkyFurry edits will automatically defer to other mods.

# What this does: 
This mod has 5 modules, each of which performs a different function in forwarding SkyFurry's changes into the winning override. This is done to significantly reduce the size and number of SkyFurry patches you need to make for your load order. The modules are described below.

# 1- Patching NPC visuals.
This removes the need for tedious manual patches when SkyFurry collides with another mod that edits large numbers of non-visual NPC records (stats, items, ai packages, etc.). All visuals are hard overwritten- for each NPC, the patch will contain NPC data from the esp lowest in the load order that contains a record of that NPC and inherits from SkyFurry.esp.

This module allows mods containing NPC edits to be placed after SkyFurry in the load order.

# 2- Patching race data. 
This patches race visuals, keywords, flags, skills, abilities, tints, etc. that SkyFurry changes in race records. Some records are merged (actor effects, keywords), some are soft overwritten (overwrites vanilla values, but won't override other mods that are loaded after SkyFurry.esp), and some are hard overwritten (visual information, flags, skeleton). Again, all race data is taken from the esp lowest in the load order that contains a record of that NPC and inherits from SkyFurry.esp.

This module allows mods containing race edits to be placed after SkyFurry in the load order.

# 3- Patching FlowingFur.
This does exactly what it says- it forwards any FlowingFur added to outfits by SkyFurry_FlowingFur.esp into the winning override.

This module allows mods containing outfit edits to be placed after SkyFurry_FlowingFur.esp in the load order.

# 4- Patching SharpClaws.
This forwards all changes made by SkyFurry_SharpClaws.esp into the winning override. Optionally, it can also scale the unarmed damage to match the damage multiplier calculated from the winning override. For example, if base unarmed damage is 4, SharpClaws changes that to 7, and another mod later sets it to 5, this module can be configured to calculate and set the resulting unarmed damage to 8.75, matching the scaling indicated by the mod that set it to 5.

This module allows mods containing race edits to be placed after SkyFurry_SharpClaws.esp in the load order.

# 5- Patching DigiBoots
This forwards changes made by SF_Digiboots.esp into the winning override. This is mostly model and equip slot info, with a few object bounds changes.

This module allows mods containing boots edits to be placed after SF_Digiboots.esp in the load order.


# What this doesen't do: 
This does not generate *new* SkyFurry visuals for NPCs, meaning is not a replacement for the SkyFurry XEdit patcher which generates visuals for NPCs added by mods. That should still be run on mods that add NPCs.

# Do you need this?: 
If you have mods which are conflicting with large numbers of SkyFurry's records, and hand-made patches for SkyFurry and those mods either don't exist or are incomplete (or there just aren't patches for your specific combo of multiple mods), this can save you many, many hours of patching. In my case, this was the combination of SkyFurry, AI Overhaul, Requiem, and several other mods. 

# How to use: 
This patcher allows for more load-order flexibility. As long as you put the output of this patch after everything which modifies relevant records (Except for specific cases that need to be loaded even later such as "Requiem for the Indifferent.esp", which will incorporate this patch's changes anyway), SkyFurry's base files and SkyFurry patches containing NPC, race, outfit, and boots records can and should be placed *before* other mods that edit those same records (the SkyFurry installation instructions will say otherwise- this patcher aims to remove that requirement). The winning overrides will have the necessary information either merged, or erased and replaced with SkyFurry's values.

# Notes:
This patcher always prioritizes data from the latest override that inherits from SkyFurry.esp (Or SkyFurry_FlowingFur.esp, SkyFurry_SharpClaws.esp, or SF_Digiboots.esp, each for their respective module), so if you have patches which intentionally overwrite SkyFurry data and also have the correct masters set, the data will be included according to your load order.

This is very much beta code, and may be expanded upon in the future.