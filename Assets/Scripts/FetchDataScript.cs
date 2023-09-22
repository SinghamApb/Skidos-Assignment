using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;


public class FetchDataScript : MonoBehaviour
{

    private const string apiUrl = "https://testinterest.s3.amazonaws.com/interest.json";
    public TMP_Text statusText;
    public GameObject dataDisplayPrefab;
    public Button fetchBtn;
    public Transform dataDisplayParent;
    public List<Interest> interests;

    public void Awake()
    {
        interests = LoadData();
      
    }


    
    public void FetchData()
    {
        StartCoroutine(GetRequest(apiUrl));
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("nhi hora");
                Debug.LogError(webRequest.error);
            }
            else
            {
               
                string jsonData = webRequest.downloadHandler.text;
                interests = JsonConvert.DeserializeObject<List<Interest>>(jsonData);
                Debug.Log("Response code: " + webRequest.responseCode);
                Debug.Log("Downloaded text: " + webRequest.downloadHandler.text);
                statusText.text = "Data fetched and saved!";
                SaveData(interests);
                fetchBtn.interactable = false;
            }

        }
    }
      

    public void ShowFetchedData()
    {
       
        foreach (Transform child in dataDisplayParent)
        {
            Destroy(child.gameObject);
        }

        
        if (interests != null)
        {
            Debug.Log("Object Intanstiated");
            foreach (var interest in interests)
            {
                GameObject dataDisplay = Instantiate(dataDisplayPrefab, dataDisplayParent);
                dataDisplay.transform.Find("DisplayName").GetComponent<TMP_Text>().text = "Name: " + interest.DisplayName;
                dataDisplay.transform.Find("Language").GetComponent<TMP_Text>().text = "Language: " + interest.Language;
                dataDisplay.transform.Find("InterestID").GetComponent<TMP_Text>().text = "ID: " + interest.InterestID.ToString();
                
           
                LoadImage(interest.PictureUrl, dataDisplay.transform.Find("Image").GetComponent<RawImage>());
            }
        }
        else
        {
            fetchBtn.interactable = true;
        }
    }

   
    private void LoadImage(string url, RawImage image)
    {

        StartCoroutine(LoadImageCoroutine(url, image));
    }

    private IEnumerator LoadImageCoroutine(string url, RawImage image)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            image.texture = texture;
        }
        else
        {
            Debug.LogError("Failed to load image: " + www.error);
        }
    }

    // Save and load Data
    private void SaveData(List<Interest> data)
    {
        SerializableData serializableData = new SerializableData { interestsSaves = data };

       
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream("data.bin", FileMode.Create))
        {
            
            formatter.Serialize(fileStream, serializableData);
        }

        Debug.Log("Data serialized and saved.");
    }

    private List<Interest> LoadData()
    {
        if (File.Exists("data.bin"))
        {
           
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream("data.bin", FileMode.Open))
            {
           
                SerializableData serializableData = (SerializableData)formatter.Deserialize(fileStream);
                return serializableData.interestsSaves;
            }
        }

        else
        {
            Debug.LogWarning("No saved data found.");
            return new List<Interest>();
        }
    }

    public void DeleteSave()
    {
        DeleteSavedData();

    }

    private void DeleteSavedData()
    {

        interests.Clear();

        foreach (Transform child in dataDisplayParent)
        {
            Destroy(child.gameObject);
        }

        if (File.Exists("data.bin"))
        {
            File.Delete("data.bin");
            Debug.Log("Saved data deleted.");
        }
        else
        {
            Debug.LogWarning("No saved data found to delete.");
        }
        fetchBtn.interactable = true;
    }
}


[System.Serializable]
public class Interest
{
    public string Name { get; set; }
    public string PictureUrl { get; set; }
    public string DisplayName { get; set; }
    public string Language { get; set; }
    public int InterestID { get; set; }
}
[System.Serializable]
public class SerializableData
{
    public List<Interest> interestsSaves;
}

