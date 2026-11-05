using NUnit.Framework.Internal;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class UIControl : MonoBehaviour
{
    public CanvasGroup rollMenu;
    public CanvasGroup uniqueCardMenu;
    public Image pulseMaterialSource;
    private Coroutine timerCoroutine;
    private Material pulseMaterial;

    public CanvasGroup personalSheet;
    public CanvasGroup selectionSheet;

    public bool isTurn;
    public float totalDuration = 5f;
    public float timeRemaining;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rollMenu.alpha = 1.0f;
        uniqueCardMenu.alpha = 0.0f;

        setRollFront();
        pulseMaterial = pulseMaterialSource.material;
        pulseMaterial.SetFloat("_Radius", 0f);
        itIsTurn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setRollFront()
    {
        rollMenu.interactable = true;
        uniqueCardMenu.interactable = false;

        rollMenu.gameObject.SetActive(true);
        uniqueCardMenu.gameObject.SetActive(false);
    }
    public void setCharFront()
    {
        rollMenu.interactable = false;
        uniqueCardMenu.interactable = true;

        rollMenu.gameObject.SetActive(false);
        uniqueCardMenu.gameObject.SetActive(true);

        StartCoroutine(otherPageCoroutine(0f, 1f, 1f, uniqueCardMenu));
    }

    public void itIsTurn()
    {
        selectionSheet.gameObject.SetActive(true);
        StartCoroutine(otherPageCoroutine(0f, 1f, 5f, selectionSheet));
    }

    public void itIsNotTurn()
    {
        selectionSheet.gameObject.SetActive(false);
        StartCoroutine(otherPageCoroutine(1f, 0f, 5f, selectionSheet));
    }

    public void timeToRollForButton()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(pulsingOutCoroutine(0f, 1f, 1f));
    }

    private IEnumerator pulsingOutCoroutine(float startValue, float endValue, float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float currentVal = Mathf.Lerp(startValue, endValue, elapsed / time);

            // Set the shader property
            pulseMaterial.SetFloat("_Radius", currentVal);
            rollMenu.alpha = 1f - (currentVal * 1.5f);

            yield return null; // Wait for the next frame
        }

        pulseMaterial.SetFloat("_Radius", endValue); // Ensure exact end value
        setCharFront();
    }

    private IEnumerator otherPageCoroutine(float startValue, float endValue, float time, CanvasGroup target)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float currentVal = Mathf.Lerp(startValue, endValue, elapsed / time);

            // Set the shader property
            target.alpha = currentVal * 1.5f;

            yield return null; // Wait for the next frame
        }
    }
}
