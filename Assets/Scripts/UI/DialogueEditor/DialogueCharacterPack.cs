using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rabbyte;
using System;
public class DialogueCharacterPack : MonoBehaviour
{
    DialogueManager manager;
    public CharacterPack pack;
    SimpleSBDFile dialogueFile;
    DialogueCharacterPackManager packManager;

    List<string> characters = new List<string>();
    List<string> emotions = new List<string>();

    List<String> copyEmotions = new List<string>();

    public TMP_Dropdown charactersDropdown;
    public TMP_Dropdown emotionsDropdown;
    public TMP_Dropdown alignment;
    public TMP_Dropdown transition;
    public UIStepper offset;
    public Toggle flipX;
    public Toggle isSpeaking;

    public Button delete;

    [HideInInspector]
    public CharacterSprite character;

    bool curCharacter;

    [HideInInspector]
    public bool hasLoaded;

    [HideInInspector]
    public int group;
    [HideInInspector]
    public int id;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Character(SimpleSBDFile file, DialogueCharacterPackManager manager, int group = -1, int id = -1)
    {
        dialogueFile = file;
        packManager = manager;
        this.group = group;
        this.id = id;

        //Debug.Log(dialogueFile.characterPack.Count);
        this.manager = FindObjectOfType<DialogueManager>();
        characters = this.manager.characterList;
    }
    public void AddCharacterPack(SimpleSBDFile file, DialogueCharacterPackManager manager, int group = -1, int id = -1)
    {
        Character(file, manager, group, id);
        pack = dialogueFile.characterPack[id];
        SetCharacter();
        ManagerSetup();
    }

    public void SetCharacterPack(CharacterPack pack, SimpleSBDFile file, DialogueCharacterPackManager manager, int group = -1, int id = -1, string character = "")
    {
        Character(file, manager, group, id);

        //Debug.Log(dialogueFile.characterPack.Count);
        this.pack = pack;

        foreach(CharacterSprite sprite in FindObjectsOfType<CharacterSprite>(true))
        {
            if(sprite.charName == character)
            {
                this.character = sprite;
                break;
            }
        }

        charactersDropdown.ClearOptions();
        charactersDropdown.AddOptions(characters);

        emotions.Clear();
        foreach (Emotion emotion in dialogueFile.GetCharacters()[pack.character])
        {
            emotions.Add(emotion.expression);
        }

        emotionsDropdown.ClearOptions();
        emotionsDropdown.AddOptions(emotions);

        emotionsDropdown.value = UIUtils.GetDropdownValueByName(emotionsDropdown, pack.emotion);

        transition.ClearOptions();
        transition.AddOptions(new List<string>(System.Enum.GetNames(typeof(SpriteTransition))));
        transition.value = UIUtils.GetDropdownValueByName(transition, Enum.GetName(typeof(SpriteTransition), pack.transition));

        curCharacter = true;
        //ManagerSetup();
    }
    public void ManagerSetup()
    {
        this.manager = FindObjectOfType<DialogueManager>();
        if (this.manager != null)
        {
            //characters = this.manager.characterList;
            //SetCharacter();

            //dialogueFile.characterPack.Add(curPacks);

            charactersDropdown.onValueChanged.AddListener(delegate
            {
                ChangeCharacter(charactersDropdown);
            });
            emotionsDropdown.onValueChanged.AddListener(delegate
            {
                ChangeEmotion(emotionsDropdown);
            });
            alignment.onValueChanged.AddListener(delegate
            {
                SetAlignment(alignment);
            });
            transition.onValueChanged.AddListener(delegate{
                string option = transition.options[transition.value].text;
                pack.transition = (SpriteTransition)Enum.Parse(typeof(SpriteTransition), option);
                dialogueFile.GetLines()[group].characters[id] = pack;
            });
            offset.onValueChanged.AddListener(SetOffset);
            flipX.onValueChanged.AddListener(SetFlipX);
            isSpeaking.onValueChanged.AddListener(SetIsSpeaking);

            delete.onClick.AddListener(RemoveCharacterPack);

        }
        hasLoaded = true;
    }
    public void RemoveAllListeners()
    {
        charactersDropdown.onValueChanged.RemoveAllListeners();
        emotionsDropdown.onValueChanged.RemoveAllListeners();
        alignment.onValueChanged.RemoveAllListeners();
        offset.onValueChanged.RemoveAllListeners();
        flipX.onValueChanged.RemoveAllListeners();
        isSpeaking.onValueChanged.RemoveAllListeners();
        delete.onClick.RemoveAllListeners();
        hasLoaded = false;
    }
    public void UISetUp()
    {
        transition.ClearOptions();
        transition.AddOptions(new List<string>(System.Enum.GetNames(typeof(SpriteTransition))));
        charactersDropdown.value = UIUtils.GetDropdownValueByName(charactersDropdown, pack.character);
        emotionsDropdown.value = UIUtils.GetDropdownValueByName(emotionsDropdown, pack.emotion);
        alignment.value = UIUtils.GetDropdownValueByName(alignment, Enum.GetName(typeof(Alignment), pack.alignment));
        transition.value = UIUtils.GetDropdownValueByName(transition, Enum.GetName(typeof(SpriteTransition), pack.transition));
        offset.value = pack.offset;
        offset.startValue = pack.offset;
        flipX.isOn = pack.flipX;
        isSpeaking.isOn = pack.isSpeaking;
    }

