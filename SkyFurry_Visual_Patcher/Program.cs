using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using System.Xml.Linq;
using SkyFurry_Visual_Patcher.Utilities;
using SkyFurry_Visual_Patcher.Settings;

namespace SkyFurry_Visual_Patcher {
    public class Program {
        private static Lazy<Settings.Settings> _settings = null!;
        public static async Task<int> Main(string[] args) {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch, new PatcherPreferences()
                {
                    ExclusionMods = new List<ModKey>()
                    {
                         new ModKey("Synthesis.esp", ModType.Plugin),
                         new ModKey("Requiem for the Indifferent.esp", ModType.Plugin)
                    }
                } )
                .SetAutogeneratedSettings("settings", "settings.json", out _settings)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state) {
            if (_settings.Value.mergeVisuals) {
                patchVisuals(state);
            }
            if (_settings.Value.mergeFlowingFur) {
                patchFlowingFur(state);
            }
            if (_settings.Value.mergeSharpClaws) {
                patchSharpClaws(state);
            }
        }

        private static void patchVisuals(IPatcherState<ISkyrimMod, ISkyrimModGetter> state) {
            //check for SkyFurry
            ISkyrimModGetter? SkyFurry = state.LoadOrder.getModByFileName("SkyFurry.esp");

            if (SkyFurry == null) {
                System.Console.WriteLine("SkyFurry.esp not found");
                return;
            }
            System.Console.WriteLine("Patching base SkyFurry visuals\n");
            //get list of mods which inherit from SkyFurry, as these may contain NPCs we need to patch
            (List<String> modNames, List<ISkyrimModGetter> modsToPatch) = state.LoadOrder.getModsFromMaster("SkyFurry.esp", SkyFurry);
            System.Console.WriteLine("\nForwarding visuals from:");
            foreach (String modName in modNames) {
                System.Console.WriteLine(modName);
            }
            int ModIndex = -1;
            foreach (ISkyrimModGetter mod in modsToPatch) {
                int processed = 0;
                int ignored = 0;
                int total = mod.Npcs.Count;
                List<FormKey> modFormIDs = mod.Npcs.Select(x => x.FormKey).ToList();
                List<INpcGetter> winningOverrides = state.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>().Where(x => modFormIDs.Contains(x.FormKey)).ToList();
                ModIndex++;
                System.Console.WriteLine("\nImporting visuals from: " + modNames[ModIndex]);
                if (mod.Npcs.Count > 0) {
                    foreach (INpcGetter npc in mod.Npcs) {
                        if (processed % 10 == 0) {
                            System.Console.WriteLine(processed + "/" + total + " Npcs");
                        }
                        INpcGetter winningOverride = winningOverrides.Where(x => x.FormKey == npc.FormKey).First();
                        Npc patchNpc = state.PatchMod.Npcs.GetOrAddAsOverride(winningOverride);

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
                        if (_settings.Value.ignoreIdenticalToWinningOverride && patchNpc.Equals(winningOverride)) {
                            state.PatchMod.Npcs.Remove(npc);
                            ignored++;
                        }

                        processed++;
                    }
                }
                else {
                    System.Console.WriteLine("No NPCs found");
                }
                if (ignored > 0) {
                    System.Console.WriteLine("Ignoring " + ignored + " unchanged NPCs");
                }
            }
        }

