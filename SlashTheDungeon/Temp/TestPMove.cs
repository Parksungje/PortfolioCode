using UnityEngine;

namespace SJ._01.Code.Temp
{
    public class TestPMove : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;

        void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 moveDir = new Vector3(h, v, 0).normalized;
            transform.position += moveDir * (moveSpeed * Time.deltaTime);
        }
    }
}