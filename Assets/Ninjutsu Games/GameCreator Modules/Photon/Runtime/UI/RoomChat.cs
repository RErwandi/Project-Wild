using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace NJG.PUN
{

    /// <summary>
    /// Networked chat logic. Takes care of sending and receiving of chat messages.
    /// </summary>

    public class RoomChat : Chat, IOnEventCallback, IInRoomCallbacks
    {
        private static RoomChat mInst;

        public Color playerColor = new Color(0.6f, 1.0f, 0f);
        public Color otherColor = new Color(0.6f, 1.0f, 0f);
        public Color serverColor = new Color(0.6f, 1.0f, 0f);
        /*public string factionAName = "Blue Team";
        public string factionBName = "Red Team";
        public string factionCName = "Red Team";
        public string factionColorProperty = "characterColor";

        private ActorFaction factionA;
        private ActorFaction factionB;
        private ActorFaction factionC;
        private Color factionAColor;
        private Color factionBColor;
        private Color factionCColor;*/

        public int chatEventCode;
        /*{
            get
            {
                if (evCode == -1) evCode = PhotonRaiseEventAsset.Instance.GetDefinition("ChatEvent") == null ? 0 : PhotonRaiseEventAsset.Instance.GetDefinition("ChatEvent").eventCode;
                return evCode;
            }
        }*/

        /// <summary>
        /// Sound to play when a new message arrives.
        /// </summary>

        public AudioClip notificationSound;

        /// <summary>
        /// If you want the chat window to only be shown in multiplayer games, set this to 'true'.
        /// </summary>

        public bool destroyIfOffline = false;
        public bool notifyWhenPlayersConnect = true;

        //private int evCode = -1;
        private const string S_SPLIT = "|s|";

        protected override void Awake()
        {
            base.Awake();

            mInst = this;
            //PhotonBloxUtils.CacheTarget(gameObject);
        }

        /// <summary>
        /// We want to listen to input field's events.
        /// </summary>

        private void Start()
        {
            if (destroyIfOffline && !PhotonNetwork.InRoom)
            {
                Destroy(gameObject);
                return;
            }
            if (PhotonNetwork.NetworkingClient != null) PhotonNetwork.AddCallbackTarget(this);
            //PhotonNetwork.SendMonoMessageTargets.Add(gameObject);

            /*factionA = ActorFactionManager.Instance.GetRuntimeByIdent(factionAName, plyGameObjectIdentifyingType.screenName);
            factionB = ActorFactionManager.Instance.GetRuntimeByIdent(factionBName, plyGameObjectIdentifyingType.screenName);
            factionC = ActorFactionManager.Instance.GetRuntimeByIdent(factionCName, plyGameObjectIdentifyingType.screenName);

            if (factionA != null && factionA.varListDef.Exists(f => f.name == factionColorProperty))
            {
                factionAColor = (Color) factionA.GetFactionVarValue(factionColorProperty, factionA);
            }

            if (factionB != null && factionB.varListDef.Exists(f => f.name == factionColorProperty))
            {
                factionBColor = (Color) factionB.GetFactionVarValue(factionColorProperty, factionB);
            }

            if (factionC != null && factionC.varListDef.Exists(f => f.name == factionColorProperty))
            {
                factionCColor = (Color)factionC.GetFactionVarValue(factionColorProperty, factionC);
            }*/

            //input.gameObject.SetActive(PhotonNetwork.inRoom);
        }

        void OnJoinedRoom()
        {
            input.gameObject.SetActive(true);
        }

        void OnLeftRoom()
        {
            input.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (PhotonNetwork.NetworkingClient != null) PhotonNetwork.RemoveCallbackTarget(this);
        }

        /// <summary>
        /// Send the chat message to everyone else.
        /// </summary>

        protected override void OnSubmit(string text)
        {
            //OnRaiseEvent((byte)chatEventCode, text, PhotonNetwork.LocalPlayer.ActorNumber);

            Send(text);
        }

        /// <summary>
        /// True when input field is focused.
        /// </summary>
        public static bool IsOpen()
        {
            return mInst != null && mInst.selected; //mInst.input.isFocused;
        }

        public static void Send(string text)
        {
            //mInst.OnRaiseEvent((byte)mInst.chatEventCode, text, PhotonNetwork.LocalPlayer.ActorNumber);

            RaiseEventOptions options = RaiseEventOptions.Default;
            options.Receivers = ReceiverGroup.All;
            PhotonNetwork.RaiseEvent((byte)mInst.chatEventCode, text, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Add a new chat entry.
        /// </summary>
        /// <param name="text"></param>
        public static void Add(string text)
        {
            if(mInst == null)
            {
                Debug.LogWarning("Can't add chat messages there is no RoomChat instance found.");
                return;
            }
            Add(text, mInst.serverColor);
        }

        /// <summary>
        /// Add a new chat entry.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void Add(string text, Color color)
        {
            if (mInst != null) mInst.Add(text, mInst.serverColor, false);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == chatEventCode)
            {
                Player player = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
                if (player == null) return;
                Color color = playerColor;
                string message = (string)photonEvent.CustomData;

                if (message.Contains(S_SPLIT))
                {
                    message = message.Replace(S_SPLIT, string.Empty);
                    color = serverColor;
                }
                else
                {
                    // If the message was not sent by the player, color it differently and play a sound
                    if (player != PhotonNetwork.LocalPlayer)
                    {
                        /*if (player.HasProperty("Faction"))
                        {
                            if ((player.GetString("Faction") as string) == factionAName) color = factionAColor;
                            else if ((player.GetString("Faction") as string) == factionBName) color = factionBColor;
                            else if ((player.GetString("Faction") as string) == factionCName) color = factionCColor;
                        }
                        else
                        {*/
                            color = otherColor;
                        //}
                    }

                    // Embed the player's name into the message
                    message = string.Format("[{0}]: {1}", player.NickName, message);
                }

                Add(message, color, false);

                //if (notificationSound != null)
                //    NGUITools.PlaySound(notificationSound);
            }
        }
        
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            if(notifyWhenPlayersConnect) Add(string.Format("{0} Joined!", newPlayer.NickName), serverColor, false);
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (notifyWhenPlayersConnect) Add(string.Format("{0} Left!", otherPlayer.NickName), serverColor, false);
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
