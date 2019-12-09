using Ionic.Zip;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class RawImageDrawer : MonoBehaviour
{
    [SerializeField]
    RawImage m_RawImage;
    Texture2D m_Texture;
    public void SetData(int w, int h, ref byte[] d)
    {
        if(m_Texture != null)
        {
            Destroy(m_Texture);
        }

        //Debug.LogFormat("w:{0}, h:{1}, pixel:{2}, size:{3}", w, h, d[0], d.Length);

        m_Texture = new Texture2D(w, h, TextureFormat.RGB24, false, false);
        m_Texture.LoadImage(d);
        m_RawImage.texture = m_Texture;
        //m_Texture.Apply();
        
    }

    void Start()
    {
        m_RawImage = gameObject.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
