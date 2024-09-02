using Assets.Script.TimeEnum;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class DotController : MonoBehaviour
{

    private DotState currentState; //현재 상태
    private Dictionary<DotPatternState, DotState> states;
    private float position;
    private string dotExpression; //CSV에 의해서 string 들어옴
    private string animKey; //CSV에 의해서 string으로 들어옴 파싱 해줘야한다.

    [SerializeField] 
    GameObject mainAlert;

    [SerializeField]
    private int chapter;

    [SerializeField]
    private GameManager manager;

    [SerializeField]
    private Animator animator;

    public Animator Animator
    { get { return animator; } }

    public int Chapter
    {
        get { return chapter; }
        set { chapter = value; }
    }

    public float Position
    {
        get { return position; }
        set { position = value; }
    }

    public string AnimKey
    {
        get { return animKey; }
        set { animKey = value; }
    }

    public string DotExpression
    {
        get { return dotExpression; }
        set { dotExpression = value; }
    }

    void Awake()
    {

        animator = GetComponent<Animator>();

        Position = -1;
        dotExpression = "";

        states = new Dictionary<DotPatternState, DotState>();
        states.Clear();
        states.Add(DotPatternState.Defualt, new Idle());
        states.Add(DotPatternState.Phase, new Phase());
        states.Add(DotPatternState.Main, new Main());
        states.Add(DotPatternState.Sub, new Sub());

    }
    void Start()
    {
        chapter = manager.Chapter;
    }

    private void OnMouseDown()
    {

        if (mainAlert.activeSelf)
        {
            mainAlert.SetActive(false);

            //main 배경화면을 트리거한다.
            manager.StartMain();
        }
    }
    public void TriggerMain(bool isActive)
    {
        mainAlert.SetActive(isActive);
        /*여기서 OnClick 함수도 연결해준다.*/
        //OutPos 가 있다면 해당 Position으로 바껴야함.
    }
    public void ChangeState(DotPatternState state = DotPatternState.Defualt, string OutAnimKey = "", float OutPos = -1, string OutExpression = "")
    {
        if (states == null) return;

        if (states.ContainsKey(state) == null)
        {
            return;
        }

        if (currentState != null)
        {
            currentState.Exit(this); //이전 값을 나가주면서, 값을 초기화 시킨다.
        }

        /*Main으로 넘어가기 전에 anim_default가 뜬다.*/

        animator.SetInteger("DotState", (int)state); //현재 상태를 변경해준다.
        position = OutPos; //이전 위치를 초기화함, 그렇게 하면 모든 상태로 입장했을 때 -1이 아니여서 랜덤으로 뽑지않는다.
        dotExpression = OutExpression; //Update, Main에서만 사용하기 때문에 다른 곳에서는 사용하지 않음.
        animKey = OutAnimKey;
        chapter = manager.Chapter;
        //OutPos 가 있다면 해당 Position으로 바껴야함.
        currentState = states[state];
        currentState.Enter(this); //실행
    }
}
