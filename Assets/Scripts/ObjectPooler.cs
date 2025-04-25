using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance; // Singleton para fácil acceso

    public GameObject objectToPool; // El prefab del TillageChunk que creaste
    public int initialPoolSize = 20; // Cuántos objetos crear inicialmente

    private List<GameObject> pooledObjects;

    void Awake()
    {
        // Configurar el Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita instancias duplicadas
            return;
        }
    }

    void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < initialPoolSize; i++)
        {
            tmp = Instantiate(objectToPool); // Crea una instancia del prefab
            tmp.SetActive(false); // Desactívalo inmediatamente
            tmp.transform.SetParent(this.transform); // Opcional: Mantiene la jerarquía limpia
            pooledObjects.Add(tmp); // Añádelo a nuestra lista
        }
    }

    public GameObject GetPooledObject()
    {
        // Busca en la lista un objeto que esté inactivo
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i]; // Devuelve el objeto encontrado
            }
        }

        // Opcional: Si no hay objetos inactivos, puedes decidir crear uno nuevo
        // GameObject tmp = Instantiate(objectToPool);
        // tmp.SetActive(false);
        // tmp.transform.SetParent(this.transform);
        // pooledObjects.Add(tmp);
        // return tmp;
        // O simplemente devolver null si no quieres que el pool crezca dinámicamente
        Debug.LogWarning("Pool agotado para " + objectToPool.name);
        return null; // No se encontraron objetos disponibles
    }

    // Puedes añadir una función para devolver objetos al pool si es necesario,
    // pero a menudo basta con desactivarlos externamente (obj.SetActive(false))
    // public void ReturnObjectToPool(GameObject obj)
    // {
    //     obj.SetActive(false);
    // }
}