        private static void patchFlowingFur(IPatcherState<ISkyrimMod, ISkyrimModGetter> state) {
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
                (List<String> modNames, List<ISkyrimModGetter> modsToPatch) = state.LoadOrder.getModsFromMaster("SkyFurry_FlowingFur.esp", FlowingFur);
                int ModIndex = -1;
                foreach (ISkyrimModGetter mod in modsToPatch) {
                    int processed = 0;
                    int ignored = 0;
                    int total = mod.Outfits.Count;
                    List<FormKey> modFormIDs = mod.Outfits.Select(x => x.FormKey).ToList();
                    List<IOutfitGetter> winningOverrides = state.LoadOrder.PriorityOrder.WinningOverrides<IOutfitGetter>().Where(x => modFormIDs.Contains(x.FormKey)).ToList();
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
                                //Entries in outfit item lists are IFormLinkGetter, which need to be resolved with TryResolve(state.LinkCache) instead of being accessed directly.
                                foreach (IFormLinkGetter<IOutfitTargetGetter> itemForm in outfit.Items) {
                                    IOutfitTargetGetter? item = itemForm.TryResolve(state.LinkCache);
                                    if (item is not null && item.EditorID is not null && furTypes.Contains(item.EditorID)) {
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
                                //remove duplicate furs
                                foreach (IOutfitTargetGetter fur in fursToRemove) {
                                    if (patchOutfit.Items is not null) {
                                        patchOutfit.Items.Remove(fur);
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
        private static void patchSharpClaws(IPatcherState<ISkyrimMod, ISkyrimModGetter> state) {
            ISkyrimModGetter? sharpClaws = state.LoadOrder.getModByFileName("SkyFurry_SharpClaws.esp");
            System.Console.WriteLine("\nChecking for sharp claws...");
            if (sharpClaws != null) {
                System.Console.WriteLine("Found!");
                List<FormKey> modFormIDs = sharpClaws.Races.Select(x => x.FormKey).ToList();
                List<IRaceGetter> winningOverrides = state.LoadOrder.PriorityOrder.WinningOverrides<IRaceGetter>().Where(x => modFormIDs.Contains(x.FormKey)).ToList();
                //list sharpclaws spells
                List<String> sharpClaws_Spells = new();
                foreach (ISpellGetter spell in sharpClaws.Spells) {
                    if (spell.EditorID is not null) {
                        sharpClaws_Spells.Add(spell.EditorID);
                        System.Console.WriteLine("SharpClaws spell: "+spell.EditorID);
                    }
                }
                //patch races
                int processed = 0;
                int total = sharpClaws.Races.Count;
                foreach (IRaceGetter race in sharpClaws.Races) {
                    if (processed % 10 == 0) {
                        System.Console.WriteLine(processed+"/"+total+" Races");
                    }
                    IRaceGetter winningOverride = winningOverrides.Where(x => x.FormKey == race.FormKey).First();
                    Race patchRace = state.PatchMod.Races.GetOrAddAsOverride(winningOverride);

                    //patch impact data
                    patchRace.ImpactDataSet.SetTo(race.ImpactDataSet);

                    //patch actor effects
                    //list winning override actor effects
                    List<String> winningOverrideActorEffects = new();
                    if (winningOverride.ActorEffect is not null) {
                        //another IFormLinkGetter
                        foreach (IFormLinkGetter<ISpellRecordGetter> effectForm in winningOverride.ActorEffect) {
                            ISpellRecordGetter? effect = effectForm.TryResolve(state.LinkCache);
                            if (effect is not null && effect.EditorID is not null) {
                                winningOverrideActorEffects.Add(effect.EditorID);
                            }
                        }
                    }
                    //check every race effect record in the current mod, and if it is in SkyFurry_SharpClaws.esp but is not in the winning override, add it to the patch
                    if (race.ActorEffect is not null) {
                        foreach (IFormLinkGetter<ISpellRecordGetter> effectForm in race.ActorEffect) {
                            ISpellRecordGetter? effect = effectForm.TryResolve(state.LinkCache);
                            if (effect is not null && effect.EditorID is not null && sharpClaws_Spells.Contains(effect.EditorID) && !winningOverrideActorEffects.Contains(effect.EditorID)) {
                                if (patchRace.ActorEffect is null) {
                                    patchRace.ActorEffect = new ExtendedList<IFormLinkGetter<ISpellRecordGetter>>();
                                }
                                patchRace.ActorEffect.Add(effectForm);
                            }
                        }
                    }
                    processed++;
                }
            }
        }
    }
}

