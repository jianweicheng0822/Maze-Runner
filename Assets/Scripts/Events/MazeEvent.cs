using UnityEngine;

namespace MazeRunner.Events
{
    public abstract class MazeEvent : ScriptableObject
    {
        public string eventName;
        [TextArea] public string announcementText;
        public float duration = 15f;
        public float weight = 1f;
        public Sprite icon;

        public abstract void Activate();
        public abstract void Deactivate();
        public virtual void Tick(float deltaTime) { }
    }
}
