using Unity.Netcode.Components;
public class NetworkedRigidTrans : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
