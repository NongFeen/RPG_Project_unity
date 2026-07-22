using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ChargingPadTrigger : NetworkBehaviour
{
    [SerializeField] private float padChargedTime = 120f;
    [SerializeField] private NetworkVariable<float> padProgressTime = new NetworkVariable<float>();
    [SerializeField] private int playersInside = 0;
    [SerializeField] private float drainRate = 2;
    [SerializeField] private Sprite unChargedPad;
    [SerializeField] private Sprite chargedPad;
    public event Action OnPadFullyCharged;
    [SerializeField] private Transform fillMaskTransform;
    [Header("Charge Light")]
    [SerializeField] private Light2D padLight;
    [SerializeField] private float minimumLightIntensity = 0.5f;
    [SerializeField] private float maximumLightIntensity = 1.5f;

    public override void OnNetworkSpawn()
    {
        padProgressTime.OnValueChanged += OnPadProgressTimerUpdate;
        OnPadProgressTimerUpdate(padProgressTime.Value, padProgressTime.Value);
    }

    public override void OnNetworkDespawn()
    {
        padProgressTime.OnValueChanged -= OnPadProgressTimerUpdate;
    }

    private void OnPadProgressTimerUpdate(float previousValue, float newValue)
    {
        float percent = Mathf.Clamp01(newValue / padChargedTime);

        if (padLight != null)
            padLight.intensity = Mathf.Lerp(minimumLightIntensity, maximumLightIntensity, percent);

        SpriteMask mask = fillMaskTransform.GetComponent<SpriteMask>();
        float spriteHeight = mask.sprite.bounds.size.y;

        Vector3 scale = fillMaskTransform.localScale;
        scale.y = percent;
        fillMaskTransform.localScale = scale;

        Vector3 pos = fillMaskTransform.localPosition;
        pos.y = (-spriteHeight / 2f) + (spriteHeight * percent / 2f);
        fillMaskTransform.localPosition = pos;
        // this.TryGetComponent<SpriteRenderer>(out var currentSprite);
        // if(newValue < padChargedTime)
        // {
        //     currentSprite.sprite = unChargedPad; 
        // }
        // else
        // {
        //     currentSprite.sprite = chargedPad;
        // }
    }

    void Update()
    {
        if (!IsServer) return;

        bool wasCharged = padProgressTime.Value >= padChargedTime;
        if (playersInside > 0)
        {
            padProgressTime.Value += Time.deltaTime;
        }
        else
        {
            padProgressTime.Value -= Time.deltaTime * drainRate;
        }

        padProgressTime.Value = Mathf.Clamp(padProgressTime.Value, 0f, padChargedTime);
        if (!wasCharged && IsCharged())
        {
            OnPadFullyCharged?.Invoke();
        }
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

