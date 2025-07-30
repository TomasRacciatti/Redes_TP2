#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSoundUtility
{
    [MenuItem("Tools/Add Click Sound to All Buttons")]
    private static void AddClickSoundToAllButtons()
    {
        foreach (var button in Object.FindObjectsOfType<Button>())
        {
            if (button.GetComponent<UIButtonClickSound>() == null)
            {
                button.gameObject.AddComponent<UIButtonClickSound>();
            }
        }

        Debug.Log("UIButtonClickSound added to all buttons.");
    }
}
#endif
