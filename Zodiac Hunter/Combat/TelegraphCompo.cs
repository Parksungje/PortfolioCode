using System.Collections;
using Code.Entities;
using UnityEngine;

namespace Work.PSJ.Code.Combat
{
    [RequireComponent(typeof(LineRenderer))]
    public class TelegraphCompo : MonoBehaviour, ITelegraph, IEntityModule
    {
        [SerializeField] private Color startColor = new Color(0f, 0f, 0f, 0f);
        [SerializeField] private Color endColor = new Color(1f, 0f, 0f, 0.5f);
        
        private LineRenderer _lineRenderer;

        public void Initialize(Entity entity)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            
            _lineRenderer.enabled = false; 
            _lineRenderer.useWorldSpace = true;
        }

        public IEnumerator ShowWarning(Transform origin, Vector2 direction, float distance, float width, float duration)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.startColor = endColor;
            _lineRenderer.endColor = endColor;
            _lineRenderer.enabled = true;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                Vector2 startPos = origin.position;
                Vector2 endPos = startPos + direction * (distance * t);

                _lineRenderer.SetPosition(0, startPos);
                _lineRenderer.SetPosition(1, endPos);

                yield return null;
            }

            _lineRenderer.enabled = false;
        }

        public void CancelWarning()
        {
            StopAllCoroutines();
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }
        
        private void OnDisable()
        {
            CancelWarning();
        }
    }
}