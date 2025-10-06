using UnityEngine;

namespace _Scripts.Ships.ShipControllers
{
    [RequireComponent(typeof(TrainController))]
    public class PlayerShipController : MonoBehaviour
    {
        private TrainController train;

        void Awake()
        {
            train = GetComponent<TrainController>();
            train.enabled = true;
        }

        void Start()
        {
            train.gameObject.name = "PlayerTrain";
            train.IsPlayerControlled = true;
        }
    }
}