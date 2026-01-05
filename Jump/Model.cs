using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Assimp;


public class Model
{
    public float[] Vertices;
    public int VAO;

    public Model(string path)
    {
        LoadModel(path);
    }

    private void LoadModel(string path)
    {
        AssimpContext importer = new AssimpContext();
        Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        List<float> verts = new List<float>();

        foreach (var mesh in scene.Meshes)
        {
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                var v = mesh.Vertices[i];
                verts.Add(v.X); verts.Add(v.Y); verts.Add(v.Z);

                if (mesh.TextureCoordinateChannels[0].Count > 0)
                {
                    var uv = mesh.TextureCoordinateChannels[0][i];
                    verts.Add(uv.X); verts.Add(uv.Y);
                }
                else
                {
                    verts.Add(0); verts.Add(0);
                }
            }
        }

        Vertices = verts.ToArray();

        VAO = GL.GenVertexArray();
        int VBO = GL.GenBuffer();

        GL.BindVertexArray(VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }
}
