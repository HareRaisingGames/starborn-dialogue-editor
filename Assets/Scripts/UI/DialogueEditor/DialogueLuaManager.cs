using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rabbyte;
using TMPro;

public class DialogueLuaManager : OptionSelection
{
    public SimpleSBDFile dialogueFile;

    public TMP_InputField onStart;
    public TMP_InputField onInterval;
    public TMP_InputField onEnd;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if (manager != null)
        {
            onStart.onValueChanged.AddListener(manager.OnStartScript);
            onInterval.onValueChanged.AddListener(manager.OnIntervalScript);
            onEnd.onValueChanged.AddListener(manager.OnEndScript);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Load(SimpleSBDFile sBDFile)
    {
        dialogueFile = sBDFile;

        onStart.text = sBDFile.onStart;
        onInterval.text = sBDFile.onWord;
        onEnd.text = sBDFile.onEnd;
    }
}
