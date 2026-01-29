using UnityEngine;

[CreateAssetMenu(fileName = "NewNative", menuName = "GGJ/Native Data")]
public class CharacterData : ScriptableObject
{
    public Sprite characterSprite;
    public string maskType; 
    public bool alreadyPicked; 
}