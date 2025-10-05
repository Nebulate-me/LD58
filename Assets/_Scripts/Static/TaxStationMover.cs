using UnityEngine;

namespace _Scripts.Static
{
    [RequireComponent(typeof(TaxStationBuilding))]
    public class TaxStationMover : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 1.2f; // units per second
        [SerializeField] private bool randomizeVerticalOffset = true;
        [SerializeField] private float verticalOffsetRange = 2f;

        private Vector3 _direction = Vector3.left;
        private bool _initialized;

        private void Start()
        {
            if (randomizeVerticalOffset)
            {
                transform.position += new Vector3(0, Random.Range(-verticalOffsetRange, verticalOffsetRange), 0);
            }

            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            transform.Translate(_direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}