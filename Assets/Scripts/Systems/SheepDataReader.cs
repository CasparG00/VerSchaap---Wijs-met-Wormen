using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public class SheepDataReader : MonoBehaviour
{
    public TextAsset sheepDataFile;
    public List<SheepObject> SheepDatabase = new List<SheepObject>();
    public SheepDataViewer sheepDataViewer;

    // dummy var to test in the editor
    public bool writeToFile;

    private void Start()
    {
        sheepDataViewer.sheepDataReader = this;
        LoadSheepData(sheepDataFile);
        sheepDataViewer.CreateSheepButtonsFromDB(SheepDatabase);

        Database.InitializeDatabase();
        Debug.Log("putdb");

        foreach (var s in SheepDatabase)
        {
            Database.ProgressData(MethodType.Put, s);
        }

        TemporalDatabaseData newDatabase = new TemporalDatabaseData();
        SheepObject newSheepObject = new SheepObject();
        var response = Database.ProgressData(MethodType.Get, newSheepObject);

        Debug.Log("getdb response");

        foreach (var r in response)
        {
            Debug.Log(r);
        }

    }

    public void UpdateSheepData(SheepObject sheep)
    {
        int nChilds = sheepDataViewer.sheepUIPanel.childCount;

        //TODO do something different if adding a new sheep
        // editing existing sheep
        if (!sheepDataViewer.bAddingSheep)
        {
            // update the actual data
            // magic, ignore casing and check if names are the same
            foreach (var shp in SheepDatabase.Where(shp => string.Equals(shp.UUID, sheepDataViewer.selectedSheep.UUID, StringComparison.CurrentCultureIgnoreCase)))
            {
                shp.UUID = sheep.UUID;
                shp.sex = sheep.sex;
                shp.sheepType = sheep.sheepType;
                shp.tsBorn = sheep.tsBorn;
            }

            // update the visuals representing the data
            for (int i = 0; i < nChilds; i++)
            {
                var obj = sheepDataViewer.sheepUIPanel.GetChild(i).gameObject.GetComponentInChildren<SheepButton>();
                if (obj.sheep.UUID != sheepDataViewer.selectedSheep.UUID) continue;
                obj.SetInfo(sheep, sheepDataViewer);
                break;
            }
        }

        // TODO check if UUID doesnt already exist
        else
        {
            SheepDatabase.Add(sheep);
            sheepDataViewer.CreateNewSheepButton(sheep);
        }

        sheepDataViewer.bAddingSheep = false;
    }

    public void DeleteSheep(SheepObject sheep)
    {
        int index = -1;
        
        for (int i = 0; i < SheepDatabase.Count; i++)
        {
            var shp = SheepDatabase[i];
            if (shp.UUID.Trim() != sheep.UUID.Trim()) continue;
            index = i;
            break;
        }

        if (index == -1) return;
        Destroy(sheepDataViewer.sheepUIPanel.GetChild(index).gameObject);
        SheepDatabase.RemoveAt(index);
    }

    private void Update()
    {
        if (writeToFile)
        {
            writeToFile = false;
            var sheepDB = SheepDatabase.ToArray();
            //Debug.Log("sheepdblength = " + sheepDB.Length);
            WurmFileHandler.WriteDataToCsvFile("TESTSHEEPDATABASE", sheepDB, false);
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
        //allSheep = JsonUtility.FromJson<SheepArray>(inputFile.text);
    }

    /// <summary>
    /// Loads sheep data from a CSV file into the database.
    /// </summary>
    /// <param name="inputFile"></param>
    private void LoadSheepDataFromCsvFile(TextAsset inputFile)
    {
        SheepDatabase = WurmFileHandler.GetDataFromCsvFile<SheepObject>(inputFile);
    }
}
