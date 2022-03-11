/*using Sfs2X;
using Sfs2X.Requests;
using Sfs2X.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfsConfig : MonoBehaviour
{
    private static SmartFox sfs;
    public string Host; 
     string Zone = "JCZONE"; 
     string Room = "Lobby"; 

    void initsfs()
    {
        if (sfs != null)
        {
            sfs = null;
        }
        sfs = new SmartFox { ThreadSafeMode = false };
        sfs.AddEventListener(SFSEvent.CONNECTION, Onserverconnect);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnserverconnectLost);
        sfs.AddEventListener(SFSEvent.LOGIN, Onlogin);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnloginError);
        sfs.Connect(Host, 9933);
    }

    private void Onlogin(BaseEvent evt)
    {
        Debug.Break();
        print("Login !");
        print(evt.Params["user"]);
    }
    private void OnloginError(BaseEvent evt)
    {
        print(evt.Params["errorCode"] + "     " + evt.Params["errorMessage"]);
    }
    private void OnserverconnectLost(BaseEvent evt)
    {
        string reason = evt.Params["reason"] as string;
        print(reason); 
    }

    private void Onserverconnect(BaseEvent evt)
    {
        print(evt.Params["success"]);
        sfs.Send(new LoginRequest("", "", Zone));
    }

    // Start is called before the first frame update
    void Start()
    {
          initsfs();

    }

    private void FixedUpdate()
    {
        if (sfs != null)
        {
            sfs.ProcessEvents();
        }
    }
    private void OnApplicationQuit()
    {
        sfs.Disconnect();
    }
}
*/