namespace GameCreator.Core
{
    using UnityEngine;
    using GameCreator.Variables;
    using Photon.Pun;

#if UNITY_EDITOR
    using UnityEditor;

#endif

    [AddComponentMenu("")]
	public class ConditionPhotonPlayerCount : ICondition
	{
        public enum Operation
        {
            Equal,
            NotEqual,
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual
        }
        public Operation comparisson = Operation.Equal;
        public IntProperty count = new IntProperty();

		// EXECUTABLE: ----------------------------------------------------------------------------

		public override bool Check(GameObject target)
		{
            if (PhotonNetwork.InRoom)
            {
                int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                switch (comparisson)
                {
                    case Operation.Equal: return playerCount == this.count.GetValue(target);
                    case Operation.NotEqual: return playerCount != this.count.GetValue(target);
                    case Operation.Greater: return playerCount > this.count.GetValue(target);
                    case Operation.GreaterOrEqual: return playerCount >= this.count.GetValue(target);
                    case Operation.Less: return playerCount < this.count.GetValue(target);
                    case Operation.LessOrEqual: return playerCount <= this.count.GetValue(target);
                }
            }

            return false;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Conditions/";

        public static new string NAME = "Photon/Player Count";
		private const string NODE_TITLE = "Player Count is {0} {1} {2}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spOperation;
        private SerializedProperty spValue;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
		{
            string mid = "than";
            if (comparisson == Operation.Equal || comparisson == Operation.NotEqual) mid = "to";
            return string.Format(NODE_TITLE, this.comparisson, mid, this.count);
        }

        protected override void OnEnableEditorChild()
        {
            this.spOperation = this.serializedObject.FindProperty("comparisson");
            this.spValue = this.serializedObject.FindProperty("count");
        }

        protected override void OnDisableEditorChild()
        {
            this.spOperation = null;
            this.spValue = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spOperation);
            EditorGUILayout.PropertyField(this.spValue);

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
