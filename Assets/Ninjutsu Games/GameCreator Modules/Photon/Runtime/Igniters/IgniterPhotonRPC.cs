using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core.Hooks;
    using ExitGames.Client.Photon;
    using Photon.Pun;

    [AddComponentMenu(""), RequireComponent(typeof(PhotonView))]
    public class IgniterPhotonRPC : Igniter
    {
#if UNITY_EDITOR
        public new static string NAME = "Photon/On Photon RPC";
        public new static bool REQUIRES_COLLIDER = false;
        public new static string ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Igniters/";
        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Igniters/";
#endif
        public string rpcName;

        private PhotonView photonView;

        new private void OnEnable()
        {
#if UNITY_EDITOR
            this.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
#endif

            if (photonView == null) photonView = PhotonView.Get(gameObject);
        }
    }
}