﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactWaveHandler : MonoBehaviour
{
    public List<Renderer> affectedShaders;
    public float effectMaxDistance;
    float maxDistDefault;

    public bool pulseActive = false;
    public float pulseSpeed = 3f;
    float t = 0;
    public float minimumImpact = 5f;
    public bool expectImpact = false;

    void Start()
    {
        maxDistDefault = effectMaxDistance;   
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (expectImpact)
        {
            expectImpact = false;
            affectedShaders.Clear();
            Vector3 impactVel = collision.relativeVelocity;
            Debug.Log(impactVel);
            if (impactVel.magnitude >= minimumImpact)
            {
                float shockwaveIntensity = impactVel.magnitude / 10;
                effectMaxDistance = impactVel.magnitude;
                Debug.Log("Impact Calibrating");
                Collider[] collisionPlanes = Physics.OverlapSphere(transform.position, effectMaxDistance);
                Debug.Log(collisionPlanes.Length);
                foreach (Collider groundObject in collisionPlanes)
                {
                    if (groundObject.tag == "Deformable")
                    {
                        Renderer localShader = groundObject.GetComponent<Renderer>();
                        affectedShaders.Add(localShader);
                        localShader.material.SetFloat("_Shockwave_Distance", 0);
                        localShader.material.SetVector("_Shockwave_Position", transform.position);
                        localShader.material.SetFloat("_Shockwave_Enabled", 1);
                        localShader.material.SetFloat("_Shockwave_MaxDistance", effectMaxDistance);
                        localShader.material.SetFloat("_Shockwave_Intensity", shockwaveIntensity);
                    }
                }
                pulseActive = true;
                t = 0;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pulseActive)
        {
            t += Time.deltaTime;
            float movement = pulseSpeed * Time.deltaTime ;
            float currentDistance = 0;
            foreach (Renderer shader in affectedShaders)
            {
                currentDistance = shader.material.GetFloat("_Shockwave_Distance");
                currentDistance += movement;
                shader.material.SetFloat("_Shockwave_Distance", currentDistance);
            }

            if (currentDistance >= effectMaxDistance || Mathf.Approximately(0, movement))
            {
                pulseActive = false;
                affectedShaders.Clear();
                t = 0;
                effectMaxDistance = maxDistDefault;
            }
        }

    }
}
