using UnityEngine;
using Game.Weapons.Projectiles;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;

namespace Game.Weapons.WeaponSystem
{
    [Serializable]
    public enum WeaponAnimationState {
        Holding,
        Firing,
        Thrown
    }
    public abstract class BaseWeapon : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] protected GameObject muzzle;
        [SerializeField] protected WeaponData weaponData;
        [Header("Animation")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected Dictionary<WeaponAnimationState, AnimationClip> animationDictionary;
        protected Dictionary<WeaponAnimationState, int> animationHashes = new();

        public abstract event Action OnWeaponFire;

        protected abstract void Fire();
        protected virtual void SetAnimation(WeaponAnimationState state)
        {
            animator.SetTrigger(animationHashes[state]);
        }

        protected void AddStateHashes()
        {
            foreach (var animation in animationDictionary)
            {
                var value = animation.Value;
                animationHashes.Add(animation.Key, Animator.StringToHash(value.name));
            }
        }
    }
}
