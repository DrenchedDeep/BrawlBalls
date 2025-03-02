using Unity.Netcode.Components;

namespace MarkedForDeath
{
    public class NetworkedRigidTrans : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
