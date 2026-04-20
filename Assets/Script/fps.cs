using System;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;

public class fps : MonoBehaviour
{
    [SerializeField] public float sens = 150;
    [SerializeField] public float accel = 4;
    [SerializeField] public float maxspeed = 20;
    [SerializeField] public float jumpy = 10;
    [SerializeField] public float MinJumpyM = 0.5f;
    [SerializeField] public float MaxJumpyM = 1.5f;
    [SerializeField] public float gravM = 2;
    [SerializeField] public int wallclimbs = 1;
    public float wallrunmult = 1;
    public int wallruns = 1;
    public float speed = 0;
    public float vault = 0;
    public bool grounded = false;
    float smallening = 0.7f;
    float slowening = 0.7f;
    int wc;
    GameObject h; // vault obj
    Camera c;
    Vector2 inp;
    Vector2 look;
    CharacterController cc;
    public Boolean j=false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        c = FindFirstObjectByType<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        wc = wallclimbs;
        wallrun = wallruns;
    }

    // Update is called once per frame
    Vector3 vel=Vector3.zero;
    bool IsGrounded()
    {
        if (vel.y > 0) {return false;}
        if (Physics.Raycast(transform.position, new(0,-1,0),out RaycastHit k))
        {   //print(k.distance-(transform.localScale.y*cc.height/2)+cc.skinWidth);
        // -new Vector3(0,(transform.localScale.y*cc.height/2)+cc.skinWidth,0)
            if (k.distance-(transform.localScale.y*cc.height/2)+cc.skinWidth < .2f)
            {
                //k.collider.gameObject.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(); 
                //cc.Move(new(0,-k.distance,0)); //-k.distance+.02f
                return true;
            }
        } else {print("void?");}
        return false;
    }
    void FixedUpdate()
    {
        //print("ground: ");
        grounded = IsGrounded();
        vel.y+=gravM*Physics.gravity.y*Time.deltaTime;
        if (vault != 0)
        {
            vault-=Time.deltaTime;
            if (vault <= 0) {h.GetComponent<Collider>().enabled = true;vault=0;}
        }
        if (speed < maxspeed) {speed += accel*inp.magnitude*Time.deltaTime;} else {speed = maxspeed;}
        if (inp.magnitude < 0.2 && speed>0) {speed -= 30*Time.deltaTime;if(speed<0){speed=0;}}
        
        if (vel.z < speed && vel.z >= 0) {vel.z = speed;} else if (grounded) {vel.z /= 2;}
        if (grounded) //on ground
        {
            wc = wallclimbs;
            wallrun = wallruns;
            if (j) 
            {
                
                if (transform.localScale.y < 1)
                {
                    vel.y = jumpy*moveM();
                    vel.z += jumpy;
                } else {vel.y=jumpy*moveM();}
                
            } 
            else {vel.y=0;}
            
            
        } else //in air
        {if (vel.y>0&&!j) {vel.y-=20*Time.deltaTime;}}
        transform.Rotate(new(0,look.x*sens*Time.deltaTime,0));
        c.transform.Rotate(new(-look.y*sens*Time.deltaTime,0,0));
        cc.Move(transform.TransformDirection(new(vel.z*inp.x*Time.deltaTime, vel.y*Time.deltaTime, vel.z*inp.y*Time.deltaTime)));
        
    }
    float moveM()
    {
        return Mathf.Clamp(speed*MaxJumpyM/maxspeed, MinJumpyM, MaxJumpyM);
    }
    Ray r = new()
        {
            direction = new(0, -1, 0), 
        };
    void AttemptCrouch()
    {
        float d = cc.height*transform.localScale.y;
        r.origin = transform.position+new Vector3(0,d,0);
        cc.Raycast(r, out RaycastHit i, d-0.1f);
        if (transform.localScale.y < 1){transform.localScale = Vector3.one;maxspeed/=slowening;}
        else {transform.localScale = new(1,smallening,1);maxspeed*=slowening;}
        
    }
    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }
    public void OnMove(InputValue value)
    {   
        inp = value.Get<Vector2>();
    }
    bool wr = false;
    int wallrun;
    public void OnJump()
    {
        j=!j;
        if (wr) {wr = false;}
        else if (j)
        {
           Boolean w = Physics.SphereCast(transform.position, cc.radius, transform.TransformDirection(new(0,0,1)), out RaycastHit rh, 2);
            if (w)
            {
                if (!Physics.Raycast(transform.position+new Vector3(0,cc.height/2,0), transform.TransformDirection(new(0,0,1)), rh.distance+2))
                {
                    if (vault != 0) {h.GetComponent<Collider>().enabled = true;}
                    print("vault");
                    h=rh.collider.gameObject;
                    rh.collider.enabled = false;
                    speed = maxspeed;
                    vel.z = speed+vel.y;
                    vel.y = 10;
                    vault = 1;
                    
                } else
                {
                    if (wc != 0)
                    {
                        wc -= 1;
                        vel.y = 15;
                        speed = 0;
                    }
                } 
                
            } else if (wallrun != 0) {if (Physics.SphereCast(transform.position, cc.radius, transform.TransformDirection(1,0,0), out RaycastHit k, 2))
                {
                    wr = true;
                    wallrun -= 1;
                } else if (Physics.SphereCast(transform.position, cc.radius, transform.TransformDirection(-1,0,0), out k, 2))
                {
                    wr = true;
                    wallrun -= 1;
                }      
            }
        }
        
    }
    public void OnCrouch()
    {
        AttemptCrouch();
    }
}
