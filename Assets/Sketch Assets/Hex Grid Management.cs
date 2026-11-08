using System;
using System.Collections.Generic; // Required for Lists
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DeityInfoSelection;

public class HexGridManagement : MonoBehaviour
{
    public HexTile[] tileDesignation;
    public HexTile selection;
    public List<HexTile> selected = new List<HexTile>();
    public List<HexTile> unselected = new List<HexTile>();

    public enum TileClassification
    {
        Neutral,
        Blighted,
        Sacred
    }

    [Serializable]
    public struct TileType
    {
        public int tileNumber;
        public TileClassification tileClass;

        public TileType(int tileNum, TileClassification tileStatus)
        {
            this.tileNumber = tileNum;
            this.tileClass = tileStatus;
        }
    }

    public GameObject regularSelectionFinalize;
    public GameObject abilitySelectionFinalize;

    public List<int> blessedTiles = new List<int> { };
    public List<int> blightedTiles = new List<int> { };
    public List<TileType> presentedTiles = new List<TileType> { };
    public TextMeshProUGUI scoreboard;

    public Button abilityButton;
    public Button abilitySelectionButton;

    public bool abilityActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        regularSelectionFinalize.SetActive(false);
        abilitySelectionFinalize.SetActive(false);
        abilityActive = false;
        scoreKeeper();
    }

    public void scoreKeeper()
    {
        int blessedCount = 0;
        int blightedCount = 0;

        foreach (HexTile chosen in selected)
        {
            if (blessedTiles.Contains(chosen.selectedNumber))
            {
                blessedCount++;
            }

            else if (blightedTiles.Contains(chosen.selectedNumber))
            {
                blightedCount++;
            }
        }

        if (blessedCount == 4)
        {
            scoreboard.text = $"Sacred — {blessedCount}\nBlighted — {blightedCount}\nSACRED WIN"; // write gameover code
        } else if (blightedCount == 4)
        {
            scoreboard.text = $"Sacred — {blessedCount}\nBlighted — {blightedCount}\nBLIGHTED WIN"; // write gameover code
        } else
        {
            scoreboard.text = $"Sacred — {blessedCount}\nBlighted — {blightedCount}"; // keep going
        }
    }

    public void selectionHighlighted(HexTile focus)
    {
        if (focus.visible)
        {
            selection = focus;
            
            if (!selected.Contains(selection) && !abilityActive)
            {
                unselected.Remove(focus);
                foreach (HexTile tile in unselected)
                {
                    tile.buttonRef.interactable = false;
                }
                regularSelectionFinalize.SetActive(true);
            } else if (abilityActive)
            {
                unselected.Remove(focus);
                foreach (HexTile tile in unselected)
                {
                    tile.buttonRef.interactable = false;
                }
            }
        } else
        {
            if (!selected.Contains(focus)) {
                unselected.Add(focus);
                selection = null;
                foreach (HexTile tile in unselected)
                {
                    tile.buttonRef.interactable = true;
                }
                if (!abilityActive)
                {
                    regularSelectionFinalize.SetActive(false);
                }
            }
        }
    }

    public void selectionFinalized() // do next turn, and unselected is permentantly like that
    {
        if (selection)
        {
            selection.selected = true;
            selected.Add(selection);

            foreach (HexTile tile in unselected)
            {
                tile.buttonRef.interactable = true;
            }

            selection = null;
            regularSelectionFinalize.SetActive(false);
            scoreKeeper();
        }
    }

    public void masterControlSelection(PlayerRoleTypes playerType)
    {
        while (blessedTiles.Count < 5)
        {
            int randomNumber = UnityEngine.Random.Range(1, tileDesignation.Length + 1);

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
            int randomNumber = UnityEngine.Random.Range(1, tileDesignation.Length + 1);

            if (!blessedTiles.Contains(randomNumber) && !blightedTiles.Contains(randomNumber))
            {
                blightedTiles.Add(randomNumber);
            }
            else
            {
                continue;
            }
        }

        foreach (int blessedSpot in blessedTiles)
        {
            presentedTiles.Add(new TileType(blessedSpot, TileClassification.Sacred));
        }
        foreach (int blightedSpot in blightedTiles)
        {
            presentedTiles.Add(new TileType(blightedSpot, TileClassification.Blighted));
        }

        fogOfWarDictator(playerType);
        unselected.AddRange(tileDesignation);
    }

    void fogOfWarDictator(PlayerRoleTypes p)
    {
        switch (p)
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
                tileReveal();
                break;

            // Show all
            case PlayerRoleTypes.Trickster:
                Debug.Log("Trickster Fog of War Config");
                tileReveal();
                break;
        }
    }

    void tileReveal()
    {
        foreach (TileType definedTile in presentedTiles)
        {
            switch (definedTile.tileClass)
            {
                case TileClassification.Neutral:
                    tileDesignation[definedTile.tileNumber - 1].prepare();
                    break;
                case TileClassification.Blighted:
                    tileDesignation[definedTile.tileNumber - 1].blighted();
                    break;
                case TileClassification.Sacred:
                    tileDesignation[definedTile.tileNumber - 1].sacred();
                    break;
            }
        }
    }

    public void abilityRegister(DeityDomain deityType)
    {
        switch (deityType)
        {
            case DeityDomain.Fate:
                abilityButton.onClick.AddListener(fateAbility);
                break;
            case DeityDomain.Chaos:
                abilityButton.onClick.AddListener(chaosAbility);
                break;
            case DeityDomain.Death:
                abilityButton.onClick.AddListener(deathAbility);
                break;
            case DeityDomain.Protection:
                abilityButton.onClick.AddListener(protectionAbility);
                break;
            case DeityDomain.Illusion:
                abilityButton.onClick.AddListener(illusionAbility);
                break;
        }
    }
    //"Gods of Fate may discern if a chosen tile is Neutral, Blighted, or Sacred on their Divine Relic.",
    //"Gods of Chaos can move a placed tile on the board into a random spot on the board, determined by the Divine Relic.",
    //"Gods of Death can destroy a placed tile, returning that tile into the deck for redistribution and reuse.",
    //"Gods of Protection can protect a tile from rearrangement, or destruction, for one turn.",
    //"Gods of Illusion can exchange the spots of two placed tiles, aside from root tiles."

    // success
    public void fateAbility()
    {
        // Open Selection Menu, Disable selected tiles, Reveal if Selected Tile is Neutral, and then move turn
        abilitySelectionFinalize.SetActive(true);
        abilityActive = true;
        Debug.Log("Fate Ability Activated");
        abilitySelectionButton.onClick.AddListener(fateSelection);
    }
    public void fateSelection()
    {
        if (blessedTiles.Contains(selection.selectedNumber))
        {
            selection.sacred();
            selection.tileUnselected();
        } else if (blightedTiles.Contains(selection.selectedNumber))
        {
            selection.blighted();
            selection.tileUnselected();
        } else
        {
            selection.tileUnselected();
        }
        abilitySelectionFinalize.SetActive(false);
        abilityActive = false;
    }

    // shelve chaos for the time being. 
    public void chaosAbility()
    {
        // Open Selection Menu, Disable unselected tiles, and randomly place the selected tile within unselected tiles, then move turn
        abilitySelectionFinalize.SetActive(true);
        abilityActive = true;

        foreach (HexTile selectedTile in unselected)
        {
            selectedTile.buttonRef.enabled = false;
        }

        foreach (HexTile selectedTile in selected)
        {
            selectedTile.selectedAndAbility();
            selectedTile.buttonRef.enabled = true;
        }

        Debug.Log("Chaos Ability Activated");
        abilitySelectionButton.onClick.AddListener(chaosSelection);
    }
    public void chaosSelection()
    {
        selected.Remove(selection);

        int randomWithinUnselected = UnityEngine.Random.Range(0, unselected.Count);


        unselected.RemoveAt(randomWithinUnselected);
        HexTile newSelection = tileDesignation[unselected[randomWithinUnselected].selectedNumber];
        newSelection.tileSelected();

        selected.Add(newSelection);

        foreach (HexTile selectedTile in unselected)
        {
            selectedTile.buttonRef.enabled = true;
        }

        foreach (HexTile selectedTile in selected)
        {
            selectedTile.buttonRef.enabled = false;
        }
        abilitySelectionFinalize.SetActive(false);
        abilityActive = false;
    }

    public void deathAbility()
    {

        Debug.Log("Death Ability Activated");
    }
    public void deathSelection()
    {

    }

    public void protectionAbility()
    {   // engage protection buff on particular selected tile
        abilitySelectionFinalize.SetActive(true);
        abilityActive = true;
        Debug.Log("Protection Ability Activated");
        abilitySelectionButton.onClick.AddListener(protectionSelection);
    }
    public void protectionSelection()
    {
        selection.protectedBuff = true;
        abilitySelectionFinalize.SetActive(false);
        abilityActive = false;
    }

    // SUCCESS
    public void illusionAbility()
    {
        List<HexTile> selectedChoppingBlock = new List<HexTile>(); //the two selected tiles to be moved
        while (selectedChoppingBlock.Count < 2)
        {
            int randomEntry = UnityEngine.Random.Range(0, selected.Count);

            if (selectedChoppingBlock.Contains(selected[randomEntry]))
            {
                continue;
            } else
            {
                selectedChoppingBlock.Add(selected[randomEntry]);
            }
        }

        List<HexTile> unselectedChoppingBlock = new List<HexTile>(); // picking new homes
        while (unselectedChoppingBlock.Count < 2)
        {
            int randomEntry = UnityEngine.Random.Range(0, unselected.Count);

            if (unselectedChoppingBlock.Contains(unselected[randomEntry]))
            {
                continue;
            }
            else
            {
                unselectedChoppingBlock.Add(unselected[randomEntry]);
            }
        }

        foreach (HexTile selection in selectedChoppingBlock)
        {
            selection.tileUnselected();
            selected.Remove(selection);
        }

        foreach (HexTile selection in unselectedChoppingBlock)
        {
            selection.tileSelected();
            selectionFinalized();
        }

        Debug.Log("Illusion Ability Activated");
    }
}
