using UnityEngine;

public class SnowButton : MonoBehaviour
{
    [Tooltip("Either assign the ParticleSystem directly, or assign the parent GameObject that contains the ParticleSystem.")]
    public GameObject snowRoot;
    public ParticleSystem snowParticleSystem; // optional, will be fetched if null

    [Tooltip("Offset from the AR/Scene camera to place the emitter (forward, up)")]
    public Vector3 cameraOffset = new Vector3(0f, 1.5f, 2f);

    void Awake()
    {
        // If a root was provided but not the PS, try to find it now.
        if (snowParticleSystem == null && snowRoot != null)
            snowParticleSystem = snowRoot.GetComponentInChildren<ParticleSystem>();
    }

    // Call this from your Button OnClick
    public void SnowOn()
    {
        Debug.Log("[SnowButton] SnowOn called");

        // Try to find particle system if not set
        if (snowParticleSystem == null)
        {
            if (snowRoot != null)
                snowParticleSystem = snowRoot.GetComponentInChildren<ParticleSystem>();

            if (snowParticleSystem == null)
            {
                Debug.LogError("[SnowButton] No ParticleSystem assigned or found in snowRoot!");
                return;
            }
        }

        // Ensure simulation space = World
        var main = snowParticleSystem.main;
        if (main.simulationSpace != ParticleSystemSimulationSpace.World)
        {
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            Debug.Log("[SnowButton] set simulationSpace = World");
        }

        // Find AR/Scene camera (prefer Camera.main)
        Camera cam = Camera.main;
        if (cam == null)
        {
            // fallback: search for typical AR camera names
            var go = GameObject.Find("AR Camera") ?? GameObject.Find("Main Camera");
            if (go != null) cam = go.GetComponent<Camera>();
        }

        if (cam != null)
        {
            // reposition the snow root in front of the camera so it's visible in AR
            if (snowRoot != null)
            {
                snowRoot.transform.position = cam.transform.position + cam.transform.TransformVector(cameraOffset);
                snowRoot.transform.rotation = Quaternion.identity; // keep particles world-aligned
                Debug.Log($"[SnowButton] moved snowRoot to {snowRoot.transform.position}");
            }
            else
            {
                // if only ParticleSystem object was provided
                snowParticleSystem.gameObject.transform.position = cam.transform.position + cam.transform.TransformVector(cameraOffset);
                Debug.Log($"[SnowButton] moved particle to {snowParticleSystem.gameObject.transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("[SnowButton] Camera.main not found — particle will not be moved.");
        }

        // Ensure parent is active and particle system is active
        if (snowRoot != null) snowRoot.SetActive(true);
        else snowParticleSystem.gameObject.SetActive(true);

        // Make sure the layer is Default (so AR camera usually renders it). Change "Default" if your AR camera culling mask uses a different layer.
        int defaultLayer = LayerMask.NameToLayer("Default");
        if (defaultLayer != -1)
        {
            if (snowRoot != null) snowRoot.layer = defaultLayer;
            else snowParticleSystem.gameObject.layer = defaultLayer;
        }

        // Play the particle system
        snowParticleSystem.Play(true);
        Debug.Log("[SnowButton] ParticleSystem.Play() called. isPlaying=" + snowParticleSystem.isPlaying);
    }

    // optional helper to stop
    public void SnowOff()
    {
        if (snowParticleSystem == null && snowRoot != null)
            snowParticleSystem = snowRoot.GetComponentInChildren<ParticleSystem>();
        if (snowParticleSystem == null) return;

        snowParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (snowRoot != null) snowRoot.SetActive(false);
        else snowParticleSystem.gameObject.SetActive(false);
        Debug.Log("[SnowButton] Snow turned off.");
    }
}


