using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using SFB;
using Rabbyte;
using TMPro;
using Whisper;

public class DialogueManager : Draggable
{
    public SimpleSBDFile curFile;
    public SimpleSBDFile loadedFile;

    public Image image;
    bool isHiding = false;
    bool firstLoad = true;

    bool newFile = true;
    string filename;
    string filepath;

    #region UI Elements
    [Header("UI Elements")]
    public Button metadataButton;
    public Button dialogueButton;
    public Button charactersButton;

    public DialogueMetadataManager metadata;
    public DialogueLineManager dialogue;
    public DialogueCharacterPackManager characters;

    [HideInInspector]
    public Transform sprites;

    [HideInInspector]
    public List<SBCFile> characterFiles = new List<SBCFile>();

    [HideInInspector]
    public List<string> characterList = new List<string>();

    [HideInInspector]
    public List<string> bgList = new List<string>();

    public Image bgImage;
    public TMP_Text dialogueText;

    [HideInInspector]
    public Dictionary<int, List<DialogueCharacterPack>> packs = new Dictionary<int, List<DialogueCharacterPack>>();

    public Dictionary<int, GameObject> groups = new Dictionary<int, GameObject>();


    public AudioSource musicSource;
    public AudioSource dialogueSource;

    [HideInInspector]
    public Dictionary<int, AudioClip> dialogueClips = new Dictionary<int, AudioClip>();

    WhisperManager whisper;

    public GameObject loadingIcon;

    public enum DialogueOptions
    {
        Metadata,
        Dialogue,
        Characters
    }

    DialogueOptions curOption = DialogueOptions.Metadata;
    #endregion

    #region Raycasters
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    #endregion

    #region Binary Data
    [Header("Other Data")]
    [RangeStep(100f, 800f, 100f)]
    public int weight = 400;
    #endregion

    [System.Serializable]
    public class Dict
    {
        public int id;
        public GameObject clip;

        public Dict(int id, GameObject clip)
        {
            this.id = id;
            this.clip = clip;
        }
    }

    [HideInInspector]
    public List<Dict> dialogues = new List<Dict>();
    // Start is called before the first frame update
    void Awake()
    {
        curFile = new SimpleSBDFile();
        sprites = GameObject.Find("Sprites").transform;
        dialogueClips.Add(0, null);
        CreateCharacterGroup(0);
        whisper = FindObjectOfType<WhisperManager>();
    }
    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = FindObjectOfType<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();

        if (loadingIcon != null) loadingIcon.SetActive(false);

        if (metadataButton != null) metadataButton.onClick.AddListener(() => { SetManager(DialogueOptions.Metadata); });
        if (dialogueButton != null) dialogueButton.onClick.AddListener(() => { SetManager(DialogueOptions.Dialogue); });
        if (charactersButton != null) charactersButton.onClick.AddListener(() => { SetManager(DialogueOptions.Characters); });

