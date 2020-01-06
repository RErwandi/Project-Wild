using GameCreator.Core;
using NJG;
using Photon.Pun;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace NJG.PUN
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Actions))]
    public class CustomActionsEditor : ActionsEditor
    {
        public class Section
        {
            private const string ICONS_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/";
            private const string KEY_STATE = "network-action-section-{0}";

            private const float ANIM_BOOL_SPEED = 3.0f;

            // PROPERTIES: ------------------------------------------------------------------------

            public GUIContent name;
            public AnimBool state;

            // INITIALIZERS: ----------------------------------------------------------------------

            public Section(string name, string icon, UnityAction repaint, string overridePath = "")
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

        private GUIContent GUI_SYNC = new GUIContent("Sync Actions", "Adds a PhotonNetwork component to sync these actions.");
        private GUIContent GUI_TARGET = new GUIContent("Targets", "Adds a PhotonNetwork component to sync these actions.");
        private GUIContent GUI_TARGET_TYPE = new GUIContent("Target Type", "The target used to send this Actions RPC.");

        private const string All = "Sends the RPC to everyone else and executes it immediately on this client. Player who join later will not execute this RPC.";
        private const string Others = "Sends the RPC to everyone else. This client does not execute the RPC. Player who join later will not execute this RPC.";
        private const string MasterClient = "Sends the RPC to MasterClient only. Careful: The MasterClient might disconnect before it executes the RPC and that might cause dropped RPCs.";
        private const string AllBuffered = "Sends the RPC to everyone else and executes it immediately on this client. New players get the RPC when they join as it's buffered (until this client leaves).";
        private const string OthersBuffered = "Sends the RPC to everyone. This client does not execute the RPC. New players get the RPC when they join as it's buffered (until this client leaves).";
        private const string AllViaServer = "Sends the RPC to everyone (including this client) through the server.\nThe server's order of sending the RPCs is the same on all clients.";
        private const string AllBufferedViaServer = "Sends the RPC to everyone (including this client) through the server and buffers it for players joining later.\nThe server's order of sending the RPCs is the same on all clients.";
        private const string SpecificPlayer = "Sends the RPC to an specific Player.";

        private bool hasComponent = false;
        private ActionsNetwork.ActionRPC actionsRPC;
        private int index = -1;
        private bool initialized = false;
        private ActionsNetwork actionsNetwork;
        private SerializedObject serializedNetwork;
        private Actions actions;
        private Section section;
        private SerializedProperty spActions;
        private SerializedObject actionSerializedObject;

        public override void OnInspectorGUI()
        {
            if (serializedObject == null) return;

            base.OnInspectorGUI();
            actions = target as Actions;

            if (actions.transform.root.gameObject.GetComponent<NetworkItem>())
            {
                return;
            }

            if(this.section == null)
            {
                this.section = new Section("Network Settings", "ActionNetwork.png", this.Repaint);
            }

            if (!initialized)
            {
                actionsNetwork = actions.transform.root.gameObject.GetComponent<ActionsNetwork>();
                
                //if(actionsNetwork != null) actionsRPC = ArrayUtility.Find(actionsNetwork.actions, p => p.actions == actions);
                if (actionsNetwork != null)
                {
                    serializedNetwork = new SerializedObject(actionsNetwork);
                    actionsRPC = actionsNetwork.actions.Find(p => p.actions == actions);
                    index = actionsNetwork.actions.IndexOf(actionsRPC);
                }
                hasComponent = actionsRPC != null;
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

                    if (actionsRPC != null)
                    {
                        actionsRPC.targetType = (ActionsNetwork.ActionRPC.TargetType)EditorGUILayout.EnumPopup(GUI_TARGET_TYPE, actionsRPC.targetType);

                        if (actionsRPC.targetType == ActionsNetwork.ActionRPC.TargetType.PhotonPlayer)
                        {
                            if(serializedNetwork != null) EditorGUILayout.PropertyField(serializedNetwork.FindProperty("actions").GetArrayElementAtIndex(index).FindPropertyRelative("targetPlayer"));
                            EditorGUILayout.HelpBox(SpecificPlayer, MessageType.Info, false);
                        }
                        else
                        {
                            actionsRPC.targets = (RpcTarget)EditorGUILayout.EnumPopup(GUI_TARGET, actionsRPC.targets);

                            RpcTarget targets = actionsRPC.targets;
                            if (targets == RpcTarget.All)
                            {
                                EditorGUILayout.HelpBox(All, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.AllBuffered)
                            {
                                EditorGUILayout.HelpBox(AllBuffered, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.AllBufferedViaServer)
                            {
                                EditorGUILayout.HelpBox(AllBufferedViaServer, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.AllViaServer)
                            {
                                EditorGUILayout.HelpBox(AllViaServer, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.MasterClient)
                            {
                                EditorGUILayout.HelpBox(MasterClient, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.Others)
                            {
                                EditorGUILayout.HelpBox(Others, MessageType.Info, false);
                            }
                            else if (targets == RpcTarget.OthersBuffered)
                            {
                                EditorGUILayout.HelpBox(OthersBuffered, MessageType.Info, false);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (hasChanged) //hasComponent != lastCheck
            {
                if (actionsRPC != null)
                {
                    //DestroyImmediate(actionsNetwork, true);
                    //if (networkEditor != null) DestroyImmediate(networkEditor, true);
                    //EditorGUIUtility.ExitGUI();
                    Debug.Log("Removed actions.");
                    actionsNetwork.actions.Remove(actionsRPC);
                    if(actionsNetwork.actions.Count == 0)
                    {
                        PhotonView pv = actionsNetwork.photonView;
                        
                        DestroyImmediate(actionsNetwork, true);
                        /*if (!pv.GetComponent<CharacterNetwork>() ||
                            !pv.GetComponent<StatsNetwork>() ||
                            !pv.GetComponent<LocalVariablesNetwork>())
                        {
                            DestroyImmediate(pv, true);
                        }*/
                    }
                    //ArrayUtility.Remove(ref actionsNetwork.actions, actionsRPC);

                    actionsRPC = null;
                    actionsNetwork = null;

                    initialized = false;
                }
                else
                {
                    actionsNetwork = actions.transform.root.gameObject.GetComponent<ActionsNetwork>() ?? actions.transform.root.gameObject.AddComponent<ActionsNetwork>();

                    SerializedObject serializedObject = new UnityEditor.SerializedObject(actionsNetwork);

                    actionsRPC = new ActionsNetwork.ActionRPC() { actions = actions };
                    actionsNetwork.actions.Add(actionsRPC);
                    //ArrayUtility.Add(ref actionsNetwork.actions, actionsRPC);

                    Debug.LogFormat("Added actions: {0}", actionsRPC.actions.gameObject.name);

                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();

                    hasComponent = true;
                }
                hasChanged = false;
            }
            EditorGUILayout.Space();
        }

        /*private void OnDestroy()
        {
            if (actionsRPC != null && actionsNetwork != null)
            {
                Debug.Log("OnDestroy");
                actionsNetwork.actions.Remove(actionsRPC);
                //ArrayUtility.Remove(ref actionsNetwork.actions, actionsRPC);
            }
        }*/
    }
}