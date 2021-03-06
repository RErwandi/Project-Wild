﻿using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GameCreator.Core;
using GameCreator.Variables;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace NJG.PUN
{
    [AddComponentMenu(""), RequireComponent(typeof(PhotonView))]
    public class LocalVariablesNetwork : MonoBehaviourPun, IMatchmakingCallbacks, IInRoomCallbacks
    {
        private const string RPC = "LVPRPC";

        [System.Serializable]
        public class VarRPC
        {
            public LocalVariables variables;
        }

        public List<VarRPC> localVars = new List<VarRPC>();

        private Dictionary<string, MBVariable> VARS = new Dictionary<string, MBVariable>();
        private bool initialized;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);

#if UNITY_EDITOR
            HideStuff();
#endif
            if (PhotonNetwork.UseRpcMonoBehaviourCache)
            {
                photonView.RefreshRpcMonoBehaviourCache();
            }
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

#if UNITY_EDITOR
        private void HideStuff()
        {
            this.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        }

        private void OnValidate()
        {
            HideStuff();
        }
#endif

        private void Awake()
        {
            if (PhotonNetwork.InRoom)
            {
                if (!initialized) Initialize();
            }
        }

        private void Initialize()
        {
            if (initialized) return;

            initialized = true;

            //if (photonView.IsMine || photonView.IsSceneView)
            {
                for (int i = 0, imax = localVars.Count; i < imax; i++)
                {
                    int index = i;
                    VarRPC rpc = localVars[i];
                    if (rpc == null || !rpc.variables) continue;

                    for (int e = 0, emax = rpc.variables.references.Length; e < emax; e++)
                    {
                        var variable = rpc.variables.references[e];
                        Variable.DataType varType = (Variable.DataType)variable.variable.type;

                        if (varType == Variable.DataType.Null ||
                            varType == Variable.DataType.GameObject ||
                            varType == Variable.DataType.Sprite ||
                            varType == Variable.DataType.Texture2D) continue;

                        if (!VARS.ContainsKey(variable.variable.name))
                        {
                            VARS.Add(variable.variable.name, variable);

                            VariablesManager.events.SetOnChangeLocal(
                                this.OnVariableChange,
                                variable.gameObject,
                                variable.variable.name
                            );
                        }
                    }
                }

                //Debug.LogWarningFormat("[LocalVarsNetwork] Initialized vars: {0}", VARS.Count);
            }
        }

        private void OnDestroy()
        {
            foreach(var v in VARS)
            {
                VariablesManager.events.RemoveChangeLocal(OnVariableChange, v.Value.gameObject, v.Value.variable.name);
            }
        }

        private void OnVariableChange(string variableID)
        {
            MBVariable var = null;
            //Debug.LogWarningFormat("[LocalVarsNetwork] OnVariableChange variableID: {0}", variableID);
            if (VARS.TryGetValue(variableID, out var))
            {
                if (photonView.IsSceneView && gameObject.name.StartsWith("==>"))
                {
                    gameObject.name = string.Format("{0}", gameObject.name.Replace("==>", string.Empty));
                    return;
                }

                //Debug.LogWarningFormat("[LocalVarsNetwork] OnVariableChange Send RPC variableID: {0} value: {1}", variableID, var.variable.Get());
                photonView.RPC(RPC, RpcTarget.Others, variableID, var.variable.Get());
            }
        }

        [PunRPC]
        public virtual void LVPRPC(string variableID, object value, PhotonMessageInfo info)
        {
            MBVariable var = null;
            //Debug.LogWarningFormat("[LocalVarsNetwork] RPC variableID: {0} value: {1}", variableID, value);
            if (VARS.TryGetValue(variableID, out var))
            {
                if (info.photonView.IsSceneView)
                {
                    info.photonView.gameObject.name = string.Format("==>{0}", info.photonView.gameObject.name);
                }

                var.variable.Update(value);
                VariablesManager.events.OnChangeLocal(var.gameObject, var.variable.name);
                //Debug.LogWarningFormat("[LocalVarsNetwork] 2 RPC updatedValue: {0} networkValue: {1}", var.variable.Get(), value);
            }
            else
            {
                Debug.LogWarningFormat("Could not find variable {0} on {1}.", variableID, gameObject, gameObject);
            }
        }

        [PunRPC]
        public virtual void UpdateAll(Hashtable data)
        {
            foreach(var d in data)
            {
                MBVariable var = VARS[d.Key.ToString()];
                var.variable.Update(d.Value);
                VariablesManager.events.OnChangeLocal(var.gameObject, var.variable.name);
            }
        }


        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinedRoom()
        {
            Initialize();
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public void OnLeftRoom()
        {
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Hashtable data = new Hashtable(VARS.Count);
            foreach (var v in VARS)
            {
                data.Add(v.Key, v.Value.variable.Get());
            }
            photonView.RPC("UpdateAll", newPlayer, data);
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }
    }
}
