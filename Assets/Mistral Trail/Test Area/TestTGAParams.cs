using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Effects.Trail;

public class TestTGAParams : MonoBehaviour 
{
	public Material FuckingMat;

	public TGABakerParameter param;

	private void Start()
	{
		param = new TGABakerParameter(null, null, FuckingMat);
		Debug.Log(param.mainTexture);
	}
}
