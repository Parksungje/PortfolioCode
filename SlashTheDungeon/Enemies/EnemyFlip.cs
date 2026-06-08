using UnityEngine;

namespace SJ._01.Code.Enemies
{
    public class EnemyFlip : MonoBehaviour
    {
        private int _lookDir = 1;
    
        public void SetFlip(int dir)
        {
            if (dir == 0 || _lookDir == dir) return;
            _lookDir = dir;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            transform.localScale = scale;
        }
    }
}
