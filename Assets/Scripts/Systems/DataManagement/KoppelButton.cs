using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class KoppelButton : MonoBehaviour
{
    public TextMeshProUGUI txtKoppelName;

    public Button button;
    public Button btnDelete;
    public SheepKoppel element;
    public KoppelDataViewer dataViewer;

    // Start is called before the first frame update
    private void Start()
    {
        button.onClick.AddListener(delegate { dataViewer.ShowDetails(element); });
        btnDelete.onClick.AddListener(delegate { dataViewer.dataReader.DeleteKoppel(element); });
    }

    public void SetInfo(SheepKoppel _element, KoppelDataViewer _dataViewer)
    {
        element = _element;
        dataViewer = _dataViewer;
        txtKoppelName.text = element.koppelName;
        //WormNonScienceNameText.text = element.nonScienceName;
        //WormTypeText.text = worm.wormType.ToString();
    }
}
