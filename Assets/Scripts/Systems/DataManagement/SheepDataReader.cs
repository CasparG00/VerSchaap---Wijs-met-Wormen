using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SheepDataReader : MonoBehaviour
{
    public TextAsset sheepDataFile;
    public SheepDataViewer sheepDataViewer;
    public WormDataViewer wormDataViewer;
    public WeideDataViewer weideDataViewer;
    public KoppelDataViewer koppelDataViewer;
    public TemporalDatabaseData testDatabase;

    public void OpenFileExplorer()
    {
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
    }

    private void Start()
    {
        sheepDataViewer = GetComponent<SheepDataViewer>();
        wormDataViewer = GetComponent<WormDataViewer>();
        weideDataViewer = GetComponent<WeideDataViewer>();
        koppelDataViewer = GetComponent<KoppelDataViewer>();

        //testDatabase = WurmAPI.GetDatabase();
        LoadSheepData(sheepDataFile);

        koppelDataViewer.dataReader = this;
        koppelDataViewer.CreateButtonsFromDB(testDatabase.sheepKoppels);
        wormDataViewer.dataReader = this;
        wormDataViewer.CreateWormButtonsFromDB(testDatabase.worms);
        sheepDataViewer.sheepDataReader = this;
        sheepDataViewer.CreateSheepButtonsFromDB(testDatabase.sheeps);
        weideDataViewer.sheepDataReader = this;
        weideDataViewer.CreateSheepButtonsFromDB(testDatabase.weides);
    }

    public SheepObject GetSheepObjectByUUID(string uuid)
    {
        foreach (var sheep in testDatabase.sheeps)
        {
            if (sheep.UUID == uuid) return sheep;
        }

        return null;
    }

    public string GetKoppelUUIDByName(string name)
    {
        foreach (var koppel in testDatabase.sheepKoppels)
        {
            if (koppel.koppelName == name) return koppel.UUID;
        }

        return null;
    }

    public string GetKoppelNameByUUID(string UUID)
    {
        foreach (var koppel in testDatabase.sheepKoppels)
        {
            if (koppel.UUID == UUID) return koppel.koppelName;
        }

        return null;
    }

    public void UpdateSheepData(SheepObject sheep)
    {
        int nChilds = sheepDataViewer.sheepButtonContainer.childCount;

        // editing existing sheep
        if (!sheepDataViewer.bAddingSheep)
        {
            // update the actual data
            // magic, ignore casing and check if names are the same
            foreach (var shp in testDatabase.sheeps.Where(shp => string.Equals(shp.UUID, sheepDataViewer.selectedSheep.UUID, StringComparison.CurrentCultureIgnoreCase)))
            {
                shp.sheepTag = sheep.sheepTag;
                shp.sex = sheep.sex;
                shp.sheepType = sheep.sheepType;
                shp.tsBorn = sheep.tsBorn;

                var oldKoppelID = shp.sheepKoppelID;
                var newKoppelID = sheep.sheepKoppelID;
                // TODO check if it is a valid koppelid

                if (oldKoppelID != newKoppelID)
                {
                    // Remove the sheep from the old koppel
                    foreach (var kop in testDatabase.sheepKoppels.Where(kop => string.Equals(kop.UUID, oldKoppelID, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        kop.allSheep.Remove(sheep.UUID);
                    }

                    // Add the sheep to the new koppel
                    foreach (var kop in testDatabase.sheepKoppels.Where(kop => string.Equals(kop.UUID, newKoppelID, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        kop.allSheep.Add(shp.UUID);
                    }
                }

                shp.sheepKoppelID = newKoppelID;

                // remove sheep from old koppel and update it
            }

            // update the visuals representing the data
            for (int i = 0; i < nChilds; i++)
            {
                var obj = sheepDataViewer.sheepButtonContainer.GetChild(i).gameObject.GetComponentInChildren<SheepButton>();
                if (obj.sheep.UUID != sheepDataViewer.selectedSheep.UUID) continue;
                obj.SetInfo(sheep, sheepDataViewer);
                break;
            }
        }

        // Adding a new sheep
        // TODO check if UUID doesnt already exist
        else
        {
            testDatabase.sheeps.Add(sheep);
            var obj = sheepDataViewer.CreateNewSheepButton(sheep);
            sheepDataViewer.MoveScrollViewToElement(obj.GetComponent<RectTransform>());
        }

        sheepDataViewer.bAddingSheep = false;
    }

    public void UpdateWeideData(WeideObject weide)
    {
        int nChilds = sheepDataViewer.sheepButtonContainer.childCount;

        // editing existing sheep
        if (!weideDataViewer.bAddingElement)
        {
            // update the actual data
            // magic, ignore casing and check if names are the same
            foreach (var wds in testDatabase.weides.Where(wds => string.Equals(wds.UUID, weideDataViewer.selectedElement.UUID, StringComparison.CurrentCultureIgnoreCase)))
            {
                wds.perceelName = weide.perceelName;
                wds.surfaceQuality = weide.surfaceQuality;
                wds.surfaceSqrMtr = weide.surfaceSqrMtr;
            }

            // update the visuals representing the data
            for (int i = 0; i < nChilds; i++)
            {
                var obj = weideDataViewer.WeideButtonContainer.GetChild(i).gameObject.GetComponentInChildren<WeideButton>();
                if (obj.weide.UUID != weideDataViewer.selectedElement.UUID) continue;
                obj.SetInfo(weide, weideDataViewer);
                break;
            }
        }

        // Adding a new sheep
        // TODO check if UUID doesnt already exist
        else
        {
            testDatabase.weides.Add(weide);
            weideDataViewer.CreateNewWeideButton(weide);
        }

        weideDataViewer.bAddingElement = false;
    }

    private bool DeleteElement<T>(T element, List<T> list) where T : ObjectUUID
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
        return true;
    }

    public void DeleteSheep(SheepObject sheep)
    {
        if (DeleteElement(sheep, testDatabase.sheeps))
        {
            sheepDataViewer.RemoveSheepButton(sheep);
        }
    }

    public void DeleteKoppel(SheepKoppel koppel)
    {
        if (DeleteElement(koppel, testDatabase.sheepKoppels))
        {
            // Set koppelID to "" for all sheeps using this koppel
            foreach (var sheep in testDatabase.sheeps.Where(sheep => string.Equals(sheep.sheepKoppelID, koppel.UUID, StringComparison.CurrentCultureIgnoreCase)))
            {
                sheep.sheepKoppelID = "";
            }

            koppelDataViewer.RemoveKoppelButton(koppel);
        }
    }

    public void DeleteWeide(WeideObject weide)
    {
        if (DeleteElement(weide, testDatabase.weides))
        {
            weideDataViewer.RemoveWeideButton(weide);
        }
    }

    /// <summary>
    /// Loads data from a file into the database. The extension must be capitalized and contained in the filename if it is not a JSON file.
    /// </summary>
    /// /// <param name="inputFile"></param>
    private void LoadSheepData(TextAsset inputFile)
    {
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
    }

    /// <summary>
    /// Loads sheep data from a JSON file into the database.
    /// </summary>
    /// <param name="inputFile"></param>
    private void LoadSheepDataFromJsonFile(TextAsset inputFile)
    {
        testDatabase = JsonUtility.FromJson<TemporalDatabaseData>(inputFile.text);
        
        // Assumes the timestamp is in nanoseconds and converts it to seconds
        foreach (var s in testDatabase.sheeps)
        {
            s.tsBorn /= 1000000000;
        }
    }

    /// <summary>
    /// Loads sheep data from a CSV file into the database.
    /// </summary>
    /// <param name="inputFile"></param>
    private void LoadSheepDataFromCsvFile(TextAsset inputFile)
    {
        testDatabase.sheeps = WurmFileHandler.GetDataFromCsvFile<SheepObject>(inputFile);
    }
}
