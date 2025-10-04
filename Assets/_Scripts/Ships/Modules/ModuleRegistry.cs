using System.Collections.Generic;
using _Scripts.Utils;
using UnityEngine;

namespace _Scripts.Ships.Modules
{
    public class ModuleRegistry : MonoBehaviour, IModuleRegistry
    {
        [SerializeField] private List<ModuleConfig> modules = new List<ModuleConfig>();
        
        public bool TryGetModuleConfig(ModuleType moduleType, out ModuleConfig module)
        {
            return modules.TryGetFirst(module => module.ModuleType == moduleType, out module);
        }

        public bool TryGetLocomotiveModuleConfig(LocomotiveType locomotiveType, out ModuleConfig module)
        {
            return modules.TryGetFirst(module => module.IsLocomotive && module.LocomotiveType == locomotiveType, out module);
        }

        public bool TryGetCargoModuleConfig(CargoType cargoType, out ModuleConfig module)
        {
            return modules.TryGetFirst(module => module.IsCargo && module.CargoType == cargoType, out module);
        }
    }

    public interface IModuleRegistry
    {
        bool TryGetModuleConfig(ModuleType moduleType, out ModuleConfig module);
        bool TryGetLocomotiveModuleConfig(LocomotiveType locomotiveType, out ModuleConfig module);
        bool TryGetCargoModuleConfig(CargoType cargoType, out ModuleConfig module);
    }
}