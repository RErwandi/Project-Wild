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
    public class ActionPhotonCreateRoom : IAction
    {
        public StringProperty roomName = new StringProperty("Development");
        public IntProperty maxPlayers = new IntProperty(0);
        public IntProperty playerTTL = new IntProperty(0);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            RoomOptions roomOptions = new RoomOptions() { };
            roomOptions.PublishUserId = true;
            roomOptions.MaxPlayers = (byte)((float)maxPlayers.GetValue(target));
            roomOptions.PlayerTtl = playerTTL.GetValue(target);
            PhotonNetwork.CreateRoom(string.IsNullOrEmpty(roomName.GetValue(target)) ? null : roomName.GetValue(target), roomOptions, TypedLobby.Default);
            yield return 0;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Actions/";

        public static new string NAME = "Photon/Create Room";
        private const string NODE_TITLE = "Create Room: {0}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spRoomName;
        private SerializedProperty spMaxPlayers;
        private SerializedProperty spPlayerTTL;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.roomName);
        }

        protected override void OnEnableEditorChild()
        {
            this.spRoomName = this.serializedObject.FindProperty("roomName");
            this.spMaxPlayers = this.serializedObject.FindProperty("maxPlayers");
            this.spPlayerTTL = this.serializedObject.FindProperty("playerTTL");
        }

        protected override void OnDisableEditorChild()
        {
            this.spRoomName = null;
            this.spMaxPlayers = null;
            this.spPlayerTTL = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spRoomName);
            
            EditorGUILayout.PropertyField(this.spMaxPlayers, new GUIContent("Max Players", "0 Means no limit."));
            EditorGUILayout.PropertyField(this.spPlayerTTL);

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
