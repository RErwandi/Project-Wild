namespace GameCreator.Core
{
    using GameCreator.Variables;
    using NJG.PUN;
    using Photon.Pun;
    using Photon.Realtime;
    using UnityEngine;
    using Hashtable = ExitGames.Client.Photon.Hashtable;

    [AddComponentMenu("")]
    public class IgniterOnPhotonPlayerProperty : Igniter, IInRoomCallbacks
    {
#if UNITY_EDITOR
        public new static string NAME = "Photon/On Player Property";
        public new static string COMMENT = "Leave property empty to trigger when any Player Property changes.\nSet targetPlayer as invoker to trigger when any Player Property changes.";
        public new static string ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Igniters/";
        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Igniters/";
#endif
        public TargetPhotonPlayer targetPlayer = new TargetPhotonPlayer() { target = TargetPhotonPlayer.Target.Player };
        public StringProperty property = new StringProperty();

        new private void OnEnable()
        {
#if UNITY_EDITOR
            this.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
#endif
            if (PhotonNetwork.NetworkingClient != null) PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            if (PhotonNetwork.NetworkingClient != null) PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnPlayerPropertiesUpdate(Player player, Hashtable props)
        {
            if (props.ContainsKey(PlayerProperties.PING)) return;

            bool canExecute = true;

            Player tplayer = this.targetPlayer.GetPhotonPlayer(gameObject);
            if (tplayer != null && tplayer != player) canExecute = false;
            if (property != null && !string.IsNullOrEmpty(property.GetValue(null)) && !props.ContainsKey(property.GetValue(null))) canExecute = false;

            if (canExecute)
            {
                this.ExecuteTrigger(gameObject);
            }
        }

        public void OnPlayerEnteredRoom(Player newPlayer) { }

        public void OnPlayerLeftRoom(Player otherPlayer) { }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }

        public void OnMasterClientSwitched(Player newMasterClient) { }
    }
}