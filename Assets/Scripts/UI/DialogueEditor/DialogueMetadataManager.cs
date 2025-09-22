using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rabbyte;

public class DialogueMetadataManager : OptionSelection
{
    public TMP_InputField displayName;
    public TMP_InputField chapter;
    public TMP_InputField volume;
    public TMP_Dropdown type;

    public Button addMusic;
    public Button playMusic;
    public Button removeMusic;

    Image playButton;

    public TMP_Dropdown character;
    public Button addCharacter;
    public Button removeCharacter;

    public TMP_InputField description;

    public Button loadFile;
    public Button saveFile;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if(manager != null)
        {
            displayName.onValueChanged.AddListener(manager.SetDisplayName);
            volume.onValueChanged.AddListener(manager.SetVolume);
            chapter.onValueChanged.AddListener(manager.SetChapter);

            type.ClearOptions();
            type.AddOptions(new List<string>(System.Enum.GetNames(typeof(StoryType))));

            addCharacter.onClick.AddListener(manager.AddCharacter);
            removeCharacter.onClick.AddListener(manager.RemoveCharacter);

            description.onValueChanged.AddListener(manager.SetDescription);

            playButton = playMusic.transform.GetChild(0).GetComponent<Image>();

            addMusic.onClick.AddListener(delegate { 
                manager.AddMusic(addMusic.GetComponentInChildren<TMP_Text>());
                LoadPlayIcon();
            });

            playMusic.onClick.AddListener(delegate { manager.PlayMusic(); LoadPlayIcon(); });
            removeMusic.onClick.AddListener(delegate {
                manager.RemoveMusic();
                addMusic.GetComponentInChildren<TMP_Text>().text = "Add Music";
                LoadPlayIcon();
            });

            manager.SetType(type);

            type.onValueChanged.AddListener(delegate {
                manager.SetType(type);
            });

            loadFile.onClick.AddListener(manager.LoadFile);
            saveFile.onClick.AddListener(manager.SaveFile);

            UpdateCharacterList();
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if(manager != null)
        {
            if(playButton != null)
            {

            }
        }
    }

    public override void Load(SimpleSBDFile sBDFile)
    {
        displayName.text = sBDFile.displayName;
        volume.text = sBDFile.volume.ToString();
        chapter.text = sBDFile.chapter.ToString();
        type.value = new List<string>(System.Enum.GetNames(typeof(StoryType))).IndexOf(sBDFile.type.ToString());
        UpdateCharacterList();
        description.text = sBDFile.description;
        if(sBDFile.music != null)
        {
            addMusic.GetComponentInChildren<TMP_Text>().text = sBDFile.music.name;
            LoadPlayIcon();

        }
    }

    void LoadPlayIcon()
    {
        if (manager != null && playButton != null)
        {
            string pause = "pause_button";
            string play = "play_button";
            Texture2D text = Resources.Load<Texture2D>($"Sprites/{(manager.musicSource.isPlaying ? pause : play)}");
            Sprite sprite = Sprite.Create(text, new Rect(0.0f, 0.0f, text.width, text.height), new Vector2(0.5f, 0.5f), 100.0f);
            playButton.sprite = sprite;
        }
    }

    public void UpdateCharacterList()
    {
        character.ClearOptions();
        character.AddOptions(manager.characterList);
    }
}
