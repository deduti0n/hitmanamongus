using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public interactionActor targetInteraction;

        public bool char_duringTask = false;

        public Menu_UI Game_UI_Ref;

        [Header("Tasks")]
        public List<Character_Task> AvailableTasks = new List<Character_Task>();

        [Header("Networking")]
        public CharacterOwner Owner = CharacterOwner.Other;
        public bool Character_isSuspect = false; // Goal is to kill everyone
        public bool Character_isDead = false; // Kept dead in place -- can see the perspective of others

        public string PlayerName = "";
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

        public bool isDebug = false;

        protected override void onStart()
        {
            char_controller = GetComponent<CharacterController>();
            char_animcontroller = GetComponent<Animator>();
            char_camera = transform.Find("Camera").GetComponent<Camera>();

            //Save every spawned avatar in this list
            GameManager.Instance.networkManager.Network_AddPlayerAvatars(this);

            //Debugging mode
            if (isDebug)
            {
                GameManager.Instance.networkManager.Network_PlayerRef = this;

                StartCoroutine(DelayVoteTest());
            }

            char_StartPosition = transform.position;

            Player_Refresh();
        }

        IEnumerator DelayVoteTest()
        {
            yield return new WaitForSeconds(1f);
            List<string> players = new List<string>();
            players.Add("TESTE");
            Game_UI_Ref.Menu_OpenVotingMenu(players);
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
            PlayerName = name;
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
            if (Owner == CharacterOwner.Mine && !Character_isDead && !Character_Lock && !char_duringTask)
            {
                //Update locally
                Update_Locomotion();
                Update_Direction();

                Update_Interactions();
            }
        }

        public void Character_StartTask()
        {
            StartCoroutine(startTask());
        }

        IEnumerator startTask()
        {
            char_duringTask = true;
            yield return new WaitForSeconds(4f);
            char_duringTask = false;
        }

        public void Character_Kill()
        {
            Character_isDead = true;
            char_animcontroller.SetTrigger("Death");
        }

        public void UpdateTasks()
        {
            //Update Task list and states
            string list = "";

            foreach(Character_Task task in AvailableTasks)
            {
                list += "#Complete '" + task.Task_interaction.InteractionName + "' " + (task.Task_State? "(Completed!)\n": "\n");
            }

            Game_UI_Ref.UI_UpdateTaskList(list);
        }

        public void Update_Interactions()
        {
            //Get nearby interactions
            List<baseActor> interactions = GameManager.Instance.actorManager.GetNearbyActorsByType(ActorTypes.Interaction, transform.position, 3f);
            List<interactionActor> interactionsUp = new List<interactionActor>();

            //Filter out disabled interactions and upcasting
            foreach (interactionActor interaction in interactions)
            {
                if (interaction.isInteractible)
                    interactionsUp.Add(interaction);
            }

            //Order list by distance
            interactionsUp = interactionsUp.OrderBy(
            x => Vector3.Distance(this.transform.position, x.transform.position)
            ).ToList();

            //Get the item closest to the camera center
            targetInteraction = Camera_GetClosestInteractionToCamera(interactionsUp);

            //Execute
            if (Input.GetButtonDown("Interaction"))
            {
                targetInteraction.Execute();
                targetInteraction = null;
            }
        }

        private interactionActor Camera_GetClosestInteractionToCamera(List<interactionActor> interactionsUp)
        {
            interactionActor closest = null;
            float dot = -2;

            foreach (interactionActor interaction in interactionsUp.ToArray())
            {
                Vector3 localPoint = char_camera.transform.InverseTransformPoint(interaction.transform.position).normalized;
                Vector3 forward = Vector3.forward;
                float test = Vector3.Dot(localPoint, forward);

                if (test > dot)
                {
                    dot = test;
                    closest = interaction;
                }
            }

            return closest;
        }

        public void SetTaskCompleted(interactionActor terminal)
        {
            foreach(Character_Task task in AvailableTasks)
            {
                if (task.Task_interaction == terminal)
                    task.Task_State = true;
            }

            //Complete task and update UI
            UpdateTasks();
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