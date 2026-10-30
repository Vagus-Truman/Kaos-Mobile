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

    public void buttonEvent()
    {
        if (visible && !selected)
        {
            tileUnselected();
        } else
        {
            tileSelected();
        }
    }
}
