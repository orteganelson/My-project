using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInteractiveShaderEffects : MonoBehaviour
{
    [SerializeField]
    RenderTexture rt;

    [SerializeField]
    Transform target; // VR IMPORTANTE: Asegúrate de asignar aquí el Transform raíz
                      // de tu cámara/rig de VR (el objeto que representa la posición
                      // de la cabeza/HMD en el suelo), NO uno de los ojos (Eye Anchor).

    Camera effectCam;

    void Awake()
    {
        // (Código Awake original sin cambios...)
        if (rt == null) { /* ... error ... */ return; }
        if (target == null) { /* ... error ... */ return; }
        effectCam = GetComponent<Camera>();
        if (effectCam == null) { /* ... error ... */ return; }
        if (!effectCam.orthographic) { /* ... warning ... */ }

        Shader.SetGlobalTexture("_GlobalEffectRT", rt);
        Shader.SetGlobalFloat("_OrthographicCamSize", effectCam.orthographicSize);
    }

    private void Update()
    {
        // (Código Update original sin cambios...)
        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
        Shader.SetGlobalVector("_Position", transform.position);
    }
}