using UnityEngine;

namespace BelowZeroCreatures.MonoBehaviours;

public class TitanHolefishOxygenHandTarget : HandTarget, IHandTarget
{
    public LiveMixin liveMixin;
    public float capacity = 15;
    public float useDelay = 20;
    public FMODAsset sound; // event:/env/bubble_pop_player

    private float _timeCanUseAgain;
    
    public void OnHandHover(GUIHand hand)
    {
        if (hand.IsFreeToInteract() && CanInteract())
        {
            var main = HandReticle.main;
            main.SetText(HandReticle.TextType.Hand, "TitanHolefishOxygenUse", translate: true, GameInput.Button.LeftHand);
            main.SetIcon(HandReticle.IconType.Hand);
        }
    }

    public void OnHandClick(GUIHand hand)
    {
        if (hand.IsFreeToInteract() && CanInteract())
        {
            var player = hand.player;
            var oxygen = player.oxygenMgr;
            float amountToGive = Mathf.Min(capacity, oxygen.GetOxygenCapacity() - oxygen.GetOxygenAvailable());
            if (amountToGive > 0f)
            {
                _timeCanUseAgain = Time.time + useDelay;
                oxygen.AddOxygen(amountToGive);
                Utils.PlayFMODAsset(sound, player.transform.position);
            }
        }
    }

    private bool CanInteract()
    {
        return liveMixin.IsAlive() && Time.time > _timeCanUseAgain;
    }
}