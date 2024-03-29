using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class DataTester : MonoBehaviour
{
    // This should be replaced with a GET from the database
    private List<PairCollection> pairCollections = new List<PairCollection>();

    void Start()
    {
        // populate file
        InitPairCollections(5);
        PopulateSheeps(250);
        CreatePairCollections();
        PopulateWorms(5);
        PopulateSurfaces(20);
    }

    /// <summary>
    ///     Generate dummy data for sheep
    /// </summary>
    /// <param name="quantity">
    ///     How many sheeps do we need?
    /// </param>
    public void PopulateSheeps(int quantity)
    {
        // quantity shouldnt be bigger then 99999
        //NL-101631-6-07204
        string SheepBaseTag = "NL-101631-6-";
        List<int> sheepTagList = new List<int>();
        
        for (int i = 0; i < quantity; i++)
        {
            sheepTagList.Add(i);
        }

        System.Random rng = new System.Random();
        var shuffledList = sheepTagList.OrderBy(a => rng.Next()).ToList();

        for (int i = 0; i < quantity; i++)
        {
            SheepObject newSheep = new SheepObject();

            string sheepTagString = SheepBaseTag;
            string sheepTag = shuffledList[i].ToString();

            // Put 0s before the number if it is small
            for (int j = sheepTag.Length; j < 5; j++)
            {
                sheepTagString += "0";
            }

            sheepTagString += sheepTag;
            newSheep.UUID = Helpers.GenerateUUID();
            newSheep.sheepTag = sheepTagString;
            newSheep.tsBorn = Helpers.GetCurrentTimestamp() - Random.Range(15000, 20000);
            newSheep.sex = (Sex)Random.Range(0, Enum.GetNames(typeof(Sex)).Length);
            newSheep.sheepType = (SheepType)Random.Range(0, Enum.GetNames(typeof(SheepType)).Length - 1); //-1 to exclude 'other'

            for (int j = 0; j < Random.Range(5, 10); j++)
            {
                SheepWeight weight = new SheepWeight();
                weight.weight = Random.Range(10, 100);
                weight.timestamp = Helpers.GetCurrentTimestamp() - Random.Range(100, 10000);
                newSheep.weight.Add(weight);
            }

            var pairCollection = pairCollections[Random.Range(0, pairCollections.Count)];
            newSheep.pairCollectionID = pairCollection.UUID;
            pairCollection.allSheep.Add(newSheep.UUID);
            string[] response = WurmAPI.MethodHandler(MethodType.Post, newSheep);
            Debug.Log(Helpers.CodeToMessage(response[0]));
        }
    }

    /// <summary>
    ///     Generate dummy data for worms
    /// </summary>
    /// <param name="quantity">
    ///     How many sheeps do we need?
    /// </param>
    public void PopulateWorms(int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            WormObject newWorm = new WormObject();
            newWorm.UUID = Helpers.GenerateUUID();
            newWorm.wormType = (WormType)Random.Range(0, Enum.GetNames(typeof(WormType)).Length);

            string nonScienceName = "worm";
            Dictionaries.wormNonScienceNames.TryGetValue(newWorm.wormType, out nonScienceName);
            newWorm.nonScienceName = nonScienceName;

            string[] response = WurmAPI.MethodHandler(MethodType.Post, newWorm);
            Debug.Log(Helpers.CodeToMessage(response[0]));
        }
    }

    /// <summary>
    ///     Generate dummy data for Lots
    /// </summary>
    /// <param name="quantity">
    ///     How many sheeps do we need?
    /// </param>
    public void PopulateSurfaces(int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            LotObject newLot = new LotObject();
            newLot.UUID = Helpers.GenerateUUID();
            newLot.perceelName = $"Perceel {i}";
            newLot.surfaceSqrMtr = Random.Range(50, 500);
            newLot.surfaceQuality = Random.Range(10, 95);

            string[] response = WurmAPI.MethodHandler(MethodType.Post, newLot);
            Debug.Log(Helpers.CodeToMessage(response[0]));
        }
    }

    /// <summary>
    ///     Make sheep groups
    /// </summary>
    public void InitPairCollections(int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            PairCollection newPair = new PairCollection()
            {
                pairCollectionName = $"Koppel {i + 1}"
            };
            newPair.UUID = Helpers.GenerateUUID();
            pairCollections.Add(newPair);
        }
    }

    public void CreatePairCollections()
    {
        foreach (var newPair in pairCollections)
        {
            string[] response = WurmAPI.MethodHandler(MethodType.Post, newPair);
            Debug.Log(Helpers.CodeToMessage(response[0]));
        }
    }

    /// <summary>
    ///     Test sample of doing a request for an entire database
    /// </summary>
    /// <param name="type">
    ///     Requires the method type such as put for example
    /// </param>
    /// <param name="tempDatabase">
    ///     Requires the database in order to function properly
    /// </param>
    /// <returns>
    ///     Returns a dictionary with the http messages (unmapped) and the returned data as a complete collection, results have to be handled seperately
    /// </returns>
    public Dictionary<List<string>,List<string>> ProgressTestData(MethodType type, TemporalDatabaseData tempDatabase)
    {
        List<string> httpsMessages = new List<string>();
        List<string> data = new List<string>();

        // iterate through all Lots from database
        foreach (LotObject newObj in tempDatabase.Lots)
        {
            string[] response = WurmAPI.MethodHandler(type, newObj);
            httpsMessages.Add(response[0]);
            data.Add(response[1]);
        }

        // iterate through all sheeps from database
        foreach (SheepObject newObj in tempDatabase.sheeps)
        {
            string[] response = WurmAPI.MethodHandler(type, newObj);
            httpsMessages.Add(response[0]);
            data.Add(response[1]);
        }

        // iterate through all sheep koppels from database
        foreach (PairCollection newObj in tempDatabase.pairCollection)
        {
            string[] response = WurmAPI.MethodHandler(type, newObj);
            httpsMessages.Add(response[0]);
            data.Add(response[1]);
        }

        // iterate through all worms from database
        foreach (WormObject newObj in tempDatabase.worms)
        {
            string[] response = WurmAPI.MethodHandler(type, newObj);
            httpsMessages.Add(response[0]);
            data.Add(response[1]);
        }

        Dictionary<List<string>, List<string>> results = new Dictionary<List<string>, List<string>>();
        results.Add(httpsMessages, data);

        return results;
    }
}
