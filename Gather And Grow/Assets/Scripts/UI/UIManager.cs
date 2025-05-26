using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Mono.CSharp;

public class UIManager : MonoBehaviour {

    public static UIManager Instance { get; private set; }

    [SerializeField] private List<BaseUI> uiList;

    [SerializeField] private BaseUI openUI;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        BaseUI.OnShow += OnUIShow;
        BaseUI.OnHide += BaseUI_OnHide;

        GameInput.Instance.OnPausePerformed += OnPausePerformed;
    }

    private void BaseUI_OnHide(BaseUI obj) {
        openUI = null;
    }

    private void OnPausePerformed() {
        if (openUI != null) {
            openUI.Hide();
        } else {
            PauseMenuUI.Instance.Show();
        }
    }

    public void CloseOpenUI() {
        if (openUI != null) {
            openUI.Hide();
        }
    }


    private void OnUIShow(BaseUI ui) {
        foreach (BaseUI otherUI in uiList) {
            if (otherUI != ui && otherUI.gameObject.activeSelf) {
                otherUI.Hide();
            }
        }

        openUI = ui;
    }

    public bool IsAnyUIOpen() {
        return openUI != null;
    }

    private void OnDestroy() {
        BaseUI.OnShow -= OnUIShow;
        BaseUI.OnHide -= BaseUI_OnHide;
        GameInput.Instance.OnPausePerformed -= OnPausePerformed;
    }



}
