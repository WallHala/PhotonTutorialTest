using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;
using System.Collections;



namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Playeri idare eden class
    /// Fire inputlari ve lazer sualarini idare edir.
    /// </summary>
 public class PlayerManager : MonoBehaviourPunCallbacks,IPunObservable //PhotonView terefinden observe olunan hansisa xusisiyyeti temin etmek ucun istifade edilir.
        //Bu xususiyyetleri istifade etmek ucun bu scripti ve ya componenti PhotonView componentindeki Observable Components property-ne elave etmek lazimdir.
    {
        #region Public Fields
        //Oyuncunun baslangic ve hazirki canini teyin etmek ucun istifade edilir.
        [Tooltip("The current health of player")]
        public float Health = 1f;

        [Tooltip("The player's UI GameObject Prefab")]//UI Canvasi dasiyacaq prefabi teyin edir.
        [SerializeField] public GameObject PlayerUIPrefab;

        [Tooltip("the local player instance. bu playerin scene-de olub olmadigini bilmek ucun istifade edirik.")]
        public static GameObject localPlayerInstance;
        #endregion

        #region Private Fields
        //Oyuncunun hirerarxiya menyusunda basinda beams gameObject var.Onu disable enable ederek attack mexanizmini ise saliriq.
        [Tooltip("The Beams gameobject to control")]
        [SerializeField] private GameObject beams;

        //Oyuncu ates acan zaman asagidaki deyisen true olacaq.
        bool isFiring;

        #endregion

        #region Monobehaviour Callbacks
        private void Awake()
        {
            //Vacib hisse
            //Gamemanager.cs de istifade edilir: bununla localPlayerin yerini ve ya movcudlugunu saxlayiriq ki,
            //sceneler deyisen zaman yeniden instantiate etmesin.
            if (photonView.IsMine)
            {
                PlayerManager.localPlayerInstance = this.gameObject;
            }

            //Singletona benzer tetbiq etdik beleki scene deyisende bu local playeri mehv etmesin deye DontDestroyOnLoad() istifade etdik.
            DontDestroyOnLoad(this.gameObject);
            if (beams == null)//Eger beams (lazer sualarini dasiyan gameObject) yoxdursa
            {
                Debug.LogError("<Color=Blue><a>Missing</a></Color> Beams Reference", this);
            }
            else//Baslangicda deaktiv edirik.
            {
                beams.SetActive(false);
            }
        }
        /// <summary>
        /// Burada if sertinde istifade etdiyimiz photonView.isMine a gore diger oyunculara aid amma bizim scene de olan prefab instancelarin cameraworkunu sondurmekdir.
        /// Cunki yalniz bizim kameranin bizi izlemesini isteyirik.
        /// </summary>
        private void Start()
        {
            //evvelce butun obyektlerde olan kameralari idare eden scripti aliriq.
            CameraWork camera_Work = this.gameObject.GetComponent<CameraWork>();

            if (camera_Work != null)//Eger obyektde camera idare eden script varsa
            {
                if (photonView.IsMine)//Ve eger bu bizim local playerdirse(oz oynatdigimiz oyuncu)
                {
                    camera_Work.OnStartFollowing();//bizim oyuncunu idare etmesini temin edirik.
                }
                
            }

            else
            {
                Debug.LogError("<Color=Blue><a>Missing</a></Color> CameraWork component on playerPrefab.", this);
            }

            if (PlayerUIPrefab != null)
            {
                //Bunu GameObject seklinde yaratdiq ki, SendMessage den istifade ede bilek.
                //SendMessage hansisa objectin icinde funksiya aktivlesdirmek ve parametr oturmek ucun istifade edilir.
                //1ci parametr UIPrefabInstance-daki funksiyani, 2-ci parametr hemin funksiyanin parametrini, 3-cu ise eger 
                //ele bir komponent yada funksiya olmazsa cagirilacaq erroru bildirir.
                GameObject _UIPrefabInstance = Instantiate(PlayerUIPrefab);
                _UIPrefabInstance.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }//Eger Playerin UI prefabi varsa
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
#if UNITY_5_4_OR_NEWER//Unity 5.4 un yeni scene management sistemi var.
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;//sceneLoaded burda sehneni temsil edenlerin siyahisidir. Her defe OnSceneLoaded verdikde hemin scene-e aid temsilcini elave edir.
#endif
        }

#if !UNITY_5_4_OR_NEWER//unity 5.4 den asagi olduqda
/// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
void OnLevelWasLoaded(int level)
{
    this.CalledOnLevelWasLoaded(level);
}
#endif
        void CalledOnLevelWasLoaded(int level)//Bu metod Scene yuklenen kimi cagrilir.
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            //Asagiya dogru bir Raycast yaradiriq ve onun neyese toxunub toxunmadigini yoxlayiriq.
            //Eger neyese toxunmursa merkeze dogru oyuncunu atiriq.
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            //Teze level yuklenen zaman bizim oyuncunun UI prefabi mehv olacaq.
            //Yeniden yaranmasi ve Parametrlerinin(ad, can) yerlesdirilmesi ucun
            //UiPrefabi yeniden instance edib, SetTarget-le cani ve adi yeniden yaziriq.
            GameObject UIPrefabInstance = Instantiate(this.PlayerUIPrefab);
            UIPrefabInstance.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }


