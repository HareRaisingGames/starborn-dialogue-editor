using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using System.IO;
using SFB;
using Rabbyte;
using TMPro;
using UnityEngine.Events;

public class CharacterEditor : MonoBehaviour
{
    public SBCFile curFile;
    public SBCFile loadedFile;
    public Emotion phantom;

    public Image spriteHolder;
    public TMP_InputField characterNameField;
    public TMP_InputField expressionNameField;
    public TMP_Dropdown expressionsDropdown;
    public UIStepper scaleStepper;
    public UIStepper xStepper;
    public UIStepper yStepper;
    public Button saveCharacter;
    public Button loadCharacter;

    public Emotion curEmotion;

    public List<string> backupList;
    public GameObject warningMessage;
    public Button yes;

    Vector2 defaultSize = new Vector2(150, 150);
    Vector2 spriteSize;

    //Vector2 defaultOffsets = new Vector2(0, 0);

    public TMP_Dropdown ghostDropdown;
    public Image ghostSprite;

    // Start is called before the first frame update
    void Start()
    {
        curFile = new SBCFile(false);
        loadedFile = new SBCFile(false);
        backupList = new List<string>() { "" };
        curFile.curExp = 0;

        if (ghostSprite != null) ghostSprite.gameObject.SetActive(false);
        if (warningMessage != null) warningMessage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddImage()
    {
        var extensions = new[]
            {
            new ExtensionFilter("Image Files", "jpg", "png")
        };
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) =>
            {
                if (paths.Length > 0)
                {
                    try
                    {
                        byte[] imageData = File.ReadAllBytes(paths[0]);
                        curFile.setImage(imageData);
                        SetSprite(imageData);
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

    public void EditFileName()
    {
        curFile.filename = characterNameField.text;
    }

    public void EditEmotionName()
    {
        curFile.setName(expressionNameField.text);
        expressionsDropdown.captionText.text = curFile.curEmotion.expression;
        expressionsDropdown.options[curFile.curExp].text = curFile.curEmotion.expression;
        //Debug.Log(backupList.Count);
        backupList[curFile.curExp] = curFile.curEmotion.expression;

        if(ghostDropdown.options.Count <= 1 && expressionNameField.text != "")
        {
            for(int i = 0; i < backupList.Count; i++)
                ghostDropdown.AddOptions(new List<string>() { "" });
                
        }
        try
        {
            ghostDropdown.options[curFile.curExp + 1].text = curFile.curEmotion.expression;
        }
        catch(System.Exception e)
        {
            //Debug.Log("New Character is being loaded");
        }
    }

    public void NewCharacter()
    {
        //Debug.Log(curFile.Equals(loadedFile));
        if(curFile.Equals(loadedFile))
        {
            CreateNewCharacter();
        }
        else
        {
            warningMessage.SetActive(true);
            yes.onClick.AddListener(() => {
                CreateNewCharacter();
                No();
            });
        }
    }

    public void CreateNewCharacter()
    {
        expressionsDropdown.ClearOptions();
        ghostDropdown.ClearOptions();
        Start();
        characterNameField.text = "";
        expressionsDropdown.AddOptions(new List<string>() { "" });
        ghostDropdown.AddOptions(new List<string>() { "" });
        SelectEmotion();
    }

    public void AddEmotion()
    {
        expressionsDropdown.AddOptions(new List<string>() { "" });
        bool blank = true;
        foreach(TMP_Dropdown.OptionData optionData in expressionsDropdown.options)
        {
            if (optionData.text != "") blank = false;
        }
        if(!blank) ghostDropdown.AddOptions(new List<string>() { "" });
        curFile.addExpression();
        backupList.Add("");
    }

    public void RemoveEmotion()
    {
        string expressionName = curFile.curEmotion.expression;
        curFile.removeExpression();
        backupList.Remove(expressionName);
        expressionsDropdown.ClearOptions();
        expressionsDropdown.AddOptions(backupList);
        ghostDropdown.ClearOptions();
        List<string> ghostList = new List<string>() { "" };
        ghostList.AddRange(backupList);
        ghostDropdown.AddOptions(ghostList);
        if (expressionsDropdown.options.Count == 0)
        {
            backupList.Add("");
            expressionsDropdown.AddOptions(backupList);
        }
        if (curFile.curExp >= curFile.expressions.Count) curFile.curExp = curFile.expressions.Count - 1;
        /*expressionNameField.text = curFile.curEmotion.expression;
        characterNameField.text = curFile.filename;
        scaleStepper.value = curFile.curEmotion.scale;
        scaleStepper.field.text = scaleStepper.displayValue;
        SetSprite(curFile.curEmotion.sprite);
        SetSpriteScale();*/
        SelectEmotion();
        expressionsDropdown.value = curFile.curExp;
        SetGhostSprite();
    }

    public void SelectEmotion()
    {
        curFile.curExp = expressionsDropdown.value;
        expressionNameField.text = curFile.curEmotion.expression;
        scaleStepper.value = curFile.curEmotion.scale;
        scaleStepper.field.text = scaleStepper.displayValue;

        xStepper.value = curFile.curEmotion.offset[0];
        yStepper.value = curFile.curEmotion.offset[1];

        xStepper.field.text = xStepper.displayValue;
        yStepper.field.text = yStepper.displayValue;

        SetSprite(curFile.curEmotion.sprite);
        SetSpriteScale();
        SetOffset(false);
        SetOffset(true);
        //Debug.Log(curFile.curExp);
    }

    public void SetSprite(byte[] imageData)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageData);
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        if (imageData != null)
        {
            spriteHolder.sprite = sprite;
            spriteHolder.SetNativeSize();
            spriteSize = spriteHolder.rectTransform.sizeDelta;
        }
        else
        {
            spriteHolder.sprite = null;
            spriteHolder.rectTransform.sizeDelta = defaultSize;
        }
        SetSpriteScale();
        SetOffset(false);
        SetOffset(true);

    }

    public void SetSpriteScale()
    {
        if(scaleStepper != null && curFile != null)
            curFile.setScale(scaleStepper.value);
        if(spriteHolder.sprite != null)
        {
            spriteHolder.rectTransform.sizeDelta = spriteSize * curFile.curEmotion.scale;
        }
        //curFile.curEmotion.scale = scaleStepper.value;
    }

    public void SetOffset(bool yOffset)
    {
        int[] offset = new int[2] { 0, 0 };
        if (curFile != null) offset = curFile.curEmotion.offset;
        if(yOffset)
        {
            offset[1] = (int)yStepper.value;
        }
        else
        {
            offset[0] = (int)xStepper.value;
        }
        if (curFile != null) curFile.setOffset(offset[0],offset[1]);

        Debug.Log(offset[0] + "," + offset[1]);

        if(spriteHolder.sprite != null)
        {
            spriteHolder.rectTransform.anchoredPosition = new Vector2(offset[0], offset[1]);
        }
    }

    public void SetGhostSprite()
    {
        if (ghostDropdown.value != 0)
            phantom = curFile.expressions[ghostDropdown.value - 1];
        else
            phantom = new Emotion();

        if(phantom.sprite != null)
        {
            ghostSprite.gameObject.SetActive(true);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(phantom.sprite);
            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            ghostSprite.sprite = sprite;
            ghostSprite.SetNativeSize();
            ghostSprite.rectTransform.sizeDelta *= phantom.scale;
            ghostSprite.rectTransform.anchoredPosition = new Vector2(phantom.offset[0], phantom.offset[1]);
        }
        else
        {
            ghostSprite.gameObject.SetActive(false);
        }
    }


    public void SaveWindows()
    {
        StandaloneFileBrowser.SaveFilePanelAsync("Save Character", "", curFile.filename, "sbc", (string path) => {
            //Debug.Log(path.Length);
            if (path.Length != 0)
            {
                var filename = path.Split("\\")[path.Split("\\").Length - 1];
                filename = filename.Remove(filename.Length - 4);
                StarbornFileHandler.WriteCharacter(curFile, filename);
                StarbornFileHandler.PackCharacter(path);
                loadedFile = new(curFile);
                return;
            }

        });
    }

    public void SaveSprite()
    {
        StandaloneFileBrowser.SaveFilePanelAsync("Save Character", "", curFile.filename, "", (string path) => {
            //Debug.Log(path.Length);
            if (path.Length != 0)
            {
                try
                {
                    var filename = path.Split("\\")[path.Split("\\").Length - 1];
                    filename = filename.Remove(filename.Length - 4);
                    var folderPath = path.Remove(path.Length - 4);
                    //Debug.Log(path);
                    Directory.CreateDirectory(folderPath);
                    foreach(Emotion emotion in curFile.expressions)
                    {
                        if (emotion.sprite == null)
                            continue;

                        File.WriteAllBytes(folderPath + "\\" + filename + "_" + emotion.expression + ".png", emotion.sprite);
                    }

                    //File.WriteAllBytes(path, curFile.curEmotion.sprite);
                    //File.WriteAllBytes(path + curFile.filename + "_" + curFile.curEmotion.expression + ".png", curFile.curEmotion.sprite);
                    //loadedFile = new(curFile);
                    return;
                }
                catch(System.Exception e)
                {

                }

            }

        });
    }

    public void LoadWindows()
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
                    curFile = StarbornFileHandler.ReadCharacter(filename);
                    expressionsDropdown.ClearOptions();
                    List<string> emotions = new List<string>();
                    foreach (Emotion emotion in curFile.expressions) emotions.Add(emotion.expression);
                    expressionsDropdown.AddOptions(emotions);
                    ghostDropdown.ClearOptions();
                    List<string> ghostList = new List<string>() { "" };
                    ghostList.AddRange(emotions);
                    ghostDropdown.AddOptions(ghostList);
                    ghostDropdown.value = 0;
                    backupList = emotions;
                    curFile.curExp = 0;
                    characterNameField.text = curFile.filename;
                    /*expressionsDropdown.value = curFile.curExp;
                    expressionNameField.text = curFile.curEmotion.expression;
                    scaleStepper.value = curFile.curEmotion.scale;
                    scaleStepper.field.text = scaleStepper.displayValue;
                    SetSprite(curFile.curEmotion.sprite);
                    SetSpriteScale();*/
                    SelectEmotion();
                    //Debug.Log(expressionsDropdown.options.Count);
                    loadedFile = new(curFile);

                }
                catch (System.Exception e)
                {

                    return;
                }
            }
            await Task.Yield();
        });
    }

    public void No()
    {
        yes.onClick.RemoveAllListeners();
        warningMessage.SetActive(false);

    }
}
