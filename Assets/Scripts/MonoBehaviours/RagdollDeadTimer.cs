using UnityEngine;

public class RagdollDeadTimer : MonoBehaviour
{
    private float timer = 5f;
    bool hasColliders = true;

    private void Update()
    {
        timer -= Time.deltaTime;

        if(hasColliders && timer < 3f)
        {
            hasColliders = false;

            foreach(CharacterJoint characterJoint in GetComponentsInChildren<CharacterJoint>())
            {
                Destroy(characterJoint);
            }

            foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
            {
                Destroy(rigidbody);
            }

            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
            }
        }

        if(timer < 1f)
        {
            transform.position += Vector3.down * Time.deltaTime;
        }

        if(timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
