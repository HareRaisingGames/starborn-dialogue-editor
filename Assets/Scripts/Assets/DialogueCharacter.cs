using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueCharacter
{
    public string filename;
    public Dictionary<int, Emotion> expressions;
    
    [System.Serializable]
    public struct Emotion
    {
        public string expression;
        public Texture2D sprite;
        public float scale;
        public Vector2 offset;
        public Emotion(string[] args)
        {
            expression = "";
            sprite = null;
            scale = 1;
            offset = Vector2.zero;

        }
    }
}
