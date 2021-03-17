using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IPCA.Characters
{
    public enum CharacterOwner
    {
        Mine, // Controlled locally
        Other // Controlled remotely
    }

    public class Character_PlayerController : baseActor
    {
        //References
        private Camera char_camera;
        private CharacterController char_controller;
        private Animator char_animcontroller;

        private Vector3 char_StartPosition;

        [Header("Networking")]
        public CharacterOwner Owner = CharacterOwner.Other;
        public bool Character_isSuspect = false; // Goal is to kill everyone
        public bool Character_isDead = false; // Kept dead in place -- can see the perspective of others

        public TextMeshPro UI_PlayerName;
        public SkinnedMeshRenderer Character_Mesh;

        [Header("Player Settings")]
        public float Character_RunSpeed = 6f;

        private float char_TargetXDirection = 1f;
        private float char_TargetYDirection = 1f;
        private float char_TurnSpeed = 360f;

        private float char_vspeed = 0;
        public bool Character_Lock = false;

        [Header("Camera Settings")]
        public float Game_LookSensivity = 1f;
        public bool Camera_LookInterpolation = true;
        public Vector2 char_YLimits = new Vector2(90, -90);
        private float char_YSpeed = 360f;

        protected override void onStart()
        {
            char_controller = GetComponent<CharacterController>();
            char_animcontroller = GetComponent<Animator>();
            char_camera = transform.Find("Camera").GetComponent<Camera>();

            //Debugging mode
            if (Owner == CharacterOwner.Mine)
                GameManager.Instance.networkManager.Network_PlayerRef = this;

            char_StartPosition = transform.position;

            Player_Refresh();
        }

        public void Player_Refresh()
        {
            if (Owner == CharacterOwner.Mine)
            {
                Cursor.lockState = CursorLockMode.Locked;

                //Hide mesh
                transform.Find("Body01").gameObject.SetActive(false);
                transform.Find("Head01").gameObject.SetActive(false);
                transform.Find("PlayerInfo").gameObject.SetActive(false);

                char_camera.gameObject.SetActive(true);
                Character_SetName(GameManager.Instance.PlayerName);
            }
            else
            {
                //Disable camera on other players
                char_camera.gameObject.SetActive(false);

                transform.Find("Body01").gameObject.SetActive(true);
                transform.Find("Head01").gameObject.SetActive(true);
                transform.Find("PlayerInfo").gameObject.SetActive(true);
            }
        }

        public void Character_SetName(string name)
        {
            UI_PlayerName.text = name;
        }

        public void Character_SetColor(Vector3 colll)
        {
            //Set my color

            Color col = new Color(colll.x, colll.y, colll.z);
            Character_Mesh.materials[0].SetColor("_EmissionColor", col);
        }

        public void Character_SetColor(Color col)
        {
            Character_Mesh.materials[0].SetColor("_EmissionColor", col);
        }

        public Vector3 Character_GetColor()
        {
            //Get my color
            Color col = Character_Mesh.materials[0].GetColor("_EmissionColor");

            Vector3 Coll = new Vector3(col.r, col.g, col.b);

            return Coll;
        }

        public override void onUpdate()
        {
            if (Owner == CharacterOwner.Mine && !Character_Lock)
            {
                //Update locally
                Update_Locomotion();
                Update_Direction();
            }
        }

        public override void onFixedUpdate()
        {

        }

        public override void onLateUpdate()
        {

        }

        protected override void onDestroy()
        {

        }

        public void Character_Kill()
        {
            Character_isDead = true;

            //Play death animation

        }

        public void TeleportToSpawn()
        {
            transform.position = char_StartPosition;
        }

        private void Update_Locomotion()
        {
            //Direction
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            char_animcontroller.SetFloat("InputX", Mathf.Lerp(char_animcontroller.GetFloat("InputX"), move.z, 5f * Time.deltaTime));
            char_animcontroller.SetFloat("InputY", Mathf.Lerp(char_animcontroller.GetFloat("InputY"), move.x, 5f * Time.deltaTime));

            float velocityNormalizer = (move.x != 0f && move.z != 0f) ? .7071f : 1f;
            Vector3 inputWorldSpace = transform.TransformDirection(move);

            char_vspeed -= 9.8f * Time.deltaTime;

            Vector3 moveDir = inputWorldSpace * Character_RunSpeed * velocityNormalizer;

            moveDir.y = char_vspeed;

            char_controller.Move(moveDir * Time.deltaTime);

            if (char_controller.isGrounded) char_vspeed = 0f;
        }

        private void Update_Direction()
        {
            float InterpolationSpeed = 20f;

            char_TargetXDirection += Input.GetAxis("Mouse X") * char_TurnSpeed * Game_LookSensivity * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(transform.eulerAngles.y, char_TargetXDirection, InterpolationSpeed * Time.deltaTime), 0);

            char_TargetYDirection -= Input.GetAxis("Mouse Y") * char_YSpeed * Game_LookSensivity * Time.deltaTime;

            //Camera Y Limits
            if (char_TargetYDirection < char_YLimits.y)
                char_TargetYDirection = char_YLimits.y;

            if (char_TargetYDirection > char_YLimits.x)
                char_TargetYDirection = char_YLimits.x;

            char_camera.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(char_camera.transform.localEulerAngles.x, char_TargetYDirection, InterpolationSpeed * Time.deltaTime), char_camera.transform.localEulerAngles.y, char_camera.transform.localEulerAngles.z);
        }
    }
}