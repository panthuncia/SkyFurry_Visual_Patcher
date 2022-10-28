using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using System.Xml.Linq;

namespace SkyFurry_Visual_Patcher {
    public class Program {
        public static async Task<int> Main(string[] args) {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state) {
            //check for SkyFurry

            ISkyrimModGetter? SkyFurry = state.LoadOrder[state.LoadOrder.Keys.Where(x => ((string)x.FileName).ToLower() == "SkyFurry.esp".ToLower()).ToList().First()].Mod;

            if (SkyFurry == null) {
                System.Console.WriteLine("SkyFurry.esp not found");
                return;
            }

            //get list of mods which inherit from SkyFurry, as these may contain NPCs we need to patch
            List<ISkyrimModGetter> modsToPatch = new();
            List<String> modNames = new();
            modsToPatch.Add(SkyFurry);
            modNames.Add("SkyFurry.esp");
            foreach (ModKey modName in state.LoadOrder.Keys.ToList()) {
                var mod = state.LoadOrder[modName].Mod;
                if (mod is not null)
                    foreach (IMasterReferenceGetter master in mod.MasterReferences) {
                        if (master.Master.ToString().Equals("SkyFurry.esp")) {
                            modsToPatch.Add(mod);
                            modNames.Add(modName.ToString());
                        }
                    }
            }
            System.Console.WriteLine("\nForwarding visuals from:");
            foreach (String modName in modNames) {
                System.Console.WriteLine(modName);
            }
            int ModIndex = -1;
            foreach (ISkyrimModGetter mod in modsToPatch) {
                int processed = 0;
                int ignored = 0;
                int total = mod.Npcs.Count;
                var modFormIDs = mod.Npcs.Select(x => x.FormKey).ToList();
                var winningOverrides = state.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>().Where(x => modFormIDs.Contains(x.FormKey)).ToList();
                ModIndex++;
                //sometimes, it tries to run on itself for some reason
                if (!modNames[ModIndex].ToLower().Equals("synthesis.esp")) {
                    System.Console.WriteLine("\nImporting visuals from: " + modNames[ModIndex]);
                    if (mod.Npcs.Count > 0) {
                        foreach (INpcGetter npc in mod.Npcs) {
                            if (processed % 10 == 0) {
                                System.Console.WriteLine(processed + "/" + total + " Npcs");
                            }
                            var winningOverride = winningOverrides.Where(x => x.FormKey == npc.FormKey).First();
                            var patchNpc = state.PatchMod.Npcs.GetOrAddAsOverride(winningOverride);

                            //copy texture lighting
                            patchNpc.TextureLighting = npc.TextureLighting;

                            //copy tint layers
                            patchNpc.TintLayers.Clear();
                            foreach (ITintLayerGetter tint in npc.TintLayers) {
                                patchNpc.TintLayers.Add(tint.DeepCopy());
                            }

                            //copy head parts
                            patchNpc.HeadParts.SetTo(npc.HeadParts);

                            //copy head textures
                            patchNpc.HeadTexture.SetTo(npc.HeadTexture);

                            //copy face morph
                            if (npc.FaceMorph != null) {
                                patchNpc.FaceMorph = npc.FaceMorph.DeepCopy();
                            }

                            //copy hair color
                            patchNpc.HairColor.SetTo(npc.HairColor);

                            //copy race
                            patchNpc.Race.SetTo(npc.Race);
                            if (patchNpc.Equals(npc)) {
                                state.PatchMod.Npcs.Remove(npc);
                                ignored++;
                            }
                            processed++;
                        }
                    }
                    else {
                        System.Console.WriteLine("No NPCs found");
                    }
                    System.Console.WriteLine("Ignoring " + ignored + " unchanged NPCs");
                }
            }
        }
    }
}
