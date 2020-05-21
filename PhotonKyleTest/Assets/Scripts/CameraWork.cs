using UnityEngine;
using System.Collections.Generic;


namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Bu class cameranin oyuncunu izlemesini temin edir.
    /// </summary>
    public class CameraWork : MonoBehaviour
    {
        #region Private Fields
        //Bu deyisen oyuncu ile kamera arasindaki mesafeni temin edir.
        [Tooltip("The distance in the local x-z plane to the target")]
        [SerializeField] private float distance = 7.0f;

        //Bu deyisen ise Cameranin oyuncudan hundurluyunu temin edir.
        [Tooltip("The height we want the camera to be above the target")]
        [SerializeField] private float height = 3.0f;

        //Bu deyisen Camera hereket eden zaman daha realsitic olmasi ucun istifade olunur.(yumsaq hereket)
        [Tooltip("The smooth time lag for the height of camera")]
        [SerializeField] private float heightSmoothLag = 0.3f;
        
        //Vertical olaraq camerayla oyuncu arasinda mesafe ferqi temin edir.Meselen yerden daha cox gorunemesi ve yuxaridan daha az gorunmesi ve eksine
        [Tooltip("Allow the camera to be offseted vertically from the target,for example fiving more view of the scenery and less ground")]
        [SerializeField] Vector3 centerOffset = Vector3.zero;

        //Photon Network terefinden hansisa bir componenti instantiate edilerse onda bu deyiseni false edin , ve manual olaraq lazim olanda onStartFollowing() metodunu cagirin.
        [Tooltip("Set this as false if a component of a prefab being instaciated by Photon Network, and Manually call OnStartFollowing() when and if needed")]
        [SerializeField] bool followOnStart = false;
        //Cameranin hansi yerde oldugunu ozunde saxlayan deyisen
        Transform cameraTransform;

        //bu deyisen cameraya sahib oyuncu itdikde ve ya kamera deyisdikde yeniden qosulmaq ve oyuncunu izlemek ucun istifade edilir.
        bool isFollowing;

        //hazirki cameranin suretini teyin edir. ve bu qiymet SmoothDamp() funksiyasi her cagrildigda deyisir ve idare edilir.
        private float heightVelocity;

        //SmoothDamp() vasitesile catmaq istediyimiz hundurluyu ifade edir.
        private float targetHeight = 100000.0f;

        #endregion

        #region Monobehaviour Callbacks
        private void Start()
        {
            //Eger istenilirse target camera terefinden izlensin.
            if (followOnStart)
            {
                OnStartFollowing();
            }
        }

        /// <summary>
        /// Bu monoBehaviour metodu butun update metodlarindan sonra isleyir cunki evvelce update metodundaki oyuncu movqeyini deyismeli daha sonra ise camera hereket etmelidir.
        /// </summary>
        private void LateUpdate()
        {
            //Level yuklenen zaman target (oyuncu) destroy edilmeyecek
            //Buna gore de her scene yuklenen zaman Main Camera muxtelif yerlerde olur,ve main cameranin yeniden obyekti izlemesini temin etmek lazimdir.
            if(cameraTransform==null && isFollowing)
            {
                OnStartFollowing();
            }
            //eger follow inspector menyusundan deyisilerse
            if (isFollowing)
            {
                Apply();
            }
        }

        #endregion

        #region Public Methods
        //Cameranin oyuncunu izlemesini temin edir.
        //Bu metodu Photon Networkdan isntantiate edilen prefab instance-lari idare eden ve neyi izlemek lazim oldugu zaman istifade edin.
        public void OnStartFollowing()
        {
            cameraTransform = Camera.main.transform;
            isFollowing = true;
            //Hec neyi smoothlasdirmiriq,birbasa cameranin cekimini gosteririk.
            Cut();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Bu metodla smooth bir sekilde oyuncunu izleyirik.
        /// </summary>
        void Apply()
        {
            Vector3 targetCenter = transform.position + centerOffset;
            //oyuncunun ve kameranin donme bucagini teyin edirik.
            float origionalTargetAngle = transform.eulerAngles.y;//Oyuncunun donme bucagi
            float currentAngle = cameraTransform.eulerAngles.y;//Cameranin donme bucagi
            //camera isleyen zaman oyuncunun donme bucagina gore tenzimliyirik.
            float targetAngle = origionalTargetAngle;//Targetin hereket eden zaman camerayla birlikde eyilmemesi ucun teyin etdik.
            currentAngle = targetAngle;
            targetHeight = targetCenter.y + height;

            //Cameranin hundurluyunu smooth sekilde deyismek

            float currentHeight = cameraTransform.position.y;
            currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);
            //Bucagi dereceye cevirir ve daha sonra pozisiya tenzimleyen zaman istifade olunacaq.
            Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);
            //Asagida x-z mustevisine gore camerani sabitledik:
            //aradaki messfeni de elave etdik(distance olaraq)//Vector3.back arxaya dogru olmasini,current rotation hazirki donme bucagini temin edir.
            cameraTransform.position = targetCenter;
            cameraTransform.position += currentRotation * Vector3.back * distance;
            //Kameranin hundurluyunu teyin edirik.
            cameraTransform.position = new Vector3(cameraTransform.position.x, currentHeight, cameraTransform.position.z);
            //Hemise oyuncuya baxsin
            SetUpRotation(targetCenter);
        }

        /// <summary>
        /// Birbasa cameranin pozisiyasini oyuncunun arxa ve merkezine yerledirir.
        /// </summary>
        void Cut()
        {
            float oldHeightSmooth = heightSmoothLag;
            heightSmoothLag = 0.001f;
            Apply();
            heightSmoothLag = oldHeightSmooth;
        }


        /// <summary>
        /// Cameranin rotationunun hemise oyuncunun arxasinda ve merkezde olmasini temin edir.
        /// </summary>
        /// <param name="centerPos">Merkezi pozisiya</param>
        void SetUpRotation(Vector3 centerPos)
        {
            Vector3 cameraPos = cameraTransform.position;
            Vector3 offsetToCenter = centerPos - cameraPos;
            //Y-oxu etrafinda donmeni temin edir.
            Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x,0,offsetToCenter.z));
            Vector3 relativeOffset = Vector3.forward * distance + Vector3.down;
            cameraTransform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);
        }
        #endregion
    }
}

