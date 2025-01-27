using UnityEngine;
using System.Collections;

namespace HWRWeaponSystem
{
	public class Damage : DamageBase
	{
		public bool Explosive;
		public float ExplosionRadius = 20;
		public float ExplosionForce = 1000;
		public bool HitedActive = true;
		public float TimeActive = 0;
		private float timetemp = 0;
		private ObjectPool objPool;
	
		private void Awake ()
		{
			objPool = this.GetComponent<ObjectPool> ();	
		}

		
		private void OnEnable ()
		{
			timetemp = Time.time;
		}
	
		private void Start ()
		{
		
			if (!Owner || !Owner.GetComponent<Collider>())
				return;
		
			
		
			timetemp = Time.time;
		}

		private void Update ()
		{
			if (objPool && !objPool.Active) {
				return;
			}
		
			if (!HitedActive || TimeActive > 0) {
				if (Time.time >= (timetemp + TimeActive)) {
					Active ();
				}
			}
		}

		public void Active ()
		{
			if (Effect) {
				if (WeaponSystem.Pool != null) {
					WeaponSystem.Pool.Instantiate (Effect, transform.position, transform.rotation, 3);
				} else {
					GameObject obj = (GameObject)Instantiate (Effect, transform.position, transform.rotation);
					Destroy (obj, 3);
				}
			
			}

			if (Explosive)
				ExplosionDamage ();
		
			if (objPool) {
				objPool.OnDestroyed ();
			} else {
				Destroy (gameObject);
			}
		}

		private void ExplosionDamage ()
		{
			Collider[] hitColliders = Physics.OverlapSphere (transform.position, ExplosionRadius);
			for (int i = 0; i < hitColliders.Length; i++) {
				Collider hit = hitColliders [i];
				if (!hit)
					continue;
				if (DoDamageCheck (hit.gameObject)) {
					hit.gameObject.SendMessage ("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
					if (hit.GetComponent<Rigidbody>())
						hit.GetComponent<Rigidbody>().AddExplosionForce (ExplosionForce, transform.position, ExplosionRadius, 3.0f);
				}
			}

		}

		private void NormalDamage (Collision collision)
		{
			collision.gameObject.SendMessage ("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
		}

		private void OnCollisionEnter (Collision collision)
		{
			if (objPool && !objPool.Active && WeaponSystem.Pool!=null) {
				return;
			}
		
			if (HitedActive) {
				if (DoDamageCheck (collision.gameObject) && collision.gameObject.tag != this.gameObject.tag) {
					if (!Explosive)
						NormalDamage (collision);
					Active ();
				}
			}
		}
	}
}