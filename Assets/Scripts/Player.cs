﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public enum PlayerState { 
        Idle,
        ChargeJump,
        Jump,
        HitFloor,
        Hit
    }

    public float ChargeStart { get; set; }
    [SerializeField]
    private float minChargeTime = 0.1f;
    public bool HasCharged { get; set; }

    public AnimationCurve jumpCurve;

    public float jumpSpeed;
    public float jumpHeight;

    public PlayerState state;

    public Sprite s_idle1;
    public Sprite s_idle2;
    public Sprite s_chargejump;
    public Sprite s_jumpUp;
    public Sprite s_jumpDown;
    public Sprite s_hitFloor;
    public Sprite s_gethit;

    public float idleAnimSpeed;
    float idleAnimCount;

    public float hitFloorDelay;
    float hitFloorCount;

    Vector3 origPos;
    float previousy;

    float jumptimer;

    [SerializeField]
    ParticleSystem BloodParticleEmitter,JumpParticles;

    [HideInInspector]
    public CameraHandler cam;

    SpriteRenderer sr;
    public event System.Action OnDeath;


    float releaseTimer;
    [HideInInspector]
    public bool disabledJump;


    public bool CheckIfHit() {
        return state == PlayerState.Hit;
    }
    void SetSprite(Sprite sprite) {
        sr.sprite = sprite;
    }
	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
        state = PlayerState.Idle;
        origPos = transform.position;
        cam = GameObject.FindObjectOfType<CameraHandler>();


        // Subscribe to input events
        InputManager.OnButtonDown += new InputManager.InputHandler(HandleButtonDown);
        InputManager.OnButton += new InputManager.InputHandler(HandleButton);
        InputManager.OnButtonUp += new InputManager.InputHandler(HandleButtonUp);

        //JumpParticles.transform.parent = null;

    }

    IEnumerator StunShake()
    {
        float shakeLenght = 0.4f;
        Vector3 originalposition = transform.position;

        while (shakeLenght > 0)
        {
            shakeLenght -= Time.deltaTime;
            transform.position = originalposition + new Vector3(Mathf.Sin(shakeLenght * 100) * 0.05f, 0, 0);
            yield return new WaitForEndOfFrame();
        }
        transform.position = originalposition;
    }

    public void GetStunned()
    {
        //Debug.Log("Player stunned");
        sr.sprite = s_hitFloor;
        StartCoroutine(StunShake());
        this.enabled = false;
    }

    private void HandleButtonDown() {
        //Debug.Log("Button down");
    }

    private void HandleButton() {
        //Debug.Log("Button");
    }

    private void HandleButtonUp() {
        //Debug.Log("Button up");
    }

    public void GetHit()
    {
        //Time.timeScale = 0.5f;
        AudioManager.PlayClip(AudioManager.Instance.playerGetHit);
        state=PlayerState.Hit;
        SetSprite(s_gethit);
        cam.followPlayer = false;
        Invoke("CameraOnKill", 0.1f);
        cam.PlayBump();
        //cam.transform.Rotate(new Vector3(0f, 0f, -2f));
        BloodParticleEmitter.Play();

        if(OnDeath != null) {
            OnDeath();
        }
        

    }

    void CameraOnKill() {

        //Vector3 target = GameObject.FindObjectOfType<Enemy>().transform.position;

        Enemy[] enemiesOnScreen = GameObject.FindObjectsOfType<Enemy>();
        for (int i = 0; i < enemiesOnScreen.Length; i++)
        {
            if (enemiesOnScreen[i].hasKilledThePlayer)
            {
                //Debug.Log(enemiesOnScreen[i].transform.name);
                cam.zoomTo(enemiesOnScreen[i].transform.position);
                
                //GameObject.FindObjectOfType<Canvas>().GetComponent<Animator>().SetTrigger("GoCinematographic");
            }
                
        }
        
        

    }

    public void PlaySwingSFX()
    {
        AudioManager.PlayClip(AudioManager.Instance.swing);
    }

    public void PlayCoinSFX()
    {
        AudioManager.PlayClip(AudioManager.Instance.coin);
    }


    
	
	// Update is called once per frame
	void Update () {
		if(state == PlayerState.Idle) {
            idleAnimCount += Time.deltaTime;
			if (idleAnimCount > idleAnimSpeed)
			{
				if(sr.sprite != s_idle2)
					SetSprite(s_idle2);
				else
				{
					SetSprite(s_idle1);
				}
				idleAnimCount = 0f;
			}

            

			if (InputManager.State == InputManager.InputState.ButtonDown || 
                InputManager.State == InputManager.InputState.Button||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKey(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.M))
            {
                if (disabledJump)
                    return;
				SetSprite(s_chargejump);
				state = PlayerState.ChargeJump;
                ChargeStart = Time.time;

            }
        }




        if (state == PlayerState.ChargeJump)
        {

            // Update Charged flag
            if(Time.time - ChargeStart > minChargeTime) {
                HasCharged = true;
            } else {
                HasCharged = false;
            }



            if (Time.time - ChargeStart > minChargeTime ||
                InputManager.State == InputManager.InputState.ButtonUp ||
                Input.GetKeyUp(KeyCode.Space) ||
                Input.GetKeyUp(KeyCode.M)) {
                JumpParticles.Play();
                jumptimer = 0f;
                SetSprite(s_jumpUp);
                state = PlayerState.Jump;
                
            }
        }

        if (state == PlayerState.Jump)
        {
            jumptimer += Time.deltaTime * jumpSpeed;
            float j = jumpCurve.Evaluate(jumptimer)*jumpHeight;
            transform.position = origPos+ new Vector3(0, j, 0);
            if (transform.position.y<previousy)
            {
                SetSprite(s_jumpDown);
            }

            previousy = transform.position.y;

            if (jumptimer > 1.0f)
            {
                SetSprite(s_hitFloor);
                state = PlayerState.HitFloor;
                JumpParticles.Play();

                hitFloorCount = 0f;
                cam.PlayBump();
            }
        }

        if (state == PlayerState.HitFloor)
        {
            hitFloorCount += Time.deltaTime;
            if (hitFloorCount > hitFloorDelay)
            {
                state = PlayerState.Idle;
                ChargeStart = Mathf.Infinity;
                SetSprite(s_idle1);
                idleAnimCount = 0.0f;
            }
        }
        
	}
}
