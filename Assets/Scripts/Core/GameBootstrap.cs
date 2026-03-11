// GameBootstrap.cs
// Place on a root GameObject in every scene.
// Ensures SaveManager exists, then restores persisted state on Start.

using UnityEngine;

namespace Evetero
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (SaveManager.Instance == null)
            {
                var go = new GameObject("SaveManager");
                go.AddComponent<SaveManager>();
            }
        }

        private void Start()
        {
            SaveManager.Instance.Load();
        }
    }
}
