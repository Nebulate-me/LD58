namespace _Scripts.Ships.Modules
{
    public interface IModuleRegistry
    {
        bool TryGetModuleConfig(ModuleType moduleType, out ModuleConfig module);
        bool TryGetLocomotiveModuleConfig(LocomotiveType locomotiveType, out ModuleConfig module);
        bool TryGetCargoModuleConfig(CargoType cargoType, out ModuleConfig module);
    }
}