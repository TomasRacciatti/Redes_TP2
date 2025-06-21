using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class DieDisplay : MonoBehaviour
{
    [SerializeField] private GameObject[] _faces;

    public void ShowValue(int value)
    {
        for (int i = 0; i < _faces.Length; i++)
        {
            _faces[i].SetActive(i == value - 1);
        }
    }
}
