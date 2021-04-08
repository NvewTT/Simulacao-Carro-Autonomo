using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetImag : MonoBehaviour
{
    // Start is called before the first frame update

    private static GetImag instance;

    private Camera mycamera;

    private bool takeshot;

    void Start()
    {
        instance = this;
        mycamera = gameObject.GetComponent<Camera>();
    }

    private void OnPostRender()
    {
        if (takeshot)
        {
            takeshot = false;
            RenderTexture renderTexture = mycamera.targetTexture;

            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32,false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            texture.ReadPixels(rect,0,0);
            
            byte[] bytearray = texture.EncodeToPNG();
           // string base64String = Convert.ToBase64String(bytearray);
            Client.StartClient(bytearray);

            RenderTexture.ReleaseTemporary(renderTexture);
            mycamera.targetTexture = null;
            
        }
    }

    private void TakeImage(int width, int height)
    {
        
        mycamera.targetTexture = RenderTexture.GetTemporary(width, height);
        takeshot = true;
    }

    public static void TakeImage_S(int width, int height)
    {
        instance.TakeImage(width, height);
    }
}
