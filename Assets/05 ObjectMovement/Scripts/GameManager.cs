using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Logging;
using StarterAssets;
using UnityEngine.Animations.Rigging;

public class GameManager : MonoBehaviour
{

    private SmartFox sfs;

    private GameObject localPlayer;
    private Animator localanimator;
    //private PlayerController localPlayerController;
    private Dictionary<SFSUser, GameObject> remotePlayers = new Dictionary<SFSUser, GameObject>();
    [SerializeField] private GameObject localPlayerModel;
    [SerializeField] private GameObject remotPlayerModel;
    [SerializeField] private Transform target;

    string TargetPos = "";

    //----------------------------------------------------------
    // Unity calback methods
    //----------------------------------------------------------

    void Start()
    {



        sfs = SmartFoxConnection.Connection;

        // Register callback delegates
        sfs.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessage);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariableUpdate);
        sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
        sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);

        SpawnLocalPlayer();

        // Update settings panel with the selected model and material
        /*GameUI ui = GameObject.Find("UI").GetComponent("GameUI") as GameUI;
        ui.SetAvatarSelection(numModel);
        ui.SetColorSelection(numMaterial);*/
    }

    void FixedUpdate()
    {
        if (sfs != null)
        {
            sfs.ProcessEvents();

            // If we spawned a local player, send position if movement is dirty
            if (localPlayer != null /*&& localPlayerController != null && localPlayerController.MovementDirty*/)
            {
                List<UserVariable> userVariables = new List<UserVariable>();
                userVariables.Add(new SFSUserVariable("x", (double)localPlayer.transform.position.x));
                userVariables.Add(new SFSUserVariable("y", (double)localPlayer.transform.position.y));
                userVariables.Add(new SFSUserVariable("z", (double)localPlayer.transform.position.z));
                userVariables.Add(new SFSUserVariable("state0", GetAnimationStat0()));
                userVariables.Add(new SFSUserVariable("state1", GetAnimationStat1()));
                TargetPos = target.position.x.ToString() + ":" + target.position.y.ToString() + ":" +target.position.z.ToString();
                userVariables.Add(new SFSUserVariable("target",TargetPos));
                userVariables.Add(new SFSUserVariable("rot", (double)localPlayer.transform.rotation.eulerAngles.y));
                sfs.Send(new SetUserVariablesRequest(userVariables));
                //localPlayerController.MovementDirty = false;
            }
        }
    }
    void OnApplicationQuit()
    {
        // Before leaving, lets notify the others about this client dropping out
        RemoveLocalPlayer();
    }

    //----------------------------------------------------------
    // SmartFoxServer event listeners
    //----------------------------------------------------------

    public void OnUserExitRoom(BaseEvent evt)
    {
        // Someone left - lets make certain they are removed if they didn't nicely send a remove command
        SFSUser user = (SFSUser)evt.Params["user"];
        RemoveRemotePlayer(user);
    }

    public void OnUserEnterRoom(BaseEvent evt)
    {
        // User joined - and we might be standing still (not sending position updates); so let's send him our position
        if (localPlayer != null)
        {
            List<UserVariable> userVariables = new List<UserVariable>();
            userVariables.Add(new SFSUserVariable("x", (double)localPlayer.transform.position.x));
            userVariables.Add(new SFSUserVariable("y", (double)localPlayer.transform.position.y));
            userVariables.Add(new SFSUserVariable("z", (double)localPlayer.transform.position.z));
            userVariables.Add(new SFSUserVariable("state0", GetAnimationStat0()));
             userVariables.Add(new SFSUserVariable("state1", GetAnimationStat1()));
            TargetPos = target.position.x.ToString() + ":" + target.position.y.ToString() + ":" + target.position.z.ToString();
            userVariables.Add(new SFSUserVariable("target", TargetPos));
            sfs.Send(new SetUserVariablesRequest(userVariables));
        }
    }
    
    public void OnConnectionLost(BaseEvent evt)
    {
        print(evt.Params["reason"]);
        // Reset all internal states so we kick back to login screen
        sfs.RemoveAllEventListeners();
    }

    public void OnObjectMessage(BaseEvent evt)
    {
        // The only messages being sent around are remove messages from users that are leaving the game
        ISFSObject dataObj = (SFSObject)evt.Params["message"];
        SFSUser sender = (SFSUser)evt.Params["sender"];

        if (dataObj.ContainsKey("cmd"))
        {
            switch (dataObj.GetUtfString("cmd"))
            {
                case "rm":
                    Debug.Log("Removing player unit " + sender.Id);
                    RemoveRemotePlayer(sender);
                    break;
            }
        }
    }

    /**
     * When user variable is updated on any client, then this event is received.
     * This is where most of the game logic for this example is contained.
     */
    public void OnUserVariableUpdate(BaseEvent evt)
    {
        List<string> changedVars = (List<string>)evt.Params["changedVars"];
        SFSUser user = (SFSUser)evt.Params["user"];
        
        if (user == sfs.MySelf) return;

        if (!remotePlayers.ContainsKey(user))
        {
            // New client just started transmitting - lets create remote player
            Vector3 pos = Vector3.zero;
            string statename0 = null;
            bool statename1 = false;
            string T_pos = "";
            if (user.ContainsVariable("x") && user.ContainsVariable("y") && user.ContainsVariable("z"))
            {
                pos.x = (float)user.GetVariable("x").GetDoubleValue();
                pos.y = (float)user.GetVariable("y").GetDoubleValue();
                pos.z = (float)user.GetVariable("z").GetDoubleValue();
            }

            float rotAngle = 0;
            if (user.ContainsVariable("rot"))
            {
                rotAngle = (float)user.GetVariable("rot").GetDoubleValue();
            }
            // state = ChangeAnimationState(user);
            if (user.ContainsVariable("state0") && user.ContainsVariable("state1"))
            {
                statename0 = user.GetVariable("state0").GetStringValue();
                statename1 = user.GetVariable("state1").GetBoolValue();
                print(statename1);
            }
            if(user.ContainsVariable("target"))
            {
                T_pos = user.GetVariable("target").GetStringValue();
            }
            SpawnRemotePlayer(user, pos, Quaternion.Euler(0, rotAngle, 0),statename0,statename1,T_pos);
          
        }

        // Check if the remote user changed his position or rotation
        if (changedVars.Contains("x") && changedVars.Contains("y") && changedVars.Contains("z") && changedVars.Contains("rot") /*&& changedVars.Contains("Speed")*/)
        {
            // Move the character to a new position...

                remotePlayers[user].GetComponent<SimpleRemoteInterpolation>().SetData(
                new Vector3((float)user.GetVariable("x").GetDoubleValue(), (float)user.GetVariable("y").GetDoubleValue(), (float)user.GetVariable("z").GetDoubleValue()),
                Quaternion.Euler(0, (float)user.GetVariable("rot").GetDoubleValue(), 0),
                true, user.GetVariable("state0").GetStringValue(),
                 user.GetVariable("state1").GetBoolValue(),user.GetVariable("target").GetStringValue());
        }
    }

    public string GetAnimationStat0()
    {

        if (localanimator.GetCurrentAnimatorStateInfo(0).IsName("Walk_N"))
            return "Walk_N";
        else if (localanimator.GetCurrentAnimatorStateInfo(0).IsName("Run_N"))
            return "Run_N";
        else if (localanimator.GetCurrentAnimatorStateInfo(0).IsName("JumpStart"))
            return "JumpStart";
        else if (localanimator.GetCurrentAnimatorStateInfo(0).IsName("InAir"))
            return "InAir";
        else if (localanimator.GetCurrentAnimatorStateInfo(0).IsName("JumpLand"))
            return "JumpLand";
        else if (localanimator.GetCurrentAnimatorStateInfo(0).IsName("Dance"))
            return "Dance";

        else if (localanimator.GetFloat("Speed") > 4f)
            return "Run_N";
        else if (localanimator.GetFloat("Speed") > .5f)
            return "Walk_N";
        else return "Idle";
    }

    public bool GetAnimationStat1()
    {
        return localanimator.GetBool("Fire");
    }

    public AnimatorStat ChangeAnimationState(SFSUser user)
    {
        AnimatorStat state = new AnimatorStat();
        //state.Speed = (float)user.GetVariable("Speed").GetDoubleValue();
        //state.Jump = (bool)user.GetVariable("Jump").GetBoolValue();
        //state.Fire = (bool)user.GetVariable("Fire").GetBoolValue();
        //state.FreeFall = (bool)user.GetVariable("FreeFall").GetBoolValue();
        //state.Grounded = (bool)user.GetVariable("Grounded").GetBoolValue();

        return state;
    }

    //----------------------------------------------------------
    // Public interface methods for UI
    //----------------------------------------------------------

    public void Disconnect()
    {
        sfs.Disconnect();
    }


    //----------------------------------------------------------
    // Private player helper methods
    //----------------------------------------------------------

    private void SpawnLocalPlayer()
    {
        Vector3 pos;
        Quaternion rot;

        // See if there already exists a model - if so, take its pos+rot before destroying it
        if (localPlayer != null)
        {
            pos = localPlayer.transform.position;
            rot = localPlayer.transform.rotation;
            Camera.main.transform.parent = null;
            Destroy(localPlayer);
        }
        else
        {
            pos = GameObject.Find("StartPoint").transform.position;
            rot = Quaternion.identity;
        }

        // Lets spawn our local player model
        localPlayer = GameObject.Instantiate(localPlayerModel) as GameObject;
        localanimator = localPlayer.GetComponent<Animator>();

        localPlayer.transform.position = pos;
        localPlayer.transform.rotation = rot;


        // Since this is the local player, lets add a controller and fix the camera
        //localPlayer.AddComponent<PlayerController>();
        //localPlayerController = localPlayer.GetComponent<PlayerController>();
        localPlayer.GetComponentInChildren<TextMesh>().text = sfs.MySelf.Name;
        SettingCamera();
    }
    void SettingCamera()
    {
        var camers = GameObject.Find("Cameras");//.GetComponentsInChildren<Cinemachine.CinemachineVirtualCamera>();
        camers.transform.GetChild(0).GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = localPlayer.transform.Find("PlayerCameraRoot");
        camers.transform.GetChild(1).GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = localPlayer.transform.Find("PlayerCameraRoot");
        camers.transform.GetChild(2).GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = localPlayer.transform.Find("PlayerCameraRoot");
        var contraller = localPlayer.GetComponent<ThirdPersonShooterContraller>();
        contraller.shootingVirtualcamera = camers.transform.GetChild(1).GetComponent<Cinemachine.CinemachineVirtualCamera>();
        contraller.aimVirtualcamera = camers.transform.GetChild(2).GetComponent<Cinemachine.CinemachineVirtualCamera>();      

    }

    private void SpawnRemotePlayer(SFSUser user, Vector3 pos, Quaternion rot, string stat0,bool state1,string T_pos)
    {
        // See if there already exists a model so we can destroy it first
        if (remotePlayers.ContainsKey(user) && remotePlayers[user] != null)
        {
            Destroy(remotePlayers[user]);
            remotePlayers.Remove(user);
        } 
        // Lets spawn our remote player model
        GameObject remotePlayer = GameObject.Instantiate(remotPlayerModel) as GameObject;
        
        remotePlayer.GetComponent<ThirdPersonShooterContraller>().enabled = false;
        remotePlayer.GetComponent<ThirdPersonController>().enabled = false;
        remotePlayer.AddComponent<SimpleRemoteInterpolation>();
        
        remotePlayer.GetComponent<SimpleRemoteInterpolation>().SetData(pos, rot, false,stat0,state1,T_pos);
        // Color and name
        remotePlayer.GetComponentInChildren<TextMesh>().text = user.Name;

        // Lets track the dude
       
        remotePlayers.Add(user, remotePlayer);
    }

    private void RemoveLocalPlayer()
    {
        // Someone dropped off the grid. Lets remove him
        SFSObject obj = new SFSObject();
        obj.PutUtfString("cmd", "rm");
        sfs.Send(new ObjectMessageRequest(obj, sfs.LastJoinedRoom));
    }

    private void RemoveRemotePlayer(SFSUser user)
    {
        if (user == sfs.MySelf) return;

        if (remotePlayers.ContainsKey(user))
        {
            Destroy(remotePlayers[user]);
            remotePlayers.Remove(user);
        }
    }
}

