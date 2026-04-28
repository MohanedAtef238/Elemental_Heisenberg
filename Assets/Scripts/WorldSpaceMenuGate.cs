using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class WorldSpaceMenuGate : MonoBehaviour
{
    [SerializeField] private string startButtonName = "StartButton";
    [SerializeField] private string exitButtonName = "ExitButton";
    [SerializeField] private bool hideMenuAfterStart = true;
    [SerializeField] private Transform lockRoot;

    private UIDocument _document;
    private VisualElement _root;
    private Button _startButton;
    private Button _exitButton;
    private readonly List<BehaviourState> _lockedBehaviours = new();
    private bool _started;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
    }

    private void Start()
    {
        _root = _document.rootVisualElement;

        CacheButtons();
        WireButtons();
        CacheLockedBehaviours();
        ApplyMenuLock(true);

        _startButton?.Focus();
    }

    private void CacheButtons()
    {
        _startButton = _root.Q<Button>(startButtonName);
        _exitButton = _root.Q<Button>(exitButtonName);

        if (_startButton == null)
        {
            foreach (var button in _root.Query<Button>().Build())
            {
                if (button.text == "Start")
                {
                    _startButton = button;
                    break;
                }
            }
        }

        if (_exitButton == null)
        {
            foreach (var button in _root.Query<Button>().Build())
            {
                if (button.text == "Exit")
                {
                    _exitButton = button;
                    break;
                }
            }
        }
    }

    private void WireButtons()
    {
        if (_startButton != null)
        {
            _startButton.clicked += HandleStartClicked;
        }

        if (_exitButton != null)
        {
            _exitButton.clicked += HandleExitClicked;
        }
    }

    private void CacheLockedBehaviours()
    {
        var rigRoot = ResolveLockRoot();
        if (rigRoot == null)
        {
            return;
        }

        foreach (var behaviour in rigRoot.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour == null || behaviour == this)
            {
                continue;
            }

            if (!ShouldLockDuringMenu(behaviour))
            {
                continue;
            }

            _lockedBehaviours.Add(new BehaviourState(behaviour, behaviour.enabled));
        }
    }

    private Transform ResolveLockRoot()
    {
        if (lockRoot != null)
        {
            return lockRoot;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform.root;
        }

        return transform.root;
    }

    private static bool ShouldLockDuringMenu(Behaviour behaviour)
    {
        var type = behaviour.GetType();
        var fullName = type.FullName ?? string.Empty;
        var name = type.Name;

        if (fullName.Contains(".Locomotion."))
        {
            return true;
        }

        if (name.Contains("Locomotion") || name.Contains("Teleport") || name.Contains("Turn") || name.Contains("Move"))
        {
            return true;
        }

        // Starter Assets controller helpers hold these action references and can still drive
        // movement / teleport state even if only provider components are disabled.
        if (HasAnyField(type,
                "m_Move",
                "m_Turn",
                "m_SnapTurn",
                "m_TeleportMode",
                "m_TeleportModeCancel",
                "m_UIScroll"))
        {
            return true;
        }

        return name is "CharacterControllerDriver"
            or "ActionBasedContinuousMoveProvider"
            or "ContinuousMoveProviderBase"
            or "ActionBasedContinuousTurnProvider"
            or "ContinuousTurnProviderBase"
            or "ActionBasedSnapTurnProvider"
            or "SnapTurnProviderBase"
            or "TeleportationProvider"
            or "GrabMoveProvider"
            or "TwoHandedGrabMoveProvider"
            or "ClimbProvider";
    }

    private static bool HasAnyField(Type type, params string[] fieldNames)
    {
        foreach (var fieldName in fieldNames)
        {
            if (type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) != null)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyMenuLock(bool locked)
    {
        foreach (var state in _lockedBehaviours)
        {
            if (state.Behaviour == null)
            {
                continue;
            }

            state.Behaviour.enabled = locked ? false : state.WasInitiallyEnabled;
        }
    }

    private void HandleStartClicked()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        ApplyMenuLock(false);

        if (hideMenuAfterStart)
        {
            _root.style.display = DisplayStyle.None;
        }
    }

    private static void HandleExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private readonly struct BehaviourState
    {
        public BehaviourState(Behaviour behaviour, bool wasInitiallyEnabled)
        {
            Behaviour = behaviour;
            WasInitiallyEnabled = wasInitiallyEnabled;
        }

        public Behaviour Behaviour { get; }
        public bool WasInitiallyEnabled { get; }
    }
}
