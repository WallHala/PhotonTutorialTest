using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MyCompany.MyGame
{
    public class PlayerUIScript : MonoBehaviour
    {
        #region Public Fields
        [Tooltip("Pixel offset from the player Target")]//Canvasin Playerin uzerinden uzaqligi
        [SerializeField] private Vector3 screenOffset = new Vector3(0f, 30f, 0f);
        #endregion
        #region private Fields
        [Tooltip("UI text to display Player's name")]//Oyuncunun adini visual olaraq gostermek ucun teyin edilir.
        [SerializeField] private Text playerNameText;

        private PlayerManager target;//Bu canvasin kime aid oldugunu teyin etmek ucun istifade edilir.

        [Tooltip("UI Slider to display Player's Health")]//Oyuncunun healthbarini visual olaraq gostermek ucun teyin edilir.
        [SerializeField] private Slider playerHealthslider;

        float characterControllerHeight = 0f;
        Transform targetTransform;
        Renderer targetRenderer;
        CanvasGroup _canvasGroup;
        Vector3 targetPosition;
        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            //Bunun vasitesile bu UI Slideri Scenedeki Canvasa child edirik.
            //Eger true olarsa Slider original olcusunde qalacaq.Amma false olarsa canvasin olcusune uygun deyisecek.
            this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(),false);

            _canvasGroup = this.GetComponent<CanvasGroup>();
        }
        private void Update()
        {
            if (target == null)//Eger oyuncu oyundan ayrilarsa bu Canvasi yox etmek ucun
            {
                Destroy(this.gameObject);
                return;
            }
            //Eger playerin PlayerHealth slideri varsa
            if (playerHealthslider != null)
            {
                playerHealthslider.value = target.Health;//Playerin canini sliderle gosteririk.
            }
        }


        private void LateUpdate()
        {
            if (targetRenderer != null)
            {
                //CanvasGroup.alpha-CanvasGroupun esas obyektini teyin edir.
                //Renderer.visible- ise bu rendererin cameraya gorunub gorunmediyini temin edir.
                this._canvasGroup.alpha = targetRenderer.isVisible ? 1f:0f;
            }

            //Kritik hisse
            //Target oyuncunu ekranda izlesin.

            if (targetTransform != null)
            {
                targetPosition = targetTransform.position;
                targetPosition.y += characterControllerHeight;
                //Camera.WorldToScreenPoint-virtual alemdeki positionu screen gorunusune kecirir.Yeni bizim 2D olan canvasi 3D dunyada gostermek ucun istifade edilir.
                this.transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
            }
        }
        #endregion

        #region Public Methods
        public void SetTarget(PlayerManager _target)
        {
            if (_target == null)//Eger target yoxdursa
            {
                Debug.LogError("< Color = Red >< a > Missing </ a ></ Color > PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            target = _target;//PlayerUI scriptdeki targete oyuncunu elave edirik ki istifade ede bilek.
            if (playerNameText != null)//Eger text UI elementi varsa
            {
                playerNameText.text = target.photonView.Owner.NickName;//Hemin oyuncunun adini canvasda gostersin.
            }
            //Esas oyuncudan getdiyi movqeyi aldiq.
            targetTransform = this.target.GetComponent<Transform>();

            //Esas oyuncu ucun olan Rendereri istifade edirik.
            targetRenderer = this.target.GetComponent<Renderer>();
            //Characteri idare eden Controlleri aliriq.
            CharacterController characterController = target.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterControllerHeight = characterController.height;
            }
        }
        #endregion
    }

}
