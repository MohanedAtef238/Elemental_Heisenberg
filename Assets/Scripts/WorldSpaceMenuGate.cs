using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

[RequireComponent(typeof(UIDocument))]
public class WorldSpaceMenuGate : MonoBehaviour
{
    [SerializeField] private string startButtonName = "StartButton";
    [SerializeField] private string exitButtonName = "ExitButton";
    [SerializeField] private bool hideMenuAfterStart = true;
    [SerializeField] private Transform lockRoot;

    [Header("Interaction Range")]
    [Tooltip("The range of the interaction beam during the menu phase.")]
    [SerializeField] private float menuInteractionRange = 100f;
    [Tooltip("The range of the interaction beam during gameplay.")]
    [SerializeField] private float gameplayInteractionRange = 0.8f;
    [SerializeField] private List<CurveInteractionCaster> interactors = new();

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
        if (_document == null) return;
        
        // Wait for UI to be ready
        if (_document.rootVisualElement == null)
        {
            Invoke(nameof(InitializeMenu), 0.1f);
            return;
        }

        InitializeMenu();
    }

    private void InitializeMenu()
    {
        _root = _document.rootVisualElement;
        if (_root == null) return;

        CacheButtons();
        WireButtons();
        CacheLockedBehaviours();
        ApplyMenuLock(true);
        SetInteractionRange(menuInteractionRange);
        
        Debug.Log($"[MenuGate] Initialized. Menu range set to {menuInteractionRange}");

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
        SetInteractionRange(gameplayInteractionRange);

        if (hideMenuAfterStart)
        {
            _root.style.display = DisplayStyle.None;
        }
    }

    private static void HandleExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    /// <summary>
    /// Adjusts ONLY the ray cast distance on CurveInteractionCasters.
    /// Does NOT modify interactionLayers — that would break grabbing.
    /// World-space UI input routing is handled by PanelInputConfiguration in the scene.
    /// </summary>
    private void SetInteractionRange(float range)
    {
        var allInteractors = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(FindObjectsSortMode.None);
        
        interactors ??= new List<CurveInteractionCaster>();
        interactors.Clear();

        foreach (var interactor in allInteractors)
        {
            if (interactor == null) continue;

            // Only adjust cast distance — do NOT touch interactionLayers
            var caster = interactor.GetComponentInChildren<CurveInteractionCaster>();
            if (caster != null)
            {
                caster.castDistance = range;
                interactors.Add(caster);
                Debug.Log($"[MenuGate] Set castDistance={range} on {interactor.gameObject.name}");
            }
        }
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
