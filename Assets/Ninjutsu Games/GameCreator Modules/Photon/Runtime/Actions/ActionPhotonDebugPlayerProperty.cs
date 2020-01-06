namespace NJG.PUN
{
    using GameCreator.Core;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
    public class ActionPhotonDebugPlayerProperty : IAction
    {
        public enum DebugType
        {
            Property,
            AllInformation
        }
        public TargetPhotonPlayer target = new TargetPhotonPlayer();
        public DebugType type = DebugType.Property;
        public string property = "player property";

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Photon.Realtime.Player player = this.target.GetPhotonPlayer(target);
            if(player == null)
            {
                Debug.LogWarning("Invalid Photon Player in " + target, gameObject);
                return true;
            }

            if (type == DebugType.Property) Debug.LogFormat("Player {0} property: {1} = {2}", player.NickName, property, player.GetProperty(property));
            else Debug.Log(player.ToStringFull());
            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public static new string NAME = "Photon/Debug Player Property";
        private const string NODE_TITLE = "Debug {0} {1}";
        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Actions/";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;
        private SerializedProperty spType;
        private SerializedProperty spProperty;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.target, type == DebugType.Property ? "property: " + this.property : "All information");
        }

        protected override void OnEnableEditorChild()
        {
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spType = this.serializedObject.FindProperty("type");
            this.spProperty = this.serializedObject.FindProperty("property");
        }

        protected override void OnDisableEditorChild()
        {
            this.spTarget = null;
            this.spType = null;
            this.spProperty = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spType);
            if(type == DebugType.Property) EditorGUILayout.PropertyField(this.spProperty);

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}