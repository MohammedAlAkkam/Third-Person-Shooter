using Sfs2X;
using Sfs2X.Requests;
using Sfs2X.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X.Entities;
using Sfs2X.Util;

public class SfsConfig : MonoBehaviour
{
    string configFile = "_Scrits/sfs-config.xml";
    public bool useconfig = false;
    public static SmartFox sfs;
    public string Host = "127.0.0.1"; 
     string Zone = "BasicExamples"; 
     string Room = "The Lobby"; 

    void initsfs()
    {
        
        if (sfs != null)
        {
            sfs = null;
        }
           
			sfs = new SmartFox();print("PPPPPPPPPPPPPPPPPP");
            
        sfs.AddEventListener(SFSEvent.CONNECTION, Onserverconnect);
        sfs.AddEventListener(SFSEvent.LOGIN, Onlogin);
        sfs.AddEventListener(SFSEvent.LOGOUT, OnLogout);    
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnserverconnectLost);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnloginError);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN,OnJoinRoom);
        sfs.AddEventListener(SFSEvent.LOGOUT, OnLogout);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnJoinRoomError);
        sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnMessageSend);
       // sfs.AddEventListener(SFSEvent.CONFIG_LOAD_FAILURE, OnConfigfill);
        if(useconfig){
           
            sfs.LoadConfig(Application.dataPath + "/" + configFile,true);
        }else{
      ConfigData cfg = new ConfigData();

        cfg.Host = Host;
        cfg.Port = 9933;
        cfg.Zone = Zone;
        sfs.Connect(Host, 9933);
        }
    }

    private void OnMessageSend(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        User user = (User)evt.Params["sender"];
        print(room.Name +"   "+ user.Name   +"   " + evt.Params["message"]);
        
    }

    private void OnJoinRoomError(BaseEvent evt)
    {
      print(evt.Params["errorCode"] + "   " + evt.Params["errorMessage"]);
        
    }

    private void OnJoinRoom(BaseEvent evt)
    {
        print(evt.Params["room"]);
        sfs.Send(new PublicMessageRequest("Hello"));
    }

    private void OnConfigfill(BaseEvent evt)
    {
    }

    private void OnConfigLoad(BaseEvent evt)
    {
        //sfs.Connect(sfs.Config.Host,sfs.Config.Port);
    }

    private void OnLogout(BaseEvent evt)
    {
       print("Logout");
    }

    private void Onlogin(BaseEvent evt)
    {
        Camera.main.backgroundColor = Color.grey;
        sfs.Send(new JoinRoomRequest(Room));
        print(evt.Params["user"]);
    }
    private void OnloginError(BaseEvent evt)
    {
        print(evt.Params["errorCode"] + "     " + evt.Params["errorMessage"]);
    }
    private void OnserverconnectLost(BaseEvent evt)
    {
        string reason = evt.Params["reason"] as string + ";;;;;";
       // print(reason); 
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
