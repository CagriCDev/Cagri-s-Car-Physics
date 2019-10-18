using System;
using System.Collections;

using UnityEngine;

[System.Serializable]
public class WheelInfos
{
    public WheelCollider leftWheelCollider;
    public WheelCollider rightWheelCollider;
    public GameObject leftWheelMesh;
    public GameObject rightWheelMesh;
    public bool motor;
    public bool steering;
}
public class CagriVechiclelPhysics : MonoBehaviour
{
    [Range(0, 1)] [SerializeField] private float myTractionControl;
    [SerializeField] private float m_SlipLimit;
    public Transform carBody;
    public System.Collections.Generic.List<WheelInfos> WheelInfoss;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float brakeTorque;
    public bool braking=false;
    public float decelerationForce;
    private Transform thisTransform;
    private Transform oneSecAfterTransform;
    private Rigidbody myrb;
    private float motor;
    private float CalculatedMotor;
    private void Start()
    {
        CalculatedMotor = maxMotorTorque - (myTractionControl * maxMotorTorque);
           myrb = GetComponent<Rigidbody>();
       // myrb.centerOfMass +=new Vector3(0, 0, 1.0f);
       
    }
    public void WheelsPositionCheck(WheelInfos WheelInfos)
    {
        Vector3 position;
        Quaternion rotation;
        WheelInfos.leftWheelCollider.GetWorldPose(out position, out rotation);
        WheelInfos.leftWheelMesh.transform.position = position;
        WheelInfos.leftWheelMesh.transform.rotation = rotation;
        WheelInfos.rightWheelCollider.GetWorldPose(out position, out rotation);
        WheelInfos.rightWheelMesh.transform.position = position;
        WheelInfos.rightWheelMesh.transform.rotation = rotation;
    }
    void FixedUpdate()
    {
        motor = CalculatedMotor;
        thisTransform = gameObject.transform;
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        Debug.Log(motor);
        Debug.Log(steering);
        for (int i = 0; i < 2; i++)
        {
            if (WheelInfoss[i].steering)
            {
                Steering(WheelInfoss[i], steering);
            }
            if (WheelInfoss[i].motor)
            {
                TractionControl(WheelInfoss[i]);
                Acceleration(WheelInfoss[i], motor);
              
            }
            if (Input.GetKey(KeyCode.Space))
            {
                braking = true;
               Brake(WheelInfoss[1],motor);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                braking = false;

            }
            if (Input.GetKey(KeyCode.S))
            {
                if (WheelInfoss[i].leftWheelCollider.motorTorque > 0f)
                {
                    braking = true;
                    WheelInfoss[i].leftWheelCollider.brakeTorque += Mathf.Infinity;
                    WheelInfoss[i].rightWheelCollider.brakeTorque += Mathf.Infinity;
                }
                else
                {
                    braking = false;
                }
                //TiltoverFrontBack(1f, 1f);//fren durumunda ileri yigilma icin parametre gonder**
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                braking = false;

            }
            WheelsPositionCheck(WheelInfoss[i]);
        }
        oneSecAfterTransform = gameObject.transform;
    }
    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && CalculatedMotor >= 0)
        {
            CalculatedMotor -= 10 * myTractionControl;
        }
        else
        {
            CalculatedMotor += 10 * myTractionControl;
            if (CalculatedMotor > maxMotorTorque)
            {
                CalculatedMotor = maxMotorTorque;
            }
        }
    }
    //Torque duzenleyelim**
    private void TractionControl(WheelInfos wheelInfos)
    {
        WheelHit wheelHitL;
        WheelHit wheelHitR;

       wheelInfos.leftWheelCollider.GetGroundHit(out wheelHitL);
       wheelInfos.rightWheelCollider.GetGroundHit(out wheelHitR);   
       AdjustTorque(wheelHitL.forwardSlip);
       AdjustTorque(wheelHitR.forwardSlip);



    

          
       
    }
    private void Acceleration(WheelInfos WheelInfos, float motor)
    {
        if (motor != 0f && braking == false)
        {
            Debug.Log(WheelInfos.leftWheelCollider.motorTorque + "    look");
            WheelInfos.leftWheelCollider.brakeTorque = 0;
            WheelInfos.rightWheelCollider.brakeTorque = 0;
            WheelInfos.leftWheelCollider.motorTorque = motor;
            WheelInfos.rightWheelCollider.motorTorque = motor;
            if (WheelInfos.rightWheelCollider.motorTorque > 0)
            {
                TiltoverFrontBack(-1f,1f); // geri yigilma parametre gonder**
            }
            if (Input.GetAxis("Vertical") < 0)
            {
             
                TiltoverFrontBack(1f,1f);//fren durumunda ileri yigilma icin parametre gonder**
            }
            }
      
        //else if (motor < 0)
        //{
        //    WheelInfos.leftWheelCollider.motorTorque = motor*brakeTorque;
        //    WheelInfos.rightWheelCollider.motorTorque = motor* brakeTorque;
        //    myrb.AddForceAtPosition(gameObject.transform.position * (-Input.GetAxis("Vertical"))*brakeTorque, gameObject.transform.position*brakeTorque);
        //}
        else
        {
            Deceleration(WheelInfos, motor);
        }
    }
    private void Deceleration(WheelInfos WheelInfos, float motor)
    {
        TiltoverFrontBack(0.01f);
        if (braking != true)
        {
           // carBody.transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(carBody.transform.rotation.x - 0.010f, carBody.transform.rotation.y, carBody.transform.rotation.z, carBody.transform.rotation.w), Time.fixedDeltaTime * 35f);

          // gaz kesilmesinde yigilmayi duzeltme parametre gonder**

            WheelInfos.leftWheelCollider.brakeTorque = decelerationForce;
            WheelInfos.rightWheelCollider.brakeTorque = decelerationForce;

        }

    }
    private void ChgDeceleration(WheelInfos WheelInfos)

    {

        WheelInfos.leftWheelCollider.brakeTorque = 0;
        WheelInfos.rightWheelCollider.brakeTorque = 0;
    }

        private void Steering(WheelInfos WheelInfos, float steering)
    {
        WheelInfos.leftWheelCollider.steerAngle = steering;
        WheelInfos.rightWheelCollider.steerAngle = steering;
    }
    private void Brake(WheelInfos WheelInfos,float mtr)
    {


        WheelInfoss[0].leftWheelCollider.brakeTorque = decelerationForce;
        WheelInfoss[0].rightWheelCollider.brakeTorque = decelerationForce;

        WheelInfos.leftWheelCollider.brakeTorque = Mathf.Infinity;
        WheelInfos.rightWheelCollider.brakeTorque = Mathf.Infinity;

    }
    void TiltoverFrontBack(float tiltOverValue, float tiltzero) // tiltovervalue ileri geri yigilma durumu**
    {
        carBody.transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(carBody.transform.rotation.x* tiltzero + (0.0080f * tiltOverValue), carBody.transform.rotation.y, carBody.transform.rotation.z, carBody.transform.rotation.w), 28f * Time.fixedDeltaTime);
        if (carBody.transform.rotation.x > 1.1f || carBody.transform.rotation.x < -1.1f)
        {
            carBody.transform.rotation=new Quaternion(1.1f, carBody.transform.rotation.y, carBody.transform.rotation.z, carBody.transform.rotation.w);
        }
    }
    void TiltoverFrontBack( float tiltzero) // tiltovervalue ileri geri yigilma durumu**
    {
       
        carBody.transform.rotation = Quaternion.Lerp(carBody.transform.rotation, new Quaternion(0f, carBody.transform.rotation.y, carBody.transform.rotation.z, carBody.transform.rotation.w), Time.fixedDeltaTime * 10f);

    }

    void TiltOverSides()
    {

    }

}