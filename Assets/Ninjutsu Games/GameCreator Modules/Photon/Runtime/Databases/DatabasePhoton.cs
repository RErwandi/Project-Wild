#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

using GameCreator.Core;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Characters;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NJG.PUN
{
    public class DatabasePhoton : IDatabase
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public List<GameObject> prefabs = new List<GameObject>(0);
        public List<GameObject> attachments = new List<GameObject>(0);
        //public bool messageTarget;
        public bool monobehaviourCache;
        public int sendRate = 20;
        public int sendRateOnSerialize = 10;
        public int unreliableCommandsLimit = 10;

        public bool updatePing = true;
        public float updatePingEvery = 2f;

        public bool switchMasterClient = true;
        public float lagCheck = 3f;
        public int lagThreshold = 400;
        public int switchMasterErrors = 5;

        public string defaultName = "Player";

        private static bool initialized;

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (!initialized)
            {
                initialized = true;
                NetworkManager.Instance.Wakeup();

                var Instance = Load();

                if (Instance)
                {
#if PHOTON_UNITY_NETWORKING
                    PhotonNetwork.PrefabPool = new NJGPhotonPool(Instance);
                    //if (cache.messageTarget) PhotonNetwork.SendMonoMessageTargetType = typeof(Photon.MonoBehaviour);
                    if (PhotonNetwork.NetworkingClient != null)
                    {
                        PhotonNetwork.UseRpcMonoBehaviourCache = Instance.monobehaviourCache;

                        PhotonNetwork.NickName = Instance.defaultName + UnityEngine.Random.Range(0, 1000);

                        PhotonNetwork.SendRate = Instance.sendRate;
                        PhotonNetwork.SerializationRate = Instance.sendRateOnSerialize;
                        //PhotonNetwork.unreliableCommandsLimit = Instance.unreliableCommandsLimit;
                        //if (PhotonNetwork.SendMonoMessageTargets == null) PhotonNetwork.SendMonoMessageTargets = new HashSet<GameObject>();
                    }

#endif
                }
                else
                {
                    Debug.LogWarning("[Photon GameCreator] DatabasePhoton asset has not been created yet.");
                }
            }
        }

        public static DatabasePhoton Load()
        {
            return IDatabase.LoadDatabase<DatabasePhoton>();
        }

        public List<int> GetPrefabSuggestions(string hint)
        {
            hint = hint.ToLower();
            List<int> suggestions = new List<int>();
            for (int i = 0; i < this.prefabs.Count; ++i)
            {
                if (this.prefabs[i].name.ToLower().Contains(hint))
                {
                    suggestions.Add(i);
                }
            }

            return suggestions;
        }

        public GameObject GetPrefab(string prefab)
        {
            for(int i = 0, imax = prefabs.Count; i<imax; i++)
            {
                if(prefabs[i].name == prefab) return prefabs[i];
            }

            return null;
        }

        public GameObject GetAttachmentPrefab(string attachmentName)
        {
            for (int i = 0, imax = attachments.Count; i < imax; i++)
            {
                if (attachments[i] == null) continue;
                if (attachments[i].name == attachmentName) return attachments[i];
            }

            return null;
        }

        public string[] GetPrefabIds()
        {
            List<string> ids = new List<string>();

            for (int i = 0, imax = prefabs.Count; i < imax; i++)
            {
                GameObject sl = prefabs[i];
                if (sl == null) continue;

                ids.Add(sl.name);
            }

            return ids.ToArray();
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------
#if UNITY_EDITOR

        private const string PATH_DEFAULT_PLAYER = "Assets/Plugins/GameCreator/Characters/Prefabs/Player.prefab";
        private const string PATH_DEFAULT_CHARACTER = "Assets/Plugins/GameCreator/Characters/Prefabs/Player.prefab";
        private const string PATH_DEFAULT_GC = "Assets/Plugins/GameCreator";
        private string lastPath;

        public static bool IsDefaultPrefab(GameObject prefab)
        {
            return AssetDatabase.GetAssetPath(prefab) == PATH_DEFAULT_PLAYER || AssetDatabase.GetAssetPath(prefab) == PATH_DEFAULT_CHARACTER || AssetDatabase.GetAssetPath(prefab).Contains(PATH_DEFAULT_GC);
        }
         
        public static string GetPath(string defaultPath, string title = "Select folder to save your prefab", string folderName = "Prefabs Folder")
        {
            string fname = folderName;
            if (!string.IsNullOrEmpty(defaultPath))
            {
                fname = string.Empty;
            }
            string path = EditorUtility.SaveFolderPanel(title, string.IsNullOrEmpty(defaultPath) ? Application.dataPath : defaultPath, fname);
            //AssetDatabase.Refresh();
            return FileUtil.GetProjectRelativePath(path);
        }

        public static void FixPrefab(GameObject sourcePrefab, out GameObject newPrefab)
        {
            GameObject np = null;
            if (IsDefaultPrefab(sourcePrefab))
            {
                np = CreatePrefabCopy(sourcePrefab);
                //if (newPrefab != null) prefab = newPrefab.name;
            }
            else
            {
                
                CharacterNetwork chnet = sourcePrefab.GetComponent<CharacterNetwork>();
                Character character = sourcePrefab.GetComponent<Character>();

#if PHOTON_UNITY_NETWORKING
                PhotonView pview = sourcePrefab.GetPhotonView();
                if (!Load().prefabs.Contains(sourcePrefab))
                {
                    Load().prefabs.Add(sourcePrefab);
                    /*SerializedObject so = new SerializedObject(Load());
                    so.ApplyModifiedProperties();
                    so.Update();*/
                }

                if (chnet == null && character != null)
                {
                    chnet = sourcePrefab.AddComponent<CharacterNetwork>();
                }

                if (pview == null)
                {
                    pview = sourcePrefab.AddComponent<PhotonView>();
                }

                if (pview.Synchronization == ViewSynchronization.Off)
                {
                    pview.Synchronization = ViewSynchronization.UnreliableOnChange;
                }

                if (!pview.ObservedComponents.Contains(chnet))
                {
                    pview.ObservedComponents.Add(chnet);
                }

#endif

                np = sourcePrefab;
            }

            newPrefab = np;
        }

        public static GameObject CreatePrefabCopy(GameObject prefab)
        {
            string lastPath = DatabasePhoton.Load().lastPath;

            //if (string.IsNullOrEmpty(lastPath))
            //{
                lastPath = GetPath(lastPath);
            //}
            if (string.IsNullOrEmpty(lastPath))
            {
                Debug.LogWarning("Nothing was created!");
                //EditorUtility.DisplayDialog("Could not create Prefab", "Prefab Path folder has not been defined", "Ok");
                return null;
            }

            string prefabName = lastPath + "/" + prefab.name + ".prefab";

            DatabasePhoton.Load().lastPath = lastPath;

            //Object prefab2 = PrefabUtility.CreateEmptyPrefab(prefabName);
            //GameObject newPrefab = PrefabUtility.ReplacePrefab(prefab, prefab2, ReplacePrefabOptions.Default);
            //GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(new GameObject(prefab.name), lastPath);
            GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, lastPath);

            //DestroyImmediate(go);

            if (!DatabasePhoton.Load().prefabs.Contains(newPrefab))
            {
                DatabasePhoton.Load().prefabs.Add(newPrefab);
                /*SerializedObject so = new SerializedObject(Load());
                so.ApplyModifiedProperties();
                so.Update();*/
            }
            
            CharacterNetwork chnet = newPrefab.GetComponent<CharacterNetwork>();
            Character character = newPrefab.GetComponent<Character>();

#if PHOTON_UNITY_NETWORKING
            PhotonView pview = newPrefab.GetPhotonView();

            if (pview == null)
            {
                pview = newPrefab.AddComponent<PhotonView>();
            }

            if (chnet == null && character != null)
            {
                chnet = newPrefab.AddComponent<CharacterNetwork>();
            }

            if (pview.Synchronization == ViewSynchronization.Off)
            {
                pview.Synchronization = ViewSynchronization.UnreliableOnChange;
            }

            if (chnet != null && !pview.ObservedComponents.Contains(chnet))
            {
                pview.ObservedComponents.Add(chnet);
            }
#endif
            return newPrefab;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            IDatabase.Setup<DatabasePhoton>();
        }

        protected override string GetProjectPath()
        {
            return "Assets/Plugins/GameCreatorData/Photon/Resources";
        }

#endif
    }
}