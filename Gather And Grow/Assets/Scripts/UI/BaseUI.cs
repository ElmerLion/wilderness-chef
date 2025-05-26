using System;
using UnityEngine;

public class BaseUI : MonoBehaviour {

    public static event Action<BaseUI> OnShow;
    public static event Action<BaseUI> OnHide;

    public virtual void Show() {
        Time.timeScale = 0f;
        gameObject.SetActive(true);

        OnShow?.Invoke(this);
    }

    public virtual void Hide() {
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        OnHide?.Invoke(this);
    }
    
}
