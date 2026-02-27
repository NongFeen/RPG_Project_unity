using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChargingPadTrigger : NetworkBehaviour
{
    [SerializeField] private float padChargedTime = 120f;
    [SerializeField] private NetworkVariable<float> padProgressTime = new NetworkVariable<float>();
    [SerializeField] private int playersInside = 0;
    [SerializeField] private float drainRate = 2;
    [SerializeField] private Sprite unChargedPad;
    [SerializeField] private Sprite chargedPad;

    public override void OnNetworkSpawn()
    {
        padProgressTime.OnValueChanged += OnPadProgressTimerUpdate;
    }

    private void OnPadProgressTimerUpdate(float previousValue, float newValue)
    {
        this.TryGetComponent<SpriteRenderer>(out var currentSprite);
        if(newValue < padChargedTime)
        {
            currentSprite.sprite = unChargedPad; 
        }
        else
        {
            currentSprite.sprite = chargedPad;
        }
    }

    void Update()
    {
        if (!IsServer) return;

        if (playersInside > 0)
        {
            padProgressTime.Value += Time.deltaTime;
        }
        else
        {
            padProgressTime.Value -= Time.deltaTime * drainRate;
        }

        padProgressTime.Value = Mathf.Clamp(padProgressTime.Value, 0f, padChargedTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            playersInside++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            playersInside--;
        }
    }
    public bool IsCharged(){
        if(padProgressTime.Value >= padChargedTime) return true;
        return false;
    }
    public void ResetTime()
    {
        padProgressTime.Value = 0;
    }
    
}