        SetManager(curOption);
    }

    bool startDoubleClick = false;
    float startTime = 0;
    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if(isHovering)
        {
            if (Input.GetMouseButtonDown(0))
            {
                #region Hide The Editor
                m_PointerEventData = new PointerEventData(m_EventSystem);
                m_PointerEventData.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();

                m_Raycaster.Raycast(m_PointerEventData, results);

                foreach (RaycastResult result in results)
                {
                    if(result.gameObject == metadataButton.gameObject || result.gameObject == dialogueButton.gameObject | result.gameObject == charactersButton.gameObject)
                    {
                        if (startDoubleClick)
                        {
                            isHiding = !isHiding;
                            startTime = 0;
                            startDoubleClick = false;
                            if (!isHiding)
                                SetManager(curOption);
                        }
                        else
                            startDoubleClick = true;
                    }
                }
                #endregion
            }
        }

        if (startDoubleClick)
        {
            startTime += Time.unscaledDeltaTime;
        }

        if(startTime >= 0.25f)
        {
            startDoubleClick = false;
            startTime = 0;
        }

        image.color = isHiding ? new Color(image.color.r, image.color.g, image.color.b, 0) : new Color(image.color.r, image.color.g, image.color.b, 200f/255f);
        
        if (isHiding)
        {
            if (metadata != null)
            {
                if(firstLoad) metadata.gameObject.SetActive(true);
                metadata.gameObject.SetActive(false);
            }
            if (dialogue != null)
            {
                if (firstLoad) dialogue.gameObject.SetActive(true);
                dialogue.gameObject.SetActive(false);
            }
            if (characters != null)
            {
                if (firstLoad) characters.gameObject.SetActive(true);
                characters.gameObject.SetActive(false);
            }
        }

        firstLoad = false;

        if(!UIUtils.inFieldFocus(FindObjectsOfType<TMP_InputField>()))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                ChangeLine(-1);

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                ChangeLine(1);
        }

        dialogues.Clear();
        foreach (KeyValuePair<int, GameObject> dialogue in groups)
        {
            dialogues.Add(new Dict(dialogue.Key, dialogue.Value));
        }

        //Shortcuts
        bool modifier = 
            Input.GetKey(KeyCode.LeftControl) || 
            Input.GetKey(KeyCode.RightControl) || 
            Input.GetKey(KeyCode.LeftCommand) || 
            Input.GetKey(KeyCode.RightCommand);

        if(modifier)
        {
            if(Input.GetKeyDown(KeyCode.S))
            {
                if (newFile)
                    SaveFilePanel();
                else
                    SaveFile(filepath, filename);
            }
        }
    }

#region Main Buttons
    public void SetManager(DialogueOptions option)
    {
        isHiding = false;
        if (metadata != null) metadata.gameObject.SetActive(false);
        if (dialogue != null) dialogue.gameObject.SetActive(false);
        if (characters != null) characters.gameObject.SetActive(false);

        metadataButton.interactable = true;
        dialogueButton.interactable = true;
        charactersButton.interactable = true;

        switch(option)
        {
            case DialogueOptions.Metadata:
                if (metadata != null)
                {
                    metadata.gameObject.SetActive(true);
                    metadata.Load(curFile);
                }
                metadataButton.interactable = false;
                break;
            case DialogueOptions.Dialogue:
                if (dialogue != null)
                {
                    dialogue.gameObject.SetActive(true);
                    dialogue.Load(curFile);
                }
                dialogueButton.interactable = false;
                break;
            case DialogueOptions.Characters:
                if (characters != null)
                {
                    characters.gameObject.SetActive(true);
                    characters.Load(curFile);
                }
                charactersButton.interactable = false;
                break;
        }

        curOption = option;
    }

    public void LoadCharacters()
    {
        characterList.Clear();
        characterFiles.Clear();
        foreach(KeyValuePair<string, List<Emotion>> character in curFile.GetCharacters())
        {
            SBCFile characterFile = new SBCFile(character.Key, true);
            foreach(Emotion emotion in character.Value)
            {
                characterFile.addExpression(emotion.expression, emotion.sprite, emotion.scale, emotion.offset[0], emotion.offset[1]);
            }
            CharacterSprite characterSprite = new GameObject(character.Key).AddComponent<CharacterSprite>();
            characterSprite.transform.parent = sprites;
            characterSprite.character = characterFile;
            characterSprite.rectTransform.anchoredPosition = Vector2.zero;
            characterSprite.rectTransform.localScale = Vector3.one;
            characterSprite.gameObject.SetActive(false);
            characterFiles.Add(characterFile);
            characterList.Add(character.Key);
        }
        if (metadata != null) metadata.UpdateCharacterList();
    }
#endregion

