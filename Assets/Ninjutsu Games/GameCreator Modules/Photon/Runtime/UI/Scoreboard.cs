#pragma warning disable

//#define USE_TMP

using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;

#if USE_TMP
using TMPro;
#endif

namespace NJG.PUN
{

    public class Scoreboard : MonoBehaviourPunCallbacks
    {
        [System.Serializable]
        public class ScoreEntry
        {
            public Player player;
            public int rank;
            public int score;
        }

        [SerializeField] private int maxEntries = 5;
        [SerializeField] private bool boldLocalPlayer = true;
        [SerializeField] private string entryFormat = "{0}. {1}";
        [SerializeField] private string defaultNameFormat = "Player {0}";
        [SerializeField] private Transform container = null;
        [SerializeField] private GameObject prefab;
        [SerializeField] private List<ScoreEntry> scores = new List<ScoreEntry>(0);

        public static FirstPlayerEvent OnFirstPlayer = new FirstPlayerEvent();
        private const string SCORE = "Score";
        private const string NAME = "Name";
        private const string NO = "n0";

        [System.Serializable]
        public class FirstPlayerEvent : UnityEvent<Player> { }

        private void Start()
        {
            UpdateContent();
        }

        private void UpdateContent()
        {
            if (NetworkManager.Instance == null || NetworkManager.Instance.Players == null) return;

            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            bool playerIncluded = false;
            ScoreEntry playerEntry = null;

            scores.Clear();
            for (int i = 0; i < NetworkManager.Instance.Players.Length; i++)
            {
                var player = NetworkManager.Instance.Players[i];
                int score = player.GetScore();
                ScoreEntry entry = new ScoreEntry() { player = player, score = score, rank = i + 1 };
                if (player.NickName == PhotonNetwork.LocalPlayer.NickName) playerEntry = entry;
                scores.Add(entry);
            }

            scores.Sort((x, y) => y.score.CompareTo(x.score));

            for (int i = 0; i < scores.Count; i++)
            {
                if (i >= maxEntries) break;

                var s = scores[i];

                // Check that the player is part of the top N
                if (s.player.IsLocal) playerIncluded = true;

                // Our player is #1 lets brag about it
                if (i == 0 && OnFirstPlayer != null) OnFirstPlayer.Invoke(s.player);

                // If the player has not been added yet and the last entry is not the player ignore it
                // We will add the player afterwards
                if (i == maxEntries - 1 && !playerIncluded && !s.player.IsLocal) break;

                AddEntry(s, i + 1);
            }

            // Player was not part of the top N lets add it to the list.
            if (!playerIncluded) AddEntry(playerEntry);
        }

        private void AddEntry(ScoreEntry entry, int rank = -1)
        {
            if (entry == null || entry.player == null) return;

            GameObject go = Instantiate(prefab, container, false);
            string nickName = string.IsNullOrEmpty(entry.player.NickName) ? string.Format(defaultNameFormat, entry.player.UserId) : entry.player.NickName;
            go.name = string.Format(entryFormat, rank == -1 ? entry.rank : rank, nickName);

#if USE_TMP
            TextMeshProUGUI scoreLabel = go.transform.Find(SCORE).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameLabel = go.transform.Find(NAME).GetComponent<TextMeshProUGUI>();
#else
            Text scoreLabel = go.transform.Find(SCORE).GetComponent<Text>();
            Text nameLabel = go.transform.Find(NAME).GetComponent<Text>();
#endif

            scoreLabel.text = entry.score.ToString(NO);
            nameLabel.text = string.Format(entryFormat, rank == -1 ? entry.rank : rank, nickName);
#if USE_TMP
            if (boldLocalPlayer && entry.player.IsLocal) nameLabel.fontStyle = FontStyles.Bold;
#else
            if (boldLocalPlayer && entry.player.IsLocal) nameLabel.fontStyle = FontStyle.Bold;
#endif

        }

        public override void OnJoinedRoom()
        {
            UpdateContent();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateContent();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdateContent();
        }

        public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PlayerProperties.PING)) return;

            UpdateContent();
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            //UpdateContent();
        }
    }
}
