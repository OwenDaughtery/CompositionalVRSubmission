using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class ViveInput : MonoBehaviour
{
    [SerializeField]
    private InteractionManager rightInteractionManager = null;
    [SerializeField]
    private InteractionManager leftInteractionManager = null;
    float rightTimePressed = 0f;
    float leftTimePressed = 0f;


    void Update(){
        //Trigger down
        if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.RightHand)) {
            rightInteractionManager.pickUp();
        }

        if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.LeftHand)) {
            leftInteractionManager.pickUp();
        }

        //Trigger up
        if (SteamVR_Actions.default_GrabPinch.GetStateUp(SteamVR_Input_Sources.RightHand)) {
            rightInteractionManager.letGo();
        }

        if (SteamVR_Actions.default_GrabPinch.GetStateUp(SteamVR_Input_Sources.LeftHand))
        {
            leftInteractionManager.letGo();
        }


        //grip down
        if (SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.RightHand)) {
            rightInteractionManager.approveMarkovPair();
        }

        if (SteamVR_Actions.default_GrabGrip.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            leftInteractionManager.approveMarkovPair();
        }

        //grip up
        if (SteamVR_Actions.default_GrabGrip.GetStateUp(SteamVR_Input_Sources.RightHand)) {
            rightTimePressed = Time.time - rightTimePressed;
            if (rightTimePressed <= 0.5f){
                //Functionality not required.
            }
            else
            {
                //Functionality not required.
            }
        }

        if (SteamVR_Actions.default_GrabGrip.GetStateUp(SteamVR_Input_Sources.LeftHand))
        {
            leftTimePressed = Time.time - leftTimePressed;
            if (leftTimePressed <= 0.5f)
            {
                //Functionality not required.
            }
            else
            {
                //Functionality not required.
            }
        }

        //touch pad down
        if (SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.RightHand)) {
            Vector2 touchpadValue = (SteamVR_Actions.default_TouchpadTouch).GetAxis(SteamVR_Input_Sources.RightHand);
            if (touchpadValue.y > 0.6f)
            {
                rightInteractionManager.addNewVertex();
            }
            else if (touchpadValue.y < -0.6f)
            {
                rightInteractionManager.removeVertex();
            }

            if (touchpadValue.x > 0.6f)
            {
                rightInteractionManager.moveVertexUp();
            }
            else if (touchpadValue.x < -0.6f)
            {
                rightInteractionManager.moveVertexDown();
            }
        }

        if (SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            Vector2 touchpadValue = (SteamVR_Actions.default_TouchpadTouch).GetAxis(SteamVR_Input_Sources.LeftHand);
            if (touchpadValue.y > 0.6f)
            {
                leftInteractionManager.addNewVertex();
            }
            else if (touchpadValue.y < -0.6f)
            {
                leftInteractionManager.removeVertex();
            }

            if (touchpadValue.x > 0.6f)
            {
                leftInteractionManager.moveVertexUp();
            }
            else if (touchpadValue.x < -0.6f)
            {
                leftInteractionManager.removeVertex();
            }
        }

    }
}