#region Metadata Functions
    public void SetDisplayName(string value)
    {
        curFile.displayName = value;
    }

    public void SetVolume(string value)
    {
        curFile.volume = int.Parse(value);
    }

    public void SetChapter(string value)
    {
        curFile.chapter = int.Parse(value);
    }

    public void SetType(TMP_Dropdown dropdown)
    {
        curFile.type = (StoryType)Enum.Parse(typeof(StoryType), dropdown.options[dropdown.value].text);
    }

    public void SetDescription(string value)
    {
        curFile.description = value;
    }

    public void AddCharacter()
    {
        var extensions = new[]
{
            new ExtensionFilter("Character Files", "sbc")
        };
        StandaloneFileBrowser.OpenFilePanelAsync("Load Character", "", extensions, false, async (string[] paths) =>
        {
            if (paths.Length > 0)
            {
                try
                {
                    var filename = paths[0].Split("\\")[paths[0].Split("\\").Length - 1];
                    filename = filename.Remove(filename.Length - 4);
                    var path = StarbornFileHandler.ExtractCharacter(paths[0]);
                    SBCFile character = StarbornFileHandler.ReadCharacter(filename);
                    if(curFile.AddCharacter(character))
                    {
                        CharacterSprite characterSprite = new GameObject(filename).AddComponent<CharacterSprite>();
                        characterSprite.transform.parent = sprites;
                        characterSprite.character = character;
                        characterSprite.rectTransform.anchoredPosition = Vector2.zero;
                        characterSprite.rectTransform.localScale = Vector2.one;
                        characterSprite.gameObject.SetActive(false);
                        characterFiles.Add(character);
                        characterList.Add(filename);
                        if (metadata != null) metadata.UpdateCharacterList();
                    }
                    //Debug.Log(curFile.AddCharacter(character));
                    /*var path = StarbornFileHandler.ExtractDialogue(paths[0]);
                    curFile = StarbornFileHandler.ReadSimpleDialogue(filename);
                    if (metadata != null) metadata.Load(curFile);
                    if (dialogue != null) dialogue.Load(curFile);*/
                    //loadedFile = new(curFile);

                }
                catch (System.Exception e)
                {
                    return;
                }
            }
            await Task.Yield();
        });
    }

    public void RemoveCharacter()
    {
        if (characterFiles.Count == 0)
            return;

        if(metadata != null)
        {
            string character = metadata.character.options[metadata.character.value].text;
            curFile.RemoveCharacter(character);
            characterList.Remove(character);
            foreach(SBCFile charac in characterFiles)
            {
                if(charac.filename == character)
                {
                    characterFiles.Remove(charac);
                    break;
                }
            }
            foreach(Transform sprite in sprites)
            {
                if(sprite.gameObject.name == character)
                {
                    Destroy(sprite.gameObject);
                    break;
                }
            }

            metadata.UpdateCharacterList();
        }
    }

    public void AddMusic(TMP_Text text, Button button = null)
    {
        var extensions = new[]
        {
            new ExtensionFilter("Audio Files", "mp3", "wav", "ogg", "acc", "flac", "aiff")
        };
        StandaloneFileBrowser.OpenFilePanelAsync("Load Audio", "", extensions, false, async (string[] paths) =>
        {
            if (paths.Length > 0)
            {
                if (loadingIcon != null) loadingIcon.SetActive(true);
                try
                {
                    if (button != null) button.enabled = false;
                    curFile.music = new AudioByte(paths[0]);
                    musicSource.clip = await AudioUtils.LoadMusic(curFile.music);
                    await Task.Delay(500);
                    text.text = curFile.music.name;
                }
                catch(SystemException e)
                {

                }
            }
            if (loadingIcon != null) loadingIcon.SetActive(false);
            if (button != null) button.enabled = true;
        });
    }

    public void PlayMusic()
    {
        if(musicSource.clip != null)
            if (musicSource.isPlaying)
                musicSource.Pause();
            else
                musicSource.Play();
    }

    public void RemoveMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
        musicSource.clip = null;
        curFile.music = null;

    }

    public void LoadFile()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Dialogue Files", "sbd")
        };
        StandaloneFileBrowser.OpenFilePanelAsync("Load Dialogue", "", extensions, false, async (string[] paths) =>
        {
            if (paths.Length > 0)
            {
                if (loadingIcon != null) loadingIcon.SetActive(true);

                try
                {
                    var filename = paths[0].Split("\\")[paths[0].Split("\\").Length - 1];
                    filename = filename.Remove(filename.Length - 4);
                    var path = StarbornFileHandler.ExtractDialogue(paths[0]);
                    curFile = StarbornFileHandler.ReadSimpleDialogue(filename);

                    this.filename = filename;
                    filepath = paths[0];
                    newFile = false;

                    //Add in characters
                    LoadCharacters();
                    //Add in backgrounds
                    bgList.Clear();

                    foreach (string bg in curFile.GetBackgrounds().Keys)
                    {
                        bgList.Add(bg);
                    }

                    bgList = bgList.Distinct().ToList();

                    dialogueClips.Clear();
                    groups.Clear();
                    packs.Clear();

                    // Create character sprites for each characters
                    int i = 0;
                    if (characters != null)
                    {
                        foreach (Transform child in characters.characterPacksParent)
                        {
                            Destroy(child.gameObject);
                        }
                    }

                    // Add in dialogue audio clips
                    foreach (BetaDialogueSequence dialogue in curFile.GetLines())
                    {
                        if (dialogue.audio != null)
                            dialogueClips.Add(i, await AudioUtils.LoadDialogue(dialogue.audio));
                        else
                            dialogueClips.Add(i, null);


                        CreateCharacterGroup(i);
                        characters.positionPlacer.anchoredPosition = characters.defaultPosition;
                        for (int j = 0; j < dialogue.characters.Count; j++)
                        {
                            GameObject charPack = Instantiate(characters.packPrefab, Vector3.zero, Quaternion.identity);
                            charPack.name = "Character";
                            DialogueCharacterPack pack = charPack.GetComponent<DialogueCharacterPack>();
                            packs[i].Add(pack);
                            //Out of bounds
                            charPack.GetComponent<DialogueCharacterPack>().SetCharacterPack(dialogue.characters[j], curFile, characters, i, j, dialogue.characters[j].character);
                            charPack.transform.parent = groups[i].transform;
                            charPack.transform.localScale = Vector3.one;
                            charPack.GetComponent<RectTransform>().anchoredPosition = characters.placement;
                            characters.positionPlacer.anchoredPosition -= new Vector2(0, charPack.GetComponent<RectTransform>().sizeDelta.y + 10);
                        }
                        i++;
                    }

                    //Update metadata
                    if (metadata != null)
                    {
                        metadata.gameObject.SetActive(true);
                        metadata.Load(curFile);
                        if (curOption != DialogueOptions.Metadata) metadata.gameObject.SetActive(false);
                    }
                    //Update dialogue data
                    if (dialogue != null)
                    {
                        dialogue.gameObject.SetActive(true);
                        dialogue.Load(curFile);
                        if (curOption != DialogueOptions.Dialogue) dialogue.gameObject.SetActive(false);
                    }
                    dialogueText.text = curFile.text;

                    //Update character data per line
                    if (characters != null)
                    {
                        characters.gameObject.SetActive(true);
                        characters.Load(curFile);
                        if (curOption != DialogueOptions.Characters) characters.gameObject.SetActive(false);

                        foreach (Transform character in sprites.transform)
                        {
                            character.gameObject.SetActive(false);
                        }

                        foreach(KeyValuePair<int, List<DialogueCharacterPack>> p in packs)
                        {
                            foreach(DialogueCharacterPack pack in p.Value)
                            {
                                pack.UISetUp();
                                if (!pack.hasLoaded)
                                {
                                    pack.ManagerSetup();
                                }
                            }
                        }
                        for (int k = 0; k < curFile.characterPack.Count; k++)
                        {
                            CharacterSprite character = null;
                            foreach(CharacterSprite pack in FindObjectsOfType<CharacterSprite>(true))
                            {
                                if(pack.charName == curFile.characterPack[k].character)
                                {
                                    character = pack;
                                    break;
                                }

                                //if(pack.character)
                            }
                            
                            /*packs[curFile.id][k].UISetUp();
                            if (!packs[curFile.id][k].hasLoaded)
                            {
                                packs[curFile.id][k].ManagerSetup();
                            }*/

                            if (character != null)
                            {
                                character.gameObject.SetActive(true);
                                character.flipX = packs[curFile.id][k].pack.flipX;
                                character.expression = packs[curFile.id][k].pack.emotion;
                                Alignment align = packs[curFile.id][k].pack.alignment;
                                float xPos = 0;
                                switch (align)
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
                                character.position = new Vector2(xPos, 0);
                                character.SetXOffset(packs[curFile.id][k].pack.offset);
                                //character.position;
                            }

                            float y = packs[curFile.id][k].GetComponent<RectTransform>().sizeDelta.y + 10;
                            characters.positionPlacer.anchoredPosition -= new Vector2(0, y);
                            characters.addButton.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, y);

                        }
                    }

                    // Load in music file if exists
                    if (curFile.music != null)
                        musicSource.clip = await AudioUtils.LoadMusic(curFile.music);

                    SetDialogueClip(dialogueClips[curFile.id]);
                }
                catch (System.Exception e)
                {
                    StackTrace st = new StackTrace(e, true);

                    // Get the top stack frame (where the exception was thrown)
                    StackFrame frame = st.GetFrame(0);

                    // Get the line number from the stack frame
                    int line = frame.GetFileLineNumber();

                    // Get the file name
                    string fileName = frame.GetFileName();

                    // Log or use the information
                    UnityEngine.Debug.LogError($"Exception occurred in {fileName} at line {line}: {e.Message}");
                    return;
                }
            }
            await Task.Yield();
            if (loadingIcon != null) loadingIcon.SetActive(false);
        });
    }

    public void SaveFilePanel()
    {
        StandaloneFileBrowser.SaveFilePanelAsync("Save Dialogue File", "", (curFile.fileName != "" || curFile.fileName != null) ? curFile.fileName : "dialogue", "sbd", (string path) => {
            //Debug.Log(path.Length);
            if (path.Length != 0)
            {
                var filename = path.Split("\\")[path.Split("\\").Length - 1];
                filename = filename.Remove(filename.Length - 4);
                SaveFile(path, filename);

                return;
            }

        });
    }

    async void SaveFile(string filepath, string filename)
    {
        if (loadingIcon != null) loadingIcon.SetActive(true);
        curFile.fileName = filename;
        StarbornFileHandler.WriteSimpleDialogue(curFile, filename);
        StarbornFileHandler.PackDialogue(filepath);
        await Task.Delay(500);
        this.filename = filename;
        this.filepath = filepath;

        newFile = false;
        if (loadingIcon != null) loadingIcon.SetActive(false);
    }

    public void CreateNewFile()
    {
        bgList.Clear();
        dialogueClips.Clear();
        groups.Clear();
        packs.Clear();

        if (characters != null)
        {
            foreach (Transform child in characters.characterPacksParent)
            {
                Destroy(child.gameObject);
            }
        }

        filename = "";
        filepath = "";
        newFile = true;

    }
