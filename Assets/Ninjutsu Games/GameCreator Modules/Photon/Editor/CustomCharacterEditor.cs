using GameCreator.Characters;
using GameCreator.Core;
using NJG;
#if PHOTON_RPG
using NJG.GC.AI;
using NJG.RPG;
#endif
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace NJG.PUN
{
    public class Section2
    {
        public const string ICONS_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/";
        private const string KEY_STATE = "network-action-section-{0}";

        private const float ANIM_BOOL_SPEED = 3.0f;

        // PROPERTIES: ------------------------------------------------------------------------

        public GUIContent name;
        public AnimBool state;

        // INITIALIZERS: ----------------------------------------------------------------------

        public Section2(string name, string icon, UnityAction repaint, string overridePath = "")
        {
            this.name = new GUIContent(
                string.Format(" {0}", name),
                this.GetTexture(icon, overridePath)
            );

            this.state = new AnimBool(this.GetState());
            this.state.speed = ANIM_BOOL_SPEED;
            this.state.valueChanged.AddListener(repaint);
        }

        // PUBLIC METHODS: --------------------------------------------------------------------

        public void PaintSection()
        {
            GUIStyle buttonStyle = (this.state.target
                ? CoreGUIStyles.GetToggleButtonNormalOn()
                : CoreGUIStyles.GetToggleButtonNormalOff()
            );

            if (GUILayout.Button(this.name, buttonStyle))
            {
                this.state.target = !this.state.target;
                string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
                EditorPrefs.SetBool(key, this.state.target);
            }
        }

        // PRIVATE METHODS: -------------------------------------------------------------------

        private bool GetState()
        {
            string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
            return EditorPrefs.GetBool(key, false);
        }

        private Texture2D GetTexture(string icon, string overridePath = "")
        {
            string path = Path.Combine(string.IsNullOrEmpty(overridePath) ? ICONS_PATH : overridePath, icon);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerCharacter), true)]
    public class CustomPlayerCharacterEditor : PlayerCharacterEditor
    {
        private const string ICONS_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/";
        private GUIContent GUI_SYNC = new GUIContent("Sync Character", "Enables synchronization of over the network for this Character.");

        private bool hasComponent = false;
        private bool initialized = false;
        private CharacterNetwork characterNetwork;
        private CharacterNetworkEditor characterNetworkEditor;
        private Section section;
        private SerializedProperty spActions;
        private SerializedObject actionSerializedObject;

        private Texture2D GetTexture(string icon, string overridePath = "")
        {
            string path = Path.Combine(string.IsNullOrEmpty(overridePath) ? ICONS_PATH : overridePath, icon);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject == null) return;

            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical();
            GUILayout.Space(-3);

            if (this.section == null)
            {
                this.section = new Section("Network Settings", GetTexture("ActionNetwork.png"), this.Repaint);
            }

            if (!initialized)
            {
                characterNetwork = character.GetComponent<CharacterNetwork>();
                if(characterNetwork != null && characterNetworkEditor == null) characterNetworkEditor = (CharacterNetworkEditor)Editor.CreateEditor(characterNetwork, typeof(CharacterNetworkEditor));

                hasComponent = characterNetwork != null;
                initialized = true;
            }

            bool hasChanged = false;

            this.section.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.section.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUI.BeginChangeCheck();
                    hasComponent = EditorGUILayout.Toggle(GUI_SYNC, hasComponent);
                    hasChanged = EditorGUI.EndChangeCheck();
                    
                    if (characterNetworkEditor != null)
                    {
                        characterNetworkEditor.serializedObject.Update();
                        characterNetworkEditor.PaintInspector();
                        characterNetworkEditor.serializedObject.ApplyModifiedProperties();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (hasChanged)
            {
                if (characterNetwork != null)
                {
                    DestroyImmediate(characterNetworkEditor);
                    DestroyImmediate(characterNetwork, true);
                    EditorGUIUtility.ExitGUI();

                    characterNetwork = null;
                    characterNetworkEditor = null;
                    initialized = false;
                }
                else
                {
                    characterNetwork = character.GetComponent<CharacterNetwork>() ?? character.gameObject.AddComponent<CharacterNetwork>();

                    characterNetwork.SetupPhotonView();

                    characterNetworkEditor = (CharacterNetworkEditor)Editor.CreateEditor(characterNetwork, typeof(CharacterNetworkEditor));
                    characterNetwork.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

                    hasComponent = true;
                }
                hasChanged = false;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Character), true)]
    public class CustomCharacterEditor : CharacterEditor
    {
        private GUIContent GUI_SYNC = new GUIContent("Sync Character", "Enables synchronization of over the network for this Character.");

        private bool hasComponent = false;
        private bool initialized = false;
        private CharacterNetwork characterNetwork;
        private CharacterNetworkEditor characterNetworkEditor;
        private Section2 section;
        private SerializedProperty spActions;
        private SerializedObject actionSerializedObject;

        public override void OnInspectorGUI()
        {
            if (serializedObject == null) return;

            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical();
            GUILayout.Space(-3);

            if (this.section == null)
            {
                this.section = new Section2("Network Settings", "ActionNetwork.png", this.Repaint);
            }

            if (!initialized)
            {
                characterNetwork = character.GetComponent<CharacterNetwork>();
                if (characterNetwork != null && characterNetworkEditor == null) characterNetworkEditor = (CharacterNetworkEditor)Editor.CreateEditor(characterNetwork, typeof(CharacterNetworkEditor));

                hasComponent = characterNetwork != null;
                initialized = true;
            }

            bool hasChanged = false;

            this.section.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.section.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUI.BeginChangeCheck();
                    hasComponent = EditorGUILayout.Toggle(GUI_SYNC, hasComponent);
                    hasChanged = EditorGUI.EndChangeCheck();

                    if (characterNetworkEditor != null)
                    {
                        characterNetworkEditor.serializedObject.Update();
                        characterNetworkEditor.PaintInspector();
                        characterNetworkEditor.serializedObject.ApplyModifiedProperties();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (hasChanged)
            {
                if (characterNetwork != null)
                {
                    DestroyImmediate(characterNetworkEditor);
                    DestroyImmediate(characterNetwork, true);
                    EditorGUIUtility.ExitGUI();

                    characterNetwork = null;
                    characterNetworkEditor = null;
                    initialized = false;
                }
                else
                {
                    characterNetwork = character.GetComponent<CharacterNetwork>() ?? character.gameObject.AddComponent<CharacterNetwork>();

                    characterNetwork.SetupPhotonView();

                    characterNetworkEditor = (CharacterNetworkEditor)Editor.CreateEditor(characterNetwork, typeof(CharacterNetworkEditor));
                    characterNetwork.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    //characterNetwork.hideFlags = HideFlags.None;

                    hasComponent = true;
                }
                hasChanged = false;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}