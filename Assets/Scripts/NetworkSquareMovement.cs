
using UnityEngine;
using Unity.Netcode;

public class NetworkSquareMovement : NetworkBehaviour
{
    public void RequestMove()
    {
        if (!IsSpawned)
            return;

        MoveSquareRpc();
    }

    [Rpc(SendTo.Server)]
    private void MoveSquareRpc()
    {
        transform.position += Vector3.right;
    }
}
