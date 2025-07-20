using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rabbyte;
using System.Threading.Tasks;
using SFB;
using System;
using System.IO;

public class ImageLoader : MonoBehaviour
{
    Image img;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenWindows()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Image Files", "jpg", "png")
        };
        StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) =>
        {
            if (paths.Length > 0)
            {
                //try
                //{
                    byte[] imageData = File.ReadAllBytes(paths[0]);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(imageData);
                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    img.sprite = sprite;
                    return;
                //}
                //catch (System.Exception e)
                //{

                    //return;
                //}
            }
            await Task.Yield();
        });
    }
}
