using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;



namespace Com.MyCompany.MyGame
{
    public class GameManagerScript : MonoBehaviourPunCallbacks
    {

        #region Public Fields
        //Singletonu tetbiq etmek ucun static istifade olunur.
        public static GameManagerScript GameManagerInstance;

        //Bu deyisen oyuncu yaratmaq ucun prefabi temsil edir.
        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        
        #endregion


        #region Monobehaviour Callbacks
        private void Start()
        {
            //singletonu burada tetbiq edirik. beleki basqa scriptlerde singleton seklinde istifade etmek ucun sadece
            //GameManagerScript.GameManagerInstance.method ve ya variable istifade etmek olur.
            GameManagerInstance = this;

            if (playerPrefab == null)//Eger oyuncunun prefabi yoxdursa
            {
                Debug.LogError("<Color=Blue><a>Missing</a></Color> playerPrefab reference.Please set it up in GameObject 'Game Manager'",this);
            }
            else
            {
                if (PlayerManager.localPlayerInstance == null)//Eger scene deyisen zaman oyunda bizim oyuncu yoxdursa instantiate etsin.Burdan bize aid olan oyuncuya reference gonderdik.
                {
                    //SceneManager.GetActiveScene().name-hazirki oldugumuz scene-in adini alir.
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);
                    //Otaqda olduqda local oyuncu ucun character prefabin bir koypasini yaradiriq.
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
                else//Eger varsa melumat bildirsin
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManager.GetActiveScene().name);
                }
            }
        }
        #endregion

        #region Photon Callacks

        /// <summary>
        /// yeni oyuncu daxil olduqda bu metod ise dusur.
        /// </summary>
        /// <param name="newPlayer"></param>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName);
            if (PhotonNetwork.IsMasterClient)//Arena deyise bilmesi ucun master client olmaq lazimdir.Bunu sorgulayiriq.Eger master clientdirse
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }
        }
        /// <summary>
        /// Her hansi oyuncu otagi terk etdikde
        /// </summary>
        /// <param name="otherPlayer"></param>
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName);
            if (PhotonNetwork.IsMasterClient)//Arena deyise bilmesi ucun master client olmaq lazimdir.Bunu sorgulayiriq.Eger master clientdirse
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }
        }

        /// <summary>
        /// Otaqdaki oyuncu otaqdan cixan zaman bu metod cagrilir. Bu metod cagrilanda Esas oyuna giris ekranini yuklemek lazimdir.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }


        #endregion


        #region Public Methods
        //Bu metodu buttona yerlesdireceyik ki, oyuncu oyun zamani basib cixa bilsin.
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
        #endregion



        #region Private Methods
        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)//Master Client-otagin yaradicisidir.
            {
                Debug.LogError("PhotonNetwork: Level yuklemeye calisirsiz amma Master client deyilsiz.");
            }
            Debug.LogFormat("PhotonNetwork:Loading Level:{0}", PhotonNetwork.CurrentRoom.PlayerCount);//PlayerCount hazirki otaqdaki oyuncu sayini gosterir.LogFormat ise string icinde {} istifade etmeyimizi temin edir.
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);//Otaq adlari "Room for 1,2,3,4" dur. buna gore oyuncu sayina uygun otaq yukleyirik.SceneManagerden istifade etmedik cunki arena server uzerinden deyismelidir.
        }
        #endregion


    }
}

