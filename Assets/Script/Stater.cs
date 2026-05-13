using Unity.VisualScripting;
using UnityEngine;

public class Stater : MonoBehaviour
{
    Animator a;
    fps f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        a=GetComponent<Animator>();
        f=GetComponentInParent<fps>();
    }

    // Update is called once per frame
    float oldvault=0;
    void Update()
    {
        a.SetFloat("Speed", f.speed/f.maxspeed);
        if (f.speed==0)
        {
            a.SetFloat("Speed", -1);
        }
        
        
        if (f.vault > oldvault)
        {
            a.SetTrigger("Vault"); 
        }
        a.SetBool("Jumping", f.j && f.vel.y>=0);
        oldvault = f.vault;
        a.SetBool("Falling", !f.grounded && !a.GetBool("Jumping"));
        a.SetInteger("Wr", f.wr);
    }
}
