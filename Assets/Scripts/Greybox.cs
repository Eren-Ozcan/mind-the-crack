using UnityEngine;

namespace MindTheCrack
{
    /// <summary>
    /// Boxes and quads without GameObject.CreatePrimitive.
    ///
    /// CreatePrimitive attaches a collider, which drags in the physics module.
    /// This game has no physics at all, so on device IL2CPP had stripped the
    /// module and every primitive failed with "Can't add component because
    /// class 'CapsuleCollider' doesn't exist". Building the meshes here keeps
    /// physics out of the build entirely instead of asking the stripper to
    /// spare something we never wanted.
    /// </summary>
    public static class Greybox
    {
        static Mesh _cube;
        static Mesh _quad;
        static Shader _shader;

        public static Shader Shader
        {
            get
            {
                if (_shader == null) _shader = Resources.Load<Shader>("Greybox");
                return _shader;
            }
        }

        public static Material NewMaterial(Color colour) =>
            new(Shader) { color = colour };

        public static GameObject Cube(string name, Transform parent, Material material)
        {
            _cube ??= BuildCube();
            return Build(name, parent, _cube, material);
        }

        public static GameObject Quad(string name, Transform parent, Material material)
        {
            _quad ??= BuildQuad();
            return Build(name, parent, _quad, material);
        }

        static GameObject Build(string name, Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        static Mesh BuildQuad()
        {
            var mesh = new Mesh { name = "greybox_quad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f), new Vector3(0.5f,  0.5f, 0f),
            };
            mesh.normals = new[] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh BuildCube()
        {
            var mesh = new Mesh { name = "greybox_cube" };

            // Six separate faces so each keeps its own flat normal.
            Vector3[] dirs =
            {
                Vector3.up, Vector3.down, Vector3.forward,
                Vector3.back, Vector3.right, Vector3.left,
            };

            var verts = new Vector3[24];
            var norms = new Vector3[24];
            var tris = new int[36];

            for (int f = 0; f < 6; f++)
            {
                Vector3 n = dirs[f];
                Vector3 u = Vector3.Cross(n, Mathf.Abs(n.y) > 0.5f ? Vector3.forward : Vector3.up).normalized;
                Vector3 v = Vector3.Cross(n, u).normalized;
                Vector3 c = n * 0.5f;

                int b = f * 4;
                verts[b + 0] = c - u * 0.5f - v * 0.5f;
                verts[b + 1] = c + u * 0.5f - v * 0.5f;
                verts[b + 2] = c + u * 0.5f + v * 0.5f;
                verts[b + 3] = c - u * 0.5f + v * 0.5f;

                for (int i = 0; i < 4; i++) norms[b + i] = n;

                int t = f * 6;
                tris[t + 0] = b + 0; tris[t + 1] = b + 2; tris[t + 2] = b + 1;
                tris[t + 3] = b + 0; tris[t + 4] = b + 3; tris[t + 5] = b + 2;
            }

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
