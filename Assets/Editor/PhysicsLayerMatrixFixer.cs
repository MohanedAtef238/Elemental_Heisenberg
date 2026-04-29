#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot editor utility: disables physics collision between the
/// 'Interactable' layer and the 'Ignore Raycast' (Player) layer so vials
/// cannot exert physics pushback on the CharacterController.
/// Run via Tools > Fix Physics Layer Matrix.
/// </summary>
public static class PhysicsLayerMatrixFixer
{
    [MenuItem("Tools/Fix Physics Layer Matrix")]
    public static void FixMatrix()
    {
        int interactableLayer = LayerMask.NameToLayer("Interactable");
        int playerLayer       = LayerMask.NameToLayer("Player"); // Layer 9 — XR Rig

        if (interactableLayer == -1)
        {
            Debug.LogError("[PhysicsLayerMatrixFixer] 'Interactable' layer not found. Add it in Project Settings > Tags & Layers first.");
            return;
        }

        // Disable Interactable <-> Player (Ignore Raycast) collision
        Physics.IgnoreLayerCollision(interactableLayer, playerLayer, true);

        // Optionally also disable Interactable <-> Interactable self-collision
        // (prevents vials from pushing each other off shelves before combination).
        // Comment out the line below if you want vials to physically interact.
        // Physics.IgnoreLayerCollision(interactableLayer, interactableLayer, true);

        // Persist to project settings
        PhysicsManagerSerializer.Apply(interactableLayer, playerLayer);

        Debug.Log($"[PhysicsLayerMatrixFixer] ✓ Layer '{LayerMask.LayerToName(interactableLayer)}' ({interactableLayer}) " +
                  $"no longer collides with '{LayerMask.LayerToName(playerLayer)}' ({playerLayer}).");
    }
}

/// <summary>
/// Writes the layer collision matrix change into ProjectSettings/DynamicsManager.asset
/// so it persists across Editor restarts (Physics.IgnoreLayerCollision is runtime-only).
/// </summary>
internal static class PhysicsManagerSerializer
{
    public static void Apply(int layerA, int layerB)
    {
        // Access the internal Physics Manager asset through SerializedObject
        var physicsManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset");
        if (physicsManagerAsset == null || physicsManagerAsset.Length == 0)
        {
            Debug.LogWarning("[PhysicsLayerMatrixFixer] Could not load DynamicsManager.asset — changes are runtime-only this session.");
            return;
        }

        SerializedObject so = new SerializedObject(physicsManagerAsset[0]);
        SerializedProperty matrix = so.FindProperty("m_LayerCollisionMatrix");

        if (matrix == null)
        {
            Debug.LogWarning("[PhysicsLayerMatrixFixer] m_LayerCollisionMatrix not found — changes are runtime-only.");
            return;
        }

        // The matrix is stored as a flat uint array of 32 entries (one per layer).
        // Each entry is a bitmask of which layers that layer collides with.
        // Disable bit: matrix[layerA] &= ~(1u << layerB) and vice-versa.
        int idxA = layerA;
        int idxB = layerB;

        uint valA = (uint)matrix.GetArrayElementAtIndex(idxA).longValue;
        uint valB = (uint)matrix.GetArrayElementAtIndex(idxB).longValue;

        valA &= ~(1u << idxB);
        valB &= ~(1u << idxA);

        matrix.GetArrayElementAtIndex(idxA).longValue = valA;
        matrix.GetArrayElementAtIndex(idxB).longValue = valB;

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        Debug.Log("[PhysicsLayerMatrixFixer] ✓ Physics matrix saved to DynamicsManager.asset.");
    }
}
#endif
