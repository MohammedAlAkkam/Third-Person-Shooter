using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;

/**
 * Extremely simple and dumb interpolation script.
 * But it works for this example.
 */
public class SimpleRemoteInterpolation : MonoBehaviour
{


    private Vector3 desiredPos;
    private Quaternion desiredRot;
    public bool name=  false;
    Animator animator;
    private float dampingFactor = 5f;
    [SerializeField]GameObject target;
    [SerializeField] RigBuilder rigB;
    bool state = false;
    void Start()
    {

        animator = GetComponent<Animator>();
        desiredPos = this.transform.position;
        desiredRot = this.transform.rotation;
    }

    public void SetData(Vector3 pos, Quaternion rot, bool interpolate, string state0,bool Shooting,string T_pos)
    {
        
        state = Shooting;
        print(state + "!!!!!!!!!!!!!!!!!");
        var Tpos = T_pos.Split(':');
        if(!target)
        {
            print("newtarget");
            target = Instantiate(new GameObject("RemotePlayerTarget"));
        }
        target.transform.position =new Vector3( float.Parse(Tpos[0]),float.Parse(Tpos[1]),float.Parse(Tpos[2]));
        /*if (state1 == "Walk_N")
            name = true;*/
            if(rigB == null)
            {
        GetComponentInChildren<MultiAimConstraint>().data.sourceObjects = new WeightedTransformArray { new WeightedTransform(target.transform, 1) };
          rigB = GetComponent<RigBuilder>();
          rigB.Build();
            }

        // If interpolation, then set the desired pososition+rotation; else force set (for spawning new models)
        if (interpolate)
        {
            desiredPos = pos;
            desiredRot = rot;
        }
        else
        {
            this.transform.position = pos;
            this.transform.rotation = rot;
        }
        if (!animator)
            animator = GetComponent<Animator>();
        if(!animator.GetAnimatorTransitionInfo(0).IsName(state0))
        animator.Play(state0,0);
        //if (!animator.GetAnimatorTransitionInfo(1).IsName(state1))
        animator.SetBool("Fire",Shooting);
    }

    void Update()
    {
        rigB.layers[0].active = state;
        this.transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * dampingFactor);
        this.transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * dampingFactor);
    }
}