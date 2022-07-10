using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    public Image healthFill;
    public Image armorFill;

    // Update is called once per frame
    void Update()
    {
        if (InputHandler.instance == null)
            return;

        healthFill.fillAmount = InputHandler.instance.state.health / InputHandler.instance.state.maxhealth;
        armorFill.fillAmount = InputHandler.instance.state.armor / InputHandler.instance.state.maxArmor;
    }
}
