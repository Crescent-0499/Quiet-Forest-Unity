using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponState : MonoBehaviour
{
    public TMPro.TMP_Text ammoText;
    public TMPro.TMP_Text weaponnameText;
    void Start()
    {
        weaponnameText.text = "Unarmed";
        ammoText.text = "";
    }

    public void Refresh(int ammoCount, int maxammo, string weaponName)
    {
        if (weaponName == null) 
        { 
            weaponnameText.text = "Unarmed";
            ammoText.text = ""; 
            return; 
        }
        if(weaponName == "Axe")
        { 
            weaponnameText.text = "Axe";
            ammoText.text = ""; 
        }
        else
        {
            weaponnameText.text = weaponName;
            ammoText.text = ammoCount.ToString() + "/" + maxammo.ToString();
        }
    }
}
