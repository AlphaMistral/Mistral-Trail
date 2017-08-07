using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Effects.Trail;

public class TestTGABaker : MonoBehaviour 
{
	TGABakerParameter param;

	public Material mat;

	private void Start()
	{
		param = new TGABakerParameter(null, null, mat);
		TrailTGABaker.Initialize(param);
		TrailTGABaker.GenerateTexture();
	}
}
