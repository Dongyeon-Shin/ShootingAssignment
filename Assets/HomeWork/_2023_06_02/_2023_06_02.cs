using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class _2023_06_02 : MonoBehaviour
{
    [SerializeField]
    private bool fire = false;
    private ParticleSystem hitEffect;
    private TrailRenderer trailRenderer;

    private void Awake()
    {
        hitEffect = GameManager.Resource.Load<ParticleSystem>("Prefabs/HitEffect");
        trailRenderer = GameManager.Resource.Load<TrailRenderer>("Prefabs/BulletTrail");
    }
    RaycastHit hit;
    private void Update()
    {
        if(fire)
        {
            Debug.DrawRay(transform.position, transform.forward, Color.red, 30f);
            if (Physics.Raycast(transform.position, transform.forward,out hit, 30f))
            {
                StartCoroutine(HitEffectRoutine(hit));
                StartCoroutine(BulletTrailRoutine(hit));
            }
            fire = false;

        }
    }
    IEnumerator HitEffectRoutine(RaycastHit hit)
    {
        ParticleSystem effect = GameManager.Pool.Get(hitEffect, hit.point, Quaternion.LookRotation(hit.normal), this.transform);
        yield return new WaitForSeconds(1f);
        GameManager.Pool.Release(effect);
    }
    IEnumerator BulletTrailRoutine(RaycastHit hit)
    {
        TrailRenderer bulletTrail = GameManager.Pool.Get(trailRenderer, transform.position, Quaternion.LookRotation(hit.normal), this.transform);
        //TODO: 시작위치를 muzzlePosition으로 바꾸기
        float totalTime = Vector3.Distance(transform.position, hit.point) / 30;
        float time = 0;
        while (time < 1)
        {
            bulletTrail.transform.position = Vector3.Lerp(transform.position, hit.point, time);
            time += Time.deltaTime / totalTime;
            yield return null;
        }
        GameManager.Pool.Release(bulletTrail);
    }
}
