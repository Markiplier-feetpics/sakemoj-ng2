using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class fps : MonoBehaviour
{
    [SerializeField] public float sens = 150; // camera responsiveness
    [SerializeField] public float accel = 4; // acceleration
    [SerializeField] public float maxspeed = 20; // maximum movement speed
    [SerializeField] public float jumpy = 10; // base jump height
    [SerializeField] public float MinJumpyM = 0.5f; // min jump height multiplier
    [SerializeField] public float MaxJumpyM = 1.5f; // max jump height mult when moving (speed/maxspeed)
    [SerializeField] public float gravM = 2; // gravity multiplier
    [SerializeField] public int wallclimbs = 1; // amount of wallclimbs before needing to touch ground (-1 = inf)
    [SerializeField] public float wallclimbboost = 10; //velocity from wallclimbs
    [SerializeField] public float maxwallruny = 5; // max wallrun y velocity
    
    public float wallrunmult = 1; // horizontal velocity factor when jumping out of a wallrun
    public int wallruns = 1; // same as wallclimbs but with wallruns
    public bool samesidewallrun = false; // if false: need to alternate sides
    public float speed = 0; // current speed
    public float vault = 0; // time before collision toggle of vaulted object (h)
    public bool grounded = false; // selfexplanitory
    float smallening = 0.7f; // size change when crouching
    float slowening = 0.7f; // speed change when crouching
    int wc; // current wallclimbs
    GameObject h; // vault obj
    Camera c; // camera
    Vector2 inp; // wasd input direction
    Vector2 look; // mouse movement direction
    CharacterController cc; 
    public Boolean j=false; // is jumping
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
    public Vector3 vel=Vector3.zero;
    float Clamp(float n, float min, float max)
    {
        if (n < min) {return min;}
        if (n > max) {return max;}
        return n;
    }
    bool IsGrounded()
    {
        if (vel.y > 0) {return false;}
        if (Physics.SphereCast(transform.position, cc.radius, new(0,-1,0),out RaycastHit k))
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
    void Update()
    {
        transform.Rotate(new(0,look.x*sens*Time.deltaTime,0));
        c.transform.Rotate(new(-look.y*sens*Time.deltaTime,0,0));
        
    }
    void FixedUpdate()
    {
        if (transform.position.y < -5) {transform.position = new(0,2,0); return;}
        //print("ground: ");
        grounded = IsGrounded();
        vel.y+=gravM*Physics.gravity.y*Time.deltaTime;
        if (wr!=0)
        {
            if (!Physics.SphereCast(transform.position, cc.radius, transform.TransformDirection(-wr,0,0), out RaycastHit k, 2) || math.abs(vel.z)<1 )
            {
                wr = 0;
            }
            vel.y = Clamp(vel.y, -maxwallruny, maxwallruny);
        } 
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
            wrs = 0;
            wr = 0;
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
    public int wr = 0; // wallrun side (1 = right, -1 = left)
    int wallrun; // current amount
    int wrs = 0; // last wallrun side
    float wcs=1/2;
    public void OnJump()
    {
        j=!j;
        if (wr!=0) {wr = 0;vel.z*=wallrunmult;vel.y=jumpy*moveM()*2;}
        else if (j)
        {
           Boolean w = Physics.SphereCast(transform.position, cc.radius*wcs, transform.TransformDirection(new(0,0,1)), out RaycastHit rh, 2);
            if (w && !Physics.SphereCast(transform.position+new Vector3(0,cc.height/2,0), cc.radius*wcs, transform.TransformDirection(new(0,0,1)), out RaycastHit n, rh.distance+2))
            { // vault
                if (vault != 0) {h.GetComponent<Collider>().enabled = true;}
                print("vault");
                h=rh.collider.gameObject;
                rh.collider.enabled = false;
                speed = maxspeed;
                vel.z = speed+vel.y;
                vel.y = 10;
                vault = 0.5f;
                    
            } else if (w && wc != 0) 
            { // wallclimb
                wc -= 1;
                vel.y = wallclimbboost;
                speed = 0;
            } else if (!grounded && wrs!=1 && wallrun != 0 && Physics.SphereCast(transform.position, cc.radius, transform.TransformDirection(1,0,0), out RaycastHit k, 2)||!grounded && wrs!=-1 && wallrun!=0 && Physics.SphereCast(transform.position, cc.radius, transform.TransformDirection(-1,0,0), out k, 2))
            { // wallrun
                wr = (int)math.sign(transform.InverseTransformDirection(k.normal).x);
                if (!samesidewallrun) {wrs = -wr;}
                wallrun -= 1;
                print(wr);
            }
        }
    }
    float GetRotation(Vector3 v1, Vector3 v2)
    {
        Vector3 at = v1-v2;
        float angle = Mathf.Atan2(at.y, at.x) * Mathf.Rad2Deg;
        return angle;
    }
    public void OnInteract()
    {
        
    }
    public void OnCrouch()
    {
        AttemptCrouch();
    }
}
