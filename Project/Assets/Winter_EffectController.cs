using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Winter_EffectController : Base_EffectController
{
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_BaseController);
        foreach (var hit in hits)
        {
            PlayParticle(hit.point,wait: 3.4f,usePsLifeTime:false,emitAmount:1);
            break;
        }
    }
}
