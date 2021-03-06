﻿using UnityEngine;
using System.Collections;

public class CameraHandler : MonoBehaviour {

    Transform playerpos;

    public bool followPlayer;
    public AnimationCurve bump;
    public float bumpTime;
    public float bumpAmplitude;
    Vector3 origPos;

    [SerializeField]
    AnimationCurve zoomcurve;
    [SerializeField]
    float ZoomTime;

	// Use this for initialization
	void Start () {
        origPos = transform.position;
        playerpos = GameObject.FindGameObjectWithTag("Player").transform;
	}


    IEnumerator CoBump() {

        float c = 0.0f;
        
        do
        {
            
            c += Time.deltaTime*1/bumpTime;
            transform.position = origPos + new Vector3(0f,1f,0f)*bump.Evaluate(c)*bumpAmplitude;
            yield return new WaitForEndOfFrame();

        } while (c < 1.0f);

        transform.position = origPos;
       
    }

    public void PlayBump()
    {
        StartCoroutine(CoBump());
    }

    IEnumerator ZoomToPosition(Vector2 targetPos) {

        float originalOr = Camera.main.orthographicSize;
        float targetOrthoSize = 0.4f;
        Vector2 origPos = transform.position;
        
       // Camera.main.orthographicSize = 0.4f;

        float t = 0f;
        
        while(t<ZoomTime)
        {
            t += Time.deltaTime;
             yield return new WaitForEndOfFrame();

            

            transform.position = Vector2.Lerp(origPos,targetPos, zoomcurve.Evaluate( t/ZoomTime));
            Camera.main.orthographicSize = Mathf.Lerp(originalOr, targetOrthoSize,zoomcurve.Evaluate( t / ZoomTime));
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        }

        GameObject.FindObjectOfType<Canvas>().GetComponent<Animator>().SetTrigger("GoCinematographic");
        //GetComponent<Animator>().SetTrigger("GoCinematographic");
        
           

        


        //transform.position = new Vector3(targetPos.x, targetPos.y, -1);
        

    
    }
    public void zoomTo(Vector2 targetPos) {
        //Debug.Log("Going to "+targetPos.ToString());
        followPlayer = false;
        StartCoroutine(ZoomToPosition(targetPos));
    }
	
	// Update is called once per frame
	void Update () {

        

        if (followPlayer)
        {

            if (playerpos.position.y > 0)
            {
                transform.position = new Vector3(playerpos.position.x, transform.position.y, transform.position.z);
            }

            else
                transform.position = new Vector3(playerpos.position.x, playerpos.position.y, transform.position.z);
        
        }
            
	
	}
}
