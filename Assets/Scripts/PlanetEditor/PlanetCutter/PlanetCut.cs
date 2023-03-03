using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class PlanetCut : MonoBehaviour
{
    public static PlanetCut Instance;
    void Awake() => Instance = this;

    List<GameObject> PlanetsOnLine(Vector2 start, Vector2 end)
    {
        List<GameObject> result = new ();
        foreach (var component in PlanetComponentsController.Instance.AllGravityComponents)
        {
            var spriteBounds = component.Handler.GetComponent<SpriteMask>().bounds;
            Vector2 spriteMin = new Vector2(spriteBounds.min.x, spriteBounds.min.y);
            Vector2 spriteMax = new Vector2(spriteBounds.max.x, spriteBounds.max.y);
            
            if(UniverseLine.Intersect(start, end, spriteMin, spriteMax)) result.Add(component.Handler.gameObject);
        }

        return result;
    }

    public async Task Slice(Vector2 pointA, Vector2 pointB)
    {
        List<GameObject> planetsToCut = PlanetsOnLine(pointA, pointB);
        foreach (var planet in planetsToCut)
        {
            var planetT = planet.transform.parent;
            
            // now we should create two new game objects -> calculate mass of them -> add a bit of force and offset to de-attach them
            var sprite = planet.GetComponent<SpriteMask>().sprite;
            Vector2 planetPos = planet.transform.position;
            float radius = planet.transform.lossyScale.x / 2;
            float originalArea = CalculatePolygonArea(planet.GetComponent<PolygonCollider2D>().points);

            // cloning planet
            var cloneT = Instantiate(planetT.gameObject, planetT.parent);
            var cloneBase = cloneT.transform.GetChild(0).gameObject;

            var clonedHandler = cloneBase.GetComponent<PlanetComponentHandler>();
            var originalHandler = planet.GetComponent<PlanetComponentHandler>();
            while (clonedHandler.MyComponent is null) await Task.Yield();

            clonedHandler.MyComponent.PlanetColor = originalHandler.MyComponent.PlanetColor;
            clonedHandler.MyComponent.IsOriginalPlanet = false;
            cloneT.transform.position = planetT.position;
            
            var sprites = UniversePictures.SlicedSprite(sprite, pointA, pointB, planetPos, radius);
            
            // making two slices
            ApplySlice(planet, sprites[0], originalArea);
            ApplySlice(cloneBase, sprites[1], originalArea);

            planetT.transform.position += new Vector3(.1f, .1f, 0);
            cloneT.transform.position -= new Vector3(.1f, .1f, 0);
        }
    }

    void ApplySlice(GameObject target, Sprite sprite, float originalArea)
    { 
        // slice sprite
        target.GetComponent<SpriteMask>().sprite = sprite;
        // slice collider
        PolygonCollider2D polygonCollider = target.GetComponent<PolygonCollider2D>();
        SliceCollider(target.GetComponent<SpriteMask>(), polygonCollider);
            
        // center transform to center of new sprite
        var center = GetCenterFromCollider(polygonCollider);
        Debug.Log(center);
        MovePivot(center, target.transform.parent);
        
        // separate them
        float divider = CalculatePolygonArea(polygonCollider.points)/originalArea;
        var handler = target.GetComponent<PlanetComponentHandler>();
        handler.MyComponent.Mass *= divider;
    }

    public void SliceCollider(SpriteMask mask, PolygonCollider2D polygonCollider)
    {
        var sprite = mask.sprite;
        Vector2[] vertices = sprite.vertices;
        vertices = SortVerticesClockwise(vertices);
        
        polygonCollider.SetPath(0, vertices);
        polygonCollider.pathCount = 1;
    }
    
    
    // Helper method to sort the vertices in clockwise order
    private Vector2[] SortVerticesClockwise(Vector2[] vertices)
    {
        // Find the center point of the vertices
        Vector2 center = Vector2.zero;
        foreach (var vert in vertices)
        {
            center += vert;
        }
        center /= vertices.Length;

        // Sort the vertices by angle relative to the center point
        List<Vector2> sortedVertices = new List<Vector2>(vertices);
        sortedVertices.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.y - center.y, a.x - center.x);
            float angleB = Mathf.Atan2(b.y - center.y, b.x - center.x);
            return angleA.CompareTo(angleB);
        });

        return sortedVertices.ToArray();
    }
    
    void MovePivot(Vector3 position, Transform target)
    {
        Vector3 offset = target.position - position;
        Debug.Log(offset);
        foreach (Transform child in target)
            child.transform.position += offset;
        target.position = position;
    }

    Vector2 GetCenterFromCollider(PolygonCollider2D collider)
    {
        Vector2[] vertices = collider.GetPath(0);
        float area = 0f;
        Vector2 centroid = Vector2.zero;
        for (int i = 0; i < vertices.Length; i++) {
            Vector2 vertex1 = vertices[i];
            Vector2 vertex2 = vertices[(i + 1) % vertices.Length];
            float crossProduct = vertex1.x * vertex2.y - vertex2.x * vertex1.y;
            area += crossProduct;
            centroid += (vertex1 + vertex2) * crossProduct;
        }
        centroid /= 3f * area;
        
        return collider.transform.TransformPoint(centroid);
    }
    
    float CalculatePolygonArea(Vector2[] points)
    {
        float area = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = i == points.Length - 1 ? points[0] : points[i + 1];

            area += (p1.x * p2.y - p2.x * p1.y);
        }

        area /= 2;
        area = Mathf.Abs(area);

        return area;
    }
}