#endregion

#region Dialogue Line Functions
    public void AddLine()
    {
        BetaDialogueSequence curLine = curFile.curLine;
        curFile.InsertLineByValues(curLine.id + 1, curLine.name, "", curLine.background, curFile.minigame);
        //Remove any lines from files ahead
        int start = curLine.id + 2;
        for (int i = start; i < curFile.GetLines().Count; i++)
        {
            if (dialogueClips.ContainsKey(curFile.GetLines()[i].id - 1))
            {
                AudioClip clip = dialogueClips[curFile.GetLines()[i].id - 1];
                dialogueClips.Remove(curFile.GetLines()[i].id - 1);
                dialogueClips.Add(i, clip);
            }

            if(groups.ContainsKey(curFile.GetLines()[i].id - 1))
            {
                GameObject group = groups[curFile.GetLines()[i].id - 1];
                groups.Remove(curFile.GetLines()[i].id - 1);
                groups.Add(i, group);
                groups[i].name = i.ToString();
            }

            if (packs.ContainsKey(curFile.GetLines()[i].id - 1))
            {
                List<DialogueCharacterPack> pack = packs[curFile.GetLines()[i].id - 1];
                packs.Remove(curFile.GetLines()[i].id - 1);
                packs.Add(i, pack);
            }
        }
        if(dialogueClips.ContainsKey(curLine.id + 1))
            dialogueClips.Remove(curLine.id + 1);
        dialogueClips.Add(curLine.id + 1, null);

        if (groups.ContainsKey(curLine.id + 1))
            groups.Remove(curLine.id + 1);

        if (packs.ContainsKey(curLine.id + 1))
            packs.Remove(curLine.id + 1);

        if (dialogue != null) dialogue.Load(curFile);
        if(characters != null)
        {
            //CreateCharacterGroup(curLine.id + 1);
            CloneCharacterGroup(curLine.id + 1, groups[curLine.id]);
        }

    }
    public void RemoveLine(int id)
    {
        curFile.RemoveLineAtIndex(id);
        dialogueClips.Remove(id);
        GameObject oldGroup = groups[id];
        packs.Remove(id);
        groups.Remove(id);
        Destroy(oldGroup);

        for (int i = id; i < curFile.GetLines().Count; i++)
        {
            if (dialogueClips.ContainsKey(curFile.GetLines()[i].id))
            {
                AudioClip clip = dialogueClips[curFile.GetLines()[i].id];
                dialogueClips.Remove(curFile.GetLines()[i].id);
                dialogueClips.Add(i, clip);
            }
            if (groups.ContainsKey(curFile.GetLines()[i].id - 1))
            {
                GameObject group = groups[curFile.GetLines()[i].id - 1];
                groups.Remove(curFile.GetLines()[i].id - 1);
                groups.Add(i, group);
                groups[i].name = i.ToString();
            }
            if(packs.ContainsKey(curFile.GetLines()[i].id - 1))
            {
                List<DialogueCharacterPack> pack = packs[curFile.GetLines()[i].id - 1];
                packs.Remove(curFile.GetLines()[i].id - 1);
                packs.Add(i, pack);
            }
            curFile.GetLines()[i].id = i;

        }
        
        if(curFile.GetLines().Count == 0)
        {
            dialogueClips.Clear();
            curFile.AddLine();
            dialogueClips.Add(0, null);
            CreateCharacterGroup(0);
        }
        else if(id >= curFile.GetLines().Count - 1)
        {
            curFile.SetDialogue(curFile.GetLines().Count - 1);
        }

        if (dialogue != null) dialogue.Load(curFile);
    }
    public void ChangeLine(int i)
    {
        curFile.ChangeDialogue(i);
        if (dialogue != null) dialogue.Load(curFile);
        if (characters != null)
        {
            characters.Load(curFile);
            characters.ChangeSelections();
        }
        //dialogueText.text = $"<font-weight = {weight}>{}</font-weight>";
        dialogueText.text = curFile.text;
        SetDialogueClip(dialogueClips[curFile.id]);

    }

    public void EditName(string name)
    {
        curFile.name = name;
    }

    public void EditTextDialogue(string text)
    {
        curFile.text = text;
        dialogueText.text = text;
    }

    public void SetBackground(TMP_Dropdown dropdown)
    {
        curFile.background = dropdown.options[dropdown.value].text;
    }

    public void SetAutoSkip(bool toggle)
    {
        curFile.autoSkip = toggle;
    }

    public void SetMinigame(TMP_Dropdown dropdown)
    {
        curFile.minigame = dropdown.value != 0 ? dropdown.options[dropdown.value].text : "";
    }

    public void AddDialogueAudio(TMP_InputField field = null, TMP_Text button = null)
    {
        var extensions = new[]
        {
            new ExtensionFilter("Audio Files", "mp3", "wav", "ogg", "acc", "flac", "aiff")
        };
        StandaloneFileBrowser.OpenFilePanelAsync("Load Audio", "", extensions, false, async (string[] paths) =>
        {
            if (paths.Length > 0)
            {
                try
                {
                    dialogueSource.Stop();
                    curFile.audio = new AudioByte(paths[0]);
                    dialogueClips[curFile.id] = await AudioUtils.LoadDialogue(curFile.audio);
                    if (loadingIcon != null) loadingIcon.SetActive(true);
                    if(whisper != null)
                    {
                        WhisperResult result = await whisper.GetTextAsync(dialogueClips[curFile.id]);
                        string text = result.Result;
                        if (text[0] == ' ') text = text.Substring(1);
                        /*if(dialogue != null)
                        {
                            dialogue.gameObject.SetActive(true);
                            if(dialogue.textField.text == "")
                                dialogue.textField.text = text;
                            if (curOption != DialogueOptions.Dialogue) dialogue.gameObject.SetActive(false);
                        }*/

                        if (field != null) field.text = text;
                        if (button != null) button.text = curFile.audio.name;
                    }
                    SetDialogueClip(dialogueClips[curFile.id]);

                }
                catch (SystemException e)
                {

                }
            }

            if (loadingIcon != null) loadingIcon.SetActive(false);
        });
    }

    void SetDialogueClip(AudioClip audio)
    {
        if(audio != null)
        {
            dialogueSource.clip = audio;
            dialogueSource.Play();
        }
    }
    public void PlayDialogueClip()
    {
        if (dialogueSource.isPlaying)
        {
            dialogueSource.Stop();
            return;
        }

        dialogueSource.Play();
    }

    public void RemoveDialogueClip()
    {
        if (dialogueSource.isPlaying)
            dialogueSource.Stop();
        dialogueSource.clip = null;
        dialogueClips[curFile.id] = null;
    }

    public void AddBackground()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Image Files", "jpg", "png")
        };
        StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) =>
        {
            if (paths.Length > 0)
            {
                string filename = paths[0].Split("\\")[paths[0].Split("\\").Length - 1];
                filename = filename.Remove(filename.Length - 4);
                try
                {
                    byte[] imageData = File.ReadAllBytes(paths[0]);
                    if (!curFile.GetBackgrounds().ContainsKey(paths[0]) || !curFile.HasExistingBGBytes(imageData))
                    {
                        curFile.AddBackground(filename, imageData);
                        bgList.Add(filename);
                        bgList = bgList.Distinct().ToList();
                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(imageData);
                        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        bgImage.sprite = sprite;

                        if (dialogue != null)
                        {
                            dialogue.gameObject.SetActive(true);
                            dialogue.UpdateBackgroundList();
                            dialogue.SetBackground(filename);
                            SetBackground(dialogue.background);
                            if (curOption != DialogueOptions.Dialogue) dialogue.gameObject.SetActive(false);
                        }
                    }
                    return;
                }
                catch (System.Exception e)
                {

                    return;
                }
            }
            await Task.Yield();
        });
    }
