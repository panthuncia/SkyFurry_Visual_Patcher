using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using System.Xml.Linq;
using SkyFurry_Visual_Patcher.Utilities;

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
            System.Console.WriteLine("Patching base SkyFurry visuals\n");
            //get list of mods which inherit from SkyFurry, as these may contain NPCs we need to patch
            List<ISkyrimModGetter> modsToPatch = new();
            List<String> modNames = new();
            modsToPatch.Add(SkyFurry);
            modNames.Add("SkyFurry.esp");
            foreach (ModKey modName in state.LoadOrder.Keys.ToList()) {
                var mod = state.LoadOrder[modName].Mod;
                if (mod is not null)
                    foreach (IMasterReferenceGetter master in mod.MasterReferences) {
                        //list mods that inherit from SkyFurry and are not synthesis.esp
                        if (master.Master.ToString().Equals("SkyFurry.esp") && !(modName.ToString().ToLower().Equals("synthesis.esp") || modName.ToString().ToLower().Equals("requiem for the indifferent.esp"))) {
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

                        //copy face parts
                        if (npc.FaceParts is not null) {
                            patchNpc.FaceParts = npc.FaceParts.DeepCopy();
                        }

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

                        //copy weight
                        patchNpc.Weight = npc.Weight;

                        //copy height
                        patchNpc.Height = npc.Height;


                        //remove unchanged NPCs
                        if (patchNpc.Equals(winningOverride)) {
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
            //Now check for flowing fur addon and forward changes from it and anything that inherits from it
            ISkyrimModGetter? FlowingFur = state.LoadOrder[state.LoadOrder.Keys.Where(x => ((string)x.FileName).ToLower() == "SkyFurry_FlowingFur.esp".ToLower()).ToList().First()].Mod;
            System.Console.WriteLine("\nChecking for flowing fur...");
            if (FlowingFur != null) {
                System.Console.WriteLine("Found!");
                //get a list of furs added by SkyFurry_FlowingFur.esp so that we can detect them in NPCs
                List<String> furTypes = new();
                foreach (IArmorGetter furType in FlowingFur.Armors) {
                    if (furType.EditorID is not null) {
                        furTypes.Add(furType.EditorID.ToString());
                    }
                }
                (modNames, modsToPatch) = state.LoadOrder.getPatchableModsFromMaster("SkyFurry_FlowingFur.esp", FlowingFur);
                ModIndex = -1;
                foreach (ISkyrimModGetter mod in modsToPatch) {
                    int processed = 0;
                    int ignored = 0;
                    int total = mod.Outfits.Count;
                    var modFormIDs = mod.Outfits.Select(x => x.FormKey).ToList();
                    var winningOverrides = state.LoadOrder.PriorityOrder.WinningOverrides<IOutfitGetter>().Where(x => modFormIDs.Contains(x.FormKey)).ToList();
                    ModIndex++;
                    System.Console.WriteLine("\nImporting furs from: " + modNames[ModIndex]);
                    if (mod.Outfits.Count > 0) {
                        foreach (IOutfitGetter outfit in mod.Outfits) {
                            if (processed % 10 == 0) {
                                System.Console.WriteLine(processed + "/" + total + " outfits");
                            }
                            IOutfitGetter winningOverride = winningOverrides.Where(x => x.FormKey == outfit.FormKey).First();
                            Outfit patchOutfit = state.PatchMod.Outfits.GetOrAddAsOverride(winningOverride);
                            if (outfit.Items is not null) {
                                List<IOutfitTargetGetter> fursToRemove = new();
                                foreach (IFormLinkGetter<IOutfitTargetGetter> itemForm in outfit.Items) {
                                    IOutfitTargetGetter? item = itemForm.TryResolve(state.LinkCache);
                                    if (item is not null && item.EditorID is not null && furTypes.Contains(item.EditorID)){
                                        //Apparently the item list can be null, so if we need to add fur we need to check for this
                                        if (patchOutfit.Items is null) {
                                            patchOutfit.Items = new ExtendedList<IFormLinkGetter<IOutfitTargetGetter>>();
                                        }
                                        //mark any existing furs for deletion. We can't delete them here because we can't iterate on an ExtendedList and modify it at the same time
                                        foreach (IFormLinkGetter<IOutfitTargetGetter> potentialFurForm in patchOutfit.Items) {
                                            IOutfitTargetGetter? potentialFurItem = potentialFurForm.TryResolve(state.LinkCache);
                                            if (potentialFurItem is not null && potentialFurItem.EditorID is not null && furTypes.Contains(potentialFurItem.EditorID)) {
                                                fursToRemove.Add(potentialFurItem);
                                            }
                                        }
                                        patchOutfit.Items.Add(itemForm);
                                    }
                                }
                            }
                            //remove unchanged outfits
                            if (patchOutfit.Equals(winningOverride)) {
                                state.PatchMod.Outfits.Remove(outfit);
                                ignored++;
                            }

                            processed++;
                        }
                    }
                    else {
                        System.Console.WriteLine("No NPCs found");
                    }
                    System.Console.WriteLine("Ignoring " + ignored + " unchanged Outfits");
                }
            }
        }
    }
}
