using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class ParticlePop : MonoBehaviour
    {
    	public ParticleSystem particles;
    	public float Radius = 220.0f;
    	public string SFX = "BalloonPop";

    	void Update()
    	{
    		if (Input.touchCount > 0)
    		{
    			bool found = false;
    			for (int i = 0; i < Input.touchCount; i++)
    			{
    				Touch touch = Input.GetTouch(i);
    				ParticleSystem.Particle[] particleArray = new ParticleSystem.Particle[particles.particleCount];
    				int particleCount = particles.GetParticles(particleArray);

    				Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(touch.position);
    				touchWorldPos.z = 0f;

    				for (int j = 0; j < particleCount; j++)
    				{
    					ParticleSystem.Particle particle = particleArray[j];

    					float distance = Vector3.Distance(particle.position.SetZ(0), touchWorldPos);
    					if (distance < Radius)
    					{
    						found = true;
    						particle.remainingLifetime = 0f;
    						particleArray[j] = particle;
    						break;
    					}
    				}

    				particles.SetParticles(particleArray, particleCount);
    			}

    			if (found)
    			{
    				SoundManager.Instance.PlaySFX(SFX);
    			}
    		}
    	}
    }

}