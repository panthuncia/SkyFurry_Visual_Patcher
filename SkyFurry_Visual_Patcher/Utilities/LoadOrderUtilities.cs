using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Text;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;

namespace SkyFurry_Visual_Patcher.Utilities {
    public static class LoadOrderUtilities {
        public static (List<String>, List<ISkyrimModGetter>) getPatchableModsFromMaster(this ILoadOrder<IModListing<ISkyrimModGetter>> LoadOrder, String masterName, ISkyrimModGetter master) {
            List<ISkyrimModGetter> modsToPatch = new();
            List<String> modNames = new();
            modsToPatch.Add(master);
            modNames.Add(masterName);
            foreach (ModKey modName in LoadOrder.Keys.ToList()) {
                var mod = LoadOrder[modName].Mod;
                if (mod is not null)
                    foreach (IMasterReferenceGetter currentMaster in mod.MasterReferences) {
                        //list mods that inherit from master and are not synthesis.esp or requiem for the indifferent.esp
                        if (currentMaster.Master.ToString().Equals(masterName) && !(modName.ToString().ToLower().Equals("synthesis.esp") || modName.ToString().ToLower().Equals("requiem for the indifferent.esp"))) {
                            modsToPatch.Add(mod);
                            modNames.Add(modName.ToString());
                        }
                    }
            }
            return (modNames, modsToPatch);
        }
    }
}