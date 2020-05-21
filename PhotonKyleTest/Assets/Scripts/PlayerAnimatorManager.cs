using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Com.MyCompany.MyGame
{
    public class PlayerAnimatorManager : MonoBehaviourPun//Bu class(MonoBehaviourPun) PhotonView componentini getComponent istifade etmeden
        //Scriptde istifade etmeyi temin edir.
    {
        #region Private variables
        /// <summary>
        /// Bu deyisen oyuncunun smooth ve realistic sekilde donmesini temin edir.
        /// </summary>
        [SerializeField] private float directionDampTime = 0.25f;
        private Animator animator;
        #endregion
        #region MonoBehaviour Callbacks
        void Start()    
        {
            animator = GetComponent<Animator>();
            //Eger oyuncunun animator komponenti yoxdursa
            if (!animator)
            {
                Debug.LogError("PlayerAnimator manager is missing Animator Component",this);
            }
        }

        void Update()
        {
            //PhotonView.isMine- hal-hazirda oyunda olan oyunculardan hansinin menim oldugunu deqiqlesdirmek ucun istifade edilir.
            //Eger false dursa bu oyuncu basqa computer terefinden idare edilir.
            //isConnected=i yoxladiq cunki offline modda oyuncunu modify etmek ve tenzimlemek ucun istifade ede bilek.
            if(photonView.IsMine==false && PhotonNetwork.IsConnected == true)
            {
                return;
            }
            if (!animator)
            {
                return;
            }
            //Bu kod setri hazirki animasiyanin melumatini alir.
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            //Bu if serti ile yalniz qacaraq tullanmani temin edirik.
            if(stateInfo.IsName("Base Layer.Run"))//Hazirki aktiv state-in adinin Run olub-olmadigini yoxlayir.Base Layer yazdig cunku animatorda Run base layerdedi.
            {
                //eger sag mouse duymesine ve ya alt duymesine basilibsa
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetTrigger("Jump");
                }
            }
            float HorizontalInput = Input.GetAxis("Horizontal");
            float VerticalInput = Input.GetAxis("Vertical");
            //Oyuncu arxa buttona basan zaman(z ve ya asagi ox) bu zaman vertical-in qiymeti 0 ve -1 arasi olur.
            //Bunun olmamasi ucun(cunki oyuncu yalniz ireli gedir) Vertical 0dan kicik olduqda onu 0-a beraberlesdiririk.
            if (VerticalInput < 0)
            {
                VerticalInput = 0;
            }
            //Easing-yumsaltma.Oyuncunun hereketlerini ani deyil realistic sekilde olmasi ucun onlari bir birine vururuq ve easing tetbiq edirik(easing functions etrafli melumat)
            //Bunun evezine Mathf.Abs() de istifade etmek olar.
            animator.SetFloat("Speed", HorizontalInput * HorizontalInput + VerticalInput * VerticalInput);
            //Horizontal uzre donduymuze gore (sag ve sol) HorizontalInputu istifade etdik.
            //Time.DeltaTime ise oyuncunun donmesini FPS(Frame per Seconddan) qeyri-asili edir.
            animator.SetFloat("Direction", HorizontalInput, directionDampTime, Time.deltaTime);
        }
        #endregion
    }
}
