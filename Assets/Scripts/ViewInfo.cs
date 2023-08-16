using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

public class ViewInfo : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text infoTxt;
    [SerializeField]
    private Button updateBtn;
    [SerializeField]
    private RawImage image;

    private JObject camState;


    public void UpdateView(JObject camera)
    {
        this.camState = camera;
        this.infoTxt.text = this.camState.ToString();

        this.updateBtn.interactable = true;
    }

    public void SetImage(byte[] imageData)
    {
        // Create a texture. Texture size does not matter, since
        // LoadImage will replace with the size of the incoming image.
        Texture2D tex = new Texture2D(2, 2);
        ImageConversion.LoadImage(tex, imageData);
        this.image.texture = tex;
    }

    public JObject GetView()
    {
        return this.camState;
    }

}
