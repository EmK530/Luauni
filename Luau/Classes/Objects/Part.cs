using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[ExecuteInEditMode]
public class Part : MonoBehaviour
{
    public readonly string ClassName = "BasePart";

    public object Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    private MeshRenderer mr;
    private Rigidbody rb;

    public Color3 BrickColor
    {
        get
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            Color c = mr.material.GetColor("_BaseColor");
            return new Color3(c.r, c.g, c.b);
        }
        set
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            mr.material.SetColor("_BaseColor", new Color(value.r, value.g, value.b));

        }
    }
    public Color3 Color
    {
        get
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            Color c = mr.material.GetColor("_BaseColor");
            return new Color3(c.r, c.g, c.b);
        }
        set
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            mr.material.SetColor("_BaseColor", new Color(value.r, value.g, value.b));
        }
    }

    public string Name
    {
        get { return gameObject.name; }
        set
        {
            gameObject.name = value;
        }
    }

    public double CollisionGroupId
    {
        get { return 1d; }
        set
        {
            Logging.Warn("Support for this property is postponed.", "Part:CollisionGroupId");
        }
    }

    [SerializeField]
    private bool _anchored = true;
    public bool Anchored
    {
        get { if (rb == null) { rb = gameObject.GetComponent<Rigidbody>(); } return rb.isKinematic; }
        set
        {
            if (rb == null) { rb = gameObject.GetComponent<Rigidbody>(); }
            rb.isKinematic = false;
        }
    }

    public RBXScriptSignal Touched = new RBXScriptSignal();

    public static bool isObject = true;

    private Color _lastColor;
    [SerializeField]
    private Color _color;
    private Enum.Material _lastMaterial;
    [SerializeField]
    private Enum.Material _material = Enum.Material.Plastic;
    private Enum.Material Material
    {
        get { return _material; }
        set
        {
            _material = value;

        }
    }
    private Material _mat;
    [SerializeField]
    private UnityEngine.Vector3 _multiplier = new UnityEngine.Vector3(0.1f, 0.1f, 0.1f);
    private UnityEngine.Vector3 _currentScale;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Calculate();
            _mat = new Material(Resources.Load<Material>("Materials/Materials/" + _material.ToString()));
            _lastMaterial = _material;
            mr.sharedMaterial = _mat;
            _mat.SetColor("_BaseColor", _color);
            _lastColor = _color;
        }
        rb = GetComponent<Rigidbody>();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = _anchored;
    }
    void Update()
    {
#if (UNITY_EDITOR)
        {
            if (mr != null)
            {
                Calculate();
                if(_lastMaterial != _material)
                {
                    _lastMaterial = _material;
                    _mat = new Material(Resources.Load<Material>("Materials/Materials/" + _material.ToString()));
                    mr.sharedMaterial = _mat;
                }
                if(_lastColor != _color)
                {
                    _lastColor = _color;
                    _mat.SetColor("_BaseColor", _color);
                }
            }
        }
#endif
    }

    private void Calculate()
    {
        UnityEngine.Vector3 cmp = new UnityEngine.Vector3(transform.localScale.x * _multiplier.x, transform.localScale.y * _multiplier.y, transform.localScale.z * _multiplier.z);
        if (_currentScale == cmp) return;
        if (CheckForDefaultSize()) return;
        _currentScale = cmp;
        var mesh = GetMesh();
        mesh.uv = SetupUvMap(mesh.uv);
        mesh.name = "Cube Instance";
    }

    private Mesh GetMesh()
    {
        Mesh mesh;
#if UNITY_EDITOR
        var meshFilter = GetComponent<MeshFilter>();
        var meshCopy = Instantiate(meshFilter.sharedMesh);
        mesh = meshFilter.mesh = meshCopy;
#else
        mesh = GetComponent<MeshFilter>().mesh;
#endif
        return mesh;
    }

    private UnityEngine.Vector2[] SetupUvMap(UnityEngine.Vector2[] meshUVs)
    {
        var width = _currentScale.x;
        var depth = _currentScale.z;
        var height = _currentScale.y;

        //Front
        meshUVs[2] = new Vector2(0, height);
        meshUVs[3] = new Vector2(width, height);
        meshUVs[0] = new Vector2(0, 0);
        meshUVs[1] = new Vector2(width, 0);

        //Back
        meshUVs[7] = new Vector2(0, 0);
        meshUVs[6] = new Vector2(width, 0);
        meshUVs[11] = new Vector2(0, height);
        meshUVs[10] = new Vector2(width, height);

        //Left
        meshUVs[19] = new Vector2(depth, 0);
        meshUVs[17] = new Vector2(0, height);
        meshUVs[16] = new Vector2(0, 0);
        meshUVs[18] = new Vector2(depth, height);

        //Right
        meshUVs[23] = new Vector2(depth, 0);
        meshUVs[21] = new Vector2(0, height);
        meshUVs[20] = new Vector2(0, 0);
        meshUVs[22] = new Vector2(depth, height);

        //Top
        meshUVs[4] = new Vector2(width, 0);
        meshUVs[5] = new Vector2(0, 0);
        meshUVs[8] = new Vector2(width, depth);
        meshUVs[9] = new Vector2(0, depth);

        //Bottom
        meshUVs[13] = new Vector2(width, 0);
        meshUVs[14] = new Vector2(0, 0);
        meshUVs[12] = new Vector2(width, depth);
        meshUVs[15] = new Vector2(0, depth);

        return meshUVs;
    }

    private bool CheckForDefaultSize()
    {
        if (_currentScale != UnityEngine.Vector3.one) return false;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DestroyImmediate(GetComponent<MeshFilter>());
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(cube);
        return true;
    }
}