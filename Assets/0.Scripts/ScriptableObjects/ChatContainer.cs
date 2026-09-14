using UnityEngine;

[CreateAssetMenu(fileName = "ChatContainer", menuName = "Chat/ChatContainer")]
public class ChatContainer : ScriptableObject
{
	public bool isCameraReturnToOrigin;
    public ChatSequence sequence;
}
