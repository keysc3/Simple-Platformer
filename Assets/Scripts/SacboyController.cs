using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

/*
* Purpose: Implements character movement and controls for the player.
*
* @author Colin Keys
*/
public class SacboyController : MonoBehaviour
{
    public Sacboy sacboy;
    public Vector3 playerCollCenter;
    public GameObject grabbedObject = null;
    public Collider closestHit;

    private Rigidbody rb;
    private Animator animator;
    private BoxCollider playerCollider;
    private GrabJoint grabJointScript;

    [SerializeField] private float speed = 50.0f;
    [SerializeField] private float maxVelocity = 7.0f;
    [SerializeField] private float jumpSpeed = 400.0f;
    [SerializeField] private float grabLength = 1.1f;
    [SerializeField] private bool debug;

    private float move;
    private float shiftCollPad = 0.2f;
    private float shiftMaxDist = 2.0f;
    private float grabHeld = 0.0f;
    private float maxAngle = 45f;
    private float currentAngle = 0.0f;
    private Vector3 rotationVec = Vector3.zero;
    private Vector3 playerCollSize;
    private Vector3 grabbedPoint;
    private Vector3 slopeMove;
    private LayerMask playerMask;
    private LayerMask groundMask;
    private RaycastHit hitInfo;
    private RaycastHit shiftHit;
    private RaycastHit slopeHit;
    private bool grounded;
    private bool grabbing = false;
    private bool onSlope = false;
    private bool smoothShiftRunning = false;
    private bool lastFrameSlope = false;
    private bool lastFrameGrounded = false;


    public delegate void OnPause(Sacboy sacboy);
    public static event OnPause onPause;


    // Called when the script instance is being loaded.
    void Awake(){
        rb = GetComponent<Rigidbody>();
        sacboy = new Sacboy();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider>();
        playerMask = LayerMask.GetMask("Player") | LayerMask.GetMask("Checkpoint") | LayerMask.GetMask("ScoreBall") | LayerMask.GetMask("ScoreBallHitbox");
        groundMask = LayerMask.GetMask("Ground");
        grabJointScript = GetComponent<GrabJoint>();
    }

    // Called when the object becomes enabled and active.
    void OnEnable(){
        sacboy.Actions.Enable();
        sacboy.Actions.ShiftLeft.performed += ShiftLeft;
        sacboy.Actions.ShiftRight.performed += ShiftRight;
        sacboy.Actions.Jump.performed += Jump;
        sacboy.Actions.Jump.canceled += JumpRelease;
        sacboy.Actions.Grab.canceled += GrabRelease;
        sacboy.Actions.Pause.performed += Pause;
    }

    // Called when the object is destroyed or the behavior is disabled.
    void OnDisable(){
        sacboy.Actions.Disable();
        sacboy.Actions.ShiftLeft.performed -= ShiftLeft;
        sacboy.Actions.ShiftRight.performed -= ShiftRight;
        sacboy.Actions.Jump.performed -= Jump;
        sacboy.Actions.Jump.canceled -= JumpRelease;
        sacboy.Actions.Grab.canceled -= GrabRelease;
        sacboy.Actions.Pause.performed -= Pause;
    }
    // Called before the first frame update
    void Start()
    {
        rb.useGravity = true;
        playerCollSize = playerCollider.bounds.size;
        playerCollCenter = playerCollider.bounds.center;
    }

    /* 
    *   isGrounded - Checks to see if the player is grounded using a BoxCast and sets the isGrounded bool.
    */
    void isGrounded(){
        //Centered on the player collider and half the player collider size with a small height.
        if(Physics.BoxCast(playerCollCenter, new Vector3((playerCollSize.x/2)-0.01f, 0.1f, (playerCollSize.z/2)-0.01f), Vector3.down, transform.rotation, playerCollSize.y/2, ~playerMask)){
            grounded = true;
        }
        else{
            /*if(onSlope)
                grounded = true;
            else
                grounded = false;*/
            grounded = false;
        }
    }

