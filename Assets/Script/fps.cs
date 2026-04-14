using System;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

public class fps : MonoBehaviour
{
    [SerializeField] public float sens = 150;
    [SerializeField] public float accel = 4;
    [SerializeField] public float maxspeed = 20;
    [SerializeField] public float jumpy = 10;
    [SerializeField] public float MinJumpyM = 0.5f;
    [SerializeField] public float MaxJumpyM = 1.5f;
    [SerializeField] public float gravM = 2;
    public float speed = 0;
    public float vault = 0;
    public bool grounded = false;
    float smallening = 0.7f;
    float slowening = 0.7f;
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
    }

    // Update is called once per frame
    Vector3 vel=Vector3.zero;
    bool IsGrounded()
    {
        if (Physics.Raycast(transform.position, new(0,-1,0),out RaycastHit k))
        {   print(k.distance-(transform.localScale.y*cc.height/2)+cc.skinWidth);
        // -new Vector3(0,(transform.localScale.y*cc.height/2)+cc.skinWidth,0)
            if (k.distance-(transform.localScale.y*cc.height/2)+cc.skinWidth < .2f)
            {
                k.collider.gameObject.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(); 
                //cc.Move(new(0,-k.distance,0)); //-k.distance+.02f
                return true;
            }
        } else {print("void?");}
        return false;
    }
    void FixedUpdate()
    {
        print("ground: ");
        grounded = IsGrounded();
        vel.y+=gravM*Physics.gravity.y*Time.deltaTime;
        if (vault != 0)
        {
            vault-=Time.deltaTime;
            if (vault <= 0) {h.GetComponent<Collider>().enabled = true;vault=0;}
        }
        if (speed < maxspeed) {speed += accel*inp.magnitude*Time.deltaTime;} else {speed = maxspeed;}
        if (inp.magnitude < 0.2 && speed>0) {speed -= 30*Time.deltaTime;if(speed<0){speed=0;}}
        vel.z = speed;
        if (grounded) //on ground
        {
            if (j) 
            {
                if (transform.localScale.y < 1)
                {
                    vel.y = jumpy*moveM()/2;
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
    public void OnJump()
    {
        j=!j;
        if (j)
        {
           Boolean w = Physics.SphereCast(transform.position, cc.radius, transform.TransformDirection(new(0,0,1)), out RaycastHit rh, 2);
            if (w)
            {
                if (!Physics.Raycast(transform.position+new Vector3(0,2,0), transform.TransformDirection(new(0,0,1)), rh.distance))
                {
                    print("vault");
                    h=rh.collider.gameObject;
                    rh.collider.enabled = false;
                    vel.z = speed+vel.y;
                    vel.y = 10;
                    vault = 1;
                    
                }
                
            } 
        }
        
    }
    public void OnCrouch()
    {
        AttemptCrouch();
    }
}
