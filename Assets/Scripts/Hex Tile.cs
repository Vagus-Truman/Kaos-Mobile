using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HexTile : MonoBehaviour
{
    public int selectedNumber;
    public TextMeshProUGUI textBox;
    public Image hexSelection;
    public Button buttonRef;
    public HexGridManagement hexGrid;

    public bool visible;
    public bool selected;

    private void Start()
    {
        visible = false;
        textBox.raycastTarget = false;
    }

    public void blank()
    {
        textBox.text = "";
        hexSelection.enabled = false;
    }

    public void sacred()
    {
        textBox.text = "+";
        hexSelection.enabled = false;
    }

    public void blighted()
    {
        textBox.text = "-";
        hexSelection.enabled = false;
    }

    public void tileSelected()
    {
        hexSelection.enabled = true;
        visible = true;
        hexGrid.selectionHighlighted(this);
    }

    public void tileUnselected()
    {
        hexSelection.enabled = false;
        visible = false;
        hexGrid.selectionHighlighted(this);
    }

    public void SetOccupiedVisual(bool occupied)
    {
        // Record whether this hexagon is occupied.
        selected = occupied;

        // Clear the temporary selection state.
        visible = false;

        // Keep occupied hexagons highlighted.
        hexSelection.enabled = occupied;

        // Prevent players from selecting occupied hexagons.
        buttonRef.interactable = !occupied;
    }



    public void buttonEvent()
    {
        NetworkBoardController board =
            FindAnyObjectByType<NetworkBoardController>();

        // Reject interactions from the Host and inactive players.
        if (board == null || !board.IsMyTurn)
        {
            Debug.Log("Tile selection blocked: not your turn.");
            return;
        }

        // Occupied hexagons cannot be selected again.
        if (selected)
        {
            return;
        }

        if (visible)
        {
            tileUnselected();
        }
        else
        {
            tileSelected();
        }
    }

}
