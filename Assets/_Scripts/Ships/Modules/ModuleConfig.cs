using Sirenix.OdinInspector;
using UnityEngine;

namespace _Scripts.Ships.Modules
{
    [CreateAssetMenu(menuName = "LD58/Create ModuleConfig", fileName = "ModuleConfig", order = 0)]
    public class ModuleConfig : ScriptableObject
    {
        [SerializeField] private ModuleType moduleType = ModuleType.None;
        [SerializeField, ShowIf(nameof(IsCargo))] private CargoType cargoType = CargoType.None;
        [SerializeField] private int maxModuleHealth = 3;
        [SerializeField] private int score = 100;
        
        public bool IsCargo => moduleType == ModuleType.Cargo;
        
        public ModuleType ModuleType => moduleType;
        public CargoType CargoType => moduleType == ModuleType.Cargo 
            ? cargoType 
            : CargoType.None;
        public int MaxModuleHealth => maxModuleHealth;
        public int Score => score;
    }
}