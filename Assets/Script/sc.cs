using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class sc : MonoBehaviour
{
    [SerializeField] float size = 1;
    [SerializeField] float sense = 10; //mouse sensetivity
    [SerializeField] float launchpower = 5; // max launch power
    [SerializeField] float terminalvelocity = 100; // lp*tm for max speed
    [SerializeField] float maxsquish = 1.7f; // should not exceed 2.0f
    [SerializeField] float timespeed = 0.5f; // unused
    [SerializeField] float combopower = 1.5f;
    [SerializeField] Slider bar; // stamina bar
    [SerializeField] Slider combo;
    [SerializeField] Text t; // score text
    [SerializeField] Text bal; // balance text
    [SerializeField] GameObject indicator;
    [SerializeField] Image upgmenu;
    Rigidbody2D rb;
    int score = 0;
    int cscore = 0; //score in current combo
    int maxcombo = 3;
    float scoremult = 1;

    void UpdRotation()
    {
        Vector3 at = rb.linearVelocity.normalized;
        float angle = Mathf.Atan2(at.y, at.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    Quaternion GetRotation(Vector3 v1, Vector3 v2)
    {
        Vector3 at = v1-v2;
        float angle = Mathf.Atan2(at.y, at.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0, 0, angle);
    }
    void UpdSize()
    {
        float vx = math.abs(rb.linearVelocity.magnitude);
        vx /= terminalvelocity;
        vx *= maxsquish;
        if (vx <= 1)
        {
            vx = 1;
        }
        else if (vx >= maxsquish)
        {
            vx = maxsquish;
        }
        gameObject.transform.localScale = new(vx, math.abs(2 * size - vx), 0);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        upgmenu.transform.localScale *= 0;
        if (maxsquish >= 2)
        {
            maxsquish = 1.9f;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            start();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            launch();
        }
        if (origin != Vector2.one * -1)
        {
            indicator.transform.localScale = new Vector3(0.4f,0.6f,1);
            indicator.transform.position = (origin-GetMousePos()).normalized;
            indicator.transform.position += transform.position;
            indicator.transform.rotation = GetRotation(indicator.transform.position, transform.position);
            indicator.transform.Rotate(new(0,0,-90));
        } else {indicator.transform.localScale = Vector3.zero;}
        if (combo.value > 0) {combo.value -= Time.deltaTime;} else {cscore=0;}
    }
    Boolean grounded = true;
    void FixedUpdate()
    {
        if (grounded)
        {
            transform.localScale = new(1, 1);
            rb.totalForce *= 0;
            rb.angularVelocity *= 0;
            bar.value = bar.maxValue;
        }
        else
        {
            UpdRotation();
            UpdSize();
        } 
        if (rb.linearVelocity.magnitude > terminalvelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * terminalvelocity;
        }
        
    }
    Vector2 origin = new(-1,-1);
    Vector2 GetMousePos()
    {
        Vector2Control m = Mouse.current.position;
        Vector2 v = Vector2.zero;
        v.x = m.x.value;
        v.y = m.y.value;
        return v;
    }
    
    void start()
    {
        if (bar.value <= 0)
        {
            return;
        }
        origin = GetMousePos();
        
    }
    void launch()
    {
        if (origin == Vector2.one * -1 || bar.value <= 0)
        {
            origin = new(-1,-1);
            return;
        }
        grounded = false;
        bar.value -= 1;
        Vector2 dist = origin - GetMousePos();
        dist.x /= Screen.width;
        dist.y /= Screen.height;
        origin = Vector2.one * -1;
        Vector2 nv = launchpower * sense * dist;
        rb.linearVelocity = nv;

        if (rb.linearVelocity.magnitude > launchpower * 2)
        {
            rb.linearVelocity = 2 * launchpower * rb.linearVelocity.normalized;
        }
        
    }
    void timetoggle() // might use when aiming
    {
        if (rb.gravityScale < 1)
        {
            rb.gravityScale = 1;
            rb.linearVelocity /= timespeed;
        } else
        {
            rb.gravityScale = 0;
            rb.linearVelocity *= timespeed;
        }
    }
    void OnCollisionEnter2D(Collision2D hit)
    {
        if (hit.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        } else if (hit.gameObject.CompareTag("Death"))
        {
            died();
        } else if (hit.gameObject.CompareTag("Orb"))
        {
            float m = 1;
            Destroy(hit.gameObject);
            Destroy(hit.gameObject);
            bar.value = bar.maxValue;
            if (hit.gameObject.name.Contains("big")){m = 2;}
            if (combo.value > 0) {m*=cscore*combopower;}
            score += (int)math.ceil(scoremult*m);
            if (cscore<maxcombo) {cscore += 1;}
            combo.value = combo.maxValue;
            t.text = "Score: " + score * 100;
        }
        //rb.linearVelocity *= 0;
    }
    float GetIncrease(String t)
    {
        int b;
        int n = t.IndexOf('+');
        String m = t.Filter(false,true,true,false,false);
        String[] d = m.Split(" ");
        print(d[1]);
        String s = t.Substring(n+1, t.IndexOf(' ')-n-1);
        if (t.Contains('%')) {b=int.Parse(s.Substring(0,s.Length-1))/100;} 
        else {b = int.Parse(s.Filter(false,true,false,false,false));}
        return b;
    }
    public void AddStat(String stat)
    {
        stat = stat.ToLower();
        if (stat.Contains("orb") && stat.Contains("value"))
        {
            scoremult += GetIncrease(stat);
        } else if (stat.Contains("combo") && stat.Contains("time"))
        {
            combo.maxValue += GetIncrease(stat);
            
        } else if (stat.Contains("combo") && stat.Contains("power"))
        {
            maxcombo += (int)math.ceil(GetIncrease(stat));
        } else if (stat.Contains("combo"))
        {
            combopower += GetIncrease(stat);
        } else if (stat.Contains("stamina"))
        {
            int n = (int)math.ceil(GetIncrease(stat));
            bar.maxValue += n;
            bar.value += n;
        }
    }
    
    void died()
    {
        bar.value = 0;
        if (rb.linearVelocityY < 0) {rb.linearVelocityY*=-1;}
        rb.gravityScale = 0;
        t.text = "Score: 0";
        int cash = int.Parse(bal.text.Substring(bal.text.LastIndexOf(' '))) + score;
        bal.text = "Balance: " + cash.ToString();
        score = 0;
        upgmenu.transform.localScale = Vector3.one;
        // change scene to upgrade menu
    }
    public void Begin()
    {
        transform.SetPositionAndRotation(new(0, 1, 0), quaternion.identity);
        rb.linearVelocity *= 0;
        bar.value = bar.maxValue;
        gameObject.isStatic = false;
        rb.gravityScale = 1;
        upgmenu.transform.localScale *= 0;
        bar.value = bar.maxValue;
    }
    
}
