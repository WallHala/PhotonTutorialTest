using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using System.Collections;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// PlayerNameInputField -Oyuncunun adi daxil edilen yer.Bura yazilan soz oyuncunun basi uzerinde gorunecek.
    /// </summary>
    
    [RequireComponent(typeof(InputField))]//Bu class GameObjectin mutleq sekilde InputField olmasini ve ya malik olmasini temin edir.Eger gameObjectde yoxdursa avtomatik olaraq elave edir.
    public class PlayerNameInputField : MonoBehaviour
    {
        #region Private sabit verilenler

        //Yazi xetalarina yol acmamaq ucun PlayerPrefKey-i saxlayir.
        const string playerNamePrefKey = "PlayerName";
        #endregion



        #region MonoBehaviour Callbacks


        private void Start()
        {
            string default_name = string.Empty;//Default name olaraq bos string verir.
            InputField _inputField = this.GetComponent<InputField>();//InputField ile islemek ucun onu komponent olaraq alir.
            if (_inputField != null)//Eger inputField varsa
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))//Eger PlayerPrefs-in playerNamePrefKey adinda key-i varsa.
                {
                    default_name = PlayerPrefs.GetString(playerNamePrefKey);//Evvelceden teyin olunmus adi aliriq.
                    _inputField.text = default_name;//Evvelceden teyin olunmus adi inputFieldin textine yaziriq.
                }
            }

            PhotonNetwork.NickName = default_name;//Network uzre NickName-mizi PlayerPrefe uygun teyin edirik.
        }
        #endregion


        #region Public Methods
       
        public void SetPlayerName(string value)//Bu metodu InputField componentindeki OnValueChanged() funksiyasina elave edirik. OnValueChanged() InputFieldin texti her defe deyisdikde ise dusur.
        {
            //#Vacib hisse burda stringin bos olub olmamasi yoxlanilir.
            if (string.IsNullOrEmpty(value))//Eger bosdursa
            {
                Debug.LogError("Player name is null or empty");
                return;
            }
            PhotonNetwork.NickName = value;//Deyilse Nickname olaraq value elave edilir.
            PlayerPrefs.SetString(playerNamePrefKey, value);//Ve NickName oyuncunun cihaz yaddasina yazilir.
        }

        #endregion















    }

}

