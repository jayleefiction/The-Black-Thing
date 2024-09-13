using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectPool
{
    //메모리 활성화 비활성화를 의미하는 의미하는 클래스 생성
    public class PoolItem
    {
        public PoolItem() { }
        public PoolItem(bool isActive, GameObject gameObject)
        {
            _isActive = isActive;
            _gameObject = gameObject;
        }
        public bool _isActive;
        public GameObject _gameObject;
    }

    private Dictionary<string, PoolItem> _memory = new Dictionary<string, PoolItem>();

    public IEnumerator LoadFromMemoryAsync(string path, System.Action<AssetBundle> callback)
    {

        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            byte[] binary = System.IO.File.ReadAllBytes(path);

            AssetBundleCreateRequest req = AssetBundle.LoadFromMemoryAsync(binary);

            yield return req;

            callback(req.assetBundle);
        }

    }
    //검색 기능
    public GameObject SearchMemory(string objectName)
    {
        if (_memory.ContainsKey(objectName))
        {
            return _memory[objectName]._gameObject;
        }

        return null; //없음 
    }

    public Dictionary<string, PoolItem> GetMemory()
    {
        return _memory;
    }

    public List<GameObject> GetValues()
    {
        List<GameObject> values = new List<GameObject>();

        foreach (PoolItem value in _memory.Values)
        {
            values.Add(value._gameObject);
        }

        return values;
    }
    public bool InsertMemory(GameObject gameObject)
    {
        if (_memory.ContainsKey(gameObject.name))
        {
            return false; //내용물이 있어서 실패
        }

        _memory.Add(gameObject.name, new PoolItem(gameObject.activeSelf, gameObject));
        return true;
    }

    public void SetActiveObject(string objectName)
    {
        if (_memory.ContainsKey(objectName) && _memory[objectName]._isActive == false)
        {
            _memory[objectName]._gameObject.SetActive(true);
            _memory[objectName]._isActive = true;
        }
    }
    public void DeactivateObject(string objectName)
    {
        if (_memory.ContainsKey(objectName) && _memory[objectName]._isActive == true)
        {
            _memory[objectName]._isActive = false;
            _memory[objectName]._gameObject.SetActive(_memory[objectName]._isActive);
        }
    }
    //재사용 모든 딕셔너리 삭제 될 때
    public void DestroyObjects()
    {
        foreach (KeyValuePair<string, PoolItem> item in _memory)
        {
            GameObject.Destroy(item.Value._gameObject);
        }

        _memory.Clear();
    }
}
