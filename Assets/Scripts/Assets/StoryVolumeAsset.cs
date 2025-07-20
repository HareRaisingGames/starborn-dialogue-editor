using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.File
{
    public struct StoryVolumeAsset
    {
        [System.Serializable]
        public struct DialogueSequence
        {
            //Text line for each dialogue
            public DialogueText[] text;

            //Images
            public byte[] backgroundImg;
            public byte[] foregroundImg;

            //Character Sprites
            public List<CharacterPack> characters;

            //Minigame Settings
            public bool minigame; //If this section is a minigame
            public string minigameName;

            public DialogueSequence(string[] args)
            {
                text = new DialogueText[9];
                string[] languageNames = System.Enum.GetNames(typeof(Language));

                for (int i = 0; i < text.Length; i++)
                {
                    text[i] = new DialogueText(languageNames[i], i);
                }

                backgroundImg = null;
                foregroundImg = null;

                characters = new List<CharacterPack>();

                minigame = false;
                minigameName = "";
            }
        }

        [System.Serializable]
        public struct DialogueText
        {
            public string name;
            public Language language;
            public string text;
            public AudioClip voice;
            public int id;

            public DialogueText(string language, int id)
            {
                this.name = "";
                this.language = (Language)System.Enum.Parse(typeof(Language), language);
                this.text = "";
                this.voice = null;
                this.id = id;
            }
        }

        [System.Serializable]
        public struct CharacterPack
        {
            public DialogueCharacter character;
            public Alignment alignment;
            public float offset;
            public bool flipX;
        }

        [System.Serializable]
        public enum Language
        {
            english,
            spanish,
            french,
            japanese,
            chinese,
            korean,
            german,
            russian,
            italian
        }

        [System.Serializable]
        public enum Alignment
        {
            left,
            center,
            right
        }

        public string filename;
        public List<DialogueSequence> dialogue;
        public int storyOrder;
        public AudioClip music;
        public StoryVolumeAsset(string[] args)
        {
            filename = "";
            dialogue = new List<DialogueSequence>();
            storyOrder = 0;
            music = null;
        }
    }
}
