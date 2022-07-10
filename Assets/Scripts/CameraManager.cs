using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    float h;
    float v;


    public float x;
    public float y;
    public Transform mainCamera;
    public Transform pivot;
    public Transform finalpose;
    public Transform target;
    public Canvas cs;
    public bool aim;

    public LayerMask Collision;

    public void Look(Vector2 pos)
    {
        h = pos.y;
        v = pos.x;
    }
    public void Update()
    {
        if (!InputHandler.instance.state.death)
        {
            x += h * Time.timeScale;
            y += v * Time.timeScale;
            pivot.transform.eulerAngles += new Vector3(-h, v, 0);
        }
        Followplayer();
        HandleCollision();
    }
    void Followplayer()
    {
        if(!InputHandler.instance.state.death)
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * 8 );
    }
    void HandleCollision()
    {

        if (Time.timeScale == 0)
            return;
        float distance = finalpose.localPosition.z;
        distance = Mathf.Abs(distance);
        Vector3 dir = finalpose.position - pivot.position;
        Vector3 defCam = mainCamera.localPosition;

        if (Physics.SphereCast(pivot.transform.position,0.3f, dir, out RaycastHit hit, distance, Collision))
        {

            float dis = Vector3.Distance(finalpose.position, hit.point) + 0.2f;
            defCam.z = Mathf.Lerp(defCam.z, dis, Time.deltaTime * 8);
            collidingOnZ = true;
        }
        else
        {
            collidingOnZ = false;
            defCam.z = Mathf.Lerp(defCam.z, 0, Time.deltaTime * 5);
        }
        defCam.z += recoilZ;
        mainCamera.localPosition = defCam;
    }

    float recoilZ;
    bool collidingOnZ;
    public IEnumerator BackRecoil(float duration , float _recoilZ = -0.015f)
    {
        float timer = 0;
        yield return new WaitWhile(() =>
        {
            recoilZ = Mathf.MoveTowards(recoilZ, _recoilZ, Time.deltaTime);
            timer += Time.deltaTime;
            return  timer < 0.1f && !collidingOnZ;
        });
        if (InputHandler.instance.moveamount > 0)
            recoilZ *= 2;
        timer = 0;
        float startrec = recoilZ;

        while (timer < duration)
        {
            float t = timer / duration;
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            recoilZ = Mathf.Lerp(startrec, 0, t);
            timer += Time.deltaTime;
            yield return null;
        }
        recoilZ = 0;
    }

}
