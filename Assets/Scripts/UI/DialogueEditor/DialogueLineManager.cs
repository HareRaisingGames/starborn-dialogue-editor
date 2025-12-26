using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rabbyte;

public class DialogueLineManager : OptionSelection
{
    public TMP_InputField nameField;
    public TMP_InputField textField;
    public TMP_Dropdown background;
    public Button removeBackground;
    public TMP_Dropdown minigame;
    public Button audioButton;
    Image playButton;
    public Button playAudio;
    public Button removeAudio;
    public Toggle autoPlay;
    public TMP_Text lines;
    public Button addLine;
    public Button removeLine;
    // Start is called before the first frame update

    List<string> bgList = new List<string>();
    public override void Start()
    {
        base.Start();
        if(manager != null)
        {
            lines.text = "Lines " + (manager.curFile.GetLines().IndexOf(manager.curFile.curLine) + 1) + "/" + manager.curFile.GetLines().Count;

            nameField.onValueChanged.AddListener(manager.EditName);
            textField.onValueChanged.AddListener(manager.EditTextDialogue);

            autoPlay.onValueChanged.AddListener(manager.SetAutoSkip);

            addLine.onClick.AddListener(manager.AddLine);

            removeLine.onClick.AddListener(delegate
            {
                manager.RemoveLine(manager.curFile.id);
            });

            UpdateBackgroundList();

            minigame.ClearOptions();
            minigame.AddOptions(new List<string>(Enum.GetNames(typeof(Minigame))));
            minigame.onValueChanged.AddListener(delegate
            {
                manager.SetMinigame(minigame);
            });

            UpdateMinigameList();

            playButton = playAudio.transform.GetChild(0).GetComponent<Image>();

            audioButton.onClick.AddListener(delegate
            {
                manager.AddDialogueAudio(textField, audioButton.GetComponentInChildren<TMP_Text>());
            });

            playAudio.onClick.AddListener(delegate
            {
                manager.PlayDialogueClip();
            });

            removeAudio.onClick.AddListener(delegate
            {
                manager.RemoveDialogueClip();
                audioButton.GetComponentInChildren<TMP_Text>().text = "Add";
            });

            background.onValueChanged.AddListener(delegate {
                manager.SetBackground(background);

                if (manager.curFile.background != null)
                {
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(manager.curFile.getBackground());
                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    manager.bgImage.sprite = sprite;
                    DialogueUtils.SetImageFixedPosition(manager.bgImage);
                }
            });

            removeBackground.onClick.AddListener(manager.RemoveBackground);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (manager != null)
            lines.text = "Lines " + (manager.curFile.GetLines().IndexOf(manager.curFile.curLine) + 1) + "/" + manager.curFile.GetLines().Count;

        if (Input.GetKeyDown(KeyCode.Tab))
            textField.text = "";

        LoadPlayIcon();
    }

    public override void Load(SimpleSBDFile sBDFile)
    {
        if (manager != null)
        {
            if(!StringUtils.CompareLists(bgList, manager.bgList))
            {
                background.ClearOptions();
                background.AddOptions(manager.bgList);
                SetBackground(sBDFile.background);
            }

            bgList = StringUtils.CopyList(manager.bgList);
            background.RefreshShownValue();

            if (sBDFile.background != null && sBDFile.background != "")
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(sBDFile.getBackground());
                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                manager.bgImage.sprite = sprite;
                DialogueUtils.SetImageFixedPosition(manager.bgImage);
            }
            else
            {
                manager.bgImage.sprite = null;
                manager.bgImage.rectTransform.sizeDelta = DialogueManager.mainImageDimensions;
            }

            //background.value = background.options.IndexOf(sBDFile.background);
        }


        //minigame.value = sBDFile.minigame != "" && sBDFile.minigame != null ? (int)(Minigame)Enum.Parse(typeof(Minigame), sBDFile.minigame) : 0;
        
        UpdateMinigameList();
        nameField.text = sBDFile.name;
        textField.text = sBDFile.text;
        autoPlay.isOn = sBDFile.autoSkip;
        lines.text = "Lines " + (sBDFile.GetLines().IndexOf(sBDFile.curLine) + 1) + "/" + sBDFile.GetLines().Count;
        audioButton.GetComponentInChildren<TMP_Text>().text = sBDFile.audio != null ? sBDFile.audio.name : "Add";

        LoadPlayIcon();

    }
    void LoadPlayIcon()
    {
        if (manager != null && playButton != null)
        {
            string pause = "stop_button";
            string play = "play_button";
            Texture2D text = Resources.Load<Texture2D>($"Sprites/{(manager.dialogueSource.isPlaying ? pause : play)}");
            Sprite sprite = Sprite.Create(text, new Rect(0.0f, 0.0f, text.width, text.height), new Vector2(0.5f, 0.5f), 100.0f);
            playButton.sprite = sprite;
        }
    }
    public void SetBackground(string name)
    {
        background.value = UIUtils.GetDropdownValueByName(background, name);
        background.RefreshShownValue();
    }

    public void UpdateBackgroundList()
    {
        if(manager != null)
        {
            background.ClearOptions();
            background.AddOptions(manager.bgList);
            SetBackground(manager.curFile.background);
        }
    }

    void UpdateMinigameList()
    {
        if(manager != null)
        {
            minigame.value = UIUtils.GetDropdownValueByName(minigame, manager.curFile.minigame);
            minigame.RefreshShownValue();
        }
    }
}
