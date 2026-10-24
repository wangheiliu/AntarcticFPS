using UnityEngine;
using UnityEngine.Pool;

using System.Collections;
using System;

namespace Game.Weapons
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        private IObjectPool<Projectile> _pool;
        private bool isBulletInUse;
        private Coroutine lifeTimeCycle;
        private Vector3 _dir = Vector3.zero;
        public event Action<Projectile, GameObject> OnBulletHit;

        public void SetPool(IObjectPool<Projectile> pool)
        {
            _pool = pool;
        }

        internal void EnableBullet()
        {
            isBulletInUse = true;
            gameObject.SetActive(true);
            lifeTimeCycle = StartCoroutine(BulletLifetimeCycle());
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            OnBulletHit?.Invoke(this, collision.gameObject);
            if (collision.gameObject.CompareTag("Weapon") || collision.gameObject.CompareTag("Player"))
            {
                return;
            }
            ReturnToPool(this);
        }

        protected virtual void FixedUpdate()
        {
            if (isBulletInUse)
            {
                rb.AddForce(_dir, ForceMode.Impulse);
            }
        }

        public void ReturnToPool(Projectile projectile)
        {
            if (!isBulletInUse) return;

            isBulletInUse = false;
            lifeTimeCycle = null;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            gameObject.SetActive(false);

            if (lifeTimeCycle != null)
            {
                StopCoroutine(lifeTimeCycle);
                lifeTimeCycle = null;
            }
            _pool?.Release(projectile);
        }

        protected virtual IEnumerator BulletLifetimeCycle(float seconds = 2f)
        {
            yield return new WaitForSeconds(seconds);
            ReturnToPool(this);
        }

        public virtual void Launch(Vector3 originVector, float speed, float lifetime = 2f)
        {
            transform.forward = originVector.normalized;
            
            Vector3 direction = transform.forward * speed;
            rb.linearVelocity = direction;
            _dir = direction;

            if (lifeTimeCycle != null)
            {
                StopCoroutine(lifeTimeCycle);
            }

            lifeTimeCycle = StartCoroutine(BulletLifetimeCycle(lifetime));

        }
    }
}
