using UnityEngine;

public class TrainCollisionDetection : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet entrant est un wagon
        if (other.CompareTag("Wagon"))
        {
            // Logique quand un wagon entre dans la zone de la locomotive
            Debug.Log("Un wagon est entré dans la zone de la locomotive : " + other.gameObject.name);
        }
        else
        {
            Debug.LogError($"Collision {other.gameObject.name} / {gameObject.name} ");
        }
    }

}
