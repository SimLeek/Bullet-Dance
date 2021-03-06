﻿//
//SpringCollider for unity-chan!
//
//Original Script is here:
//ricopin / SpringCollider.cs
//Rocket Jump : http://rocketjump.skr.jp/unity3d/109/
//https://twitter.com/ricopin416
//
using UnityEngine;
using System.Collections;

namespace UnityChan
{
	public class SpringCollider : MonoBehaviour
	{
		//半径
		public float radius = 0.5f;
        public bool draw_gizmos = false;

		private void OnDrawGizmosSelected ()
		{
            if (draw_gizmos)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, radius);
            }
		}
	}
}