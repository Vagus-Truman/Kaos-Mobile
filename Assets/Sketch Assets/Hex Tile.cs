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
    public bool protectedBuff = false;

    public bool visible;
    public bool selected;

    private void Start()
    {
        prepare();
    }

    public void prepare()
    {
        visible = false;
        textBox.raycastTarget = false;
        hexSelection.enabled = false;
        SetAlpha(1f);
    }

    public void SetAlpha(float alphaValue)
    {
        // Get the current color
        Color tempColor = hexSelection.color;

        // Update the alpha channel (0.0f = completely see-through, 1.0f = opaque)
        tempColor.a = alphaValue;

        // Assign the updated color back to the image
        hexSelection.color = tempColor;
    }

    public void selectedAndAbility()
    {
        SetAlpha(0.5f);
    }

    public void blank()
    {
        textBox.text = "";
        hexSelection.enabled = false;
        SetAlpha(1f);
    }

    public void sacred()
    {
        textBox.text = "+";
        hexSelection.enabled = false;
        SetAlpha(1f);
    }

    public void blighted()
    {
        textBox.text = "-";
        hexSelection.enabled = false;
        SetAlpha(1f);
    }

    public void tileSelected()
    {
        hexSelection.enabled = true;
        visible = true;
        hexGrid.selectionHighlighted(this);
        SetAlpha(1f);
    }

    public void tileUnselected()
    {
        hexSelection.enabled = false;
        visible = false;
        hexGrid.selectionHighlighted(this);
        SetAlpha(1f);
    }

    public void buttonEvent()
    {
        if (visible && !selected)
        {
            tileUnselected();
        } else
        {

            tileSelected();
            //if (hexGrid.abilityActive)
            //{
            //    selectedAndAbility();
            //    tileSelected();
            //} else
            //{
            //    tileSelected();
            //}
        }
    }
}
