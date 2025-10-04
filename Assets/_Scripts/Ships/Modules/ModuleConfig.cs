using Sirenix.OdinInspector;
using UnityEngine;

namespace _Scripts.Ships.Modules
{
    [CreateAssetMenu(menuName = "LD58/Create ModuleConfig", fileName = "ModuleConfig", order = 0)]
    public class ModuleConfig : ScriptableObject
    {
        [SerializeField] private ModuleType moduleType = ModuleType.None;
        [SerializeField, ShowIf(nameof(IsLocomotive))] private LocomotiveType locomotiveType = LocomotiveType.None;
        [SerializeField, ShowIf(nameof(IsCargo))] private CargoType cargoType = CargoType.None;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int maxModuleHealth = 3;
        [SerializeField] private int score = 100;
        
        public bool IsLocomotive => moduleType == ModuleType.Locomotive;
        public bool IsCargo => moduleType == ModuleType.Cargo;
        
        public ModuleType ModuleType => moduleType;
        public LocomotiveType LocomotiveType => IsLocomotive 
            ? locomotiveType 
            : LocomotiveType.None;
        public CargoType CargoType => IsCargo
            ? cargoType 
            : CargoType.None;
        public GameObject Prefab => prefab;
        public int MaxModuleHealth => maxModuleHealth;
        public int Score => score;
    }
}