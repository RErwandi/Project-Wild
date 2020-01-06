#if PHOTON_UNITY_NETWORKING
namespace NJG.PUN
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;
    using GameCreator.Variables;
    using Photon.Realtime;
    using Photon.Pun;

    [System.Serializable]
    public class TargetPhotonPlayer
    {
        public enum Target
        {
            Player,
            Invoker,
            GameObject,
            Id, 
            //LocalVariable,
            //GlobalVariable,
            //ListVariable
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Target target = Target.Id;
        public GameObjectProperty character;
        public int playerId;
        public HelperLocalVariable local = new HelperLocalVariable();
        public HelperGlobalVariable global = new HelperGlobalVariable();
        public HelperGetListVariable list = new HelperGetListVariable();

        private int cacheInstanceID;
        private Player cachePlayer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Player GetPhotonPlayer(GameObject invoker)
        {
            PhotonView view = null;

            switch (this.target)
            {
                case Target.Player:
                    if (PhotonNetwork.LocalPlayer != null)
                    {
                        cachePlayer = PhotonNetwork.LocalPlayer;
                    }
                    else if (HookPlayer.Instance && HookPlayer.Instance.GetInstanceID() != this.cacheInstanceID)
                    {
                        view = HookPlayer.Instance.Get<PhotonView>();
                        if (view) cachePlayer = view.Owner;
                        CacheInstanceID(HookPlayer.Instance.gameObject);
                    }
                    break;

                case Target.Invoker:
                    if (!invoker)
                    {
                        this.cachePlayer = null;
                        break;
                    }

                    if (this.cachePlayer == null || invoker.GetInstanceID() != this.cacheInstanceID)
                    {
                        view = invoker.GetComponentInChildren<PhotonView>();
                        if (view)
                        {
                            this.cachePlayer = view.Owner;
                            CacheInstanceID(invoker);
                        }
                    }

                    break;

                case Target.GameObject:
                    if (this.character != null)
                    {
                        GameObject go = this.character.GetValue(invoker);
                        if (go && go.GetInstanceID() != this.cacheInstanceID)
                        {
                            view = go.GetComponentInChildren<PhotonView>();
                            if(view) cachePlayer = view.Owner;
                            CacheInstanceID(go);
                        }
                    }
                    break;
                case Target.Id:
                    cachePlayer = PhotonNetwork.CurrentRoom.GetPlayer(playerId);
                    break;
            }

            return cachePlayer;
        }

        private void CacheInstanceID(GameObject go)
        {
            this.cacheInstanceID = go.GetInstanceID();
        }

        // UTILITIES: -----------------------------------------------------------------------------

        public override string ToString()
        {
            string result = "(unknown)";
            switch (this.target)
            {
                case Target.Player: result = "Player"; break;
                case Target.Invoker: result = "Invoker"; break;
                case Target.GameObject:
                    result = (this.character == null ? "(none)" : this.character.ToString());
                    break;
                case Target.Id:
                    result = "Photon Player Id: "+this.playerId;
                    break;
                //case Target.LocalVariable: result = this.local.ToString(); break;
                //case Target.GlobalVariable: result = this.global.ToString(); break;
                //case Target.ListVariable: result = this.list.ToString(); break;
            }

            return result;
        }
    }
}
#endif