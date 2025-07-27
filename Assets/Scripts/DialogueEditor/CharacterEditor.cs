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

public class CharacterEditor : MonoBehaviour
{
    public SBCFile curFile;
    public SBCFile loadedFile;

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

    Vector2 defaultSize = new Vector2(150, 150);
    Vector2 spriteSize;

    Vector2 defaultOffsets = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        curFile = new SBCFile();
        loadedFile = new SBCFile();
        backupList = new List<string>() { "" };
        curFile.curExp = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Debug.Log(curFile.filename);
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
    }

    public void NewCharacter()
    {
        Debug.Log(curFile.Equals(loadedFile));
    }

    public void CreateNewCharacter()
    {

    }

    public void AddEmotion()
    {
        expressionsDropdown.AddOptions(new List<string>() { "" });
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
        if (curFile.curExp >= curFile.expressions.Count) curFile.curExp = curFile.expressions.Count - 1;
        expressionNameField.text = curFile.curEmotion.expression;
        characterNameField.text = curFile.filename;
        scaleStepper.value = curFile.curEmotion.scale;
        scaleStepper.field.text = scaleStepper.value.ToString();
        SetSprite(curFile.curEmotion.sprite);
        SetSpriteScale();
        expressionsDropdown.value = curFile.curExp;
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

        if(spriteHolder.sprite != null)
        {
            spriteHolder.rectTransform.anchoredPosition = new Vector2(offset[0], offset[1]);
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
                //loadedFile = new(curFile);
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
                    backupList = emotions;
                    curFile.curExp = 0;
                    /*expressionsDropdown.value = curFile.curExp;
                    expressionNameField.text = curFile.curEmotion.expression;
                    characterNameField.text = curFile.filename;
                    scaleStepper.value = curFile.curEmotion.scale;
                    scaleStepper.field.text = scaleStepper.displayValue;
                    SetSprite(curFile.curEmotion.sprite);
                    SetSpriteScale();*/
                    SelectEmotion();
                    //Debug.Log(expressionsDropdown.options.Count);

                }
                catch (System.Exception e)
                {

                    return;
                }
            }
            await Task.Yield();
        });
    }
}
