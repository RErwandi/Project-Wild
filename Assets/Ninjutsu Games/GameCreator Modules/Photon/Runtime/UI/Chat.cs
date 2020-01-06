#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY)
#define MOBILE
#endif

using GameCreator.Characters;
using GameCreator.Core.Hooks;
using System;
using System.Collections;
using System.Collections.Generic;
#if USE_TMP
using TMPro;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NJG
{

    /// <summary>
    /// Generic chat window functionality.
    /// </summary>

    public class Chat : MonoBehaviour
    {
        //static Chat mInst;

        public GameObject prefab;

        /// <summary>
        /// Font used by the labels created by the chat window.
        /// </summary>

        //public Font font;

        /// <summary>
        /// Font used by the labels created by the chat window.
        /// </summary>

        //public int fontSize = 22;

        /// <summary>
        /// Input field used for chat input.
        /// </summary>


#if USE_TMP
        public TMP_InputField input;
#else
        public InputField input;
#endif
        public GameObject placeholder;

        /// <summary>
        /// Root object for chat window's history. This allows you to position the chat window's text.
        /// </summary>

        public Transform history;

        /// <summary>
        /// Maximum number of lines kept in the chat window before they start getting removed.
        /// </summary>

        public int maxLines = 10;

        /// <summary>
        /// Maximum width of each line.
        /// </summary>

        //public int lineWidth = 0;

        /// <summary>
        /// Seconds that must elapse before a chat label starts to fade out.
        /// </summary>

        public float fadeOutStart = 10f;

        /// <summary>
        /// How long it takes for a chat label to fade out in seconds.
        /// </summary>

        public float fadeOutDuration = 5f;

        /// <summary>
        /// Whether messages will fade out over time.
        /// </summary>

        public bool allowChatFading = true;

        /// <summary>
        /// Whether the activate the chat input when Return key gets pressed.
        /// </summary>

        public bool activateOnReturnKey = true;

        //IsFocused has 1 frame delay.
        [HideInInspector] public bool selected;

        public bool disableInputOnSubmit = false;
        public bool disablePlayerWhenTyping = true;

        public string defaultText = "Chat";
        public string defaultMobile = "ChatMobile";
        private const char SPACE_CHAR = '\n';

        private static WaitForEndOfFrame WFEOF = new WaitForEndOfFrame();

        private class ChatEntry
        {
            public CanvasGroup group;
            public Transform transform;
#if USE_TMP
            public TextMeshProUGUI label;
#else
            public Text label;
#endif
            public Color color;
            public float time;
            public int lines = 0;
            public float alpha = 0f;
            public bool isExpired = false;
            public bool shouldBeDestroyed = false;
            public bool fadedIn = false;
        }

        private List<ChatEntry> mChatEntries = new List<ChatEntry>();
        //private int mBackgroundHeight = -1;
        private bool mIgnoreNextEnter = false;

        /// <summary>
        /// For things you want to do after OnSubmitInternal method has ran.
        /// </summary>
        public UnityEvent LateEndEdit = new UnityEvent();

        protected virtual void Awake()
        {
            //mInst = this;

            if (input != null)
            {
#if USE_TMP
                input.onSelect.AddListener(s => Select());
                input.onDeselect.AddListener(s => UnSelect());
#endif
                input.onValueChanged.AddListener(OnValueChanged);
                input.onEndEdit.AddListener(OnSubmitInternal);
            }
        }

        private void OnDestroy()
        {
            if (input != null)
            {
#if USE_TMP
                input.onSelect.RemoveListener(s => Select());
                input.onDeselect.RemoveListener(s => UnSelect());
#endif
                input.onValueChanged.RemoveListener(OnValueChanged);
                input.onEndEdit.RemoveListener(OnSubmitInternal);
            }
        }

        private void OnValueChanged(string arg0)
        {
            if (disablePlayerWhenTyping && HookPlayer.Instance && HookPlayer.Instance.Get<PlayerCharacter>().IsControllable())
            {
                HookPlayer.Instance.Get<PlayerCharacter>().characterLocomotion.SetIsControllable(false);
            }
        }

        public void Select()
        {
            selected = true;
            allowChatFading = false;
            if (disablePlayerWhenTyping && HookPlayer.Instance && HookPlayer.Instance.Get<PlayerCharacter>().IsControllable())
            {
                HookPlayer.Instance.Get<PlayerCharacter>().characterLocomotion.SetIsControllable(false);
            }
            if (placeholder) placeholder.SetActive(false);
        }

        public void UnSelect()
        {
            selected = false;
            allowChatFading = true;
            if (disablePlayerWhenTyping && HookPlayer.Instance && !HookPlayer.Instance.Get<PlayerCharacter>().IsControllable())
            {
                HookPlayer.Instance.Get<PlayerCharacter>().characterLocomotion.SetIsControllable(true);
            }
        }

        /// <summary>
        /// Handle inputfield onEndEdit event.
        /// </summary>

        public void OnSubmitInternal(string content)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                input.text = string.Empty;

                if (!string.IsNullOrWhiteSpace(content))
                    OnSubmit(content);

                if (disableInputOnSubmit) input.interactable = false;
                if (placeholder) placeholder.SetActive(true);

                mIgnoreNextEnter = true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    input.text = string.Empty;
                    
                    if (Input.GetMouseButtonDown(0)) //Workaround for mouse click triggering EventSystem.current.alreadySelecting for some reason
                    {
                        if (disableInputOnSubmit) StartCoroutine(nameof(SetInputFieldNotInteractableAtEndOfFrame));
                    }
                    else
                    {
                        if (disableInputOnSubmit) input.interactable = false;
                    }
                    if (placeholder) placeholder.SetActive(true);
                }
            }

            input.DeactivateInputField();
            if (!EventSystem.current.alreadySelecting) EventSystem.current.SetSelectedGameObject(null, null);
            if (LateEndEdit != null) LateEndEdit.Invoke();

            if (disablePlayerWhenTyping && HookPlayer.Instance)
            {
                HookPlayer.Instance.Get<PlayerCharacter>().characterLocomotion.SetIsControllable(true);
            }
        }

        private IEnumerator SetInputFieldNotInteractableAtEndOfFrame()
        {
            yield return WFEOF;
            input.interactable = false;
        }

        public void ClearHistory()
        {
            for (int i = 0, imax = mChatEntries.Count; i< imax; i++)
            {
                ChatEntry e = mChatEntries[i];
                mChatEntries.RemoveAt(i);
                Destroy(e.label.gameObject);
            }
        }

        /// <summary>
        /// Custom submit logic for what happens on chat input submission.
        /// </summary>

        protected virtual void OnSubmit(string text)
        {
        }

        /// <summary>
        /// Add a new chat entry.
        /// </summary>

        private GameObject InternalAdd(string text, Color color, bool tintBackground)
        {
            ChatEntry ent = new ChatEntry();
            ent.time = Time.unscaledTime;
            ent.color = color;
            mChatEntries.Add(ent);

            GameObject go = Instantiate(prefab, history != null ? history : transform, false) as GameObject;
            go.SetActive(true);
            ent.group = go.GetComponent<CanvasGroup>();

#if USE_TMP
            ent.label = go.GetComponentInChildren<TextMeshProUGUI>();
#else
            ent.label = go.GetComponentInChildren<Text>();
#endif
            ent.transform = go.transform;
            //ent.label.pivot = UIWidget.Pivot.BottomLeft;
            ent.transform.localScale = new Vector3(1f, 0.001f, 1f);
            //ent.label.transform.localPosition = Vector3.zero;
            //ent.label.width = lineWidth;
            //ent.label.bitmapFont = font;
            //ent.label.fontSize = fontSize;

            //ent.label.color = ent.label.bitmapFont.premultipliedAlphaShader ? new Color(0f, 0f, 0f, 0f) : new Color(color.r, color.g, color.b, 0f);
            if (tintBackground)
            {
                go.GetComponent<Image>().color = color;
                ent.label.text = text;
            }
            else
            {
                ent.label.color = color;
                ent.label.text = text;
            }
            //else ent.label.text = "<color=#" + EncodeColor32(color) + ">" + text + "</color>";
            //ent.label.overflowMethod = UILabel.Overflow.ResizeHeight;
            ent.lines = ent.label.text.Split(SPACE_CHAR).Length;

            for (int i = mChatEntries.Count, lineOffset = 0; i > 0;)
            {
                ChatEntry e = mChatEntries[--i];

                if (i + 1 == mChatEntries.Count)
                {
                    // It's the first entry. It doesn't need to be re-positioned.
                    lineOffset += e.lines;
                }
                else
                {
                    // This is not a first entry. It should be tweened into its proper place.
                    //int pixelOffset = lineOffset * (int)e.label.rectTransform.sizeDelta.y;

                    if (lineOffset + e.lines > maxLines && maxLines > 0)
                    {
                        e.isExpired = true;
                        e.shouldBeDestroyed = true;

                        if (e.alpha == 0f)
                        {
                            mChatEntries.RemoveAt(i);
                            Destroy(e.label.gameObject);
                            continue;
                        }
                    }
                    lineOffset += e.lines;
                    //e.label.transform.DOLocalMove(new Vector3(0f, pixelOffset, 0f), 0.2f);
                    //TweenPosition.Begin(e.label.gameObject, 0.2f, new Vector3(0f, pixelOffset, 0f));
                }
            }

            return go;
        }

        /// <summary>
        /// Update the "alpha" of each line and update the background size.
        /// </summary>

        protected virtual void Update()
        {
            if (activateOnReturnKey && (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter)))
            {
                if (!mIgnoreNextEnter)
                {
                    input.interactable = true;
                    input.Select();
                    input.ActivateInputField();

                    EventSystem.current.SetSelectedGameObject(input.gameObject, null);
                    input.OnPointerClick(new PointerEventData(EventSystem.current));
                }
                mIgnoreNextEnter = false;
            }

            int height = 0;

            for (int i = 0; i < mChatEntries.Count;)
            {
                ChatEntry e = mChatEntries[i];
                float alpha;

                if (e.isExpired)
                {
                    // Quickly fade out expired chat entries
                    alpha = Mathf.Clamp01(e.alpha - Time.unscaledDeltaTime);
                }
                else if (!allowChatFading || Time.unscaledTime - e.time < fadeOutStart)
                {
                    // Quickly fade in new entries
                    alpha = Mathf.Clamp01(e.alpha + Time.unscaledDeltaTime*5f);
                }
                else if (Time.unscaledTime - (e.time + fadeOutStart) < fadeOutDuration)
                {
                    // Slowly fade out entries that have been visible for a while
                    alpha = Mathf.Clamp01(e.alpha - Time.unscaledDeltaTime/fadeOutDuration);
                }
                else
                {
                    // Quickly fade out chat entries that should have faded by now,
                    // but likely didn't due to the input being selected.
                    alpha = Mathf.Clamp01(e.alpha - Time.unscaledDeltaTime);
                }

                if (e.alpha != alpha)
                {
                    e.alpha = alpha;

                    if (!e.fadedIn && !e.isExpired)
                    {
                        // This label has not yet faded in, we want to scale it in, 
                        // as it looks better and goes well with the scaled background.
                        float labelHeight = Mathf.Lerp(0.001f, 1f, e.alpha);
                        e.transform.localScale = new Vector3(1f, labelHeight, 1f);
                    }

                    // Fade in or fade out the label
                    //if (e.label.bitmapFont != null && e.label.bitmapFont.premultipliedAlphaShader)
                    //{
                    //    e.label.color = Color.Lerp(new Color(0f, 0f, 0f, 0f), e.color, e.alpha);
                    /*}
                else
                {
                    e.label.alpha = e.alpha;
                }*/
                    e.group.alpha = e.alpha;

                    if (alpha == 1f)
                    {
                        // The chat entry has faded in fully
                        e.fadedIn = true;
                    }
                    else if (alpha == 0f && e.shouldBeDestroyed)
                    {
                        // This chat entry has expired and should be removed
                        mChatEntries.RemoveAt(i);
                        Destroy(e.label.gameObject);
                        continue;
                    }
                }

                // If the line is visible, it should be counted
                if (e.alpha > 0.01f) height += e.lines*(int) e.label.rectTransform.sizeDelta.y;
                ++i;
            }

            // Resize the background if its height has changed
            //if (background != null && mBackgroundHeight != height)
            //    ResizeBackground(height, !allowChatFading);
        }

        /// <summary>
        /// Resize the background to fit the specified height in pixels.
        /// </summary>

        /*void ResizeBackground(int height, bool instant)
    {
        mBackgroundHeight = height;

        if (height == 0)
        {
            if (instant)
            {
                background.height = 2;
                background.enabled = false;
            }
            else
            {
                UITweener tween = TweenHeight.Begin(background, 0.2f, 2);
                EventDelegate.Add(tween.onFinished, DisableBackground, true);
            }
        }
        else
        {
            background.enabled = true;

            if (instant)
            {
                UITweener tween = background.GetComponent<TweenScale>();
                if (tween != null) tween.enabled = false;
                background.height = height + backgroundPadding;
            }
            else TweenHeight.Begin(background, 0.2f, height + backgroundPadding);
        }
    }

    /// <summary>
    /// When the background resizing tween finishes, disable the background.
    /// </summary>

    public void DisableBackground() { background.enabled = false; }*/

        /// <summary>
        /// Add a new chat entry.
        /// </summary>

        //static public void Add (string text) { Add(text, new Color(0.7f, 0.7f, 0.7f, 1f)); }

        /// <summary>
        /// Add a new chat entry.
        /// </summary>

        public GameObject Add(string text, Color color, bool tintBackground)
        {
            return InternalAdd(text, color, tintBackground);
        }

    }
}
