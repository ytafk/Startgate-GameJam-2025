using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Pickup Prompt")]
    public TMP_Text pickupPromptText;

    [Header("Slots HUD")]
    public TMP_Text slotsText;

    void Start()
    {
        if (pickupPromptText)
            pickupPromptText.enabled = false;
    }

    // G: Pistol al / deðiþtir
    public void SetPickupPrompt(string msg)
    {
        if (!pickupPromptText) return;

        bool show = !string.IsNullOrWhiteSpace(msg);
        pickupPromptText.text = msg;
        pickupPromptText.enabled = show;
    }

    // Slot HUD
    public void SetSlots(string slot1, string slot2, int activeSlot)
    {
        if (!slotsText) return;

        string s1 = string.IsNullOrEmpty(slot1) ? "-" : slot1;
        string s2 = string.IsNullOrEmpty(slot2) ? "-" : slot2;

        if (activeSlot == 1) s1 = $"[{s1}]";
        if (activeSlot == 2) s2 = $"[{s2}]";

        slotsText.text = $"1: {s1}\n2: {s2}";
    }
}
