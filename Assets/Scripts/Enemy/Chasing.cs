using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Chasing : MonoBehaviour {
	NetworkManager networkManager;
	Animator animator;
	HealthManager healthManager;
	NavMeshAgent agent;
	AudioSource audioSource;
	public GameObject target;
	public float damage = 15.0f;
	public bool isAttacking = false;
	public bool shouldChase = true;
	public bool isInLateUpdate = false;
	public bool shouldUpdate = true;

	public AudioClip attackSound;
	public AudioClip deathSound;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		healthManager = GetComponent<HealthManager>();
		agent = GetComponent<NavMeshAgent>();
		audioSource = GetComponent<AudioSource>();

		networkManager = GameObject.Find("GameManager").GetComponent<NetworkManager>();
	}

	// Update is called once per frame
	void Update () {
		if(!shouldUpdate) return;

		if(!healthManager.IsDead) {
			if(!isAttacking) {
				
					float distance = GetActualDistanceFromTarget();

						isInLateUpdate = false;
						agent.destination = target.transform.position;
		
			}

			animator.SetFloat("SpeedMultiplier", agent.speed);

			if(agent.pathPending) return;

			CheckAttack();
		}
		else {
			shouldUpdate = false;
			shouldChase = false;
			
			audioSource.PlayOneShot(deathSound);

			Destroy(agent);

			StartCoroutine(RemoveGameObject());
			return;
		}
	}


	float GetActualDistanceFromTarget() {
		return GetDistanceFrom(target.transform.position, this.transform.position);
	}

	float GetDistanceFrom(Vector3 src, Vector3 dist) {
		return Vector3.Distance(src, dist);
	}

	float origSpeed;

	void CheckAttack() {
	
		float distanceFromTarget = GetActualDistanceFromTarget();
		
		Vector3 direction = target.transform.position - this.transform.position;
		float angle = Vector3.Angle(direction, this.transform.forward);

		if(!isAttacking && distanceFromTarget <= 2.0f && angle <= 60f) {
			isAttacking = true;
			shouldChase = false;

			origSpeed = agent.speed;
			agent.speed = 0;

			audioSource.PlayOneShot(attackSound);
			animator.SetTrigger("Attack");

			HealthManager targetHealthManager = target.GetComponent<HealthManager>();

			if(targetHealthManager) {
				targetHealthManager.ApplyDamage(damage);
			}

			StartCoroutine(ResetAttacking());
		}
	}

	IEnumerator ResetAttacking() {
		yield return new WaitForSeconds(1.4f);

		isAttacking = false;
		shouldChase = true;

		if(!healthManager.IsDead) 
		{
			agent.speed = origSpeed;
		}

		yield break;
	}

	IEnumerator RemoveGameObject() {
		yield return new WaitForSeconds(5f);
	
		Destroy(gameObject);
	}
}
