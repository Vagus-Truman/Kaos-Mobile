using System;
using System.Collections.Generic; // Required for Lists
using System.Linq; // Required for Contains()
using TMPro;
using UnityEngine;
using static DeityInfoSelection;

public class HexGridManagement : MonoBehaviour
{
    public TextMeshProUGUI[] tileDesignation;

    [Serializable]
    public struct TileType
    {
        public int tileNumber;
        public bool falseIfBlighted;

        public TileType(int tileNum, bool tileStatus)
        {
            this.tileNumber = tileNum;
            this.falseIfBlighted = tileStatus;
        }
    }

    public List<int> blessedTiles = new List<int> { };
    public List<int> blightedTiles = new List<int> { };
    public List<TileType> presentedTiles = new List<TileType> { };

    public DeityInfoSelection.PlayerType playerRole;
    public DeityInfoSelection playerReference;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void masterControlSelection()
    {
        playerRole = playerReference.playerTypeDesignation;

        while (blessedTiles.Count < 5)
        {
            int randomNumber = UnityEngine.Random.Range(0, tileDesignation.Length);

            if (!blessedTiles.Contains(randomNumber) && !blightedTiles.Contains(randomNumber))
            {
                blessedTiles.Add(randomNumber);
            }
            else
            {
                continue;
            }
        }

        // blighted selection
        while (blightedTiles.Count < 5)
        {
            int randomNumber = UnityEngine.Random.Range(0, tileDesignation.Length);

            if (!blessedTiles.Contains(randomNumber) && !blightedTiles.Contains(randomNumber))
            {
                blightedTiles.Add(randomNumber);
            }
            else
            {
                continue;
            }
        }

        for (int i = 0; i < blessedTiles.Count; i++)
        {
            presentedTiles.Add(new TileType(blessedTiles[i], true));
        }

        for (int i = 0; i < blightedTiles.Count; i++)
        {
            presentedTiles.Add(new TileType(blightedTiles[i], false));
        }

        fogOfWarDictator();
    }

    void fogOfWarDictator()
    {
        switch (playerRole.PlayerRoleType)
        {
            // show only four
            case PlayerRoleTypes.Creator:
                Debug.Log("Creator Fog of War Config");
                List<TileType> fourSelectedPresent = new List<TileType> {};

                while (fourSelectedPresent.Count < 4)
                {
                    int randomNumber = UnityEngine.Random.Range(0, presentedTiles.Count);
                    if (fourSelectedPresent.Contains(presentedTiles[randomNumber]))
                    {
                        continue;
                    } else
                    {
                        fourSelectedPresent.Add(presentedTiles[randomNumber]);
                    }
                }

                presentedTiles.Clear();
                presentedTiles.AddRange(fourSelectedPresent);
                break;

            // Show all
            case PlayerRoleTypes.Trickster:
                Debug.Log("Trickster Fog of War Config");
                break;
        }
        tileReveal();
    }

    void tileReveal()
    {
        for (int i = 0; i < tileDesignation.Length; i++)
        {
            tileDesignation[i].text = "";
        }

        foreach (TileType definedTile in presentedTiles)
        {
            if (definedTile.falseIfBlighted)
            {
                tileDesignation[definedTile.tileNumber].text = "+";
            }
            else
            {
                tileDesignation[definedTile.tileNumber].text = "-";
            }
        }
    }
}
