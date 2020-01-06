using UnityEngine;

[System.Serializable]
public class PrefabIDAttribute : PropertyAttribute { }

namespace GameCreator.Core
{
    using UnityEngine;
    using GameCreator.Variables;

    [System.Serializable]
    public class TargetPrefabId
    {
        public enum Target
        {
            CachedPrefab,
            GameObject
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Target target = Target.CachedPrefab;
        public GameObjectProperty gameObject;
        [PrefabID]public string prefab;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetPrefabId()
        {
            string result = null;

            switch (this.target)
            {
                case Target.CachedPrefab:
                    result = prefab;
                    break;
                case Target.GameObject:
                    result = this.gameObject.GetValue(null).name;
                    break;
            }

            return result;
        }

        // UTILITIES: -----------------------------------------------------------------------------

        public override string ToString()
        {
            string result = "(unknown)";
            switch (this.target)
            {
                case Target.CachedPrefab: result = string.IsNullOrEmpty(prefab) ? "(none)" : prefab; break;
                case Target.GameObject: result = this.gameObject.ToString(); break;
            }

            return result;
        }
    }
}