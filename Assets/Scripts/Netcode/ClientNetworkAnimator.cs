using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Sample.Utilities.ClientAuthority
{
    [DisallowMultipleComponent]
    public class ClientNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
