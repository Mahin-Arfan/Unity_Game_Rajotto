using UnityEngine;

public class Recoil : MonoBehaviour
{
    //Rotation
    public Vector3 currentRotation;
    public Vector3 targetRotation;

    public float returnSpd = 0f;
    public float snappinss = 0f;
    //BulletSpread
    public float currentBulletSpread = 0f;
    float targetBulletSpread = 0f;
    float maxBulletSpread = 0.5f;

    void Start()
    {
        
    }

    void Update()
    {
        //Recoil
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpd * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappinss * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
        //Bullet Spread
        targetBulletSpread = Mathf.Lerp(targetBulletSpread, 0f, returnSpd * Time.deltaTime);
        currentBulletSpread = Mathf.Lerp(currentBulletSpread, targetBulletSpread, snappinss * Time.fixedDeltaTime);
        
        if (Mathf.Abs(currentBulletSpread) < 0.0001f)
        {
            currentBulletSpread = 0f;
            targetBulletSpread = 0f;
        }
    }

    public void RecoilFire(float recX, float recY, float recZ, float RSpeed, float Snap)
    {
        returnSpd = RSpeed;
        snappinss = Snap;
        targetRotation += new Vector3(recX, Random.Range(-recY, recY), Random.Range(-recZ, recZ));
    }

    public void BulletSpread(float spreadAmount)
    {
        maxBulletSpread = spreadAmount;
        targetBulletSpread += spreadAmount;
    }
}
