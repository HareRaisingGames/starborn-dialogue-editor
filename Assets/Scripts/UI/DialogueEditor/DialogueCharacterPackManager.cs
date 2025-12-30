using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rabbyte;


public class DialogueCharacterPackManager : OptionSelection
{
    public GameObject packPrefab;
    public TMP_Text message;
    public GameObject scroller;
    public Button addButton;
    public SimpleSBDFile dialogueFile;

    //[HideInInspector]
    public Transform characterPacksParent;

    public RectTransform positionPlacer;
    public Vector2 defaultPosition;

    public Vector2 defaultButtonPos;
    [HideInInspector]
    public Vector2 placement => positionPlacer.anchoredPosition;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if(manager != null)
        {
            dialogueFile = manager.curFile;

            message.gameObject.SetActive(dialogueFile.GetCharacters().Count == 0);
            scroller.SetActive(!message.gameObject.activeInHierarchy);
        }

        if (!UIUtils.containsEvents(addButton.onClick))
            addButton.onClick.AddListener(delegate
            {
                AddCharacterPack(dialogueFile.id);
            });

    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Load(SimpleSBDFile sBDFile)
    {
        dialogueFile = sBDFile;

        message.gameObject.SetActive(sBDFile.GetCharacters().Count == 0);
        scroller.SetActive(!message.gameObject.activeInHierarchy);
        if (scroller.activeInHierarchy)
        {
            ChangeSelections();
        }

        addButton.GetComponent<RectTransform>().anchoredPosition = defaultButtonPos;
        positionPlacer.anchoredPosition = defaultPosition;
        if(manager != null)
        {
            foreach (Transform character in manager.sprites.transform)
            {
                character.gameObject.SetActive(false);
            }
            for (int i = 0; i < dialogueFile.characterPack.Count; i++)
            {
                CharacterSprite character = null;
                foreach (CharacterSprite pack in FindObjectsOfType<CharacterSprite>(true))
                {
                    if (pack.charName == dialogueFile.characterPack[i].character)
                    {
                        character = pack;
                        break;
                    }

                    //if(pack.character)
                }

                //manager.packs[dialogueFile.id][i].UISetUp();
                //if (!manager.packs[dialogueFile.id][i].hasLoaded)
                //{
                //manager.packs[dialogueFile.id][i].ManagerSetup();
                //}

                //CharacterSprite character = manager.packs[dialogueFile.id][i].character;
                if (character != null)
                {
                    character.gameObject.SetActive(true);
                    character.flipX = dialogueFile.characterPack[i].flipX;
                    character.expression = dialogueFile.characterPack[i].emotion;
                    Alignment align = dialogueFile.characterPack[i].alignment;
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
                    character.SetXOffset(dialogueFile.characterPack[i].offset);
                    //character.position;
                }

                float y = manager.packs[dialogueFile.id][i].GetComponent<RectTransform>().sizeDelta.y + 10;
                positionPlacer.anchoredPosition -= new Vector2(0, y);
                addButton.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, y);

            }
        }

    }
    public void ChangeSelections()
    {
        if (scroller.activeInHierarchy)
        {
            if(manager != null)
                foreach (GameObject pack in manager.groups.Values)
                {
                    pack.SetActive(false);
                }
            if (characterPacksParent.Find(dialogueFile.id.ToString()) != null)
            {
                characterPacksParent.Find(dialogueFile.id.ToString()).gameObject.SetActive(true);
            }
            addButton.GetComponent<RectTransform>().anchoredPosition = defaultButtonPos;
            positionPlacer.anchoredPosition = defaultPosition;
            //Debug.Log(manager.unassignedCharacters[dialogueFile.id].Count);
            addButton.gameObject.SetActive(manager.unassignedCharacters[dialogueFile.id].Count != 0);

            for (int i = 0; i < dialogueFile.characterPack.Count; i++)
            {
                CharacterSprite character = null;

                foreach (CharacterSprite pack in FindObjectsOfType<CharacterSprite>(true))
                {
                    if (pack.charName == dialogueFile.characterPack[i].character)
                    {
                        character = pack;
                        break;
                    }

                    //if(pack.character)
                }

                //manager.packs[dialogueFile.id][i].UISetUp();
                //if (!manager.packs[dialogueFile.id][i].hasLoaded)
                //{
                //manager.packs[dialogueFile.id][i].ManagerSetup();
                //}

                //CharacterSprite character = manager.packs[dialogueFile.id][i].character;
                //dialogueFile.characterPack[i].character = character;
                if (character != null)
                {
                    character.gameObject.SetActive(true);
                    character.flipX = dialogueFile.characterPack[i].flipX;
                    character.expression = dialogueFile.characterPack[i].emotion;
                    Alignment align = dialogueFile.characterPack[i].alignment;
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
                    character.SetXOffset(dialogueFile.characterPack[i].offset);
                    //character.position;
                }

                float y = manager.packs[dialogueFile.id][i].GetComponent<RectTransform>().sizeDelta.y + 10;
                positionPlacer.anchoredPosition -= new Vector2(0, y);
                addButton.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, y);
            }
        }
    }

    public void AddCharacterPack(int parentID = -1)
    {
        if (manager != null)
        {
            GameObject charPack = Instantiate(packPrefab, Vector3.zero, Quaternion.identity);
            charPack.name = "Character";
            charPack.transform.parent = manager.groups[parentID].transform;
            //charPack.GetComponent<RectTransform>().anchoredPosition = addButton.GetComponent<RectTransform>().anchoredPosition;
            charPack.GetComponent<RectTransform>().anchoredPosition = placement;
            charPack.transform.localScale = Vector3.one;
            CharacterPack pack = new CharacterPack();
            int count = dialogueFile.characterPack.Count;
            //characterPack.Add(charPack.GetComponent<DialogueCharacterPack>().gameObject);
            manager.packs[parentID].Add(charPack.GetComponent<DialogueCharacterPack>());
            dialogueFile.characterPack.Add(pack);
            charPack.GetComponent<DialogueCharacterPack>().AddCharacterPack(dialogueFile, this, parentID, count);
            positionPlacer.anchoredPosition -= new Vector2(0, charPack.GetComponent<RectTransform>().sizeDelta.y + 10);
            addButton.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, charPack.GetComponent<RectTransform>().sizeDelta.y + 10);
            addButton.gameObject.SetActive(manager.unassignedCharacters[parentID].Count != 0);
        }

    }

    public void ClearAllPacks()
    {

    }

    public void RemoveCharacterPack(int id, bool delete = false)
    {
        dialogueFile.characterPack.RemoveAt(id);
        if(manager != null)
        {
            string character = manager.assignedCharacters[dialogueFile.id][id];
            manager.assignedCharacters[dialogueFile.id].Remove(character);
            if(!delete)
                manager.unassignedCharacters[dialogueFile.id].Add(character);
            manager.packs[dialogueFile.id][id].UpdateCharacterList();
            manager.packs[dialogueFile.id].RemoveAt(id);
            for (int i = id; i < manager.packs[dialogueFile.id].Count; i++)
            {
                GameObject pack = manager.packs[dialogueFile.id][id].gameObject;
                pack.GetComponent<DialogueCharacterPack>().id = id;
                pack.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, pack.GetComponent<RectTransform>().sizeDelta.y + 10);
            }

            addButton.gameObject.SetActive(true);
            addButton.GetComponent<RectTransform>().anchoredPosition = defaultButtonPos;
            positionPlacer.anchoredPosition = defaultPosition;
            for (int i = 0; i < dialogueFile.characterPack.Count; i++)
            {
                float y = manager.packs[dialogueFile.id][i].gameObject.GetComponent<RectTransform>().sizeDelta.y + 10;
                positionPlacer.anchoredPosition -= new Vector2(0, y);
                addButton.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, y);
            }
        }

        //packs.RemoveAt(id);
        //charac
    }
}
