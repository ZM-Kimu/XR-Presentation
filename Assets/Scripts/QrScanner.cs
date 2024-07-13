using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;



[System.Serializable]
public class ServerResponse
{
    public bool isChanged;
    public string[] result;
    public double time = -1;
}



[System.Serializable]
public class RequestData
{
    public ServerResponse lastData;
}


public class QrScanner : MonoBehaviour
{
    public string url = "http://10.1.86.50:5000/result";
    public GameObject[] modelPrefabs; // 在Inspector中添加所有的Prefab引用
    public Transform hands;

    private ServerResponse lastData;
    private void Start()
    {
        StartCoroutine(RequestDataEveryInterval(0.1f));
    }

    IEnumerator RequestDataEveryInterval(float interval)
    {
        while (true)
        {
            yield return GetDataFromServer(url);
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator GetDataFromServer(string url)
    {
        UnityWebRequest request = new(url, "POST");
        RequestData requestData = new() { lastData = lastData };
        string jsonRequest = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
            ProcessServerData(request.downloadHandler.text);
        else
            Debug.LogError("Error: " + request.error);
    }

    void ProcessServerData(string jsonData)
    {
        var data = JsonUtility.FromJson<ServerResponse>(jsonData);
        lastData = data;
        if (data.isChanged)
            foreach (var model in data.result)
                InstantiateModel(model);
    }

    void InstantiateModel(string model)
    {
        GameObject modelPrefab = FindModelByName(model);
        if (modelPrefab != null)
        {
            GameObject instantiatedModel = Instantiate(modelPrefab);
            Vector3 vector = hands.transform.position;
            vector.z += 0.5f;
            instantiatedModel.transform.position = vector;
            instantiatedModel.SetActive(true);
        }
    }

    GameObject FindModelByName(string modelName)
    {
        foreach (var prefab in modelPrefabs)
            if (prefab.name == modelName)
                return prefab;
        Debug.LogWarning("Model not found: " + modelName);
        return null;
    }
}
