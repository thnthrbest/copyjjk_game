using UnityEngine;

public class DirectionalLightFollow : MonoBehaviour
{
    [Tooltip("Target player to follow. If null, will automatically find the GameObject with the 'Player' tag.")]
    public Transform player;

    [Tooltip("If true, the light will maintain its initial rotation relative to the player at Start.")]
    public bool keepInitialOffset = true;

    [Tooltip("Custom rotation offset if keepInitialOffset is false.")]
    public Vector3 customOffset = new Vector3(50f, -30f, 0f);

    private Quaternion relativeRotation;

    void Start()
    {
        // If player is not assigned, find it by tag
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("DirectionalLightFollow: Player GameObject not found in the scene.");
            }
        }

        // Initialize relative rotation
        if (player != null)
        {
            if (keepInitialOffset)
            {
                // Calculate the initial rotation offset: PlayerRotation * Relative = LightRotation
                // => Relative = PlayerRotation^-1 * LightRotation
                relativeRotation = Quaternion.Inverse(player.rotation) * transform.rotation;
            }
            else
            {
                relativeRotation = Quaternion.Euler(customOffset);
            }
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            // Try to find the player again in case of scene reload or respawn
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (keepInitialOffset)
                {
                    relativeRotation = Quaternion.Inverse(player.rotation) * transform.rotation;
                }
            }
            else
            {
                return;
            }
        }

        // Apply rotation based on player's rotation
        if (keepInitialOffset)
        {
            transform.rotation = player.rotation * relativeRotation;
        }
        else
        {
            transform.rotation = player.rotation * Quaternion.Euler(customOffset);
        }
    }
}
