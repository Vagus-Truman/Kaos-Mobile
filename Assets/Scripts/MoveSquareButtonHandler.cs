
using UnityEngine;

public class MoveSquareButtonHandler : MonoBehaviour
{
    public void MoveSquare()
    {
        NetworkSquareMovement square = FindFirstObjectByType<NetworkSquareMovement>();

        if (square == null)
        {
            Debug.LogWarning(
                "NetworkSquare has not spawned yet."
            );
            return;
        }

        square.RequestMove();
    }
}
