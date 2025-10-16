//CT
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[AddComponentMenu("Audio/Audio Manager")]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AkBank m_akBank;

    [Header("Events")]
    [SerializeField] private List<KeyEventPairs> m_events = new List<KeyEventPairs>();

    [Header("RTPCs")]
    [SerializeField] private List<KeyRTPCPairs> m_rtpcs = new List<KeyRTPCPairs>();
    private Dictionary<string, AK.Wwise.RTPC> m_rtpcDictionary;
    

    [SerializeField] private bool m_playFallbackEvent;
    [SerializeField] private AK.Wwise.Event m_fallbackEvent;

    [Header("Other")]
    [SerializeField] private AK.Wwise.State m_startPause;
    [SerializeField] private AK.Wwise.State m_endPause;

    public enum MusicIntensity { None, SplashScreen, MainMenu, Tutorial, Calm, Action, EndGame };
    private Dictionary<MusicIntensity, AK.Wwise.State> m_stateDictionary = new Dictionary<MusicIntensity, AK.Wwise.State>();
    private Dictionary<string, AK.Wwise.Event> m_soundDictionary;

    [System.Serializable]
    private class MusicState
    {
        public AK.Wwise.State state;
        public MusicIntensity intensity;
    }

    [System.Serializable]
    private class KeyEventPairs
    {
        public string key;
        public AK.Wwise.Event eventReference;
    }

    [System.Serializable]
    private class KeyRTPCPairs
    {
        public string key;
        public AK.Wwise.RTPC RTPC;
    }
    #region Unity Functions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeDictionaries();
    }

    private void Start()
    {
        if (Instance == this)
        {
            m_akBank.enabled = true;
            m_akBank.HandleEvent(gameObject);
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion

    #region Public Functions
    public static void CallEvent(string _eventName, GameObject _gameObject = null)
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.m_soundDictionary.TryGetValue(_eventName, out AK.Wwise.Event eventReference))
        {
            if (eventReference.IsValid())
            {
                GameObject targetObject = _gameObject == null ? Instance.gameObject : _gameObject;

                AkUnitySoundEngine.PostEvent(eventReference.Id, targetObject);
            }
            else
            {
                if (Instance.m_playFallbackEvent == true)
                {
                    AkUnitySoundEngine.PostEvent(Instance.m_fallbackEvent.Id, Instance.gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning($"No event with the name '{_eventName}' was found! Please add it to the list.");
        }
    }

    /// <summary>
    /// Pauses all instances of the specified Wwise event on the provided GameObject, or on the manager GameObject if none is provided.
    /// </summary>
    /// <param name="_eventName">The name of the event to pause.</param>
    /// <param name="_gameObject">Optional GameObject on which to apply the pause action. Defaults to the manager GameObject if null.</param>
    public static void PauseEvent(string _eventName, GameObject _gameObject = null)
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.m_soundDictionary.TryGetValue(_eventName, out AK.Wwise.Event eventReference))
        {
            if (eventReference.IsValid())
            {
                GameObject targetObject = _gameObject == null ? Instance.gameObject : _gameObject;
                AkUnitySoundEngine.ExecuteActionOnEvent(
                    eventReference.Id,
                    AkActionOnEventType.AkActionOnEventType_Pause,
                    targetObject);
            }
            else
            {
                Debug.LogWarning($"Event '{_eventName}' was found but is not valid. Cannot pause.");
            }
        }
        else
        {
            Debug.LogWarning($"No event with the name '{_eventName}' was found! Please add it to the list.");
        }
    }

    /// <summary>
    /// Resumes all instances of the specified Wwise event on the provided GameObject, or on the manager GameObject if none is provided.
    /// </summary>
    /// <param name="_eventName">The name of the event to resume.</param>
    /// <param name="_gameObject">Optional GameObject on which to apply the resume action. Defaults to the manager GameObject if null.</param>
    public static void ResumeEvent(string _eventName, GameObject _gameObject = null)
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.m_soundDictionary.TryGetValue(_eventName, out AK.Wwise.Event eventReference))
        {
            if (eventReference.IsValid())
            {
                GameObject targetObject = _gameObject == null ? Instance.gameObject : _gameObject;
                AkUnitySoundEngine.ExecuteActionOnEvent(
                    eventReference.Id,
                    AkActionOnEventType.AkActionOnEventType_Resume,
                    targetObject);
            }
            else
            {
                Debug.LogWarning($"Event '{_eventName}' was found but is not valid. Cannot resume.");
            }
        }
        else
        {
            Debug.LogWarning($"No event with the name '{_eventName}' was found! Please add it to the list.");
        }
    }

    public static void StopAllEvents(GameObject _gameObject = null)
    {
        if (_gameObject != null)
        {
            AkUnitySoundEngine.StopAll(_gameObject);
        }
        {
            AkUnitySoundEngine.StopAll();
        }
    }
    

    public static void SetSwitch(string _groupName, string _switchName, GameObject _gameObject = null)
    {
        if (_gameObject == null)
        {
            AkUnitySoundEngine.SetSwitch(_groupName, _switchName, Instance.gameObject);
        }
        else
        {
            AkUnitySoundEngine.SetSwitch(_groupName, _switchName, _gameObject);
        }
    }

    public static void SetPauseState(bool _paused)
    {
        if (_paused)
        {
            Instance.m_startPause.SetValue();
        }
        else
        {
            Instance.m_endPause.SetValue();
        }
    }

    /// <summary>
    /// Sets the RTPC value associated with the given key on a specific GameObject or globally if none is provided.
    /// </summary>
    /// <param name="_key">The string key associated with the RTPC.</param>
    /// <param name="_value">The value to set.</param>
    /// <param name="_gameObject">The GameObject to apply the RTPC to, or null for global.</param>
    public static void SetRTPC(string _key, float _value, GameObject _gameObject = null)
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.m_rtpcDictionary.TryGetValue(_key, out AK.Wwise.RTPC rtpcReference))
        {
            if (rtpcReference.IsValid())
            {
                if (_gameObject == null)
                {
                    rtpcReference.SetGlobalValue(_value);
                }
                else
                {
                    rtpcReference.SetValue(_gameObject, _value);
                }
            }
            else
            {
                Debug.LogWarning($"RTPC '{_key}' exists but is not valid.");
            }
        }
        else
        {
            Debug.LogWarning($"No RTPC with the key '{_key}' was found! Please add it to the list.");
        }
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Initializes dictionaries for faster lookups.
    /// </summary>
    private void InitializeDictionaries()
    {
        m_soundDictionary = new Dictionary<string, AK.Wwise.Event>();

        foreach (KeyEventPairs eventReference in m_events)
        {
            if (m_soundDictionary.ContainsKey(eventReference.key) == false)
            {
                m_soundDictionary.Add(eventReference.key, eventReference.eventReference);
            }
            else
            {
                Debug.LogWarning($"Duplicate music key detected: {eventReference.key}. Only the first entry will be used.");
            }
        }

        m_rtpcDictionary = new Dictionary<string, AK.Wwise.RTPC>();

        foreach (KeyRTPCPairs rtpcPair in m_rtpcs)
        {
            if (m_rtpcDictionary.ContainsKey(rtpcPair.key) == false)
            {
                m_rtpcDictionary.Add(rtpcPair.key, rtpcPair.RTPC);
            }
            else
            {
                Debug.LogWarning($"Duplicate RTPC key detected: {rtpcPair.key}. Only the first entry will be used.");
            }
        }

    }
    #endregion
}