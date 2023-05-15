using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Rotates the players arms to point in the direction of the object they grabbed.
*
* @author Colin Keys
*/
public class GrabbingAnimation : MonoBehaviour
{
    public GameObject rightClavicle;
    public GameObject leftClavicle;

    public GameObject rightArm;
    public GameObject leftArm;

    private SacboyController player;
    private GameObject grabbedObject = null;
    private GameObject armEmpty = null;
    private Vector3 leftOffset = Vector3.zero;
    private Vector3 rightOffset = Vector3.zero;
    private Vector3 leftArmRotation = Vector3.zero;
    private Vector3 rightArmRotation = Vector3.zero;

    // Called when the script instance is being loaded.
    void Awake(){
        //rightClavicle = GameObject.Find("Bip01 R Clavicle");
        //leftClavicle = GameObject.Find("Bip01 L Clavicle");
        //rightArm = GameObject.Find("Bip01 R UpperArm");
        //leftArm = GameObject.Find("Bip01 L UpperArm");
        player = GetComponent<SacboyController>();
    }

    // Called after all update functions have been called.
    void LateUpdate(){
        rightArmRotation = Vector3.zero;
        leftArmRotation = Vector3.zero;
        grabbedObject = player.grabbedObject;
        // Only rotate if grabbing something.
        if(grabbedObject != null){
            SetVars();
            //Right
            CreateArmParent("Right", rightArm, rightClavicle);
            RotateArm(rightArm, player.closestHit, player.playerCollCenter, rightOffset, rightArmRotation);
            DestroyArmParent(rightArm, rightClavicle);
            //Left
            CreateArmParent("Left", leftArm, leftClavicle);
            RotateArm(leftArm, player.closestHit, player.playerCollCenter, leftOffset, leftArmRotation);
            DestroyArmParent(leftArm, leftClavicle);
        }
    }

    /*
    *   RotateArm - Rotates the arm parent and then the arm to create a rotation that points towards the grab point with an offset.
    *   @param arm - GameObject of the arm to rotate.
    *   @param closestHit - Collider of the closest grabbed collider.
    *   @param playerCollCenter - Vector3 of the center of the players box collider in the world.
    *   @param offSet - Vector3 of the offset from the point grabbed to point the arm towards.
    *   @param armRotation - Vector3 of the euler value to rotate the arm by after rotating the parent.
    */
    private void RotateArm(GameObject arm, Collider closestHit, Vector3 playerCollCenter, Vector3 offSet, Vector3 armRotation){
            // Closest point to the clavicle.
            Vector3 grabbedPoint = closestHit.ClosestPoint(new Vector3(playerCollCenter.x, playerCollCenter.y + 0.15f, playerCollCenter.z));
            // Get the world position of the grab joints anchor.
            Vector3 anchor = gameObject.GetComponent<SpringJoint>().connectedAnchor;
            Vector3 world = grabbedObject.transform.TransformPoint(anchor);
            // Direction to point the arm in.
            Vector3 direction = world - armEmpty.transform.position;
            // Apply the offset.
            direction.x = direction.x + offSet.x;
            direction.y = direction.y - offSet.y;
            direction.z = direction.z - offSet.z;
            // Create a rotation from the players up to the direction calculated.
            Quaternion toRotation = Quaternion.FromToRotation(transform.up, direction);
            // Apply created rotation to the arms parent.
            armEmpty.transform.rotation = toRotation;
            // Rotate the arm by the given euler angle if it isn't at it already.
            if(arm.transform.localEulerAngles != armRotation)
                arm.transform.localEulerAngles = armRotation;
    }

    /*
    *   CreateArmParent - Creates an empty GameObject as a child of the given clavicle. The empty GameObject becomes 
    *   the parent of that side of the models arm.
    *   @param armName - String of the side of the body.
    *   @param arm - GameObject of the players arm.
    *   @param clavicle - GameObject of the players clavicle.
    */
    private void CreateArmParent(string armName, GameObject arm, GameObject clavicle){
        armEmpty = null;
        // Only create a parent if one doesn't exist.
        if(GameObject.Find(armName + " Arm Parent") == null){
            // Set the new GameObjects parent to be the given clavicle.
            armEmpty = new GameObject(armName + " Arm Parent");
            armEmpty.transform.SetParent(clavicle.transform, true);
            // Set the new GameObjects transform properties to be the given arms.
            armEmpty.transform.localPosition = arm.transform.localPosition;
            armEmpty.transform.localEulerAngles = arm.transform.localEulerAngles;
            armEmpty.transform.localScale = arm.transform.localScale;
            // Set the given arms parent to be the new GameObject and zero its transform.
            arm.transform.SetParent(armEmpty.transform, true);
            arm.transform.localPosition = Vector3.zero;
            arm.transform.localEulerAngles = Vector3.zero;
            arm.transform.localScale = new Vector3(1, 1, 1);   
        }
    }

    /*
    *   DestoryArmParent - Destroy the parent of the arm and set its parent back to the clavicle.
    *   @param arm - GameObject of the arm.
    *   @param clavicle - GameObject of the clavicle.
    */
    private void DestroyArmParent(GameObject arm, GameObject clavicle){
        arm.transform.SetParent(clavicle.transform, true);
        Destroy(armEmpty);
    }

    /*
    *   SetVars - Sets the euler rotation and direction offsets to be used depending on the players forward direction.
    */
    private void SetVars(){
        // Facing right.
        if(transform.eulerAngles.y == 0){
            rightArmRotation.y = 90f;
            leftArmRotation.y = -90f;
            rightOffset = new Vector3(0.25f, 0.5f, 0f);
            leftOffset = new Vector3(-0.25f, 0.5f, 0f);
        }
        // Facing left.
        if(transform.eulerAngles.y == 180){
            rightArmRotation.y = -90f;
            leftArmRotation.y = 90f;
            rightOffset = new Vector3(-0.25f, 0.5f, 0f);
            leftOffset = new Vector3(0.25f, 0.5f, 0f);
        }
        // Facing front.
        if(transform.eulerAngles.y == 90){
            rightArmRotation.y = 180f;
            leftArmRotation.y = 0f;
            rightOffset = new Vector3(0.5f, 0, 0.5f);
            leftOffset = new Vector3(0.5f, 0, -0.5f);
        }
    }
}
