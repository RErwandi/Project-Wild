namespace GameCreator.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;
    using GameCreator.Variables;
    using Photon.Realtime;
    using Photon.Pun;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
    public class ActionPhotonJoinOrCreateRoom : IAction
    {
        public StringProperty roomName = new StringProperty();
        public IntProperty maxPlayers = new IntProperty(0);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            RoomOptions roomOptions = new RoomOptions() { };
            roomOptions.MaxPlayers = (byte)maxPlayers.GetValue(target);
            roomOptions.PublishUserId = true;
            return PhotonNetwork.JoinOrCreateRoom(string.IsNullOrEmpty(roomName.GetValue(target)) ? null : roomName.GetValue(target), roomOptions, TypedLobby.Default);
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Actions/";

        public static new string NAME = "Photon/Join Or Create Room";
        private const string NODE_TITLE = "Join Or Create Room: {0}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spRoomName;
        private SerializedProperty spMaxPlayers;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, string.IsNullOrEmpty(this.roomName.GetValue(null)) ? "null" : this.roomName.GetValue(null));
        }

        protected override void OnEnableEditorChild()
        {
            this.spRoomName = this.serializedObject.FindProperty("roomName");
            this.spMaxPlayers = this.serializedObject.FindProperty("maxPlayers");
        }

        protected override void OnDisableEditorChild()
        {
            this.spRoomName = null;
            this.spMaxPlayers = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spRoomName);
            
            EditorGUILayout.PropertyField(this.spMaxPlayers, new GUIContent("Max Players", "0 Means no limit."));

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
