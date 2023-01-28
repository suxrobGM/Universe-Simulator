using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlanetLinePath))]
public class GravityObject : MonoBehaviour
{
    public bool DemoPlanet = false;
    public float Mass = 10f;
    public float Radius = 5f;
    public string PlanetName;
    private GravityObjectsController Controller;
    private Rigidbody2D _rigidbody2D;
    private List<GravityObject> listHolder;

    // if player starts drag during pause, there will be auto pause
    private bool tempPause = false;
    private float dragTime = 0;

    public Vector2 InitialVelocity;
    public Vector2 StartPos;

    private Vector2 moveVector;

    public GameObject NameHolder;
    
    private float GravitionalConstantValue = 0.06674f;

    public Vector2 CurrentGravityForceVector;
    
    // Start is called before the first frame update
    void Start()
    {
        if(DemoPlanet)
            UpdatePlanet();
    }
    public void UpdatePlanet()
    {
        transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
        StartPos = transform.position;

        Controller = GravityObjectsController.Instance;
        if(!Controller.AllGravityObjects.Contains(this))
            Controller.AddGravityObject(this);
        
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.mass = Mass;
        _rigidbody2D.velocity = InitialVelocity;
        StartPos = transform.position;

        this.name = PlanetName;
        if (NameHolder == null)
            CreatePlanetNameHolder();
        else
            NameHolder.name = PlanetName;
    }

    void CreatePlanetNameHolder()
    {
        NameHolder = new GameObject(PlanetName);
        NameHolder.AddComponent<PlanetNameHolder>().PlanetController = this;
        Text text = NameHolder.AddComponent<Text>();
        text.text = PlanetName;
        text.color = Color.white;
        text.font = GlobalVariables.Instance.GlobalFont;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.BoldAndItalic;

        NameHolder.transform.SetParent(GameObject.Find("PlanetsNames").transform);
        NameHolder.transform.localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0)
        {
            CurrentGravityForceVector = Vector2.zero;
            foreach (var obj in listHolder)
                ApplyAndCalculateForce(Vector2.Distance(transform.position, obj.transform.position), obj.Mass,
                    transform.position - obj.transform.position);
            _rigidbody2D.AddForce(CurrentGravityForceVector, ForceMode2D.Impulse);
        }
    }
    
    private void ApplyAndCalculateForce(float distance, float mass, Vector2 vectorDist)
    {
        float forceValue = GravitionalConstantValue * mass * Mass / (distance * distance);
        float proportionScale = forceValue / distance;

        float xForceValue = -proportionScale * vectorDist.x;
        float yForceValue = -proportionScale * vectorDist.y;

        CurrentGravityForceVector += new Vector2(xForceValue, yForceValue);
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0 && !Controller.RemovingPlanet)
            moveVector = new Vector2(transform.position.x - Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                transform.position.y - Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        
        else if(Time.timeScale == 0 && Controller.RemovingPlanet)
            Controller.RemovePlanet(gameObject);
    }

    private void OnMouseUp()
    {
        if(dragTime < 0.25f)
            GameObject.FindWithTag("EditorController").GetComponent<EditorHandler>().ShowPanel(gameObject);
        
        if(Controller.Reseted)
            StartPos = transform.position;

        if (tempPause && Time.timeScale == 0)
        {
            Controller.PlayPause();
            tempPause = false;
        }

        dragTime = 0;
    }

    private void OnMouseDrag()
    {
        if (Time.timeScale == 0)
        {
            dragTime += Time.unscaledDeltaTime;
            
            if (dragTime >= .25f || !tempPause)
            {
                transform.position =
                    new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x + moveVector.x,
                        Camera.main.ScreenToWorldPoint(Input.mousePosition).y + moveVector.y);

                GameObject currentLineHolder = GetComponent<PlanetLinePath>().GetLine();

                var path = GetComponent<PlanetLinePath>();
                if (path.Lines.Count > 0 && !path.Lines[^1].Finished)
                {
                    Destroy(path.Lines[^1].SegmentHolder);
                    path.Lines.Remove(path.Lines[^1]);
                }

                if (currentLineHolder != null && !currentLineHolder.activeSelf)
                {
                    Destroy(currentLineHolder);
                }
            }
        }
        else
        {
            tempPause = true;
            Controller.PlayPause();
        }
    }

    public void UpdatePrivateList() => listHolder = Controller.GetObjects(this);
}
