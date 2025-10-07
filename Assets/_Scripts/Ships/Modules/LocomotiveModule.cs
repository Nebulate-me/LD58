using UnityEngine;
using Utilities.Prefabs;

namespace _Scripts.Ships.Modules
{
    [RequireComponent(typeof(ShipModule))]
    public class LocomotiveModule : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShipModule shipModule;
        
        private void Awake() => shipModule = GetComponent<ShipModule>();
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (shipModule.Ship != null && 
                other.TryGetComponent(out ShipModule module) &&
                module.Ship == null)
            {
                module.AttachToShip(shipModule.Ship);
                Debug.Log($"{name} picked up loose module {module.name}");
            }
        }
    }
}