using UnityEngine;
using Photon.Pun;
using Photon.Realtime;//Cross platform network engine kitabxanasidir.


//IConnectionCallbacks: connection related callbacks.(elaqe vw qosulmaya aid callback)
//IInRoomCallbacks: callbacks that happen inside the room.(otaq icinde bas veren callback)
//ILobbyCallbacks: lobby related callbacks.(lobby-e aid olan callbacklar)
//IMatchmakingCallbacks: matchmaking related callbacks.(oyun yaratmaya aid olan callbacklar)
//IOnEventCallback: a single callback for any received event. This is equivalent to the C# event OnEventReceived.(Hansisa bir evente gore gelen tek callback)
//IWebRpcCallback: a single callback for receiving WebRPC operation response.(WebRPC operationlara gore gelen callbacklar)
//IPunInstantiateMagicCallback: a single callback for instantiated PUN prefabs.(Instantiate olunmus PUN prefablara gore gelen callbacklar
//IPunObservable: PhotonView serialization callbacks.
//IPunOwnershipCallbacks: PUN ownership transfer callbacks.

namespace Com.MyCompany.MyGame//namespaceler eyni adli scriptlerin bir-biri ile toqqusmamasi ucun istifade olunur.
{
public class LauncherScript : MonoBehaviourPunCallbacks
{
        //#region #endregion kodlari seliqeli hisseler seklinde yazmaq ucun istifade edilir.
        #region Private Serializable Fields

        [Tooltip("Bu deyisen otaga daxil ola bilecek max oyuncu sayini temin edir. Eger otaq doludursa qosulmaq isteyen oyuncu yeni otaq yaradir.")]
        [SerializeField] private byte maxPlayersPerRoom = 4;
        #endregion

        #region private Fields
        ///<summary>
        ///Bu clientin oyun versiya nomresidir. Ferqli versiyalara malik oyuncular bu kod vasitesile ayrilir.
        /// </summary>
        string gameVersion = "1";
        /// <summary>
        /// isConnecting istifadeci istedikce ve connect buttonuna basdiqda oyuncunun otaga daxil olmasi ucun istifade edilir.
        /// Eger bu deyisen olmasa otagdan cixib esas menyuya qayidan zaman onConnectedToMaster()de verdiyimiz if() sertini vere bilmirik ve JoinRandomRoom() metodu
        /// ve onunla birlikde JoinRandomRoom() override metodu isleyecek ve oyuncu her defe cixanda ardicil olaraq otaq yaradacaq.Amma eger connect buttonunu idare eden
        /// connect metodu vasitesile isConnectingi deyissek bu zaman yalniz buttona basildiqda otaga qosula bilecek.
        /// </summary>
        bool isConnecting;
        #endregion
        #region Public Fields
        [Tooltip("Istifadecilerin adini daxil ede bileceyi, oyuna qosulma buttonunu ve ad Input fieldini aktivlesdiren UI paneli")]
        [SerializeField] private GameObject controlPanel;
        [Tooltip("Oyunculari qosulma baresinde melumatlandiran UI Text obyekti")]
        [SerializeField] private GameObject progressLabel;
        #endregion
        #region MonoBehaviourPunCallbacks Callbacks
        ///<summary>
        ///Ilkin serverle elaqe yaradilan zaman MonoBehaviour metodlari cagrilir.
        ///</summary>
        void Awake()
        {
            //#kritik hisse
            //Bu metod PhotonNetwork.LoadLevel() metodunun master client(otaq yaradicisinda) cagrilmasini temin edir ve hemin otaga qosulan diger oyuncularin oyuna sinxronizasiyasi temin edir.
            PhotonNetwork.AutomaticallySyncScene = true;
            //Oyundaki arena olcusu oyuncu sayina gore deyiseceka(Her defe ferqli scene-de arena olacaq). Bu metodu isletmesek master clientde arena olcusu deyisecek amma diger clientlerde arena olcusu qalacaq.
        }
        ///<summary>
        ///Ilkin serverle elaqe yaradilan zaman bu MonoBehaviour metodu da cagrilir.
        ///</summary>
        void Start()
        {
            // Connect(); Bunu commente aldiq cunki artiq qosulmani buttonla edirik.
            progressLabel.SetActive(false);//progress gosteren texti deaktivlesdirdik.
            controlPanel.SetActive(true);//esas giris panelini(ad qosulma buttonu olan paneli) aktivlesdirdik.
        }
        #endregion
        #region Public Methods
        ///<summary>
        ///Asagidaki metod servere qosulmani temin edir.
        ///Eger qosulubsa hansisa otaga qosulmaga calisir.
        ///Eger qosulmayibsa oyunu Photon Cloud Networka qosmaga calisir.
        /// </summary>
        
        public void Connect()
        {
            isConnecting = true;
            progressLabel.SetActive(true);//progressLabeli sondurduk.
            controlPanel.SetActive(false);//ControlPaneli aktivlesdirdik.
            //Qosulmani yoxlariyiq, eger qosulmusuqsa Random bir otaga girmeye calisiriq.
            if (PhotonNetwork.IsConnected)
            {
                //#Kritik hisse- Random bir otaga qosulmaga calisiriq. Eger qosula bilmirikse, OnJoinRandomFailed()-de xeberdar edilirik ve otaq yaradiriq.
                PhotonNetwork.JoinRandomRoom();
            }

            else
            {
                //Kritik eger qosulmamisiqsa evvelce Photonun online serverine qosulmaliyiq
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("Esas servere qosuldu");
            //#Kritik hisse- Random bir otaga qosulmaga calisiriq. Eger qosula bilmirikse, OnJoinRandomFailed()-de xeberdar edilirik ve otaq yaradiriq.
            if (isConnecting)//Eger isConnecting True olarsa otaga qosula bilsin.
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("OnDisconnected() method was called");
        }

        //Bu metod serverde random otaq olmadiqda ve ya oyuncu hansisa random otaga qosula bilmedikde yaranir.
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Serverde random hansisa otaq olmadigi ucun OnJoinRandomFailed() metodu cagrildi.Yeni otaq yaradilir \n PhotonNetwork.CreateRoom()metodu cagrilir");
            //Kritik melumat-Random qosulmaga otaq yoxdur cunki otaqlar ya yoxdur yada doludur. Bunun ucun yeni otaq yaradiriq.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers=maxPlayersPerRoom});
        }


        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() metodu cagrildi.Indi oyuncu otaqdadir");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)//Eger otaga daxil olmus ilk oyuncuyuqsa
            {
                //Kritik melumat-ilk sehne yuklenir.
                Debug.Log("'Room for 1' scene-i yuklenir");
                PhotonNetwork.LoadLevel("Room for 1");
            }
        }
        #endregion

    }

}

