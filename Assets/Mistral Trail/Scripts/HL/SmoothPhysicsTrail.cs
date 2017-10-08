using UnityEngine;
using System.Collections;
using Mistral.Effects.Trail;
using Mistral.Utility.DataStructure;

public class SmoothPhysicsTrail : SmoothTrail
{
    #region Public Variables

    [Range(-20f, 20f)]
    public float gravity = 0.0f;

    public Vector3 force = new Vector3(0f, 0f, 0f);

    [Range(0.0f, 10f)]
    public float inheritVelocity = 0f;

    [Range(0.0f, 10f)]
    public float drag = 0f;

    [Range(0.0f, 10f)]
    public float frequency = 1f;

    [Range(0.001f, 10f)]
    public float amplitude = 2f;

    public float turbulenceStrength = 0f;

    public AnimationCurve velocityByDistance = AnimationCurve.EaseInOut(0, 1, 1, 1);

    public float aproximatedFlyDistance = -1;

	#endregion

	#region Override Methods

	protected override void CustomChanges(RingBuffer<SmoothTrail.AdditionalPoint> cps, float deltatime)
    {
        UpdateForce(cps, deltatime);
    }

    #endregion

    #region Private Methods

    private void UpdateForce(RingBuffer<SmoothTrail.AdditionalPoint> cps, float deltaTime)
    {
        if (cps.Count < 1)
            return;
        Vector3 g = gravity * Vector3.down * deltaTime;
        Vector3 f = transform.rotation * force * deltaTime;
        for (int i = 0; i < cps.Count; i++)
        {
            Vector3 turbulence = Vector3.zero;
            if (turbulenceStrength > 0.000001f)
            {
                Vector3 pos = cps[i].position / frequency;

                turbulence.x += ((Mathf.PerlinNoise(pos.z, pos.y) * 2 - 1) * amplitude) * Time.deltaTime * turbulenceStrength / 10f;
                turbulence.y += ((Mathf.PerlinNoise(pos.x, pos.z) * 2 - 1) * amplitude) * Time.deltaTime * turbulenceStrength / 10f;
                turbulence.z += ((Mathf.PerlinNoise(pos.y, pos.x) * 2 - 1) * amplitude) * Time.deltaTime * turbulenceStrength / 10f;
            }
            Vector3 currentForce = g + f + turbulence;
            if (aproximatedFlyDistance > 0.01f)
            {
				float distance = Mathf.Abs((cps[i].distance2Src));
				currentForce *= velocityByDistance.Evaluate(Mathf.Clamp01(distance / aproximatedFlyDistance));
				if (drag > 0.00001f)
					currentForce -= drag * currentForce * deltaTime;
            }
			cps[i].velocity += currentForce;
            cps[i].position += cps[i].velocity * deltaTime;
        }

    }

	#endregion

	#region MonoBehaviours

	protected override void Start()
	{
		base.Start();
	}

	#endregion
}
