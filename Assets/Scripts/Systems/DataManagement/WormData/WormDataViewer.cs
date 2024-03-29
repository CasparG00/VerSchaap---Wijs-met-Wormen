using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public class WormDataViewer : DataViewer
{
    [Header("Prefabs")]
    public GameObject wormButtonPrefab;
    
    [Header("UI Panels")]
    public RectTransform wormButtonContainer;
    public GameObject overviewPanel;
    public GameObject detailsPanel;

    [Header("Worm variable fields")]
    public TextMeshProUGUI detailsPanelTitle;
    public TMP_InputField inputWormType;        //WormType, scientific name
    public TMP_InputField inputNonScienceName;  //Non-science name
    public TMP_InputField inputDescription;     //Description name
    public Image wormImg;                       //Worm egg image. Must match the name of inputwormtype

    //TODO
    //List<WormMedicines> effectiveMedicines
    //List<WormResistences> resistences
    //List<WormSymptoms> symptoms
    //List<WormFaveConditions> faveConditions
    //List<string> extraRemarks

    [HideInInspector]
    public WormObject selectedWorm;
    [HideInInspector]
    public bool bAddingElement = false;
    [HideInInspector]
    public SheepDataReader dataReader;

    public Dictionary<string, Sprite> wormImages = new Dictionary<string, Sprite>();

    public override GameObject CreateNewButton(ObjectUUID objToAdd)
    {
        WormObject worm = objToAdd as WormObject;
        var buttonGameObject = Instantiate(wormButtonPrefab, wormButtonContainer);
        buttonGameObject.GetComponentInChildren<WormButton>().SetInfo(worm, this);
        return buttonGameObject;
    }

    public override void RemoveButton(ObjectUUID objToRemove)
    {
        base.RemoveButton(objToRemove);
    }

    private void LoadImages()
    {
        var textures = Resources.LoadAll("WormImages/Eggs", typeof(Sprite));

        foreach (var t in textures)
        {
            wormImages.Add(t.name, (Sprite)t);
        }
    }

    public void UpdateImage(string wormName)
    {
        Sprite spr = null;
        wormImages.TryGetValue(wormName, out spr);
        wormImg.sprite = spr;
    }

    private void Start()
    {
        SetupDetailsPanel();
        LoadImages();
    }

    public void CreateWormButtonsFromDB(List<WormObject> wormDatabase)
    {
        foreach (WormObject w in wormDatabase)
        {
            CreateNewButton(w);
        }
    }

    public void SetupDetailsPanel()
    {
        //WormType[] valsWormType = (WormType[])Enum.GetValues(typeof(WormType));
    }

    /// <summary>
    /// Called whenever an worm is selected
    /// </summary>
    /// <param name="worm"></param>
    public void ShowDetails(WormObject worm)
    {
        selectedWorm = worm;
        overviewPanel.SetActive(false);
        detailsPanel.SetActive(true);
        detailsPanelTitle.text = worm.nonScienceName;
        inputNonScienceName.SetTextWithoutNotify(worm.nonScienceName);
        inputWormType.SetTextWithoutNotify(worm.wormType.ToString());
        inputDescription.SetTextWithoutNotify(worm.eggDescription);
        UpdateImage(selectedWorm.wormType.ToString());
    }
}
