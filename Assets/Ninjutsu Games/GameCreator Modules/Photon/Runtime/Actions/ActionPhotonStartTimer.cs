namespace GameCreator.Core
{
    using UnityEngine;
    using GameCreator.Variables;
    using NJG.PUN;
    using Photon.Pun;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
	public class ActionPhotonStartTimer : IAction
	{
		public bool limitTime;
        public IntProperty duration = new IntProperty();

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (this.duration.GetValue(target) > 0) PhotonNetwork.CurrentRoom.SetDurationTime(this.duration.GetValue(target));
                NetworkManager.SetStartTime();
            }
            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Actions/";

        public static new string NAME = "Photon/Start Room Timer";
		private const string NODE_TITLE = "Start Room Timer {0}";
		private const string NODE_TITLE_LIMIT = "with a limit of {0}(s)";
		private const string NODE_TITLE_NOLIMIT = "with no limit";
		private const string TIME = "with no limit";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spDuration;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(NODE_TITLE, this.duration.GetValue(null) > 0 ? string.Format(NODE_TITLE_LIMIT, this.duration) : NODE_TITLE_NOLIMIT);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spDuration = this.serializedObject.FindProperty("duration");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spDuration = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spDuration, new GUIContent("Time Limit"));
            if (this.duration.value < 0) this.duration.value = 0;

            if (this.duration.GetValue(null) > 0) EditorGUILayout.HelpBox("(seconds)", MessageType.None, false);
            else EditorGUILayout.HelpBox("(unlimited)", MessageType.None, false);

            EditorGUILayout.HelpBox("Only the Master Client can execute this.", MessageType.Warning, false);

            this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
