using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    private LookupTables tables;

    private int[] edgeTable;
    private int[] triTable;

    [Header("Metaball Settings")]
    public float resolution;
    public float scale;
    public float isoLevel;

    private GameObject[] metaballs;

    [Header("Generated Points")]
    private List<Vector3> points = new List<Vector3>();
    private List<float> densityValues = new List<float>(); // Renamed from depthValues

    // Mesh settings
    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {

        metaballs = GameObject.FindGameObjectsWithTag("Metaball");

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Initialize the edgeTable and triTable from the LookupTables script.
        tables = GetComponent<LookupTables>();
        edgeTable = tables.edgeTable;
        triTable = tables.triTable;

        GeneratePointsAndDensity();
    }

    // Update is called once per frame
    void Update()
    {
        metaballs = GameObject.FindGameObjectsWithTag("Metaball");
        UpdateMesh();
    }

    void GeneratePointsAndDensity()
    {
        // Generate points on the matrix for marching cubes
        for (int k = 0; k < resolution; k++)
        {
            for (int j = 0; j < resolution; j++)
            {
                for (int i = 0; i < resolution; i++)
                {
                    float x = -(scale / 2) + scale * i / (resolution - 1);
                    float y = -(scale / 2) + scale * j / (resolution - 1);
                    float z = -(scale / 2) + scale * k / (resolution - 1);

                    points.Add(new Vector3(x, y, z));
                    densityValues.Add(0f); // Initialize density values to 0
                }
            }
        }
    }

    List<Vector3> RunMarchingCubes()
    {
        for (int i = 0; i < densityValues.Count; i++)
        {
            densityValues[i] = 0f;
        }

        foreach (GameObject metaballReference in metaballs)
        {
            Metaball references = metaballReference.GetComponent<Metaball>();

            Vector3 position = references.position;
            float radius = references.radius;

            for (int i = 0; i < points.Count; i++)
            {
                float distance = radius - Vector3.Distance(position, points[i]);
                densityValues[i] += Mathf.Exp(-distance * distance);
            }
        }

        List<Vector3> vlist = new List<Vector3>(12);

        for (int i = 0; i < 12; i++)
        {
            vlist.Add(Vector3.zero);
        }

        int resolutionSquared = (int)resolution * (int)resolution;

        List<Vector3> trianglePoints = new List<Vector3>();

        for (int z = 0; z < resolution - 1; z++)
        {
            for (int y = 0; y < resolution - 1; y++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    // Generating indexes
                    int p = x + (int)resolution * y + resolutionSquared * z;
                    int px = p + 1;
                    int py = p + (int)resolution;
                    int pxy = py + 1;
                    int pz = p + resolutionSquared;
                    int pxz = px + resolutionSquared;
                    int pyz = py + resolutionSquared;
                    int pxyz = pxy + resolutionSquared;

                    // Generating scalar values for indexes
                    float value0 = densityValues[p];
                    float value1 = densityValues[px];
                    float value2 = densityValues[py];
                    float value3 = densityValues[pxy];
                    float value4 = densityValues[pz];
                    float value5 = densityValues[pxz];
                    float value6 = densityValues[pyz];
                    float value7 = densityValues[pxyz];

                    int cubeindex = 0;
                    if (value0 < isoLevel) cubeindex |= 1;
                    if (value1 < isoLevel) cubeindex |= 2;
                    if (value2 < isoLevel) cubeindex |= 8;
                    if (value3 < isoLevel) cubeindex |= 4;
                    if (value4 < isoLevel) cubeindex |= 16;
                    if (value5 < isoLevel) cubeindex |= 32;
                    if (value6 < isoLevel) cubeindex |= 128;
                    if (value7 < isoLevel) cubeindex |= 64;

                    int bits = edgeTable[cubeindex];
                    if (bits == 0)
                    {
                        continue;
                    }

                    float mu = 0.5f;

                    if ((bits & 1) != 0)
                    {
                        mu = (isoLevel - value0) / (value1 - value0);
                        vlist[0] = Vector3.Lerp(points[p], points[px], mu);
                    }
                    if ((bits & 2) != 0)
                    {
                        mu = (isoLevel - value1) / (value3 - value1);
                        vlist[1] = Vector3.Lerp(points[px], points[pxy], mu);
                    }
                    if ((bits & 4) != 0)
                    {
                        mu = (isoLevel - value2) / (value3 - value2);
                        vlist[2] = Vector3.Lerp(points[py], points[pxy], mu);
                    }
                    if ((bits & 8) != 0)
                    {
                        mu = (isoLevel - value0) / (value2 - value0);
                        vlist[3] = Vector3.Lerp(points[p], points[py], mu);
                    }
                    // top of the cube
                    if ((bits & 16) != 0)
                    {
                        mu = (isoLevel - value4) / (value5 - value4);
                        vlist[4] = Vector3.Lerp(points[pz], points[pxz], mu);
                    }
                    if ((bits & 32) != 0)
                    {
                        mu = (isoLevel - value5) / (value7 - value5);
                        vlist[5] = Vector3.Lerp(points[pxz], points[pxyz], mu);
                    }
                    if ((bits & 64) != 0)
                    {
                        mu = (isoLevel - value6) / (value7 - value6);
                        vlist[6] = Vector3.Lerp(points[pyz], points[pxyz], mu);
                    }
                    if ((bits & 128) != 0)
                    {
                        mu = (isoLevel - value4) / (value6 - value4);
                        vlist[7] = Vector3.Lerp(points[pz], points[pyz], mu);
                    }
                    // vertical lines of the cube
                    if ((bits & 256) != 0)
                    {
                        mu = (isoLevel - value0) / (value4 - value0);
                        vlist[8] = Vector3.Lerp(points[p], points[pz], mu);
                    }
                    if ((bits & 512) != 0)
                    {
                        mu = (isoLevel - value1) / (value5 - value1);
                        vlist[9] = Vector3.Lerp(points[px], points[pxz], mu);
                    }
                    if ((bits & 1024) != 0)
                    {
                        mu = (isoLevel - value3) / (value7 - value3);
                        vlist[10] = Vector3.Lerp(points[pxy], points[pxyz], mu);
                    }
                    if ((bits & 2048) != 0)
                    {
                        mu = (isoLevel - value2) / (value6 - value2);
                        vlist[11] = Vector3.Lerp(points[py], points[pyz], mu);
                    }

                    int i = 0;
                    cubeindex <<= 4;

                    while (triTable[cubeindex + i] != -1)
                    {
                        int index1 = triTable[cubeindex + i];
                        int index2 = triTable[cubeindex + i + 1];
                        int index3 = triTable[cubeindex + i + 2];

                        if (index1 >= 0 && index1 < vlist.Count &&
                            index2 >= 0 && index2 < vlist.Count &&
                            index3 >= 0 && index3 < vlist.Count)
                        {
                            trianglePoints.Add(vlist[index1]);
                            trianglePoints.Add(vlist[index2]);
                            trianglePoints.Add(vlist[index3]);
                        }
                        else
                        {
                            Debug.LogError("Invalid index detected!");
                        }

                        i += 3;
                    }
                }
            }
        }

        return trianglePoints;
    }

    void UpdateMesh()
    {
        mesh.Clear();

        List<Vector3> verticesList = RunMarchingCubes();
        Vector3[] vertices = verticesList.ToArray();

        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i += 3)
        {
            Vector3 vertIndex1 = vertices[i];
            Vector3 vertIndex2 = vertices[i + 1];
            Vector3 vertIndex3 = vertices[i + 2];

            Vector3 faceNormal = Vector3.Cross(vertIndex2 - vertIndex1, vertIndex3 - vertIndex1).normalized;

            normals[i] = faceNormal;
            normals[i + 1] = faceNormal;
            normals[i + 2] = faceNormal;
        }

        mesh.normals = normals;
    }
}
