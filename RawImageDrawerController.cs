using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawImageDrawerController : MonoBehaviour
{
    [SerializeField]
    List<RawImageDrawer> m_Drawer;

    [SerializeField]
    ImageServer m_ImageServer;

    public void OnImage(ImageServer.ImageData id)
    {
        if (m_Drawer.Count <= id.id) return;
        m_Drawer[id.id].SetData(id.w, id.h, ref id.data);
    }

    void Start()
    {
        m_ImageServer.m_OnAudioReceived = OnImage;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
