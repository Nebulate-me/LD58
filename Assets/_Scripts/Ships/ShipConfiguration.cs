using System;
using System.Collections.Generic;
using _Scripts.Ships.Modules;

namespace _Scripts.Ships
{
    [Serializable]
    public class ShipConfiguration
    {
        public FacingDirection Facing { get; set; }
        public List<ModuleConfig> Modules { get; set; }
    }
}