using System;
using System.Collections;
using HN.Code.Combat;
using Unity.VisualScripting;
using UnityEngine;

namespace SJ._01.Code.Enemies
{
    public class Slash : MonoBehaviour
    {
        //[SerializeField] private Transform _target;
        [SerializeField] private float _force = 10f;
        [SerializeField] private int _damage = 3;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private LayerMask whatIsPlayer;
        
        private DamageCaster _damageCaster = null;
        
        private Health _health;
        private Collider2D _collider2D;
        private Rigidbody2D _rbCompo;

        private void Awake()
        {
            _rbCompo = GetComponent<Rigidbody2D>();
            _damageCaster = GetComponentInChildren<DamageCaster>();
            _collider2D = GetComponent<Collider2D>();
        }

        private void Start()
        {
            StartCoroutine(DestroyCoroutine());
        }

        private IEnumerator DestroyCoroutine()
        {
            yield return new WaitForSeconds(lifeTime);
            
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_damageCaster.CastDamageOverlapBox(_damage))
            {
                _collider2D.excludeLayers = whatIsPlayer.value;
            }
        }
        
        public void Launch(Vector2 direction)
        {
            if (_rbCompo != null)
            {
                _rbCompo.AddForce(direction.normalized * _force, ForceMode2D.Impulse);
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }
        
}
