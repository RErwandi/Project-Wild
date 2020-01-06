namespace GameCreator.Core
{
    using UnityEngine;
    using NJG.PUN;
    using GameCreator.Variables;
    using Photon.Pun;
    using Photon.Realtime;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("")]
	public class ActionPhotonPlayerProperty : IAction
	{
        public enum VariableType
        {
            Int,
            Float,
            String,
            Bool
        }
        public enum Operation
        {
            Set,
            Add
        }
        public enum Permission
        {
            LocalPlayer,
            MasterClient,
            Anyone
        }
        public TargetPhotonPlayer target = new TargetPhotonPlayer() { target = TargetPhotonPlayer.Target.Player };
        public VariableType variableType = VariableType.Int;
        public Operation operation = Operation.Add;
        public Permission permission = Permission.LocalPlayer;
        public StringProperty propertyName = new StringProperty();
        public IntProperty intValue = new IntProperty();
        public NumberProperty floatValue = new NumberProperty();
        public StringProperty stringValue = new StringProperty();
        public BoolProperty boolValue = new BoolProperty();
        public bool webForward = true;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (PhotonNetwork.InRoom)
            {
                Player player = this.target.GetPhotonPlayer(target);

                if(player == null) return true;
                if(permission == Permission.LocalPlayer && !player.IsLocal) return true;
                if (permission == Permission.MasterClient && !PhotonNetwork.IsMasterClient) return true;

                if (player != null)
                {
                    switch (variableType)
                    {
                        case VariableType.Int:
                            if (operation == Operation.Set) player.SetInt(propertyName.GetValue(target), intValue.GetValue(target), webForward);
                            else player.AddInt(propertyName.GetValue(target), intValue.GetValue(target), webForward);
                            break;
                        case VariableType.Float:
                            if (operation == Operation.Set) player.SetFloat(propertyName.GetValue(target), floatValue.GetValue(target), webForward);
                            else player.AddFloat(propertyName.GetValue(target), floatValue.GetValue(target), webForward);
                            break;
                        case VariableType.String:
                            player.SetString(propertyName.GetValue(target), stringValue.GetValue(target), webForward);
                            break;
                        case VariableType.Bool:
                            player.SetBool(propertyName.GetValue(target), boolValue.GetValue(target), webForward);
                            break;
                    }
                }
            }
            else
            {
                Debug.LogWarning("You need to be inside a room in order to change a PhotonPlayer property.", gameObject);
            }
            return true;
		}

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Actions/";

        private readonly static GUIContent GUI_PERMISSION = new GUIContent("Write Permission", "Determines who is allowed to modify this property\n" +
            "\n - Local Player: Only the local player is allowed to modify this property." +
            "\n\n - Master Client: Only the Master Client is allowed to modify this property." +
            "\n\n - Anyone: Anyone is allowed to modify this property.");

        public static new string NAME = "Photon/Player Property";
		private const string NODE_TITLE = "Set Player Property {1} to {0}";
		private const string NODE_TITLE2 = "Add {0} to Player Property {1}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spTarget;
        private SerializedProperty spVariable;
        private SerializedProperty spOperation;
		private SerializedProperty spPermission;
        private SerializedProperty spPropertyName;

		private SerializedProperty spInt;
		private SerializedProperty spFloat;
		private SerializedProperty spString;
		private SerializedProperty spBool;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
		{
            object value = null;
            switch (variableType)
            {
                case VariableType.Int: value = intValue; break;
                case VariableType.Float: value = floatValue; break;
                case VariableType.String: value = stringValue; break;
                case VariableType.Bool: value = boolValue; break;
            }

            if (operation == Operation.Set) return string.Format(NODE_TITLE, value, propertyName);
            else return string.Format(NODE_TITLE2, value, propertyName);
        }

		protected override void OnEnableEditorChild ()
		{
			this.spTarget = this.serializedObject.FindProperty("target");
			this.spVariable = this.serializedObject.FindProperty("variableType");
            this.spOperation = this.serializedObject.FindProperty("operation");
			this.spPermission = this.serializedObject.FindProperty("permission");
            this.spPropertyName = this.serializedObject.FindProperty("propertyName");

			this.spInt = this.serializedObject.FindProperty("intValue");
			this.spFloat = this.serializedObject.FindProperty("floatValue");
			this.spString = this.serializedObject.FindProperty("stringValue");
			this.spBool = this.serializedObject.FindProperty("boolValue");
        }

		protected override void OnDisableEditorChild ()
		{
			this.spTarget = null;
			this.spVariable = null;
            this.spOperation = null;
			this.spPermission = null;
            this.spPropertyName = null;

			this.spInt = null;
			this.spFloat = null;
			this.spString = null;
			this.spBool = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            
            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spPropertyName);
            
            EditorGUILayout.PropertyField(this.spVariable);
            EditorGUI.indentLevel++;
            switch (variableType)
            {
                case VariableType.Int:
                    EditorGUILayout.PropertyField(this.spOperation);
                    EditorGUILayout.PropertyField(this.spInt);
                    break;
                case VariableType.Float:
                    EditorGUILayout.PropertyField(this.spOperation);
                    EditorGUILayout.PropertyField(this.spFloat);
                    break;
                case VariableType.String:
                    EditorGUILayout.PropertyField(this.spString);
                    break;
                case VariableType.Bool:
                    EditorGUILayout.PropertyField(this.spBool);
                    break;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spPermission, GUI_PERMISSION);

            this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
