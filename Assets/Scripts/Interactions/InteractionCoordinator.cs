using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Single source of truth for all alchemical interactions.
    ///
    /// Configure ALL recipes and sub-system references here in the Inspector.
    /// This coordinator owns the recipe list, evaluates ingredient matches,
    /// and delegates every effect to the correct sub-system.
    ///
    /// Flow:
    ///   1. AlchemyZone fires OnIngredientsChanged when vials enter/leave
    ///   2. Coordinator matches present vials against its _recipes list
    ///   3. Match found  → sets prep skybox via AtmosphereController, marks zone as prepped
    ///      Match lost   → resets skybox, marks zone as not prepped
    ///   4. AnvilSurface calls TryExecuteReaction() on a valid hammer strike
    ///   5. Coordinator applies screen tint + audio, then tells AlchemyZone to spawn
    ///
    /// Sub-systems:
    ///   - AlchemyZone          : ingredient tracking, vial destruction, result spawning
    ///   - AtmosphereController : skybox (prep signal + reset)
    ///   - ScreenTintController : URP post-processing tint (on execution)
    /// </summary>
    public class InteractionCoordinator : MonoBehaviour
    {
        [Header("Sub-Systems")]
        [Tooltip("Tracks ingredient vials and spawns results on command.")]
        [SerializeField] private AlchemyZone _alchemyZone;

        [Tooltip("Controls the global skybox material.")]
        [SerializeField] private AtmosphereController _atmosphereController;

        [Tooltip("Controls the URP post-processing screen tint.")]
        [SerializeField] private ScreenTintController _screenTintController;

        [Header("Recipes — Single Source of Truth")]
        [Tooltip("All alchemical recipes. Add every combination here.")]
        [SerializeField] private List<AlchemyReactionRecipe> _recipes = new();

        /// <summary>Read-only access for UI boards and other display scripts.</summary>
        public IReadOnlyList<AlchemyReactionRecipe> Recipes => _recipes;

        private AlchemyReactionRecipe _currentRecipe;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            if (_alchemyZone != null)
                _alchemyZone.OnIngredientsChanged += EvaluateIngredients;
            else
                Debug.LogError("InteractionCoordinator: AlchemyZone is not assigned!");
        }

        private void OnDestroy()
        {
            if (_alchemyZone != null)
                _alchemyZone.OnIngredientsChanged -= EvaluateIngredients;
        }

        /// <summary>
        /// Called whenever vials enter or leave the AlchemyZone.
        /// Re-evaluates the recipe match and updates prep state + skybox.
        /// </summary>
        private void EvaluateIngredients()
        {
            var present = _alchemyZone.GetPresentVialDefinitions();
            _currentRecipe = _recipes.FirstOrDefault(r => r != null && r.Matches(present));

            bool prepped = _currentRecipe != null;
            _alchemyZone.IsPrepped = prepped;

            if (prepped)
            {
                Debug.Log($"InteractionCoordinator: Recipe '{_currentRecipe.label}' matched — zone prepped.");
                _atmosphereController?.SetSkybox(_currentRecipe.prepSkybox);
            }
            else
            {
                Debug.Log("InteractionCoordinator: No matching recipe — zone cleared.");
                _atmosphereController?.ResetSkybox();
                _screenTintController?.ClearTint();
            }
        }

        /// <summary>
        /// Called by AnvilSurface when a valid hammer strike is detected.
        /// </summary>
        public void TryExecuteReaction()
        {
            if (_alchemyZone == null)
            {
                Debug.LogError("InteractionCoordinator: AlchemyZone not assigned!");
                return;
            }

            if (!_alchemyZone.IsPrepped || _currentRecipe == null)
            {
                Debug.Log("InteractionCoordinator: TryExecuteReaction — zone not prepped or no recipe.");
                return;
            }

            ExecuteRecipe(_currentRecipe);
        }

        /// <summary>
        /// Force-executes regardless of prep state. Used by debug shortcuts and ContextMenu.
        /// </summary>
        [ContextMenu("Force Trigger Reaction")]
        public void ForceTriggerReaction()
        {
            if (_alchemyZone == null)
            {
                Debug.LogError("InteractionCoordinator: AlchemyZone not assigned!");
                return;
            }

            Debug.Log("InteractionCoordinator: Force trigger fired.");

            // Attempt a fresh match if nothing is currently set
            if (_currentRecipe == null)
            {
                var present = _alchemyZone.GetPresentVialDefinitions();
                _currentRecipe = _recipes.FirstOrDefault(r => r != null && r.Matches(present));
            }

            if (_currentRecipe != null)
                ExecuteRecipe(_currentRecipe);
            else
                Debug.LogWarning("InteractionCoordinator: Force trigger — no matching recipe found.");
        }

        private void ExecuteRecipe(AlchemyReactionRecipe recipe)
        {
            Debug.Log($"InteractionCoordinator: Executing recipe '{recipe.label}'.");

            // Screen tint
            _screenTintController?.SetTint(recipe.screenTint);

            // Audio
            if (recipe.audioFeedback != null && _audioSource != null)
                _audioSource.PlayOneShot(recipe.audioFeedback);

            // Cache the recipe reference and clear coordinator state before spawn
            // (AlchemyZone.ExecuteSpawn will clear IsPrepped internally)
            var recipeToRun = recipe;
            _currentRecipe = null;

            // Delegate spawning and VFX to AlchemyZone
            _alchemyZone.ExecuteSpawn(recipeToRun);
        }
    }
}
