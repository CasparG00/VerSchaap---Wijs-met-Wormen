using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SheepDataReader : MonoBehaviour
{
    public TextAsset sheepDataFile;
    public SheepDataViewer sheepDataViewer;
    public WormDataViewer wormDataViewer;
    public LotDataViewer LotDataViewer;
    public PairCollectionDataViewer PairCollectionDataViewer;
    //public TemporalDatabaseData Database.GetDatabase();

    public GameObject pnlDelete;
    public Button btnDeleteConfirm;
    public Button btnDeleteCancel;

    public bool bConfirmToDelete;
    public Toggle bDeleteToggle;

    public TMP_InputField FarmerUUIDInputField;

    public List<string> receivedAnswers = new List<string>();

    public void OpenFileExplorer()
    {
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
    }

    private void OnEnable()
    {
        EventSystem<string>.AddListener(EventType.checkDatabaseResponse, CheckResponse);
    }

    private void OnDisable()
    {
        EventSystem<string>.RemoveListener(EventType.checkDatabaseResponse, CheckResponse);
    }

    public void CheckResponse(string methodUUID)
    {
        if (Database.requests.Contains(methodUUID) && DBST.Instance.dataPackages.ContainsKey(methodUUID))
        {
            Debug.Log("Received UUID: " + methodUUID);
            Database.requests.Remove(methodUUID);
            receivedAnswers.Add(methodUUID);
        }
        else
        {
            Debug.LogError("Method UUID not found: " + methodUUID);
        }
    }

    private IEnumerator UploadSheep()
    {
        foreach (var s in Database.GetDatabase().sheeps)
        {
            SheepObject newSheep = new SheepObject();
            newSheep.UUID = s.UUID;
            string[] result = Database.ProgressData(MethodType.Post, newSheep);

            Debug.Log("result length = " + result.Length);

            for (int i = 0; i < result.Length; i++)
            {
                Debug.Log("resultpart " + i + ": " + result[i]);
            }

            Debug.Log(Helpers.CodeToMessage(result[0]));
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator UploadLots()
    {
        foreach (var s in Database.GetDatabase().Lots)
        {
            string[] result = Database.ProgressData(MethodType.Post, s);

            Debug.Log("result length = " + result.Length);

            for (int i = 0; i < result.Length; i++)
            {
                Debug.Log("resultpart " + i + ": " + result[i]);
            }

            Debug.Log(Helpers.CodeToMessage(result[0]));
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator TestMethod()
    {
        yield return 0;
    }

    private void Start()
    {
        btnDeleteCancel.onClick.AddListener(delegate { pnlDelete.SetActive(false); });

        FarmerUUIDInputField.SetTextWithoutNotify("4dea7913-f4d7-42b3-be55-47f97b8576b2");

        sheepDataViewer = GetComponent<SheepDataViewer>();
        wormDataViewer = GetComponent<WormDataViewer>();
        LotDataViewer = GetComponent<LotDataViewer>();
        PairCollectionDataViewer = GetComponent<PairCollectionDataViewer>();

        PairCollectionDataViewer.dataReader = this;
        wormDataViewer.dataReader = this;
        sheepDataViewer.sheepDataReader = this;
        LotDataViewer.sheepDataReader = this;
        


        LoadSheepData(sheepDataFile);
        //StartCoroutine(TestMethod());
        OnDatabaseLoaded();
    }

    /// <summary>
    /// Creates the buttons for all elements in the database when it has finished loading
    /// </summary>
    public void OnDatabaseLoaded()
    {
        PairCollectionDataViewer.CreateButtonsFromDB(Database.GetDatabase().pairCollection);
        wormDataViewer.CreateWormButtonsFromDB(Database.GetDatabase().worms);
        sheepDataViewer.CreateSheepButtonsFromDB(Database.GetDatabase().sheeps);
        LotDataViewer.CreateSheepButtonsFromDB(Database.GetDatabase().Lots);
    }

    private T GetElementByUUID<T>(string uuid, List<T> list) where T : ObjectUUID
    {
        foreach (var element in list)
        {
            if (element.UUID == uuid) return element;
        }

        return null;
    }

    public SheepObject GetSheepObjectByUUID(string uuid)
    {
        return GetElementByUUID(uuid, Database.GetDatabase().sheeps);
    }

    public PairCollection GetSheepKoppelByUUID(string uuid)
    {
        return GetElementByUUID(uuid, Database.GetDatabase().pairCollection);
    }

    public LotObject GetLotObjectByUUID(string uuid)
    {
        return GetElementByUUID(uuid, Database.GetDatabase().Lots);
    }

    public string GetKoppelNameByUUID(string uuid)
    {
        PairCollection kop = GetSheepKoppelByUUID(uuid);
        if (kop != null) return kop.pairCollectionName;
        return null;
    }

    public string GetKoppelUUIDByName(string name)
    {
        foreach (var koppel in Database.GetDatabase().pairCollection)
        {
            if (koppel.pairCollectionName == name) return koppel.UUID;
        }

        return null;
    }

    public void UpdateSheepData(SheepObject sheep)
    {
        int nChilds = sheepDataViewer.buttonContainer.childCount;

        // editing existing sheep
        if (sheepDataViewer.panelMode == DetailsPanelMode.EditingElement)
        {
            // update the actual data
            var shp = GetSheepObjectByUUID(sheepDataViewer.selectedSheep.UUID);

            if (shp != null)
            {
                shp.sheepTag = sheep.sheepTag;
                shp.sex = sheep.sex;
                shp.sheepType = sheep.sheepType;
                shp.tsBorn = sheep.tsBorn;

                var oldKoppelID = shp.pairCollectionID;
                var newKoppelID = sheep.pairCollectionID;
                // TODO check if it is a valid koppelid

                if (oldKoppelID != newKoppelID)
                {
                    // Remove the sheep from the old koppel
                    PairCollection oldKop = GetSheepKoppelByUUID(oldKoppelID);
                    oldKop?.allSheep.Remove(sheep.UUID);

                    // Add the sheep to the new koppel
                    PairCollection newKop = GetSheepKoppelByUUID(newKoppelID);
                    newKop?.allSheep.Add(shp.UUID);
                }

                shp.pairCollectionID = newKoppelID;
            }

            // update the visuals representing the data
            for (int i = 0; i < nChilds; i++)
            {
                var obj = sheepDataViewer.buttonContainer.GetChild(i).gameObject.GetComponentInChildren<SheepButton>();
                if (obj.sheep.UUID != sheepDataViewer.selectedSheep.UUID) continue;
                obj.SetInfo(sheep, sheepDataViewer);
                break;
            }
        }

        // Adding a new sheep
        // TODO check if UUID doesnt already exist
        else
        {
            Database.GetDatabase().sheeps.Add(sheep);
            var obj = sheepDataViewer.CreateNewButton(sheep);
            sheepDataViewer.MoveScrollViewToElement(obj.GetComponent<RectTransform>());
        }

        sheepDataViewer.panelMode = DetailsPanelMode.None;
    }

    public void UpdateLotData(LotObject Lot)
    {
        int nChilds = sheepDataViewer.buttonContainer.childCount;

        // editing existing Lot
        if (LotDataViewer.panelMode == DetailsPanelMode.EditingElement)
        {
            // update the actual data
            LotObject wds = GetLotObjectByUUID(LotDataViewer.selectedElement.UUID);
            
            if (wds != null)
            {
                wds.perceelName = Lot.perceelName;
                wds.surfaceQuality = Lot.surfaceQuality;
                wds.surfaceSqrMtr = Lot.surfaceSqrMtr;
            }

            // update the visuals representing the data
            for (int i = 0; i < nChilds; i++)
            {
                var obj = LotDataViewer.LotButtonContainer.GetChild(i).gameObject.GetComponentInChildren<LotButton>();
                if (obj.Lot.UUID != LotDataViewer.selectedElement.UUID) continue;
                obj.SetInfo(Lot, LotDataViewer);
                break;
            }
        }

        // Adding a new Lot
        // TODO check if UUID doesnt already exist
        else
        {
            Database.GetDatabase().Lots.Add(Lot);
            LotDataViewer.CreateNewButton(Lot);
        }

        LotDataViewer.panelMode = DetailsPanelMode.None;
    }

    private bool DeleteElement<T>(T element, List<T> list, DataViewer dataViewer = null) where T : ObjectUUID
    {
        int index = -1;

        for (int i = 0; i < list.Count; i++)
        {
            var tmp = list[i];
            if (tmp.UUID.Trim() != element.UUID.Trim()) continue;
            index = i;
            break;
        }

        if (index == -1) return false;
        list.RemoveAt(index);

        // Make the dataviewer remove the button associated with the object
        if (dataViewer != null) dataViewer.RemoveButton(element);
        return true;
    }

    public void DeleteSheep(SheepObject sheep)
    {
        DeleteElement(sheep, Database.GetDatabase().sheeps, sheepDataViewer);
    }

    public void DeleteKoppel(PairCollection koppel)
    {
        if (DeleteElement(koppel, Database.GetDatabase().pairCollection, PairCollectionDataViewer))
        {
            // Set koppelID to "" for all sheeps using this koppel
            foreach (var sheep in Database.GetDatabase().sheeps.Where(sheep => string.Equals(sheep.pairCollectionID, koppel.UUID, StringComparison.CurrentCultureIgnoreCase)))
            {
                sheep.pairCollectionID = "";
            }
        }
    }

    public void DeleteButtonClicked(ObjectUUID obj)
    {
        bConfirmToDelete = bDeleteToggle.isOn;

        UnityEngine.Events.UnityAction sheepDeleteEvent = delegate { DeleteSheep(obj as SheepObject); };
        UnityEngine.Events.UnityAction LotDeleteEvent = delegate { DeleteLot(obj as LotObject); };
        UnityEngine.Events.UnityAction sheepKoppelDeleteEvent = delegate { DeleteKoppel(obj as PairCollection); };

        if (bConfirmToDelete)
        {
            pnlDelete.SetActive(true);
            btnDeleteConfirm.onClick.RemoveAllListeners();

            switch (obj)
            {
                case SheepObject sheepObject:
                    btnDeleteConfirm.onClick.AddListener(sheepDeleteEvent);
                    break;
                case LotObject LotObject:
                    btnDeleteConfirm.onClick.AddListener(LotDeleteEvent);
                    break;
                case PairCollection sheepKoppel:
                    btnDeleteConfirm.onClick.AddListener(sheepKoppelDeleteEvent);
                    break;
            }

            btnDeleteConfirm.onClick.AddListener(delegate { pnlDelete.SetActive(false); });
        }

        else
        {
            switch (obj)
            {
                case SheepObject sheepObject:
                    sheepDeleteEvent.Invoke();
                    break;
                case LotObject LotObject:
                    LotDeleteEvent.Invoke();
                    break;
                case PairCollection sheepKoppel:
                    sheepKoppelDeleteEvent.Invoke();
                    break;
            }
        }
    }

    public void DeleteLot(LotObject Lot)
    {
        DeleteElement(Lot, Database.GetDatabase().Lots, LotDataViewer);
    }

    /// <summary>
    /// Loads data from a file into the database. The extension must be capitalized and contained in the filename if it is not a JSON file.
    /// </summary>
    /// /// <param name="inputFile"></param>
    private void LoadSheepData(TextAsset inputFile)
    {
        /*
        if (inputFile == null)
        {
            Debug.LogError("Sheep input data file missing!");
            return;
        }

        if (inputFile.name.Contains("CSV"))
        {
            LoadSheepDataFromCsvFile(inputFile);
        }

        // Assume it is a JSON file if no capitalized extension is present in the filename
        else
        {
            LoadSheepDataFromJsonFile(inputFile);
        }
        */
    }

    /*
    /// <summary>
    /// Loads sheep data from a JSON file into the database.
    /// </summary>
    /// <param name="inputFile"></param>
    private void LoadSheepDataFromJsonFile(TextAsset inputFile)
    {
        Database.GetDatabase() = JsonUtility.FromJson<TemporalDatabaseData>(inputFile.text);
        
        // Assumes the timestamp is in nanoseconds and converts it to seconds
        foreach (var s in Database.GetDatabase().sheeps)
        {
            s.tsBorn /= 1000000000;
        }
    }

    */

    /// <summary>
    /// Loads sheep data from a CSV file into the database.
    /// </summary>
    /// <param name="inputFile"></param>
    private void LoadSheepDataFromCsvFile(TextAsset inputFile)
    {
        Database.GetDatabase().sheeps = WurmFileHandler.GetDataFromCsvFile<SheepObject>(inputFile);
    }
}
