﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]

public class RandomPulseLight : MonoBehaviour {

	public float minIntensity = 0.25f;
	public float maxIntensity = 0.5f;

	private Light newLight = new Light ();

	float random;

	void Start()
	{
		random = Random.Range(0.0f, 65535.0f);
		newLight = gameObject.GetComponent<Light> ();
	}

	void Update()
	{
		float noise = Mathf.PerlinNoise (random, Time.time);
		newLight.intensity = Mathf.Lerp (minIntensity, maxIntensity, noise);
	}
}