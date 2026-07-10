using System.Collections.Generic;
using UnityEngine;
using MazeRunner.Core;

namespace MazeRunner.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerEffectHandler : MonoBehaviour
    {
        private class ActiveEffect
        {
            public IPlayerEffect Effect;
            public float RemainingTime;
        }

        private PlayerController controller;
        private readonly List<ActiveEffect> activeEffects = new List<ActiveEffect>();

        public event System.Action<IPlayerEffect> OnEffectAdded;
        public event System.Action<IPlayerEffect> OnEffectRemoved;

        void Start()
        {
            controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var active = activeEffects[i];
                active.Effect.Tick(controller, Time.deltaTime);
                active.RemainingTime -= Time.deltaTime;

                if (active.RemainingTime <= 0f)
                {
                    active.Effect.Remove(controller);
                    OnEffectRemoved?.Invoke(active.Effect);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public void AddEffect(IPlayerEffect effect)
        {
            if (!effect.IsStackable)
            {
                for (int i = activeEffects.Count - 1; i >= 0; i--)
                {
                    if (activeEffects[i].Effect.EffectName == effect.EffectName)
                    {
                        activeEffects[i].RemainingTime = effect.Duration;
                        return;
                    }
                }
            }

            effect.Apply(controller);
            activeEffects.Add(new ActiveEffect { Effect = effect, RemainingTime = effect.Duration });
            OnEffectAdded?.Invoke(effect);
        }

        public void ClearAllEffects()
        {
            foreach (var active in activeEffects)
            {
                active.Effect.Remove(controller);
                OnEffectRemoved?.Invoke(active.Effect);
            }
            activeEffects.Clear();
        }

        public bool HasEffect(string effectName)
        {
            foreach (var active in activeEffects)
            {
                if (active.Effect.EffectName == effectName) return true;
            }
            return false;
        }
    }
}
