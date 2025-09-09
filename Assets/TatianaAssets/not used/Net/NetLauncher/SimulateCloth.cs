///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   script used (with my modifications): https://www.youtube.com/watch?v=3aF7-TtgCGg ; https://github.com/Mauriits/ClothSimulation ;
///   Created: 05.07.2025
///   Last Change: 11.07.2025
///   ESA project stage: 
///   Last Change: 11.08.2025

///   Simulation of cloth behaviour.
///   Should be attached to GO NetCloth  


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateCloth : MonoBehaviour
{

    [Header("–– Corner Anchors ––")]// added 30.06.2025
    [Tooltip("Drag exactly objects, one for each corner of the cloth. Order: br, bl, tr, tl.")]
    public GameObject[] lockPoints = new GameObject[4];
    
    [SerializeField] private float TOLERANCE = 0.03f;// the lower, the stiffer the cloth, it was 0.1f
    [SerializeField] private int numIterationsSolveConstraints = 20;

    public Vector3[] lockedPositions;
    public bool[] isPositionLocked;

    const float EPSILON = 0.5f;
    float DAMPING = 0.99f;

    Mesh mesh;

    Vector3 acceleration;
    int[] vertIndicesSorted;
    Vector3[] vertices;
    Vector3[] positions;
    Vector3[] oldPositions;
    float restLength, restLengthDiagonal;
    int sqrtVertCount;

    //GameObject[] spheres;//SPHERES - disabled
    float[] radiusOther;
    Vector3[] positionOther;

    GameObject[] boxes;
    BoxCollider[] boxColliders;

    [Header("Objects (will be auto assigned):")]
    [SerializeField] private GameObject NetLauncher;

    void Awake()
    {
        AutoAssignObjects();
    }

    // Use this for initialization
    void Start()
    {
        lockedPositions = new Vector3[4];
        isPositionLocked = new bool[4];

        // Set rotation temporarily to zero to make initializing easier
        Quaternion tempOrientation = transform.rotation;
        transform.rotation = Quaternion.identity;

        /*
        ////SPHERES - disabled
        // Get positions and radii of possible spheres to collide with
        spheres = GameObject.FindGameObjectsWithTag("Sphere");
        positionOther = new Vector3[spheres.Length];
        radiusOther = new float[spheres.Length];
        for (int i = 0; i < spheres.Length; i++)
        {
            positionOther[i] = spheres[i].transform.position;
            radiusOther[i] = spheres[i].GetComponent<SphereCollider>().radius * spheres[i].transform.localScale.x;
        }
        */

        //  gather all your cubes (tagged e.g. “BoxColliderTrigger”)
        boxes = GameObject.FindGameObjectsWithTag("BoxColliderTrigger");
        boxColliders = new BoxCollider[boxes.Length];
        for (int j = 0; j < boxes.Length; j++)
            boxColliders[j] = boxes[j].GetComponent<BoxCollider>();

        // Get cloth mesh vertex positions
        acceleration = new Vector3(0.0f, -9.81f, 0.0f);
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.MarkDynamic();
        vertices = mesh.vertices;

        // 2) generate UVs across the entire cloth
        {
            var verts = vertices;
            var uvs = new Vector2[verts.Length];
            var bounds = mesh.bounds;
            var min = bounds.min;
            var size = bounds.size;

            for (int i = 0; i < verts.Length; i++)
            {
                float u = (verts[i].x - min.x) / size.x;
                float v = (verts[i].z - min.z) / size.z;
                uvs[i] = new Vector2(u, v);
            }
            mesh.uv = uvs;
        }
        

        oldPositions = new Vector3[vertices.Length];
        positions = new Vector3[vertices.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.TransformPoint(vertices[i]);
            oldPositions[i] = positions[i];
        }

        // Calculate rest lengths of edges
        sqrtVertCount = (int)Mathf.Sqrt(vertices.Length);
        restLength = positions[1].x - positions[0].x;
        restLengthDiagonal = Mathf.Sqrt(restLength * restLength + restLength * restLength);

        // Sort vertices
        float halfMeshWidth = (sqrtVertCount * restLength) / 2.0f;
        vertIndicesSorted = new int[vertices.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            float localx = positions[i].x - transform.position.x + halfMeshWidth;
            float localz = positions[i].z - transform.position.z + halfMeshWidth;

            vertIndicesSorted[(int)((localx / restLength)) + sqrtVertCount * (int)((localz / restLength))] = i;
        }

        // Set mesh back to actual orientation
        transform.rotation = tempOrientation;
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.TransformPoint(vertices[i]);
            oldPositions[i] = positions[i];
        }
        //Debug.Log("Verts at startup: " + mesh.vertexCount);
        MakeMeshDoubleFaced();
        //Debug.Log("Verts at startup: " + mesh.vertexCount);
        vertices = mesh.vertices;   // now vertices.Length == positions.Length * 2

        //Turn all four locks “on”
        for (int i = 0; i < 4; i++)// added 30.06.2025
        {
            isPositionLocked[i] = lockPoints[i] != null;
        }
    }

    void FixedUpdate()
    {
        VerletIntegrate();
        for (int iter = 0; iter < numIterationsSolveConstraints; ++iter)
        {
            for (int i = 0; i < positions.Length; ++i)
            {
                /*
                ////SPHERES - disabled
                // sphere collisions
                for (int j = 0; j < spheres.Length; ++j)
                    SatisfySphereConstraint(i, positionOther[j], radiusOther[j]);
                */

                // box collisions
                for (int j = 0; j < boxColliders.Length; ++j)
                    SatisfyBoxConstraint(i, boxColliders[j]);

                // cloth spring constraints
                SatisfyClothConstraints(i);
            }
            SolveLockedConstraints();
        }
        ApplyToMesh();
        //ApplyToMeshOneSided()
    }

    void Update()
    {
        // write `positions[]` back into your mesh …
        //ApplyToMesh();
    }

    void SatisfyClothConstraints(int index)
    {
        // Structural constraints
        if (index + 17 < positions.Length)
            SatisfyDistanceConstraint(index, index + 17, restLength);

        if (index - 17 >= 0)
            SatisfyDistanceConstraint(index, index - 17, restLength);

        if ((index + 1) % 17 != 0 && index + 1 < positions.Length)
            SatisfyDistanceConstraint(index, index + 1, restLength);

        if ((index + 1) % 17 != 1 && index - 1 >= 0)
            SatisfyDistanceConstraint(index, index - 1, restLength);


        // Shear constraints
        if ((index + 1) % 17 != 0 && index + 18 < positions.Length)
            SatisfyDistanceConstraint(index, index + 18, restLengthDiagonal);

        if ((index + 1) % 17 != 1 && index + 16 < positions.Length)
            SatisfyDistanceConstraint(index, index + 16, restLengthDiagonal);

        if ((index + 1) % 17 != 1 && index - 18 >= 0)
            SatisfyDistanceConstraint(index, index - 18, restLengthDiagonal);

        if ((index + 1) % 17 != 0 && index - 16 >= 0)
            SatisfyDistanceConstraint(index, index - 16, restLengthDiagonal);

        //if (Input.GetKeyDown(KeyCode.E) && !pressed)
        //{
        //    pressed = true;
        //    sid += 1;
        //    Debug.Log("!!!!" + sid);
        //}
        //else if (Input.GetKeyDown(KeyCode.Q) && !pressed)
        //{
        //    sid -= 1;
        //    pressed = true;
        //}
        //if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
        //    pressed = false;

        //Vector3 temp = new Vector3(positions[vertIndicesSorted[sid]].x, positions[vertIndicesSorted[sid]].y, positions[vertIndicesSorted[sid]].z);
        //spheres[0].transform.position = temp;
        //positions[3].x = -5.379427f - 19.2f;
        //positions[4].x = -5.379427f + 19.4f;
        //positions[5].x = -6.046001f + 19.4f;
        //Debug.Log(positions[8].x + ", " + positions[7].x + ", " + positions[9].x + ", ");

        //positions[8 + sqrtVertCount].y = 12.5f;       

    }

    void SatisfyDistanceConstraint(int indexA, int indexB, float distance)
    {
        indexA = vertIndicesSorted[indexA];
        indexB = vertIndicesSorted[indexB];

        Vector3 diffVec = positions[indexA] - positions[indexB];
        float dist = diffVec.magnitude;
        float difference = distance - dist;

        // normalize
        diffVec /= dist;

        // Change positions of A and B to satisfy distance constraint
        if (Mathf.Abs(difference) > TOLERANCE)// tolerance is set close to zero
        {
            Vector3 correction = diffVec * (difference * 0.5f);
            positions[indexA] += correction;
            positions[indexB] -= correction;
        }
    }

    void SatisfyEnvironmentConstraints(int index)
    {
        // Platform constraint
        if (positions[index].y < EPSILON)
            positions[index].y = EPSILON;

        // Sphere collision constraints
        for (int i = 0; i < positionOther.Length; i++)
        {
            Vector3 diff = positions[index] - positionOther[i];
            float dist = diff.magnitude;

            //normalize
            diff /= dist;

            if (dist < radiusOther[i])
                positions[index] += diff * (radiusOther[i] - dist + EPSILON);
        }
    }

    void SolveLockedConstraints()
    {
        // precompute your corner vertex IDs
        int[] cornerVerts = new int[] {
        vertIndicesSorted[16],                   // bottom-right
        vertIndicesSorted[0],                    // bottom-left
        vertIndicesSorted[positions.Length - 1], // top-right
        vertIndicesSorted[positions.Length - 17] // top-left
    };

        for (int i = 0; i < lockPoints.Length; i++)
        {
            if (!isPositionLocked[i] || lockPoints[i] == null)
                continue;

            int vi = cornerVerts[i];
            Vector3 clothPos = positions[vi];
            Vector3 cubePos = lockPoints[i].transform.position;

            Vector3 diff = clothPos - cubePos;
            float dist = diff.magnitude;
            if (dist < EPSILON)
                continue;

            // half the correction goes to cloth, half to cube
            Vector3 correction = diff * 0.5f;

            // move cloth point
            positions[vi] -= correction;

            /*// move cube via its Rigidbody
            var rb = lockPoints[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                // kinematic or non-kinematic both accept MovePosition
                rb.MovePosition(cubePos + correction);
            }
            else
            {
                // fallback if you forgot the Rigidbody
                lockPoints[i].transform.position = cubePos + correction;
            }*/
        }
    }

    public void ChangeTolerance(float newValue)
    {
        TOLERANCE = newValue;
    }

    public void ChangeDamping(float newValue)
    {
        DAMPING = newValue;
    }

    // source: https://answers.unity.com/questions/280741/how-make-visible-the-back-face-of-a-mesh.html
    void MakeMeshDoubleFaced()
    {
        // grab old data
        var oldVerts = mesh.vertices;
        var oldNorms = mesh.normals;
        var oldTris = mesh.triangles;
        var oldUVs = mesh.uv;
        var oldTangs = mesh.tangents;
        int szV = oldVerts.Length;
        int szT = oldTris.Length;

        // 1) Build doubled‐size verts + norms
        var newVerts = new Vector3[szV * 2];
        var newNorms = new Vector3[szV * 2];
        for (int i = 0; i < szV; i++)
        {
            newVerts[i] = oldVerts[i];
            newVerts[i + szV] = oldVerts[i];
            newNorms[i] = oldNorms[i];
            newNorms[i + szV] = -oldNorms[i];    // reversed
        }

        // 2) Build doubled‐size UVs
        var newUVs = new Vector2[szV * 2];
        for (int i = 0; i < szV; i++)
        {
            newUVs[i] = oldUVs[i];
            newUVs[i + szV] = oldUVs[i];
        }

        // 3) Build doubled‐size tangents (if you use normal maps)
        var newTangs = new Vector4[szV * 2];
        for (int i = 0; i < szV; i++)
        {
            newTangs[i] = oldTangs[i];
            newTangs[i + szV] = oldTangs[i];
        }

        // 4) Double up triangles, reversing winding on the back‐faces
        var newTris = new int[szT * 2];
        for (int i = 0; i < szT; i += 3)
        {
            // front‐face
            newTris[i] = oldTris[i];
            newTris[i + 1] = oldTris[i + 1];
            newTris[i + 2] = oldTris[i + 2];
            // back‐face (notice swapped winding!)
            int j = i + szT;
            newTris[j] = oldTris[i] + szV;
            newTris[j + 1] = oldTris[i + 2] + szV;
            newTris[j + 2] = oldTris[i + 1] + szV;
        }

        // 5) Assign back to the mesh
        mesh.vertices = newVerts;
        mesh.normals = newNorms;
        mesh.uv = newUVs;
        mesh.tangents = newTangs;
        mesh.triangles = newTris;
    }

    /// <summary>
    /// Simple Verlet integrator on your world‐space positions[] array.
    /// </summary>
    void VerletIntegrate()
    {
        Vector3 temp, velocity;
        float dt2 = Time.fixedDeltaTime * Time.fixedDeltaTime;
        for (int i = 0; i < positions.Length; i++)
        {
            // store current
            temp = positions[i];

            // compute velocity
            velocity = positions[i] - oldPositions[i];

            // advance
            positions[i] += velocity * DAMPING + acceleration * dt2;

            // save last
            oldPositions[i] = temp;
        }
    }

    /// <summary>
    /// Takes your world-space positions[] and writes them back
    /// into the mesh’s vertices[] (including the duplicated back-faces),
    /// then updates the mesh.
    /// </summary>
    void ApplyToMesh()
    {
        // make sure 'vertices' is your Mesh.vertices array (size = positions.Length * 2)
        for (int i = 0; i < positions.Length; i++)
        {
            // transform world → local
            Vector3 localPos = transform.InverseTransformPoint(positions[i]);
            vertices[i] = localPos;
            vertices[i + positions.Length] = localPos;  // the doubled face
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    void ApplyToMeshOneSided()
    {
        // mesh.vertices.Length == positions.Length
        // so we just overwrite each one
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 localPos = transform.InverseTransformPoint(positions[i]);
            vertices[i] = localPos;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();  // you can remove this if you pre‐set mesh.bounds in Start()
    }

    void SatisfySphereConstraint(int i, Vector3 center, float radius)
    {
        Vector3 diff = positions[i] - center;
        float dist = diff.magnitude;
        float target = radius + EPSILON;
        if (dist < target)
        {
            Vector3 n = diff / dist;
            positions[i] = center + n * target;
            oldPositions[i] = positions[i];    // “lock” the history too
        }

    }

    void SatisfyBoxConstraint(int i, BoxCollider box)
    {
        // 1) Bring the point into the box’s local space
        Vector3 localPt = box.transform.InverseTransformPoint(positions[i]) - box.center;
        Vector3 halfExtents = box.size * 0.5f;

        // 2) Clamp to the box’s AABB in local space → gives closest point on (or inside) the box
        Vector3 clamped = new Vector3(
            Mathf.Clamp(localPt.x, -halfExtents.x, halfExtents.x),
            Mathf.Clamp(localPt.y, -halfExtents.y, halfExtents.y),
            Mathf.Clamp(localPt.z, -halfExtents.z, halfExtents.z)
        );

        // 3) Transform that closest-on-surface point back into world space
        Vector3 closestLocal = clamped + box.center;
        Vector3 closestWorld = box.transform.TransformPoint(closestLocal);

        // 4) If our cloth point is inside (or closer than EPSILON), push it out
        Vector3 diff = positions[i] - closestWorld;
        float dist = diff.magnitude;
        if (dist < EPSILON)
        {
            // if zero-length (exactly at center), pick some axis normal:
            Vector3 n = (dist > 0f ? diff.normalized : box.transform.up);
            positions[i] = closestWorld + n * EPSILON;
            oldPositions[i] = positions[i];   // lock history too
        }
    }

    private void AutoAssignObjects()
    {
        // Auto-assign netLauncher
        if (NetLauncher == null)
        {
            NetLauncher = SceneUtils.FindDeep("NetLauncher");
            if (NetLauncher == null)
                Debug.LogError("Couldn't find and assign 'NetLauncher'.");
        }
    }

    //numIterationsSolveConstraints = 20;

    public void Iterate5timesConstraints()
    {
        numIterationsSolveConstraints = 5;
    }

    public void Iterate10timesConstraints()
    {
        numIterationsSolveConstraints = 10;
    }

    public void Iterate20timesConstraints()
    {
        numIterationsSolveConstraints = 20;
    }
}
