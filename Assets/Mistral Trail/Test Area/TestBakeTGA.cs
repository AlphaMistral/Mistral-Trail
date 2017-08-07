using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Effects.Trail;

public class TestBakeTGA : MonoBehaviour 
{
	public AnimationCurve sizeOverLife;
	public Gradient colorOverLife;

	public Texture2D mainTex;
	public Color mainTexTint;

	private void Start()
	{
		SmoothTrail st = GetComponent<SmoothTrail>();
		sizeOverLife = st.parameter.sizeOverLife;
		colorOverLife = st.parameter.colorOverLife;

		mainTex = st.parameter.trailMaterial.GetTexture("_MainTex") as Texture2D;
		mainTexTint = st.parameter.trailMaterial.GetColor("_TintColor");

		TGABakerParameter param = new TGABakerParameter(sizeOverLife, colorOverLife, mainTex, mainTexTint, 512, 64);
		TrailTGABaker.Initialize(param);

		File.WriteAllBytes(Application.dataPath + "/Images/Fucker.tga", TrailTGABaker.GenerateTexture());
		Debug.Log(TrailTGABaker.GenerateTexture().Length);
	}
}