#if UNITY_5_4_OR_NEWER
        public override void OnDisable()//Bu metod object yox olan zaman ve ya disabled edilen zaman cagrilir.
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;//Evvelki scene-in melumatlarini temsilci siyahisindan cixarir.
        }
#endif
        private void Update()
        {
            if (photonView.IsMine)//Atisi bizim prefabin etdiyini temin etmek ucun istifade edilir.
            {
                ProcessInputs();
            }
            

            //Beamsin aktivlik veziyyetini deyisir.
            if(beams!=null && isFiring != beams.activeInHierarchy)//Eger beams null deyilse( scripte elave edilibse) ve isFiring Truedursa amma beams hiyerarxiyada aktiv deyilse(ve ya eksi)
                //Onda beams-in aktivliyini isFiring kimi etsin.(truedursa true ,false-dursa false olsun).
            {
                beams.SetActive(isFiring);
            }


            if (Health <= 0)
            {
                GameManagerScript.GameManagerInstance.LeaveRoom();
            }
        }

        /// <summary>
        /// Bu metod other(diger) colliderlerin triggere daxil olan zaman ise dusur.Burada triggerler beamlar olacaq.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            //Eger oyuncu bizim oynadigimiz oyuncunun scripti deyilse
            if (!photonView.IsMine)
            {
                return;
            }
            //Eger userin adindaki stringde Beam adli hissecik yoxdursa
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f;

        }
        /// <summary>
        /// Bu ise beam-a(lazere) daimi meruz qalan zaman isleyecek.
        /// </summary>
             private void OnTriggerStay(Collider other)
        {
            //Eger oyuncu bizim oynadigimiz oyuncunun scripti deyilse
            if (!photonView.IsMine)
            {
                return;
            }
            //Eger userin adindaki stringde Beam adli hissecik yoxdursa
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            //Canin fpsden asili olaraq deyil, yavas-yavas azalmasi ucun istifade edilir.
            Health -= 0.1f*Time.deltaTime;

        }
#endregion

        #region IpunObservable Implementation //Bu stream boyunca oyuncularin etdiyi actionlari gondermek ucun istifade edilir.
        //PhotonTransformView ve ya AnimatorView avtomatik olaraq hereketi ve animasiyani sinxronlasdirir.
        //Amma diger actionlar ucun PhotonSerializeView isletmek lazimdir.
        public void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info)//Stream vasitesile prefab instancelari arasinda melumat oturur. Bu prefabda bas veren actionu basqa oyuncunun kompyuterindeki bu prefabin kopyasina tetbiq edir.
        {
            if (stream.IsWriting)//Eger stream melumat topluyursa
            {
                stream.SendNext(isFiring);//isFiringi melumat olaraq gonderirik ki, diger oyuncunun kompyuterindeki oyuncumuzun kopyasi da ates acsin.
                stream.SendNext(Health);//Eyni qayda ile cani da sinxronlasdiririq.
            }
            else if (stream.IsReading)//Eger melumati streamdan oxuyuruqsa
            {
                //streamdan gelen dataya diqqet.datanin novunden asili olaraq cevirmek lazimdir.(meselen booldursa (bool) seklinde ceviririk.)
                this.isFiring = (bool)stream.ReceiveNext();//Diger oyuncularin ates acma informasiyasini bize gonderir ki, hemin oyuncularin bizim kompyuterdeki kopyasi da ates aca bilsin.
                this.Health = (float)stream.ReceiveNext();//Eyni qayda ile cani da sinxronlasdiririq.
            }
        }
        #endregion


        #region Private Methods
#if UNITY_5_4_OR_NEWER//Eger unity 5.4 den yuxaridirsa bu metod isletmek lazimdir.
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)//Bu metod Scene yuklenen zaman hemin scenden alinan melumati gostermek ve istifade etmek ucun istifade edilir.Melumati sceneLoaded temsilci siyahisina oturur.
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
#endif
        #endregion


        #region Custom
        void ProcessInputs()
        {
            //Eger duyme basilibsa
            if (Input.GetButtonDown("Fire1"))
            {
                if(!isFiring)//Ve eger ates acilmirsa
                {
                    isFiring = true;//Ates acsin
                }

            }
            if (Input.GetButtonUp("Fire1"))//Eger duyme buraxilibsa
            {
                if (isFiring)//Eger ates acirsa
                {
                    isFiring = false;//Ates acmagi dayandirsin.
                }
            }
        }
#endregion
       
    }
}
