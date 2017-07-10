using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MouseFollower : MonoBehaviour
{

    public List<TrailRenderer_Base> Trails;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButton(0))
	    {
            Trails.ForEach(a => a.Emit = true);

            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 0.01f));

	    }
	    else
	    {
            Trails.ForEach(a => a.Emit = false);
	    }
	}

}
