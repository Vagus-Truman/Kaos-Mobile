using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeityInfoSelection : MonoBehaviour
{
    public Sprite TriangleFilled;
    public Sprite TriangleEmpty;
    public int chargeCount;
    private int currentChargeCount;

    public Image Charge1;
    public Image Charge2;
    public Image Charge3;

    public TextMeshProUGUI deityNaming;
    public TextMeshProUGUI deityInfoBreakdown;

    [Serializable]
    public struct DeityInfo
    {
        public string DeityName;
        public string DeityDescription;

        public DeityInfo(string name, string description)
        {
            this.DeityName = name;
            this.DeityDescription = description;
        }
    }

    public DeityInfo[] deityInfoArray;
    private DeityInfo deityAssignment;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Charge1.sprite = TriangleFilled;
        Charge2.sprite = TriangleFilled;
        Charge3.sprite = TriangleFilled;
        currentChargeCount = chargeCount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeitySelection()
    {
        int deitySelect = UnityEngine.Random.Range(0, deityInfoArray.Length);
        deityAssignment = deityInfoArray[deitySelect];

        deityNaming.text = deityAssignment.DeityName;
        deityInfoBreakdown.text = deityAssignment.DeityDescription;
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
            case 0:
                break;
        }
    }
}