    void OnDrawGizmos()
    {
        if (debug){
            playerCollider = GetComponent<BoxCollider>();
            playerCollSize = playerCollider.bounds.size;
            playerCollCenter = playerCollider.bounds.center;

            RaycastHit leftShiftHit;
            RaycastHit rightShiftHit;
            RaycastHit leftSlopeHit;
            RaycastHit rightSlopeHit;
    
            bool leftSlopeHitDetect = Physics.Raycast(new Vector3(playerCollCenter.x, (playerCollCenter.y - playerCollSize.y/2)+0.1f, playerCollCenter.z - playerCollSize.z/2), Vector3.down, out leftSlopeHit, 0.6f, ~playerMask);
            bool rightSlopeHitDetect = Physics.Raycast(new Vector3(playerCollCenter.x, (playerCollCenter.y - playerCollSize.y/2)+0.1f, playerCollCenter.z + playerCollSize.z/2), Vector3.down, out rightSlopeHit, 0.6f, ~playerMask);
            bool leftHitDetect = Physics.BoxCast(playerCollCenter, new Vector3(playerCollSize.x/2 + shiftCollPad, playerCollSize.y/2, playerCollSize.z/2 + shiftCollPad), Vector3.left, out leftShiftHit, transform.rotation, shiftMaxDist);
            bool rightHitDetect = Physics.BoxCast(playerCollCenter, new Vector3(playerCollSize.x/2 + shiftCollPad, playerCollSize.y/2, playerCollSize.z/2 + shiftCollPad), Vector3.right, out rightShiftHit, transform.rotation, shiftMaxDist);
            bool groundHitDetect = Physics.BoxCast(playerCollCenter, new Vector3((playerCollSize.x/2)-0.01f, 0.1f, (playerCollSize.z/2)-0.01f), Vector3.down, transform.rotation, playerCollSize.y/2, ~playerMask);
            bool grabRangeHitDetect = Physics.CheckSphere(new Vector3(playerCollCenter.x, playerCollCenter.y + 0.15f, playerCollCenter.z), grabLength, ~playerMask);
            
            // Check for slope hits
            Gizmos.color = Color.green;
            if(leftSlopeHitDetect){
                Gizmos.color = Color.red;
            }
            else{
                Gizmos.color = Color.cyan;
            }
            Gizmos.DrawRay(new Vector3(playerCollCenter.x, (playerCollCenter.y - playerCollSize.y/2)+0.1f, playerCollCenter.z - playerCollSize.z/2), Vector3.down*0.6f);
            if(rightSlopeHitDetect){
                Gizmos.color = Color.red;
            }
            else{
                Gizmos.color = Color.cyan;
            }
            Gizmos.DrawRay(new Vector3(playerCollCenter.x, (playerCollCenter.y - playerCollSize.y/2)+0.1f, playerCollCenter.z + playerCollSize.z/2), Vector3.down*0.6f);
            // Check if there has been a left shift hit yet
            if (leftHitDetect)
            {
                Gizmos.color = Color.red;
                // Draw a Ray forward from GameObject toward the hit
                Gizmos.DrawRay(playerCollCenter, Vector3.left * leftShiftHit.distance);
                // Draw a cube that extends to where the hit exists
                Gizmos.DrawWireCube(playerCollCenter + Vector3.left * leftShiftHit.distance, new Vector3(playerCollSize.x + shiftCollPad, playerCollSize.y, playerCollSize.z + shiftCollPad));
            }
            // If there hasn't been a hit yet, draw the ray at the maximum distance
            else
            {
                // Draw a Ray forward from GameObject toward the maximum distance
                Gizmos.color = Color.green;
                Gizmos.DrawRay(playerCollCenter, Vector3.left * 2.0f);
                // Draw a cube at the maximum distance
                Gizmos.DrawWireCube(playerCollCenter + Vector3.left * shiftMaxDist, new Vector3(playerCollSize.x + shiftCollPad, playerCollSize.y, playerCollSize.z + shiftCollPad));
            }

            // Check if there has been a right shifthit yet
            if (rightHitDetect)
            {
                Gizmos.color = Color.red;
                // Draw a Ray forward from GameObject toward the hit
                Gizmos.DrawRay(playerCollCenter, Vector3.right * rightShiftHit.distance);
                // Draw a cube that extends to where the hit exists
                Gizmos.DrawWireCube(playerCollCenter + Vector3.right * rightShiftHit.distance, new Vector3(playerCollSize.x + shiftCollPad, playerCollSize.y, playerCollSize.z + shiftCollPad));
            }
            // If there hasn't been a hit yet, draw the ray at the maximum distance
            else
            {
                // Draw a Ray forward from GameObject toward the maximum distance
                Gizmos.color = Color.green;
                Gizmos.DrawRay(playerCollCenter, Vector3.right * shiftMaxDist);
                // Draw a cube at the maximum distance
                Gizmos.DrawWireCube(playerCollCenter + Vector3.right * shiftMaxDist, new Vector3(playerCollSize.x + shiftCollPad, playerCollSize.y, playerCollSize.z + shiftCollPad));
            }
            // Check for grounding
            if(groundHitDetect)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(playerCollCenter.x, playerCollCenter.y - (playerCollSize.y/2), playerCollCenter.z)+(Vector3.down * 0.01f), new Vector3(playerCollSize.x-0.01f, 0.02f, playerCollSize.z-0.01f));
            // Check for objects in grab range
            if(grabRangeHitDetect)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(playerCollCenter.x, playerCollCenter.y + 0.15f, playerCollCenter.z), grabLength);

        }
    }

    /* 
    *   Pause - Pause performed action for Action Map. Invokes the onPause function if they player presses the pause button.
    */
    void Pause(InputAction.CallbackContext context){
            onPause?.Invoke(sacboy);
    }

    /* 
    *   Jump - Jump performed action for Action Map. Applies upward force to the rigidbody if player is grounded.
    */
    void Jump(InputAction.CallbackContext context){
        if(grounded){
            if(onSlope){
                if(currentAngle > maxAngle)
                    return;
            }
            if(rb.velocity.y != 0){
                float y = rb.velocity.y;
                y = -y;
                rb.AddForce(new Vector3(0, y, 0), ForceMode.VelocityChange);
            }
            rb.AddForce(jumpSpeed * Vector3.up);
        }
    }

    /* 
    *   JumpRelease - Jump canceled action for Action Map. Applies negative 1/3 of the rigidbody velocity to allow for variation in jump height.
    */
    void JumpRelease(InputAction.CallbackContext context){
        if(rb.velocity.y > 0){
            float currentUpVel = rb.velocity.y;
            currentUpVel = -(currentUpVel/3);
            rb.AddForce(new Vector3(0, currentUpVel, 0), ForceMode.VelocityChange);
        }
    }

    /* 
    *   Grab - Grab performed action for Action Map if the grab button is held down. Checks if grabbable collider is in range and grabs if it is.
    *   Note: Action map function not used so that tracking of the grab button being held can be done.
    */
    void Grab(){
        // Make sure no grab join has been created yet.
        if (gameObject.GetComponent<SpringJoint>() == null){
            grabbedObject = null;
            // Check for any colliders in player grab range.
            Collider [] hitColliders = Physics.OverlapSphere(new Vector3(playerCollCenter.x, playerCollCenter.y + 0.15f, playerCollCenter.z), grabLength, ~playerMask);
            int i = 0;
            // Loop through the collider list and find if any of them are grabbable. If they are set necessary variables for grabbing,
            while (i < hitColliders.Length){
                if(hitColliders[i].tag == "Grabbable" || hitColliders[i].tag == "Moveable"){
                    grabbedObject = hitColliders[i].gameObject;
                    closestHit = hitColliders[i];
                }
                // If there is a grabbable object get the closest point to the grab range and make a grab joint with it.
                if(grabbedObject != null){
                    grabbedPoint = hitColliders[i].ClosestPoint(new Vector3(playerCollCenter.x, playerCollCenter.y + 0.15f, playerCollCenter.z));
                    grabJointScript.Create(grabbedPoint, grabbedObject);
                    // If an object is moveable rotate the player so that they face the object.
                    if(grabbedObject.tag == "Moveable") 
                        if(grabbedObject.transform.position.z > rb.transform.position.z)
                            rotationVec = Vector3.forward;
                        else
                            rotationVec = Vector3.back;
                    // Set grabbing bool
                    grabbing = true;
                }
                i++;
            }
        }
    }

    /* 
    *   GrabRelease - Grab canceled action for Action Map if the grab button is released. Destroys grab joint and sets necessary variables.
    */
    void GrabRelease(InputAction.CallbackContext context) {
        grabJointScript.Destroy();
        grabbing = false;
        grabbedObject = null;
    }

    /* 
    *   ShiftLeft - ShiftLeft performed action for Action Map. Allows the player to shift left (away from the screen).
    */
    void ShiftLeft(InputAction.CallbackContext context) {
        // Only allow shifting if player is grounded, not grabbing, and not in the last lane already.
        if(!grabbing && grounded)
            if(((Mathf.Round(transform.position.x - 2.0f)) >= -2.0f))
                // BoxCast left using the playe colliders size and position to make sure no object is blocking the shift.
                if (!Physics.BoxCast(playerCollCenter, new Vector3(playerCollSize.x/2 + shiftCollPad, playerCollSize.y/2, playerCollSize.z/2 + shiftCollPad), Vector3.left, out shiftHit, transform.rotation, shiftMaxDist)){
                    StartCoroutine(SmoothShift(transform.position.x - 2.0f));
                }
    }

    /* 
    *   ShiftRight - ShiftRight performed action for Action Map. Allows the player to shift right (towards the screen).
    */
    void ShiftRight(InputAction.CallbackContext context) {
        // Only allow shifting if player is grounde, not grabbing, and not in the first lane already.
        if(!grabbing && grounded)
            if(((Mathf.Round(transform.position.x + 2.0f)) <= 2.0f))
                // BoxCast right using the player colliders size and position to make sure no object is blocking the shift.
                if (!Physics.BoxCast(playerCollCenter, new Vector3(playerCollSize.x/2 + shiftCollPad, playerCollSize.y/2, playerCollSize.z/2 + shiftCollPad), Vector3.right, out shiftHit, transform.rotation, shiftMaxDist)){
                    StartCoroutine(SmoothShift(transform.position.x + 2.0f));
                }
    }

    /* 
    *   SmoothShift - Coroutine to shift the player smoothly to the target X direction.
    *   @param targetX - float x value to move the players transform to.
    */
    IEnumerator SmoothShift(float targetX){
        // Only shift if not currently shifting.
        smoothShiftRunning = true;
        Vector3 startPos = transform.position;
        float progress = 0.0f;
        // Continue shifting until target position reached.
        while(transform.position.x != targetX){
            // Update animation progress.
            progress = Mathf.Clamp01(progress + (30f * Time.deltaTime));
            // Set target y and z position based off the players velocity.
            Vector3 targetPos = new Vector3(targetX, transform.position.y + (rb.velocity.y * Time.deltaTime), transform.position.z + (rb.velocity.z * Time.deltaTime));
            // Set target x based off animation progress. 
            targetPos.x = Mathf.Lerp(startPos.x, targetPos.x, progress);
            transform.position = targetPos;
            yield return null;
        }
        smoothShiftRunning = false;
        yield break;
    }

    /* 
    *   HandleAnimation - Sets the animation bools bases on grounding and velocity.
    */
    void HandleAnimation(){
        if(grounded)
            animator.SetBool("isJumping", false);
        if(Mathf.Abs(rb.velocity.z) > 0.01f && transform.eulerAngles.y != 90f)
            animator.SetBool("isRunning", true);
        else
            animator.SetBool("isRunning", false);
    }

    /* 
    *   SetGravity - Sets the gravity to on or off based off if the player is on a valid slope or not.
    */
    private void SetGravity(){
        // Only use gravity on the player if not on a slope or the slope they are on is less than the max angle allowed for a slope.
        if(onSlope && grounded){
            if(currentAngle <= maxAngle)
                rb.useGravity = false;
            else
                rb.useGravity = true;
        }
        else
            rb.useGravity = true;
    }

    /*
    * SetMovementValues - Set the players max velocity and speed value based on whether or not they are grabbing in the air.
    */
    private void SetMovementValues(){
        if(!grounded && grabbing){
            maxVelocity = 3.0f;
            speed = 10.0f;
        }
        else{
            maxVelocity = 7.0f;
            speed = 50.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {   
        // Get player collider center and size.
        playerCollCenter = playerCollider.bounds.center;
        playerCollSize = playerCollider.bounds.size;
        // Only use gravity on the player if not on a slope or the slope they are on is less than the max angle allowed for a slope.
        SetGravity();
        // Set the players rotation based on the input direction and slope angle.
        LookRotation();
        // Set animation bools
        HandleAnimation();
        // Set player speed and max velocity.
        SetMovementValues();
        // Get value of left stick x-axis position.
        move = sacboy.Actions.Move.ReadValue<float>();
        // Check if the grab button is held, if it is perform the grab function.
        grabHeld = sacboy.Actions.Grab.ReadValue<float>();
        if(grabHeld == 1 && !grabbing)
            Grab();
        // Get the movement value for the player for when they are on a slope.
        slopeMove = Vector3.ProjectOnPlane(new Vector3(0, 0, move), slopeHit.normal).normalized;
        // Check for ground in any lane while player is falling and place them where there is some.
        AutoSwitchLanes();
    }

    // Called every fixed frame rate frame, has the same frequency of the physics system.
    void FixedUpdate(){
        // Set the onSlope and grounded variables for the frame.
        OnSlope();
        isGrounded();
        // If a slope transition happened, adjust for it.
        // Only want to adjust for slope change if we are transiting from the ground.
        if(lastFrameSlope != onSlope && lastFrameGrounded)
            SlopeAdjust();
        // Move or stop the player based on the movement input given.
        HandleMovementInput();
        // Slow down the player if the are grounded and over the max speeds.
        SlowPlayer();
        // Set last frame variables.

        lastFrameSlope = onSlope;
        lastFrameGrounded = grounded;
    }

    /* 
    *   HandleMovementInput - Moves or stops the player based on the current movement input.
    */
    private void HandleMovementInput(){
        // Move player when receiving input.
        if(move != 0.0f){
            // If player hasn't exceeded max velocity or the movement direction from the last input is different than the current velocity.
            if((Mathf.Abs(rb.velocity.z) < maxVelocity || Mathf.Sign(move) != Mathf.Sign(rb.velocity.z))){
                // Apply slope movement force if on a slope.
                if(onSlope){
                    // Only apply force if on a slope angled less than the max angle and below max velocity.
                    if(currentAngle <= maxAngle){
                        if(rb.velocity.magnitude < maxVelocity)
                            rb.AddForce(slopeMove * speed);
                    }
                    else{
                        // Allow player to move off a disallowed slope instead of only sliding down.
                        if(SlopeAngle(slopeHit.normal, transform.forward) < 90f)
                            rb.AddForce(new Vector3(0, 0, (move * speed)));
                        else{
                            // Soften the players landing and falling on an disallowed slope.
                            if(rb.velocity.y>0f)
                                rb.AddForce(new Vector3(0, -(rb.velocity.y)*0.2f, 0), ForceMode.VelocityChange);
                        }
                    }
                }
                // Apply non-slope movement
                else
                    rb.AddForce(new Vector3(0, 0, (move * speed)));
            }
        }
        // Stop player when receiving no input.
        else{
            // If grabbing and not grounded then slow the player down. This logic allows the player to still swing from grab joint with no movement input.
            if(!(grabbing && !grounded)){
                float currentz = -(rb.velocity.z);
                // Handle stopping the player when on a slope and grounded
                if(onSlope){
                    // If on an allowed slope stop the player by adding the opposite of its current velocity.
                    if(currentAngle <= maxAngle && grounded)
                        rb.AddForce(new Vector3(0, -(rb.velocity.y), currentz), ForceMode.VelocityChange);
                }
                else
                    rb.AddForce(new Vector3(0, 0, currentz), ForceMode.VelocityChange);
            }
        }
    }

    /* 
    *   SlowPlayer - Slows the played down if they are grounded and over the max velocity.
    */
    private void SlowPlayer(){
        // Slow down the player if the are grounded and over the max speeds.
        if(grounded){
            if(Mathf.Abs(rb.velocity.z) > maxVelocity && !onSlope){
                // Apply the half of the opposite sign of the players current z velocity
                float diff = Mathf.Abs(rb.velocity.z) * 0.5f;
                if(rb.velocity.z > 0)
                    diff = -diff;
                rb.AddForce(new Vector3(0, 0, diff));
            }
        }
    }

    /* 
    *   SlopeAdjust - Adjusts the players velocity to be projected onto the plane/ground they are moving along when they
    *   transition from a slope to a non-slope or vice versa. This is to prevent the player from losing speed when transitioning as well as 
    *   lifting off of the ground. The function keeps them grounded when transitioning since the player is a rigid body.
    */
    private void SlopeAdjust(){
        // Set default normal to slope -> non-slope scenario.
        Vector3 myNormal = Vector3.up;
        // If transition is non-slope -> slope set the new normal.
        if(onSlope)
            myNormal = slopeHit.normal;
        Vector3 velocity = rb.velocity;
        // Project the players velocity onto the new normal to allow the players velocity to be kept when transitioning.
        slopeMove = Vector3.ProjectOnPlane(new Vector3(0, velocity.y, velocity.z), myNormal).normalized;
        float currentz = -(rb.velocity.z);
        float currenty = -(rb.velocity.y);
        // Remove the players current velocity and apply the new changed velocity.
        rb.AddForce(new Vector3(0, currenty, currentz), ForceMode.VelocityChange);
        rb.AddForce(slopeMove*velocity.magnitude, ForceMode.VelocityChange);
    }

    /* 
    *   SlopeAngle - Finds the angle between two Vector3's then rounds it and returns it.
    *   @param fromAngle - the vector from which the difference is measured
    *   @param toAngle - the vector to which the difference is measured
    *   @return float - a float representing the angle between two vectors.
    */
    private float SlopeAngle(Vector3 fromAngle, Vector3 toAngle){
        float angle = Vector3.Angle(fromAngle, toAngle);
        angle = Mathf.Round(angle);
        return angle;
    }

    /* 
    *   OnSlope - Checks if the player is currently one a slope or not and sets the onSlope bool using a raycast at both z-sides of the players collider.
    *   The function is setup to allow for smooth player transition between a slope and non-slope, meaning they stay grounded and maintain their speed.
    */
    private void OnSlope(){
        // Set the rays starting Z position based on the direction of the players forward vector.
        float zOrigin = playerCollCenter.z;
        float diff = playerCollSize.z/2;
        if(move < 0)
            diff = -diff;
        RaycastHit hit;
        // Check if there is ground at the end of the player models in its forward direction.
        if(Physics.Raycast(new Vector3(playerCollCenter.x, (playerCollCenter.y - playerCollSize.y/2)+0.1f, zOrigin + diff), Vector3.down, out hit, 0.6f, ~playerMask)){
            // If the ground hit is a slope set the necessary variables.
            if(hit.normal != Vector3.up){
                currentAngle  = SlopeAngle(hit.normal, Vector3.up);
                slopeHit = hit;
                onSlope = true;
                return;
            }
            // If the ground hit is not a slope check the backwards direction to see if the player is going down a slope.
            // This is necessary for when the player is at the end of a downward slope.
            else{
                if(Physics.Raycast(new Vector3(playerCollCenter.x, (playerCollCenter.y - playerCollSize.y/2)+0.1f, zOrigin - diff), Vector3.down, out hit, 0.6f, ~playerMask)){
                    // Check if the ground hit has a normal that is not 0 and less than 90 with respect to the players forward, this means they are going down a slope.
                    float angle = SlopeAngle(hit.normal, transform.forward);
                    if(hit.normal != Vector3.up && angle < 90f){
                        currentAngle  = SlopeAngle(hit.normal, Vector3.up);
                        onSlope = true;
                        slopeHit = hit;
                        return;
                    }
                }
            }
        }
        // Check if there is ground at the end of the player models backwards direction
        if(Physics.Raycast(new Vector3(playerCollCenter.x, (playerCollCenter.y - playerCollSize.y/2)+0.1f, zOrigin - diff), Vector3.down, out hit, 0.6f, ~playerMask)){
            float angle = SlopeAngle(hit.normal, transform.forward);
            // If the player is on a slope and the angle is less than 90 then they are going down a slope. 
            // The angle being 90 while the player is not moving is to account for being at the end of a downhill slope where the player is touching both the slope and the ground.
            if(hit.normal != Vector3.up && (angle < 90f || angle == 90 && move == 0)){
                currentAngle  = SlopeAngle(hit.normal, Vector3.up);
                slopeHit = hit;
                onSlope = true;
                return;
            }
            else{
                currentAngle = 0.0f;
                onSlope = false;
                return;
            }
        }
        currentAngle = 0.0f;
        onSlope = false;
    }


    /* 
    *   SlopeRotation - Rotates the player so that they are always parallel with the ground they are standing on.
    */
    private void SlopeRotation(){
        if(move == 0.0f){
            RaycastHit rotationHit;
            Vector3 rotationNorm;
            Quaternion fromRotation = transform.rotation;
            // Sets the direction of the rotation using the normal of the ground below the player.
            if(Physics.Raycast(playerCollCenter, Vector3.down, out rotationHit, (playerCollSize.y/2 + 0.5f), ~playerMask))
                rotationNorm = rotationHit.normal;
            else
                rotationNorm = Vector3.up;

            // If the player isn't already rotated to the normal of the ground, rotate them.
            if(transform.up != rotationNorm)
                transform.rotation = Quaternion.FromToRotation(transform.up, rotationNorm) * transform.rotation;
        }
    }

    /* 
    *   LookRotation - Rotates the player so they always face their input/velocity direction. If velocity is 0 they face the camera.
    */
    private void LookRotation(){
        // Only set direction based on velocity if they player is not grabbing.
        if(!grabbing){
            // Look left.
            if(move > 0){
                if(transform.eulerAngles.y != 0){
                    transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                }
            }
            // Look right.
            else if(move < 0){
                if(transform.eulerAngles.y != 180){
                    transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                }
            }
            // Face the camera if grounded and not moving.
            else{
                if(transform.eulerAngles.y != 90 && grounded){
                    transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                }
            }
        }
        // If the player is grounded and grabbing, face them towards the object they are grabbing.
        if(grabbing && grounded){
            if(grabbedObject != null){
                if(grabbedObject.transform.position.z > transform.position.z){
                    transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                }
                else{
                    transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                }
            }
        }
    }

    /*
    *   AutoSwitchLanes - Checks for ground below the player in any lane when they have a negative y velocity and moves them if found. This is done so that when the player is airborne
    *   they are automatically moved to a lane they can land on which makes for a smoother gameplay movement.
    */
    private void AutoSwitchLanes(){ 
        // Only want to auto switchif they are not grounded, falling, not grabbing.
        if(!grounded && rb.velocity.y <= 0.0f && !grabbing){
            // Set up a list to store any ground hits.
            List<KeyValuePair<RaycastHit, float>> groundHits = new List<KeyValuePair<RaycastHit, float>>();
            // Current lane raycast hit variable.
            RaycastHit curPosGroundHitInfo;
            // Check for any ground below the players current lane, add it to hit ground.
            if (ShiftRayCast(0.0f, out curPosGroundHitInfo))
                groundHits.Add(new KeyValuePair<RaycastHit, float>(curPosGroundHitInfo, 0.0f));
            // Check for ground in the lanes to the right of the player, add it to hit ground.
            if(playerCollCenter.x != 2.0f)
                groundHits = CheckForGround(groundHits, 2.0f, 4.0f);
            // Check for ground in the lanes to the left of the player, add it to hit ground.
            if(playerCollCenter.x != -2.0f)
                groundHits = CheckForGround(groundHits, -2.0f, -4.0f);
            // Calculate the highest then closest ground to the player in the hit ground list.
            int index = CalcShiftPosition(groundHits);
            // If ground that can be moved to is found that isn't in the current lane, move the player.
            if(groundHits.Count != 0 && groundHits[index].Value != 0.0f){
                if(!smoothShiftRunning)
                    StartCoroutine(SmoothShift(transform.position.x + (groundHits[index].Value)));
            }
        }
    }

    /*
    *   CheckForGround - Checks for ground in two lanes of the same direction with given x position, one being closer and the other farther. 
    *   Neither lanes are the players current lane.
    *   @param groundHits - KeyValuePair list that holds the RaycastHit info of hit ground and a float of the x distance from the player it is.
    *   @param close - float of the closest lane to be checked.
    *   @param far - float of the farthest lane to be checked.
    *   @return groundHits - KeyValuePair list of any ground hits the player is eligible to be shifted to.
    */
    private List<KeyValuePair<RaycastHit, float>> CheckForGround(List<KeyValuePair<RaycastHit, float>> groundHits, float close, float far){
        RaycastHit closeInfo;
        RaycastHit farInfo;
        Vector3 direction;
        // Determine direction of BoxCast based on the closest lanes position to the player.
        if(close > 0.0f)
            direction = Vector3.right;
        else
            direction = Vector3.left;
        // BoxCast to check close lane can be shifted into.
        if(!ShiftBoxCast(direction, shiftMaxDist)){
            // If there is ground below the player add its info to the eligible ground.
            if(ShiftRayCast(close, out closeInfo)){
                groundHits.Add(new KeyValuePair<RaycastHit, float>(closeInfo, close));
            }
            // If there is ground below the player in the far lane. Only needs to be checked if there is no object in the close lane blocking the shift.
            if(ShiftRayCast(far, out farInfo)){
                // BoxCast to check if the far lane can be shifted into.
                if(!ShiftBoxCast(direction, shiftMaxDist + 2.0f)){
                    groundHits.Add(new KeyValuePair<RaycastHit, float>(farInfo, far));
                }
            }
        }
        // Return possible shifts.
        return groundHits;
    }

    /*
    *   CalcShiftPosition - Calculates the highest ground given a list of RaycastHits on ground underneath the player. If there are multiple RaycastHits
    *   at the same height, the one closest to the player will be chosen. If there are multiple at the same height and distance then the closest to the camera is chosen.
    *   @param groundHits - KeyValuePair list of RaycastHits and floats of eligible ground and their distance from the player.
    *   @return index - int value that represent the index of the ground to be moved to in the list.
    */
    private int CalcShiftPosition(List<KeyValuePair<RaycastHit, float>> groundHits){
        int index = 0;
        // If there is more than one eligible lane to move to, find the highest one. Closest one if a tie.
        if(groundHits.Count > 1){
            for(int i = 1; i < groundHits.Count; i++){
                // Check if the current iteration grounds height is higher than the current index's.
                 if(groundHits[i].Key.point.y > groundHits[index].Key.point.y)
                    index = i;
                // Check if the current iteration grounds height is equal to the current index's.
                else if(groundHits[i].Key.point.y == groundHits[index].Key.point.y){
                    // Check which ground is closer to the player, set that ground as the index.
                    if(Mathf.Abs(playerCollCenter.x - groundHits[i].Value) < Mathf.Abs(playerCollCenter.x - groundHits[index].Value)){
                        index = i;
                    }
                }
            }
        }
        return index;
    }

    /*
    *   ShiftBoxCast - Checks to see if there is any object block the player from shifting to the direction and distance given.
    *   @param dir - Vector3 of the direction to cast the box in.
    *   @param maxShift - float of the maximum distance to cast the box.
    *   @return bool - bool value of wether or not the BoxCast collided with anything.
    */
    private bool ShiftBoxCast(Vector3 dir, float maxShift){
        return Physics.BoxCast(playerCollCenter, new Vector3(playerCollSize.x/2 + shiftCollPad, playerCollSize.y/2, playerCollSize.z/2 + shiftCollPad), 
        dir, transform.rotation, maxShift);
    }

    /*
    *   ShiftRayCast - Checks to see if there is any ground the player can stand on in the lane given.
    *   @param centerOffset - float of the distance from the center lane to check for ground.
    *   @param groundHitInfo - RaycastHit variable for the function to return the Raycast info to.
    *   @return bool - bool value of wether or not the Raycast hit any ground.
    */
    private bool ShiftRayCast(float centerOffset, out RaycastHit groundHitInfo){
        return Physics.Raycast(new Vector3(playerCollCenter.x + centerOffset, playerCollCenter.y, playerCollCenter.z), Vector3.down, 
        out groundHitInfo, (playerCollSize.y/2 + 0.5f), groundMask);
    }
}