    public DialogueCharacterPack SetPack(CharacterPack pack)
    {

        return this;
    }

    public void RemoveCharacterPack()
    {
        packManager.RemoveCharacterPack(id);
        if(character != null)
        {
            character.gameObject.SetActive(false);
            character = null;
        }
        Destroy(gameObject);
    }

    public void SetCharacter()
    {
        charactersDropdown.ClearOptions();
        charactersDropdown.AddOptions(characters);
        pack.character = charactersDropdown.options[charactersDropdown.value].text;
        ChangeCharacter(charactersDropdown);
        
    }
    public void ChangeCharacter(TMP_Dropdown dropdown)
    {
        pack.character = dropdown.options[dropdown.value].text;
        dialogueFile.GetLines()[group].characters[id] = pack;

        if (manager != null)
        {
            if(character != null)
                character.gameObject.SetActive(false);

            foreach (Transform charac in manager.sprites.transform)
            {
                if(charac.name == pack.character)
                {
                    character = charac.GetComponent<CharacterSprite>();
                    character.gameObject.SetActive(true);

                    float xPos = 0;
                    switch (pack.alignment)
                    {
                        case Alignment.left:
                            xPos = -325;
                            break;
                        case Alignment.right:
                            xPos = 325;
                            break;
                        default:
                            xPos = 0;
                            break;
                    }
                    if (character != null)
                        character.position = new Vector2(xPos, 0);
                }
            }
        }

        Dictionary<string, List<Emotion>> chars = dialogueFile.GetCharacters();

        if(!curCharacter)
        {
            List<Emotion> charEmotions = new List<Emotion>();
            if (chars.ContainsKey(dropdown.options[dropdown.value].text))
                charEmotions = chars[dropdown.options[dropdown.value].text];

            emotions.Clear();
            foreach (Emotion emotion in charEmotions)
            {
                emotions.Add(emotion.expression);
            }

            emotionsDropdown.ClearOptions();
            emotionsDropdown.AddOptions(emotions);
        }

        curCharacter = false;
        ChangeEmotion(emotionsDropdown);

    }

    public void ChangeEmotion(TMP_Dropdown dropdown)
    {
        pack.emotion = dropdown.options[dropdown.value].text;
        dialogueFile.GetLines()[group].characters[id] = pack;

        if (character != null)
            character.expression = pack.emotion; 
    }

    public void SetAlignment(TMP_Dropdown dropdown)
    {
        string option = dropdown.options[dropdown.value].text;
        pack.alignment = (Alignment)Enum.Parse(typeof(Alignment), option);
        dialogueFile.GetLines()[group].characters[id] = pack;

        float xPos = 0;
        switch(pack.alignment)
        {
            case Alignment.left:
                xPos = -325;
                break;
            case Alignment.right:
                xPos = 325;
                break;
            default:
                xPos = 0;
                break;
        }
        if (character != null)
            character.position = new Vector2(xPos, 0);
    }

    public void SetOffset(float offset)
    {
        pack.offset = offset;
        dialogueFile.GetLines()[group].characters[id] = pack;

        if (character != null)
            character.SetXOffset(offset);

        //Debug.Log(character.position);
    }

    public void SetFlipX(bool flip)
    {
        pack.flipX = flip;
        dialogueFile.GetLines()[group].characters[id] = pack;
        if (character != null)
            character.flipX = flip;
    }

    public void SetIsSpeaking(bool speaker)
    {
        pack.isSpeaking = speaker;
        dialogueFile.GetLines()[group].characters[id] = pack;
    }

    public void UpdateList()
    {
        int value = charactersDropdown.value;
        charactersDropdown.ClearOptions();
        charactersDropdown.AddOptions(characters);
        if(value >= charactersDropdown.options.Count)
        {
            charactersDropdown.value = value - 1;
            charactersDropdown.RefreshShownValue();
        }
    }
}
