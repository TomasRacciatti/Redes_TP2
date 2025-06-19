using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class ClaimController : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private TMP_Text amountText;

    [SerializeField] private DieDisplay newClaimDie;

    private int _amount = 0;
    private int _face = 1;

    private const int MinAmount = 1;
    private const int MaxAmount = 30;
    private const int MinFace = 1;
    private const int MaxFace = 6;

    private void Start()
    {
        _amount = MinAmount;
        _face   = MinFace;
        UpdateDisplay();
    }

    public void IncreaseAmount()
    {
        _amount = Mathf.Min(_amount + 1, MaxAmount);
        UpdateDisplay();
    }

    public void DecreaseAmount()
    {
        _amount = Mathf.Max(_amount - 1, MinAmount);
        UpdateDisplay();
    }

    public void IncreaseFace()
    {
        _face = Mathf.Min(_face + 1, MaxFace);
        UpdateDisplay();
    }
    

    public void DecreaseFace()
    {
        _face = Mathf.Max(_face - 1, MinFace);
        UpdateDisplay();
    }
    
    public void SubmitClaim()
    {
        if (_amount > GameManager.Instance.currentClaimQuantity)
        {
            GameManager.Instance.RPC_SetClaim(_amount, _face);
            gameObject.SetActive(false);
        }
        else if (_amount == GameManager.Instance.currentClaimQuantity)
        {
            if (_face > GameManager.Instance.currentClaimFace)
            {
                GameManager.Instance.RPC_SetClaim(_amount, _face);
                gameObject.SetActive(false);
            }
        }
        else
            Debug.Log("Invalid claim.");
        
    }

    private void UpdateDisplay()
    {
        if (amountText != null)
            amountText.text = _amount.ToString();
        if (_face != null)
            newClaimDie.ShowValue(_face);
    }
}