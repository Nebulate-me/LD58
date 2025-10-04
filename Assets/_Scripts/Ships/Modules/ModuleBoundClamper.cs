using UnityEngine;

namespace _Scripts.Ships.Modules
{
    [RequireComponent(typeof(Collider2D))]
    public class ModuleBoundClamper : MonoBehaviour
    {
        private Camera cam;
        [SerializeField] private Collider2D moduleCollider;

        void Awake()
        {
            cam = Camera.main;
            if (moduleCollider == null)
                moduleCollider = GetComponent<Collider2D>();
        }

        void Update()
        {
            ClampToScreen();
        }

        private void ClampToScreen()
        {
            Bounds bounds = moduleCollider.bounds;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Vector3 pos = transform.position;

            pos.x = Mathf.Clamp(pos.x, 
                cam.transform.position.x - halfWidth + bounds.extents.x / 2f, 
                cam.transform.position.x + halfWidth - bounds.extents.x / 2f);

            pos.y = Mathf.Clamp(pos.y, 
                cam.transform.position.y - halfHeight + bounds.extents.y / 2f, 
                cam.transform.position.y + halfHeight - bounds.extents.y / 2f);

            transform.position = pos;
        }
    }
}