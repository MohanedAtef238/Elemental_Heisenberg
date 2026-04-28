using UnityEditor;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class XRFixup : EditorWindow
{
    [MenuItem("Tools/XR/Fix Locomotion Links")]
    public static void FixLocomotion()
    {
        XROrigin origin = Object.FindFirstObjectByType<XROrigin>();
        LocomotionMediator mediator = Object.FindFirstObjectByType<LocomotionMediator>();
        LocomotionSystem system = Object.FindFirstObjectByType<LocomotionSystem>();
        XRBodyTransformer transformer = Object.FindFirstObjectByType<XRBodyTransformer>();
        TeleportationProvider teleportProvider = Object.FindFirstObjectByType<TeleportationProvider>();

        if (origin == null) { Debug.LogError("XROrigin not found."); return; }

        if (mediator != null)
        {
            SerializedObject so = new SerializedObject(mediator);
            var p1 = so.FindProperty("m_XROrigin"); if(p1!=null) p1.objectReferenceValue = origin;
            var p2 = so.FindProperty("m_BodyTransformer"); if(p2!=null) p2.objectReferenceValue = transformer;
            so.ApplyModifiedProperties();
        }

        if (system != null)
        {
            SerializedObject so = new SerializedObject(system);
            var p = so.FindProperty("m_XROrigin"); if(p!=null) p.objectReferenceValue = origin;
            so.ApplyModifiedProperties();
        }

        if (teleportProvider != null)
        {
            SerializedObject so = new SerializedObject(teleportProvider);
            var p1 = so.FindProperty("m_System"); if(p1!=null) p1.objectReferenceValue = system;
            var p2 = so.FindProperty("m_Mediator"); if(p2!=null) p2.objectReferenceValue = mediator;
            so.ApplyModifiedProperties();
        }
        
        Debug.Log("XR Locomotion links fixed.");
    }
}