#endregion


#region Character Packs
    public void CreateCharacterGroup(int id)
    {
        GameObject newGroup = new GameObject(id.ToString());
        packs.Add(id, new List<DialogueCharacterPack>());
        if (characters != null)
        {
            characters.gameObject.SetActive(true);
            newGroup.transform.parent = characters.characterPacksParent;
            newGroup.AddComponent<RectTransform>();
            newGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            newGroup.transform.localScale = Vector3.one;
            groups.Add(id, newGroup);

            if (curOption != DialogueOptions.Characters) characters.gameObject.SetActive(false);
        }

    }

    public void CloneCharacterGroup(int id, GameObject prefab)
    {
        GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        clone.name = id.ToString();

        List<DialogueCharacterPack> clonedPack = new List<DialogueCharacterPack>();
        foreach(Transform child in clone.transform)
        {
            if(child.GetComponent<DialogueCharacterPack>() != null)
            {
                clonedPack.Add(child.GetComponent<DialogueCharacterPack>());
            }
        }

        //Debug.Log(clonedPack.Count);
        packs.Add(id, clonedPack);
        if (characters != null)
        {
            characters.gameObject.SetActive(true);
            clone.transform.parent = characters.characterPacksParent;
            clone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            clone.transform.localScale = Vector3.one;
            groups.Add(id, clone);

            //curFile.characterPack = clonedPack;

            for(int i = 0; i < clonedPack.Count; i++)
            {
                clonedPack[i].SetCharacterPack(clonedPack[i].pack, curFile, characters, id, i);
                clonedPack[i].RemoveAllListeners();
                curFile.GetLines()[id].characters.Add(clonedPack[i].pack);
                clonedPack[i].ManagerSetup();
            }

            if (curOption != DialogueOptions.Characters) characters.gameObject.SetActive(false);
        }
    }
#endregion

}
