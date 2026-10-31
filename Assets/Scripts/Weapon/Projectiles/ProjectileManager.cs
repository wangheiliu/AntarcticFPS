using UnityEngine;
using UnityEngine.Pool;

using System;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEditor;



namespace Game.Weapons.Projectiles
{
    public enum ProjectileTypes
    {
        Bullet,
        Rocket,
        Grenade
    }

    [Serializable]
    public struct ProjectileData
    {
        public ProjectileTypes projectileTypes;
        public GameObject projectilePrefab;
    }

    public class ProjectileManager : MonoBehaviour
    {
        [SerializeField] private List<ProjectileData> projectileDatas;
        [SerializeField] private GameObject bulletContainer;
        private Dictionary<ProjectileTypes, IObjectPool<Projectile>> projectileToBullet = new();

        void Awake()
        {
            foreach (ProjectileData projectileData in projectileDatas)
            {
                ProjectileData config = projectileData;
                if (projectileData.projectilePrefab == null)
                {
                    return;
                }

                if (projectileToBullet.ContainsKey(projectileData.projectileTypes))
                {
                    return;
                }
                IObjectPool<Projectile> objectPool = null;

                objectPool = new ObjectPool<Projectile>(
                    createFunc: () => CreateProjectile(config.projectilePrefab, objectPool),
                    actionOnGet: projectile => projectile.EnableBullet(),
                    actionOnRelease: projectile => projectile.ReturnToPool(projectile),
                    actionOnDestroy: projectile => Destroy(projectile.gameObject),
                    defaultCapacity: 25,
                    maxSize: 100
                );

                projectileToBullet.Add(config.projectileTypes, objectPool);
            }
        }

        private Projectile CreateProjectile(GameObject bullet, IObjectPool<Projectile> _pool)
        {
            GameObject instance = Instantiate(bullet, bulletContainer.transform);
            Projectile projectile = instance.GetComponent<Projectile>();
            Debug.Log(instance.name);

            if (projectile == null)
            {
                Destroy(instance);

                throw new InvalidOperationException($"{bullet.name} must have a Projectile component");
            }

            projectile.SetPool(_pool);


            return projectile;
        }

        public Projectile GetProjectile(ProjectileTypes types)
        {
            if (projectileToBullet.TryGetValue(types, out var _pool))
            {
                if (_pool != null)
                {
                    try
                    {
                        return _pool.Get();
                    } catch (Exception e)
                    {
                        Debug.LogWarning(e + " Can't get projectile");
                    }
                }
            }
            
            return null;
        }
    }

}
