using _Scripts.Ships.Modules;
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
            SetPlayerControl(true);
        }

        public void SetPlayerControl(bool isPlayerEnabled)
        {
            train.GetType()
                .GetField("isPlayerControlled", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(train, isPlayerEnabled);
        }
    }
}