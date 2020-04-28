using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{

    //Drzewko stanów oparte na Enumach 
    public enum Task
    {
        idle, move, follow, chase, attack

    }

    //stała zmienna
    const string Animator_Speed = "Speed";
    const string Animator_Shoot = "Shoot";
    const string Animator_Alive = "Alive";

    //Tworzymy statyczną listę wszystkich obiektów, które są Iselectable
    public static List<ISelectable> SelectableUnits { get { return selectableUnits; } }
    static List<ISelectable> selectableUnits = new List<ISelectable>();

    public Transform target;

    public float HealthPercent { get { return hp / hpmax; } }
    public bool IsAlive { get { return hp > 0; } }


    Animator animator;
    //Zmienna przechowująca nasze enumy

    [SerializeField]
    float hp, hpmax = 100;
    //Chcemy aby Unit sam stworzył sobie pasek życia
    [SerializeField]
    GameObject hpBarPrefab;
    [SerializeField]
    float stoppingDistance = 1;

    protected HealthBar healthBar;
    protected Task task = Task.idle;
    protected NavMeshAgent nav;


    //Funkcja uaktywniająca się w momencie stworzenia obiektu
    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        hp = hpmax;
        //Tworzymy obiekt oraz od razu zapamiętujemy jego referencje    
        healthBar = Instantiate(hpBarPrefab, transform).GetComponent<HealthBar>();
    }

    private void Start()
    {
        if (this is ISelectable)
        {
            selectableUnits.Add(this as ISelectable);
            (this as ISelectable).SetSelected(false);
        }
    }

    private void OnDestroy()
    {
        if (this is ISelectable)
        {
            selectableUnits.Remove(this as ISelectable);
        }
    }

    void Update()
    {
        if (IsAlive)
            switch (task)
            {
                case Task.idle:Idling(); break;
                case Task.move:Moving(); break;
                case Task.follow:Following(); break;
                case Task.chase:Chasing(); break;
                case Task.attack:Attacking(); break;
                //default: break;
            }
        Animate();

    }

    protected virtual void Idling() 
    {
        nav.velocity = Vector3.zero;

    }
    protected virtual void Attacking() 
    {
        nav.velocity = Vector3.zero;

    }
    protected virtual void Moving() 
    {
        float distance = Vector3.Magnitude(nav.destination - transform.position);
        if (distance <= stoppingDistance)
        {
            task = Task.idle;
        }
        
    }
    protected virtual void Following() 
    {
        if (target)
        {
            nav.SetDestination(target.position);
        }
        else
        {
            task = Task.idle;
        }
    }
    protected virtual void Chasing() 
    {
       //todo
    }



    //rozszerzona funkcja z dodawaniem nowych możliośći -> nadawania wartości dla animatora
    protected virtual void Animate()
    {
        var speedVector = nav.velocity;
        //Zerujemy zmienna Y, ponieważ nie bedziemy sie przemieszczać w górę 
        speedVector.y = 0;
        float speed = speedVector.magnitude;

        animator.SetFloat(Animator_Speed, speed);

        animator.SetBool(Animator_Alive, IsAlive);

    }


}
