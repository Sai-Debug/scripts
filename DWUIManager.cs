using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DWUIManager : SimpleSingleton<DWUIManager>
{
    [Header("Canvas")]
    public GameObject CanvasPrefab;
    public Canvas TemporaryCanvas;
    [HideInInspector]
    public Canvas SceneCanvas;
    [Header("UI Stuff")]
    public bool LoadOnStart = true;
    public GameUI StartUI = GameUI.LogoIntro;
    public float StartFadeIn = 2;
    public GameUI BackToMenuUI;
    public GameUI PreviousUI = GameUI.DialogYesNo;
    public List<GameObject> CurrentUIObjects = new List<GameObject>();
    public GameObject CurrentDialogObject = null;

    public string UIBagFolder = "ui_bags";
    //public UIBag MainBag;
    Dictionary<GameUI, DWUIBag> _sources = new Dictionary<GameUI, DWUIBag>();
    Coroutine mainco = null;

    bool firstRun = true; // logic blocker --- fixes OnLevelWasLoaded called twice
    void Awake()
    {        
        InitSingleton();
        if(TemporaryCanvas != null)
            Destroy(TemporaryCanvas.gameObject);

        TemporaryCanvas = null;
        DWUIBag[] uis = Resources.LoadAll<DWUIBag>(UIBagFolder);
        foreach (DWUIBag ui in uis)
        {
            _sources[ui.UI] = ui;
        }
        if (this.isActiveAndEnabled)
        {
            CreateCanvas();
        }
    }
    void CreateCanvas()
    {
        if (SceneCanvas == null)
        {
            GameObject canvasObj = (GameObject)Instantiate(CanvasPrefab);
            SceneCanvas = canvasObj.GetComponent<Canvas>();
        }
    }
    void OnDestroy()
    {
        UninitSingleton();
    }

    void Start()
    {
        firstRun = false;
        if (LoadOnStart)
            OpenReplace(StartUI, StartFadeIn);
    }

    public GameObject Open(GameUI ui, float fadeTime = 0.4f)
    {
        GameObject opened = null;
        GameObject orig = _sources[ui].UIPrefab;
        if (orig != null)
        {
            CreateCanvas();
            GameObject newObj = (GameObject)Instantiate(orig);
            newObj.transform.SetParent(SceneCanvas.transform, false);
            opened = newObj;
            CurrentUIObjects.Add(newObj);
            CanvasGroup cg = newObj.GetComponent<CanvasGroup>();
            if (cg != null)
                StartCoroutine(Fade(UIFade.In, fadeTime, cg));
        }
        return opened;
    }
    public T Open<T>(GameUI ui, float fadeTime = 0.4f)
    {
        GameObject obj = Open(ui, fadeTime);
        T component = obj.GetComponent<T>();
        return component;
    }
    public T OpenReplace<T>(GameUI ui, float fadeTime = 0.4f)
    {
        GameObject obj = OpenReplace(ui, fadeTime);
        T component = obj.GetComponent<T>();
        return component;
    }
    public GameObject OpenReplace(GameUI ui, float fadeTime = 0.4f)
    {
        GameObject opened = null;
        GameObject orig = _sources[ui].UIPrefab;
        if (orig != null)
        {
            if (mainco != null)
                StopCoroutine(mainco);

            //if (CurrentUIObject!=null)
            //Close(CurrentUIObject, fadeTime);
            CreateCanvas();
            var uis = CurrentUIObjects.ToArray();
            foreach (GameObject g in uis)
            {
                Close(g, fadeTime);
            }
            CurrentUIObjects.Clear();

            GameObject newObj = (GameObject)Instantiate(orig);
            newObj.transform.SetParent(SceneCanvas.transform, false);
            opened = newObj;
            CanvasGroup cg = newObj.GetComponent<CanvasGroup>();
            if (cg != null)
                mainco = StartCoroutine(Fade(UIFade.In, fadeTime, cg));
            CurrentUIObjects.Add(newObj);
        }
        return opened;
    }
    public void Close(GameObject uiObj, float fadeTime = 0.4f)
    {
        if (uiObj != null)
        {
            if(mainco!=null)
                StopCoroutine(mainco);
            CurrentUIObjects.Remove(uiObj);
            CanvasGroup cg = uiObj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                StartCoroutine(Fade(UIFade.Out, fadeTime, cg));
            }
        }
    }
    IEnumerator Fade(UIFade fade, float fadeTime, CanvasGroup cg)
    {
        if (cg == null)
            yield break;

        float targetValue = 0;
        switch (fade)
        {
            case UIFade.In:
                targetValue = 1.0f;
                cg.alpha = 0;
                break;
            case UIFade.Out:
                targetValue = 0.0f;
                cg.alpha = 1;
                break;
        }

        float alpha = cg.alpha;
        for (float t = 0.0f; t < 1.0f; t += Time.unscaledDeltaTime / fadeTime)
        {
            if (cg == null)
                yield break;
            cg.alpha = Mathf.Lerp(alpha, targetValue, t);
            yield return new WaitForEndOfFrame();
        }

        cg.alpha = targetValue;
        //Debug.Log(cg.name);
        if (targetValue == 0.0f)
        {
            Destroy(cg.gameObject);
        }
    }
    private void OnLevelWasLoaded(int level)
    {
        if (level == 0 && firstRun == false)
        {
            Open(GameUI.StartMenu);
        }
    }

    public delegate void OnUIEventHandler(object sender, object args);
   
}
[System.Serializable]
public struct UISource
{
    public string Source;
    public GameUI UI;
}
public enum GameUI
{
    DialogYesNo,
    LogoIntro,
    LogoAlternate,
    StartMenu,
    ProfileManager,
    ProfileEditor,
    BackStory,
    Disclaimer,
    InGameMenu,
    Credits,
    Options,
    LoadScreen,
    GameHUD,
    RebornUI,
    DialogueBox,
    Blocker,
    MagesLogo,
    DialogOK,
    BatchIntro
}
public enum UIFade
{
    In,
    Out,
    None
}