using UnityEngine;

public class AradoParticula : MonoBehaviour
{
    public ParticleSystem tierraParticulas;
    public string tagTerreno = "Tierra";

    private bool enContacto = false;
    private Vector3 ultimaPosicion;
    private float umbralMovimiento = 0.01f;

    void Start()
    {
        if (tierraParticulas == null)
            tierraParticulas = GetComponentInChildren<ParticleSystem>();

        ultimaPosicion = transform.position;
        tierraParticulas.Stop();
    }

    void Update()
    {
        float distancia = Vector3.Distance(transform.position, ultimaPosicion);

        if (enContacto && distancia > umbralMovimiento)
        {
            if (!tierraParticulas.isPlaying)
                tierraParticulas.Play();
        }
        else
        {
            if (tierraParticulas.isPlaying)
                tierraParticulas.Stop();
        }

        ultimaPosicion = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(tagTerreno))
        {
            enContacto = true;
            Debug.Log("Contacto con la tierra");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag(tagTerreno))
            enContacto = false;
    }
}