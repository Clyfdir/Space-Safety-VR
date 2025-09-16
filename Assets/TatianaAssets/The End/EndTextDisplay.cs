///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 10.09.2025
///   Created: 10.09.2025

/// Script to be able to control texts in each version from one place, from object "----TheEnd" with script EndVersionsManager on it

using UnityEngine;
using UnityEngine.UI; // needed for Text

public class EndTextDisplay : MonoBehaviour
{
    private void Awake()
    {
        if (EndVersionsManager.Instance != null)
        {
            var textComponent = GetComponent<Text>(); // Legacy UI Text
            if (textComponent != null)
            {
                textComponent.text = EndVersionsManager.Instance.GetEndText();
            }
        }
    }
}
