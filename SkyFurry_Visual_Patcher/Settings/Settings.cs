using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyFurry_Visual_Patcher.Settings {
    class Settings {
        public bool ignoreIdenticalToWinningOverride { get; set; } = false;
        public bool patchNpcVisuals { get; set; } = true;
        public bool mergeArmors { get; set; } = true;
        public bool mergeRaces { get; set; } = true;
        public bool mergeFlowingFur { get; set; } = true;
        public bool mergeSharpClaws { get; set; } = true;
        public bool scaleUnarmedDamageWithWinningOverride { get; set; } = false;
        public String SkyFurryBaseModName { get; set; } = "YiffyAgeConsolidated.esp";
        public String FlowingFurModName { get; set; } = "SkyFurry_FlowingFur.esp";
        public String SharpClawsModName { get; set; } = "SkyFurry_SharpClaws.esp";
        public String DigibootsModName { get; set; } = "SF_Digiboots.esp";

    }
}
