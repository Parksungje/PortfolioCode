using System.Collections;
using UnityEngine;

namespace Work.PSJ.Code.Combat
{
    public interface ITelegraph
    {
        IEnumerator ShowWarning(Transform origin, Vector2 direction, float distance, float width, float duration);
        
        void CancelWarning();
    }
}