// using UnityEngine;
//
// public class SpringVidoContentGameManager : VidoContentGameManager
// {
//     
//     protected override void OnGmRaySyncedByOnGm()
//     {
//         hits = Physics.RaycastAll(ray_EffectManager);
//         foreach (var hit in hits)
//         {
//             PlayParticle(particlePool,hit.point);
//             break;
//         }
//     }
// }