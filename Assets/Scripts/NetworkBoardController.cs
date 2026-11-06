using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkBoardController : NetworkBehaviour
{
    [SerializeField]
    private HexGridManagement hexGrid;

    [SerializeField]
    private DeityInfoSelection deityInfo;

    [Header("Host-Only UI Configuration")]
    [SerializeField]
    private GameObject[] hideOnHost;

    private readonly Dictionary<ulong, DeityInfoSelection.PlayerType> playerRoles = new();

    private readonly Dictionary<ulong, int> playerDomains = new();

    private readonly Dictionary<ulong, int> playerRoleIndices = new();

    private readonly List<int> sacredHexes = new();
    private readonly List<int> blightedHexes = new();

    private readonly Dictionary<ulong, List<int>> playerSacredInfo = new();

    private readonly Dictionary<ulong, List<int>> playerBlightedInfo = new();

    private bool boardLayoutGenerated = false;

    private int localRoleIndex = -1;
    private int localDomainIndex = -1;

    private bool assignmentsSent = false;

    private readonly List<int> localSacredInfo = new();
    private readonly List<int> localBlightedInfo = new();

    private bool informationFragmentReceived = false;


    // One bit per hexagon: 0 = available, 1 = occupied.
    private readonly NetworkVariable<uint> occupiedHexes =
        new NetworkVariable<uint>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private readonly NetworkVariable<ulong> activePlayerId =
    new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Public properties for checking the current turn.
    public ulong ActivePlayerId => activePlayerId.Value;

    public bool IsMyTurn =>
        IsSpawned &&
        NetworkManager != null &&
        NetworkManager.IsClient &&
        !IsServer &&
        NetworkManager.LocalClientId == activePlayerId.Value;

    private void Update()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsServer &&
            Keyboard.current != null &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            Debug.Log(
                "Connected clients: " +
                NetworkManager.Singleton.ConnectedClientsIds.Count
            );
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.SceneManager.OnSceneEvent += OnNetworkSceneEvent;
        }

        occupiedHexes.OnValueChanged += OnBoardChanged;

        // Display the current board state when this object spawns.
        RefreshBoardVisuals(occupiedHexes.Value);

        Debug.Log(
            "Network board ready. Occupied hexes: " +
            occupiedHexes.Value
        );


        activePlayerId.OnValueChanged += OnActivePlayerChanged;

        if (IsServer)
        {
            foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
            {
                if (clientId == NetworkManager.ServerClientId)
                    continue;

                if (activePlayerId.Value == ulong.MaxValue ||
                    clientId < activePlayerId.Value)
                {
                    activePlayerId.Value = clientId;
                }
            }

            Debug.Log("Starting player ID: " + activePlayerId.Value);

            GeneratePlayerAssignments();
            GenerateBoardLayout();
            GenerateInformationFragments();
        }

        RefreshTurnInteraction();
        ConfigureHostUI();

        Debug.Log("Network board ready. Occupied hexes: " + occupiedHexes.Value);
    }


    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null && NetworkManager.SceneManager != null)
        {
            NetworkManager.SceneManager.OnSceneEvent -= OnNetworkSceneEvent;
        }

        occupiedHexes.OnValueChanged -= OnBoardChanged;
        activePlayerId.OnValueChanged -= OnActivePlayerChanged;
    }

    public void RequestPlacement(int hexId)
    {
        if (!IsSpawned)
        {
            Debug.LogWarning("Network board is not ready.");
            return;
        }

        SubmitPlacementRpc(hexId);
    }

    [Rpc(SendTo.Server)]
    private void SubmitPlacementRpc(
        int hexId,
        RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        // The Host controls the game but does not play.
        if (senderId == NetworkManager.ServerClientId)
        {
            Debug.LogWarning("Host cannot place tiles.");
            return;
        }

        if (senderId != activePlayerId.Value)
        {
            Debug.LogWarning(
                "Placement rejected. Player " + senderId +
                " attempted to play during player " +
                activePlayerId.Value + "'s turn."
            );

            return;
        }

        // Validate the requested position.
        if (hexGrid == null ||
            hexId < 0 ||
            hexId >= hexGrid.tileDesignation.Length ||
            hexId >= 32)
        {
            Debug.LogWarning("Invalid hexagon ID: " + hexId);
            return;
        }

        uint hexMask = 1u << hexId;

        // Reject placements on occupied hexagons.
        if ((occupiedHexes.Value & hexMask) != 0)
        {
            Debug.LogWarning("Hexagon already occupied: " + hexId);
            return;
        }

        occupiedHexes.Value |= hexMask;

        Debug.Log(
            "Player " + senderId +
            " placed on hexagon " + hexId
        );

        AdvanceTurn();
    }

    private void OnBoardChanged(uint previous, uint current)
    {
        RefreshBoardVisuals(current);
        RefreshTurnInteraction();

        Debug.Log("Synchronized board state: " + current);
    }

    private void RefreshBoardVisuals(uint boardState)
    {
        if (hexGrid == null)
        {
            Debug.LogWarning("HexGridManagement reference is missing.");
            return;
        }

        for (int i = 0; i < hexGrid.tileDesignation.Length && i < 32; i++)
        {
            bool occupied = (boardState & (1u << i)) != 0;

            HexTile tile = hexGrid.tileDesignation[i];

            if (tile != null)
            {
                tile.SetOccupiedVisual(occupied);
            }
        }
    }

    private void RefreshTurnInteraction()
    {
        if (hexGrid == null)
            return;

        bool canInteract = IsMyTurn;

        foreach (HexTile tile in hexGrid.tileDesignation)
        {
            if (tile == null || tile.buttonRef == null)
                continue;

            // Only the active player can select unoccupied hexagons.
            tile.buttonRef.interactable = canInteract && !tile.selected;
        }
    }
    private void ConfigureHostUI()
    {
        // Only the Host should hide player-specific UI.
        if (!IsServer)
            return;

        if (hideOnHost == null)
            return;

        foreach (GameObject uiObject in hideOnHost)
        {
            if (uiObject != null)
            {
                uiObject.SetActive(false);
            }
        }

        Debug.Log("Host-only board UI configured.");
    }

    private void OnActivePlayerChanged(ulong previous, ulong current)
    {
        Debug.Log("Active turn changed to player: " + current);

        RefreshTurnInteraction();
    }

    private void AdvanceTurn()
    {
        if (!IsServer)
            return;

        ulong currentPlayer = activePlayerId.Value;
        ulong nextPlayer = ulong.MaxValue;
        ulong firstPlayer = ulong.MaxValue;

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            // The Host is not a player.
            if (clientId == NetworkManager.ServerClientId)
                continue;

            // Find the first connected player.
            if (clientId < firstPlayer)
                firstPlayer = clientId;

            // Find the next player after the current one.
            if (clientId > currentPlayer && clientId < nextPlayer)
                nextPlayer = clientId;
        }

        // Return to the first player at the end of a round.
        if (nextPlayer == ulong.MaxValue)
            nextPlayer = firstPlayer;

        activePlayerId.Value = nextPlayer;

        Debug.Log("Next player's turn: " + nextPlayer);
    }

    private void GeneratePlayerAssignments()
    {
        if (!IsServer || deityInfo == null)
            return;

        List<ulong> players = new List<ulong>();

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            // Exclude the non-playing Host.
            if (clientId != NetworkManager.ServerClientId)
                players.Add(clientId);
        }

        players.Sort();

        if (players.Count != 5 ||
            deityInfo.deityInfoArray == null ||
            deityInfo.deityInfoArray.Length != 5)
        {
            Debug.LogWarning(
                "Assignments require five players and five domains."
            );
            return;
        }

        if (deityInfo.playerTypesArray == null ||
    deityInfo.playerTypesArray.Length != 5)
        {
            Debug.LogError("Exactly five role definitions are required.");
            return;
        }

        List<int> roleIndices = new List<int> { 0, 1, 2, 3, 4 };

        for (int i = roleIndices.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            int temp = roleIndices[i];
            roleIndices[i] = roleIndices[randomIndex];
            roleIndices[randomIndex] = temp;
        }

        // Shuffle the five domain indices.
        List<int> domains = new List<int> { 0, 1, 2, 3, 4 };

        for (int i = domains.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            int temp = domains[i];
            domains[i] = domains[randomIndex];
            domains[randomIndex] = temp;
        }

        playerRoles.Clear();
        playerDomains.Clear();
        playerRoleIndices.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            int roleIndex = roleIndices[i];

            playerRoles.Add(
                players[i],
                deityInfo.playerTypesArray[roleIndex]
            );

            playerRoleIndices.Add(players[i], roleIndex);
            playerDomains.Add(players[i], domains[i]);
        }

        Debug.Log(
            "Host generated private assignments for " +
            players.Count + " players."
        );
    }
    private void GenerateBoardLayout()
    {
        if (!IsServer || boardLayoutGenerated)
            return;

        if (hexGrid == null ||
            hexGrid.tileDesignation == null ||
            hexGrid.tileDesignation.Length != 19)
        {
            Debug.LogError(
                "Board generation requires exactly 19 hexagons."
            );
            return;
        }

        List<int> positions = new();

        for (int i = 0; i < 19; i++)
            positions.Add(i);

        // Shuffle all 19 board positions.
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            int temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }

        sacredHexes.Clear();
        blightedHexes.Clear();

        // First five shuffled positions become Sacred.
        for (int i = 0; i < 5; i++)
            sacredHexes.Add(positions[i]);

        // Next five positions become Blighted.
        for (int i = 5; i < 10; i++)
            blightedHexes.Add(positions[i]);

        // Remaining nine positions are Neutral.
        boardLayoutGenerated = true;

        Debug.Log(
            "Host generated board layout: " +
            "5 Sacred, 5 Blighted, 9 Neutral."
        );
    }

    private void GenerateInformationFragments()
    {
        if (!IsServer || !boardLayoutGenerated)
            return;

        if (playerRoles.Count != 5 ||
            sacredHexes.Count != 5 ||
            blightedHexes.Count != 5)
        {
            Debug.LogWarning(
                "Cannot generate information fragments: setup incomplete."
            );
            return;
        }

        playerSacredInfo.Clear();
        playerBlightedInfo.Clear();

        foreach (var entry in playerRoles)
        {
            ulong clientId = entry.Key;
            var role = entry.Value.PlayerRoleType;

            if (role == DeityInfoSelection.PlayerRoleTypes.Trickster)
            {
                // Tricksters know every special location.
                playerSacredInfo[clientId] =
                    new List<int>(sacredHexes);

                playerBlightedInfo[clientId] =
                    new List<int>(blightedHexes);
            }
            else
            {
                // Creators receive only a partial view.
                List<int> sacredOptions =
                    new List<int>(sacredHexes);

                List<int> blightedOptions =
                    new List<int>(blightedHexes);

                ShufflePositions(sacredOptions);
                ShufflePositions(blightedOptions);

                playerSacredInfo[clientId] =
                    sacredOptions.GetRange(0, 3);

                playerBlightedInfo[clientId] =
                    blightedOptions.GetRange(0, 1);
            }
        }

        Debug.Log(
            "Host generated private information fragments for " +
            playerSacredInfo.Count + " players."
        );
    }

    private void ShufflePositions(List<int> positions)
    {
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            int temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }
    }

    private void SendPrivateAssignments(SceneEvent sceneEvent)
    {
        if (!IsServer || !IsSpawned || assignmentsSent)
            return;

        if (playerRoleIndices.Count != 5 ||
            playerDomains.Count != 5 ||
            NetworkManager.ConnectedClientsIds.Count != 6)
        {
            Debug.LogWarning(
                "Private assignments not sent: expected five players."
            );
            return;
        }

        if (sceneEvent.ClientsThatTimedOut != null &&
            sceneEvent.ClientsThatTimedOut.Count > 0)
        {
            Debug.LogWarning(
                "Private assignments not sent: scene loading timed out."
            );
            return;
        }

        // Verify that every assigned player finished loading.
        foreach (ulong clientId in playerRoleIndices.Keys)
        {
            if (!NetworkManager.ConnectedClientsIds.Contains(clientId) ||
                sceneEvent.ClientsThatCompleted == null ||
                !sceneEvent.ClientsThatCompleted.Contains(clientId) ||
                !playerDomains.ContainsKey(clientId))
            {
                Debug.LogWarning(
                    "Private assignments not sent: player not ready."
                );
                return;
            }
        }

        if (playerSacredInfo.Count != 5 ||
            playerBlightedInfo.Count != 5)
        {
            Debug.LogWarning(
                "Private information fragments are not ready."
            );
            return;
        }

        foreach (ulong clientId in playerRoleIndices.Keys)
        {
            if (!playerSacredInfo.ContainsKey(clientId) ||
                !playerBlightedInfo.ContainsKey(clientId))
            {
                Debug.LogWarning(
                    "Missing private fragment for player " + clientId
                );
                return;
            }
        }

        assignmentsSent = true;

        // Deliver each assignment only to its intended player.

        foreach (ulong clientId in playerRoleIndices.Keys)
        {
            // Target this specific player.
            var target = RpcTarget.Single(
                clientId,
                RpcTargetUse.Temp
            );

            // Send the assigned role and domain first.
            ReceivePrivateAssignmentRpc(
                playerRoleIndices[clientId],
                playerDomains[clientId],
                target
            );

            // Send only this player's information fragment.
            ReceiveInformationFragmentRpc(
                playerSacredInfo[clientId].ToArray(),
                playerBlightedInfo[clientId].ToArray(),
                target
            );
        }

        Debug.Log("Private assignments sent to all five players.");
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ReceivePrivateAssignmentRpc(int roleIndex, int domainIndex, RpcParams rpcParams = default)
    {
        // The non-playing Host must not receive an assignment.
        if (IsServer)
            return;

        if (deityInfo == null ||
            deityInfo.playerTypesArray == null ||
            deityInfo.deityInfoArray == null ||
            roleIndex < 0 ||
            roleIndex >= deityInfo.playerTypesArray.Length ||
            domainIndex < 0 ||
            domainIndex >= deityInfo.deityInfoArray.Length)
        {
            Debug.LogError("Invalid private assignment received.");
            return;
        }

        localRoleIndex = roleIndex;
        localDomainIndex = domainIndex;

        // Retrieve the player's assigned definitions.
        var role = deityInfo.playerTypesArray[roleIndex];
        var domain = deityInfo.deityInfoArray[domainIndex];

        // Store the assignment locally.
        deityInfo.playerTypeDesignation = role;
        deityInfo.deityAssignment = domain;

        Debug.Log("Private player assignment received.");
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ReceiveInformationFragmentRpc(int[] sacredIds, int[] blightedIds, RpcParams rpcParams = default)
    {
        // The Host must not receive a player information fragment.
        if (IsServer)
            return;

        if (sacredIds == null || blightedIds == null ||
            localRoleIndex < 0 || deityInfo == null ||
            localRoleIndex >= deityInfo.playerTypesArray.Length)
        {
            Debug.LogError("Invalid or premature information fragment.");
            return;
        }

        bool isTrickster =
            deityInfo.playerTypesArray[localRoleIndex].PlayerRoleType ==
            DeityInfoSelection.PlayerRoleTypes.Trickster;

        int expectedSacred = isTrickster ? 5 : 3;
        int expectedBlighted = isTrickster ? 5 : 1;

        if (sacredIds.Length != expectedSacred ||
            blightedIds.Length != expectedBlighted)
        {
            Debug.LogError("Information fragment has incorrect counts.");
            return;
        }

        HashSet<int> receivedPositions = new();

        foreach (int id in sacredIds)
        {
            if (id < 0 || id >= 19 || !receivedPositions.Add(id))
            {
                Debug.LogError("Invalid Sacred information fragment.");
                return;
            }
        }

        foreach (int id in blightedIds)
        {
            if (id < 0 || id >= 19 || !receivedPositions.Add(id))
            {
                Debug.LogError("Invalid Blighted information fragment.");
                return;
            }
        }

        localSacredInfo.Clear();
        localSacredInfo.AddRange(sacredIds);

        localBlightedInfo.Clear();
        localBlightedInfo.AddRange(blightedIds);

        informationFragmentReceived = true;

        RefreshPrivateBoardInformation();

        Debug.Log(
            "Private information fragment received: " +
            localSacredInfo.Count + " Sacred, " +
            localBlightedInfo.Count + " Blighted."
        );
    }

    private void RefreshPrivateBoardInformation()
    {
        // Only participating Clients can view private information.
        if (!IsSpawned || IsServer || !informationFragmentReceived)
            return;

        if (hexGrid == null || hexGrid.tileDesignation == null)
        {
            Debug.LogWarning("Hex Grid reference is missing.");
            return;
        }

        // Clear all previously displayed private information.
        foreach (HexTile tile in hexGrid.tileDesignation)
        {
            if (tile != null)
                tile.blank();
        }

        // Reveal the Sacred locations this player knows.
        foreach (int hexId in localSacredInfo)
        {
            HexTile tile = hexGrid.tileDesignation[hexId];

            if (tile != null)
                tile.sacred();
        }

        // Reveal the Blighted locations this player knows.
        foreach (int hexId in localBlightedInfo)
        {
            HexTile tile = hexGrid.tileDesignation[hexId];

            if (tile != null)
                tile.blighted();
        }

        Debug.Log("Private board information displayed.");
    }

    public void RevealAssignedIdentity()
    {
        // Only connected players can reveal their assignments.
        if (!IsSpawned || IsServer || !IsMyClientAssigned())
        {
            Debug.LogWarning(
                "Cannot reveal identity: private assignment not received."
            );
            return;
        }

        var role = deityInfo.playerTypesArray[localRoleIndex];
        var domain = deityInfo.deityInfoArray[localDomainIndex];

        // Update the existing player information UI.
        deityInfo.playerTypeNaming.text =
            role.PlayerTypeDesignation;

        deityInfo.playerTypeInfoBreakdown.text =
            role.PlayerTypeDetails;

        deityInfo.deityNaming.text =
            domain.DeityName;

        deityInfo.deityInfoBreakdown.text =
            domain.DeityDescription;

        Debug.Log("Private role and domain displayed.");
    }

    private bool IsMyClientAssigned()
    {
        return deityInfo != null &&
               deityInfo.playerTypesArray != null &&
               deityInfo.deityInfoArray != null &&
               localRoleIndex >= 0 &&
               localRoleIndex < deityInfo.playerTypesArray.Length &&
               localDomainIndex >= 0 &&
               localDomainIndex < deityInfo.deityInfoArray.Length;
    }

    private void OnNetworkSceneEvent(SceneEvent sceneEvent)
    {
        if (!IsServer)
            return;

        if (sceneEvent.SceneEventType !=
            SceneEventType.LoadEventCompleted)
            return;

        if (sceneEvent.SceneName != "UI Sketch")
            return;

        Debug.Log("UI Sketch scene loading event completed.");

        SendPrivateAssignments(sceneEvent);
    }



}
