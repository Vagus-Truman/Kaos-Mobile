using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

public class DeityInfoSelection : MonoBehaviour
{
    public Sprite TriangleFilled;
    public Sprite TriangleEmpty;
    public int chargeCount;
    private int currentChargeCount;

    public Image Charge1;
    public Image Charge2;
    public Image Charge3;

    public enum DeityDomain
    {
        Fate,
        Chaos,
        Death,
        Protection,
        Illusion
    }

    [Serializable]
    public struct DeityInfo
    {
        private static readonly string[] domainDetails = new string[] {
        "Gods of Fate may discern if a chosen tile is Neutral, Blighted, or Sacred on their Divine Relic.",
        "Gods of Chaos can move a placed tile on the board into a random spot on the board, determined by the Divine Relic.",
        "Gods of Death can destroy a placed tile, returning that tile into the deck for redistribution and reuse.",
        "Gods of Protection can protect a tile from rearrangement, or destruction, for one turn.",
        "Gods of Illusion can exchange the spots of two placed tiles, aside from root tiles."
        };

        public DeityDomain DeityName;
        public string DeityDescription;

        public DeityInfo(DeityDomain choice)
        {
            this.DeityName = choice;
            this.DeityDescription = domainDetails[(int)DeityName];
        }
    }

    public TextMeshProUGUI deityNaming;
    public TextMeshProUGUI deityInfoBreakdown;
    public DeityInfo[] deityInfoArray;
    public DeityInfo deityAssignment;

    public enum PlayerRoleTypes
    {
        Creator,
        Trickster
    }

    [Serializable]
    public struct PlayerType
    {
        private static readonly string[] roleDetails = new string[] {
        "You have the power to place tiles on whatever spots you may please, guided by your limited insight of which are which,",
        "You have the power to place tiles on whatever spots you may please, and you know the true nature of all the spots on the board."
        };

        public string PlayerTypeDetails;
        public PlayerRoleTypes PlayerRoleType;

        public PlayerType(PlayerRoleTypes type)
        {
            this.PlayerTypeDetails = roleDetails[(int)type];
            this.PlayerRoleType = type;
        }
    }

    public TextMeshProUGUI playerTypeNaming;
    public TextMeshProUGUI playerTypeInfoBreakdown;
    public PlayerType[] playerTypesArray;
    public PlayerType playerTypeDesignation;
    public HexGridManagement hexGridReference;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Charge1.sprite = TriangleFilled;
        Charge2.sprite = TriangleFilled;
        Charge3.sprite = TriangleFilled;
        currentChargeCount = chargeCount;
    }

    private void Update()
    {
        if (currentChargeCount > 0)
        {
            // in case we're Illusion
            if (deityAssignment.DeityName == DeityDomain.Illusion)
            {
                if (hexGridReference.selected.Count < 2) // and don't have enough placed tiles
                {
                    hexGridReference.abilityButton.enabled = false;
                }
                else
                {
                    if (chargeCount > 0)
                    {
                        hexGridReference.abilityButton.enabled = true;
                    }
                }
            }
        } else
        {
            hexGridReference.abilityButton.enabled = false;
        }
    }

    public void DeitySelection()
    {
        int deitySelect = UnityEngine.Random.Range(0, deityInfoArray.Length);
        deityAssignment = deityInfoArray[deitySelect];

        deityNaming.text = deityAssignment.DeityName.ToString();
        deityInfoBreakdown.text = deityAssignment.DeityDescription;

        int playerTypeSelect = UnityEngine.Random.Range(0, deityInfoArray.Length);
        playerTypeDesignation = playerTypesArray[playerTypeSelect];

        playerTypeNaming.text = playerTypeDesignation.PlayerRoleType.ToString();
        playerTypeInfoBreakdown.text = playerTypeDesignation.PlayerTypeDetails;

        hexGridReference.masterControlSelection(playerTypeDesignation.PlayerRoleType);
        hexGridReference.abilityRegister(deityAssignment.DeityName);
    }

    public void ChargeMaintenance()
    {
        currentChargeCount--;

        if (currentChargeCount < 0)
        {
            currentChargeCount = 0;
        }

        Charge1.sprite = TriangleEmpty;
        Charge2.sprite = TriangleEmpty;
        Charge3.sprite = TriangleEmpty; 

        switch (currentChargeCount)
        {
            case 3:
                Charge1.sprite = TriangleFilled;
                Charge2.sprite = TriangleFilled;
                Charge3.sprite = TriangleFilled;
                break;

            case 2:
                Charge1.sprite = TriangleFilled;
                Charge2.sprite = TriangleFilled;
                break;

            case 1:
                Charge1.sprite = TriangleFilled;
                break;
        }
    }
}
