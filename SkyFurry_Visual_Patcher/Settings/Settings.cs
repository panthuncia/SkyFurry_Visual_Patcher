using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyFurry_Visual_Patcher.Settings {
    class Settings {
        public bool ignoreIdenticalToWinningOverride { get; set; } = true;
        public bool mergeVisuals { get; set; } = true;
        public bool mergeFlowingFur { get; set; } = true;
        public bool mergeSharpClaws { get; set; } = true;
    }
}
