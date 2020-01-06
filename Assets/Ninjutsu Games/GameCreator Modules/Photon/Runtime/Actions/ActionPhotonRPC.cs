namespace GameCreator.Core
{
    using UnityEngine;
    using Photon.Pun;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu(""), RequireComponent(typeof(PhotonView))]
    public class ActionPhotonRPC : IAction
    {
        //public ObjectNetwork networkObject;
        public RpcTarget targets = RpcTarget.Others;
        public bool onlyMasterClient = false;
        public Actions executeActions;

        public PhotonView photonView
        {
            get
            {
                if (pview == null)
                {
                    pview = PhotonView.Get(this);
                }

                return pview;
            }
        }
        private Actions currentActions;
        private PhotonView pview;

        private const string RPC = "APRPC";        

        // EXECUTABLE: ----------------------------------------------------------------------------

        private void Awake()
        {
            currentActions = GetComponent<Actions>();
            /*if (networkObject != null && !networkObject.photonView.isMine)
            {
                Destroy(this);
            }*/
        }

        public override bool InstantExecute(GameObject target, IAction[] actions, int index, params object[] parameters)
        {
            /*Debug.Log("ActionPhotonRPC " + networkObject + " / " + networkObject.photonView.isMine);
            if (networkObject != null && networkObject.photonView.isMine)
            {
                networkObject.ExecuteActions(executeActions, currentActions, targets);
            }*/
            Debug.Log("ActionPhotonRPC CanExecute: " + ((photonView.IsMine && !onlyMasterClient) || (onlyMasterClient && PhotonNetwork.IsMasterClient))+" Invoker:"+ target+" root: "+transform.root);
            if ((photonView.IsMine && !onlyMasterClient) || (onlyMasterClient && PhotonNetwork.IsMasterClient))
            {
                //ExecuteActions(executeActions, currentActions, targets);
                photonView.RPC(RPC, targets);
            }
            return true;
        }

        [PunRPC]
        public virtual void APRPC(PhotonMessageInfo info)
        {
            bool canExecute = true;

            if(targets == RpcTarget.All || targets == RpcTarget.AllBuffered || targets == RpcTarget.AllBufferedViaServer || targets == RpcTarget.AllViaServer)
            {
                canExecute = this.executeActions != currentActions;
            }
            
            Debug.LogWarning("ActionRPC " + executeActions + " / " + targets+ " canExecute "+ canExecute);

            if (this.executeActions != null && canExecute)
            {
                GameObject invoker = photonView.gameObject ?? (info.Sender.TagObject as GameObject) ?? gameObject;
                this.executeActions.actionsList.Execute(invoker, null);
                /*CoroutinesManager.Instance.StartCoroutine(
                    this.executeActions.actionsList.ExecuteCoroutine(gameObject, null)
                );*/

                //if (this.waitToFinish) yield return coroutine;
            }
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Actions/";

        public static new string NAME = "Photon/Photon RPC";
        private const string NODE_TITLE = "Send Photon RPC to {0}";
        private const string FIX = "Fix It";

        private static readonly GUIContent ONLY_MASTER = new GUIContent("Only MasterClient","If On only master client can send this message.");
        private static readonly GUIContent TARGET_OTHERS = new GUIContent("Targets", "Sends the RPC to everyone else. This client does not execute the RPC. Player who join later will not execute this RPC.");

        private const string PHOTONVIEW = "This component requires a PhotonView in order to work.";
        private const string All = "Sends the RPC to everyone else and executes it immediately on this client. Player who join later will not execute this RPC.";
        private const string Others = "Sends the RPC to everyone else. This client does not execute the RPC. Player who join later will not execute this RPC.";
        private const string MasterClient = "Sends the RPC to MasterClient only. Careful: The MasterClient might disconnect before it executes the RPC and that might cause dropped RPCs.";
        private const string AllBuffered = "Sends the RPC to everyone else and executes it immediately on this client. New players get the RPC when they join as it's buffered (until this client leaves).";
        private const string OthersBuffered = "Sends the RPC to everyone. This client does not execute the RPC. New players get the RPC when they join as it's buffered (until this client leaves).";
        private const string AllViaServer = "Sends the RPC to everyone (including this client) through the server.\nThe server's order of sending the RPCs is the same on all clients.";
        private const string AllBufferedViaServer = "Sends the RPC to everyone (including this client) through the server and buffers it for players joining later.\nThe server's order of sending the RPCs is the same on all clients.";

        // PROPERTIES: ----------------------------------------------------------------------------

        //private SerializedProperty spNetworkObject;
        private SerializedProperty spTargets;
        private SerializedProperty spActions;
        private SerializedProperty spMaster;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.targets);
        }

        protected override void OnEnableEditorChild()
        {
            //this.spNetworkObject = this.serializedObject.FindProperty("networkObject");
            this.spTargets = this.serializedObject.FindProperty("targets");
            this.spActions = this.serializedObject.FindProperty("executeActions");
            this.spMaster = this.serializedObject.FindProperty("onlyMasterClient");
        }

        protected override void OnDisableEditorChild()
        {
            //this.spNetworkObject = null;
            this.spTargets = null;
            this.spActions = null;
            this.spMaster = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            //EditorGUILayout.PropertyField(this.spNetworkObject);
            
            if(photonView == null)
            {
                if (GUILayout.Button(FIX, EditorStyles.miniButton))
                {
                    gameObject.AddComponent<PhotonView>();
                }
                EditorGUILayout.HelpBox(PHOTONVIEW, MessageType.Error);

                EditorGUILayout.Space();
            }
            EditorGUILayout.PropertyField(this.spTargets, TARGET_OTHERS);
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
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spMaster, ONLY_MASTER);
            EditorGUILayout.PropertyField(this.spActions);

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
