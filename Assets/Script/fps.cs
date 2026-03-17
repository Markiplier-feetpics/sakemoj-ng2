using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

public class fps : MonoBehaviour
{
    [SerializeField] float sens = 150;
    [SerializeField] float accel = 4;
    [SerializeField] float maxspeed = 20;
    [SerializeField] float jumpy = 10;
    [SerializeField] float MinJumpyM = 0.5f;
    [SerializeField] float MaxJumpyM = 1.5f;
    [SerializeField] float gravM = 2;
    float speed = 0;
    float smallening = 0.7f;
    float slowening = 0.7f;
    Camera c;
    Vector2 inp;
    Vector2 look;
    CharacterController cc;
    Boolean j=false;
    Boolean crotch=false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        c = FindFirstObjectByType<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    Vector3 vel=Vector3.zero;
    void Update()
    {
        vel.y+=gravM*Physics.gravity.y*Time.deltaTime;
        
        if (speed < maxspeed) {speed += accel*inp.magnitude*Time.deltaTime;} else {speed = maxspeed;}
        if (inp.magnitude < 0.2 && speed>0) {speed -= 30*Time.deltaTime;if(speed<0){speed=0;}}
        vel.z = speed;
        if (cc.isGrounded) //on ground
        {
            if (j) 
            {
                if (transform.localScale.magnitude < 3)
                {
                    vel.y = jumpy*moveM()/2;
                    vel.z += jumpy*moveM()/2;
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
        return Mathf.Clamp(vel.z*MaxJumpyM/maxspeed, MinJumpyM, MaxJumpyM);
    }
    Ray r = new()
        {
            direction = new(0, -1, 0), 
        };
    void AttemptCrouch()
    {
        float d = (cc.height+cc.radius)*transform.localScale.y;
        r.origin = transform.position+new Vector3(0,d,0);
        cc.Raycast(r, out RaycastHit i, d-0.1f);
        if (transform.localScale.y < 1){transform.localScale = Vector3.one;maxspeed/=slowening;}
        else {transform.localScale = new(1,smallening,1);maxspeed*=slowening;}
        if(cc.isGrounded) {cc.Move(new(0,(cc.height+cc.radius)*transform.localScale.y-cc.height+cc.radius,0));}
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
    }
    public void OnCrouch()
    {
        AttemptCrouch();
    }
}
