using Assets.Script.TimeEnum;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static ObjectPool;

public class ObjectManager : MonoBehaviour
{
    //모든 상태가 오브젝트들을 공유한다.
    private ObjectPool pool;

    List<GameObject>  watches;


    [SerializeField]
    GameObject skipSleep;

    Dictionary<string, GameObject> mains;

    public delegate void ActiveSystemUIDelegate(bool InActive);

    public ActiveSystemUIDelegate activeSystemUIDelegate;

    bool isObjectLoadComplete;
    float loadProgress;

    public ObjectManager()
    {
        pool = new ObjectPool();
        mains = new Dictionary<string, GameObject>();
        watches = new List<GameObject>();
    }

    public void InitMainBackground(string InPath)
    {
        string path = "";
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            path = Application.dataPath + "/Raw/"+InPath;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            path = Path.Combine(Application.streamingAssetsPath, InPath);
        }
        else
        {
            path = Application.dataPath + "/StreamingAssets"+ InPath;
        }

        Action<AssetBundle> callback = LoadMainBackground;

        StartCoroutine(pool.LoadFromMemoryAsync(path, callback));

    }

    public GameObject SetMain(string background)
    {

        if(mains.ContainsKey(background))
        {

            foreach (var w in mains)
            {
                w.Value.SetActive(false);
            }

            mains[background].SetActive(true);

            return mains[background];
        }

        return null;
    }

    // 로드 완료 여부를 반환
    public bool IsLoadObjectComplete()
    {
        return isObjectLoadComplete;
    }

    // 현재 로드 진행 상황을 반환
    public float GetLoadProgress()
    {
        return loadProgress;
    }

    private void LoadMainBackground(AssetBundle bundle)
    {

        if(bundle != null)
        {
            GameObject[] prefab = bundle.LoadAllAssets<GameObject>();
            

            foreach(GameObject pf in prefab)
            {
                GameObject realObj = Instantiate(pf,this.transform);

                string name = realObj.name.Substring(0, realObj.name.IndexOf("(")); 
                realObj.name = name; //(clone)을 찾아냄.
                mains.Add(name,realObj);
                realObj.SetActive(false);
            }

            bundle.Unload(false);
        }
    }

    // 비동기 로드를 위한 코루틴
    public IEnumerator LoadObjectAsync(string path, int chapter)
    {
        isObjectLoadComplete = false;  // 로드가 시작되므로 false로 설정
        loadProgress = 0f;  // 진행 상황 초기화

        // 동기적으로 경로에서 모든 리소스를 먼저 가져옵니다. 
        // 이것은 경로에 어떤 오브젝트가 있는지 확인하는 단계일 뿐, 아직 오브젝트를 로드하지 않음.
        System.Object[] allObjects = Resources.LoadAll(path, typeof(GameObject));
        int totalObjects = allObjects.Length;

        int i = 0;

        foreach (GameObject obj in allObjects)
        {
            // 각 오브젝트를 비동기적으로 로드
            ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(path + "/" + obj.name);
            
            while (!resourceRequest.isDone)
            {
                loadProgress = (i + resourceRequest.progress) / totalObjects;  // 진행률 업데이트
                yield return null;
            }

            if (resourceRequest.asset != null)
            {
                GameObject obj2 = resourceRequest.asset as GameObject;

                // Instantiate를 통해 오브젝트 생성 후 삽입
                GameObject newObj = Instantiate(obj2, this.transform);

                // "(Clone)" 제거
                string name = newObj.name.Substring(0, newObj.name.IndexOf("("));
                newObj.name = name;

                // InsertMemory 내 삽입
                pool.InsertMemory(newObj);
            }
            
            i++;

        }

        SettingChapter(chapter);
        yield return new WaitForSeconds(1f);
        isObjectLoadComplete = true;
    }

    public void LoadObject(string path, int chapter)
    {
        GameObject[] obj = Resources.LoadAll<GameObject>(path);
        foreach (GameObject obj2 in obj)
        {
            //Instantiate를 통해서 InsertMemory내 삽입
            GameObject newObj = Instantiate(obj2, this.transform);
            string name = newObj.name.Substring(0, newObj.name.IndexOf("("));
            newObj.name = name; //(clone)을 찾아냄.
            //newObj의 clone을 제거 
            pool.InsertMemory(newObj);
        }
    }

    //한 챕터를 넘겼을 때 호출되는 함수, 즉 Phase Watching일 때 호출한다. 
    public void SettingChapter(int chapter)
    {

        //모든 Obj를 가져와서 검사해야한다.
        List<GameObject> values = pool.GetValues();

        foreach (GameObject value in values)
        {
            if (value.GetComponent<BaseObject>().IsCurrentChapter(chapter))
            {
                value.SetActive(true);
            }
            else
            {
                value.SetActive(false);
            }
        }
    }

    public IWatchingInterface GetWatchingObject(EWatching type)
    {
        IWatchingInterface search = null;
        List<GameObject> values;
        if (watches.Count == 0)
        {
            //모든 Obj를 가져와서 검사해야한다.
            values = pool.GetValues();
        }
        else
        {
            values = watches;
        }

        foreach (GameObject value in values)
        {
            IWatchingInterface watching = value.GetComponent<IWatchingInterface>();

            if (watching != null)
            {
                if (watching.IsCurrentPattern(type))
                {
                    search = watching;
                }

                if(watches.Count < 2)
                {
                    Debug.Log(watching.ToString());
                    watches.Add(value);
                }
            }

        }

        return search;
    }

    public ISleepingInterface GetSleepingObject()
    {
        List<GameObject> values = pool.GetValues();

        foreach (GameObject value in values)
        {
            ISleepingInterface search = value.GetComponent<ISleepingInterface>();

            if (search != null)
            {
                return search;
            }
        
        }

        return null;
    }

    public void PlayThinking()
    {
        GameObject bookPile=pool.SearchMemory("phase_bookpile");

        if(bookPile)
        {
            bookPile.SetActive(false);
        }
    }
    public void Translate(LANGUAGE language)
    {
        Debug.Log("게임 오브젝트 번역합니다.\n");
    }

    public void SkipSleeping(bool isActive)
    {
        skipSleep.SetActive(isActive);
    }

}
