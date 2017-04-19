using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace rcCore
{
    public class rcUVMapEditor : EditorWindow
    {
        private GUIStyle style;

        private Mesh srcMesh;
        private Mesh uvMesh;
        private Rect uvBounds;

        private bool showUV2;
        private bool showWireframe = true;
        private bool zoom;
        private string meshLabel;

        private Renderer srcRenderer;

        enum VertAttributes { uv, uv2, tangent };
        string[] vertAttributeStrings = new string[] { "uv", "uv2", "tangent" };
        int vertAttribute = 0;

        //static private Material materialTransparentColor;
        static private Material materialBackgroundTexture;

        public rcUVMapEditor()
        {
            autoRepaintOnSceneChange = true;
            titleContent.text = "UV Map Editor";
        }

        void OnSelectionChange()
        {
            Repaint();
        }

        void Update()
        {

        }

        void OnGUI()
        {
            CreateStyle();

            Mesh newMesh = null;
            MeshFilter newMeshFilter = null;
            if (Selection.activeGameObject != null)
            {
                newMeshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
                if (newMeshFilter != null)
                {
                    srcRenderer = newMeshFilter.GetComponent<Renderer>();
                    newMesh = newMeshFilter.sharedMesh;
                }
            }
            else if (Selection.activeObject != null)
            {
                Mesh meshAsset = Selection.activeObject as Mesh;
                if (meshAsset != null)
                {
                    srcRenderer = null;
                    newMesh = meshAsset;
                }
            }

            if (newMesh != srcMesh)
            {
                srcMesh = newMesh;

                if (srcMesh != null)
                {
                    RebuildMesh();
                }
            }

            GUI.enabled = uvMesh != null && uvMesh.uv2 != null;

            int newVertAttribute = GUILayout.Toolbar(vertAttribute, vertAttributeStrings);
            if (newVertAttribute != vertAttribute)
            {
                vertAttribute = newVertAttribute;

                showUV2 = vertAttribute > 0; // anything but uv

                if (srcMesh != null)
                    RebuildMesh();
            }

            GUILayout.BeginVertical("box");

            GUILayout.Label(meshLabel);

            bool newShowWireframe = GUILayout.Toggle(showWireframe, "Show Wireframe");
            if (newShowWireframe != showWireframe)
            {
                showWireframe = newShowWireframe;
            }

            bool newZoom = GUILayout.Toggle(zoom, "Zoom");
            if (newZoom != zoom)
            {
                zoom = newZoom;
            }

            GUILayout.EndVertical();

            GUI.enabled = true;

            if (uvMesh != null)
            {
                DrawWireframe();
            }
        }

        void DrawWireframe()
        {
            Rect uvBounds2 = uvBounds;

            if (showUV2 && !zoom)
                uvBounds2.Set(0, 0, 1, 1);

            Rect rect = GetTargetRect(uvBounds2);

            GUI.color = Color.grey;
            style.normal.background = EditorGUIUtility.whiteTexture;
            GUI.Box(rect, GUIContent.none, style);

            // Draw background texture
            DrawBackgroundTexture(rect);

            // Draw wireframe
            rcGL.SetMaterialTransparentColor();

            float xScale = rect.width / uvBounds2.width;
            float size = xScale;
            var scale = new Vector3(size, -size, size);

            if (showUV2 && !zoom && srcRenderer != null && srcRenderer.lightmapIndex >= 0)
            {
                scale.x *= srcRenderer.lightmapScaleOffset.x;
                scale.y *= srcRenderer.lightmapScaleOffset.y;
            }

            float x = (rect.x - (uvBounds2.xMin * size));
            float y = (rect.yMax + (uvBounds2.yMin * size));
            var pos = new Vector3(x, y);

            if (showUV2 && !zoom && srcRenderer != null && srcRenderer.lightmapIndex >= 0)
            {
                pos.x += srcRenderer.lightmapScaleOffset.z * rect.width;
                pos.y -= srcRenderer.lightmapScaleOffset.w * rect.height;
            }

            Matrix4x4 xform = Matrix4x4.identity;
            xform.SetTRS(pos, Quaternion.identity, scale);

            if (showWireframe)
            {
                //materialTransparentColor.SetPass(0);
                rcGL.SetMaterialTransparentColor();

                GL.wireframe = true;
                Graphics.DrawMeshNow(uvMesh, xform);
                GL.wireframe = false;
            }

            var label = "(" + uvBounds2.xMin.ToString("n2") + ", " + uvBounds2.yMin.ToString("n2") + ")";
            DrawLabeledRect(rect.xMin, rect.yMax, 2.0f, Color.red, label, 200.0f, TextAnchor.LowerRight);

            label = "(" + uvBounds2.xMax.ToString("n2") + ", " + uvBounds2.yMax.ToString("n2") + ")";
            DrawLabeledRect(rect.xMax, rect.yMin, 2.0f, Color.red, label, 200.0f, TextAnchor.UpperLeft);
        }

        Rect GetTargetRect(Rect uvBounds2)
        {
            float border = 32;

            //Rect uvBounds2 = uvBounds;

            float aspectRatio = uvBounds2.width / uvBounds2.height;
            bool isWide = uvBounds2.width > uvBounds2.height;

            Rect lastRect = GUILayoutUtility.GetLastRect();
            float width = position.width - lastRect.x;
            float height = position.height - lastRect.yMax;

            Rect rect = new Rect();
            rect.x = lastRect.x;
            rect.y = lastRect.yMax;

            rect.x += border;
            rect.y += border;
            width -= border * 2;
            height -= border * 2;

            if (isWide)
            {
                rect.width = width;
                rect.height = rect.width / aspectRatio;

                if (rect.height > height)
                {
                    rect.height = height;
                    rect.width = rect.height * aspectRatio;
                }
            }
            else
            {
                rect.height = height;
                rect.width = rect.height * aspectRatio;

                if (rect.width > width)
                {
                    rect.width = width;
                    rect.height = rect.width / aspectRatio;
                }
            }

            return rect;
        }

        void DrawBackgroundTexture(Rect rect)
        {
            bool drawTexture = true;
            if (drawTexture && srcRenderer != null)
            {
                Texture2D tex = null;
                if (showUV2)
                {
                    if (srcRenderer.lightmapIndex >= 0)
                    {
                        if (srcRenderer.lightmapIndex < LightmapSettings.lightmaps.Length)
                            tex = LightmapSettings.lightmaps[srcRenderer.lightmapIndex].lightmapColor;
                    }
                    else if (srcRenderer.sharedMaterial != null)
                    {
                        if (srcRenderer.sharedMaterial.HasProperty("_MegaTex"))
                            tex = srcRenderer.sharedMaterial.GetTexture("_MegaTex") as Texture2D;
                        else if (srcRenderer.sharedMaterial.shader.name.ToLower().Contains("mtex"))
                            tex = srcRenderer.sharedMaterial.mainTexture as Texture2D;
                    }
                }
                else if (srcRenderer.sharedMaterial != null && srcRenderer.sharedMaterial.HasProperty("_MainTex") && srcRenderer.sharedMaterial.mainTexture != null)
                {
                    // tex = srcRenderer.sharedMaterial.mainTexture as Texture2D;
                }

                if (tex != null)
                {
                    if (materialBackgroundTexture == null)
                    {
                        Shader shader = Shader.Find("BigBit/Mobile-Unlit-Texture");
                        materialBackgroundTexture = new Material(shader);
                        materialBackgroundTexture.mainTexture = tex;
                        materialBackgroundTexture.color = Color.grey;
                    }

                    Rect srcRect = new Rect(0, 0, 1, 1);
                    if (zoom && srcRenderer != null)
                    {
                        if (srcRenderer.lightmapIndex >= 0)
                        {
                            srcRect.Set(srcRenderer.lightmapScaleOffset.z, srcRenderer.lightmapScaleOffset.w, srcRenderer.lightmapScaleOffset.x, srcRenderer.lightmapScaleOffset.y);
                            srcRect.width *= uvBounds.width;
                            srcRect.height *= uvBounds.height;
                        }
                        else
                        {
                            srcRect = uvBounds;
                        }
                    }
                    Graphics.DrawTexture(rect, tex, srcRect, 0, 0, 0, 0, materialBackgroundTexture);
                }
            }
        }

        public void RebuildMesh()
        {
            //if(showUV2 && srcMesh.uv2 == null)
            //    showUV2 = false;

            uvBounds = GetUVBounds(srcMesh);
            //Log.Info(uvBounds.min + " " + uvBounds.max);

            AssignUV(srcMesh);

            var submeshLabel = srcMesh.subMeshCount > 1 ? " submeshes, " : " submesh, ";
            meshLabel = "Mesh: " + srcMesh.name + " (" + srcMesh.subMeshCount + submeshLabel + srcMesh.vertexCount + " verts, " + srcMesh.triangles.Length / 3 + " tris)";
        }

        public void PrintProps(SerializedProperty prop)
        {
            prop.Next(true);
            while (prop.Next(false))
            {
                Debug.Log(prop.name);
            }
            prop.Reset();
        }

        public bool ShowUV2() { return vertAttribute != (int)VertAttributes.uv; }

        public Vector2[] GetUVs(Mesh mesh)
        {
            Vector2[] uvs = showUV2 ? mesh.uv2 : mesh.uv;

            if (showUV2 && (mesh.uv2 == null || (mesh.uv2 != null && mesh.uv2.Length == 0)))
            {
                uvs = new Vector2[mesh.vertexCount];
                int ct = 0;
                foreach (var uv in mesh.uv)
                {
                    uvs[ct++] = new Vector2(uv.x, uv.y);
                }
            }

            bool useTangents = vertAttribute == (int)VertAttributes.tangent;
            if (useTangents)
            {
                uvs = new Vector2[mesh.vertexCount];
                int ct = 0;
                foreach (var tan in mesh.tangents)
                {
                    uvs[ct++] = new Vector2(tan.x, tan.y);
                }
            }
            return uvs;
        }

        public Rect GetUVBounds(Mesh mesh)
        {
            Vector2[] uvs = GetUVs(mesh);
            return GetUVBounds(uvs);
        }

        public Rect GetUVBounds(IEnumerable<Vector2> uvs)
        {
            Rect bounds = new Rect();
            bounds.xMin = float.MaxValue;
            bounds.yMin = float.MaxValue;
            bounds.xMax = -float.MaxValue;
            bounds.yMax = -float.MaxValue;

            foreach (var uv in uvs)
            {
                if (uv.x < bounds.xMin)
                    bounds.xMin = uv.x;
                else if (uv.x > bounds.xMax)
                    bounds.xMax = uv.x;

                if (uv.y < bounds.yMin)
                    bounds.yMin = uv.y;
                else if (uv.y > bounds.yMax)
                    bounds.yMax = uv.y;
            }

            return bounds;
        }

        public void AssignUV(Mesh mesh)
        {
            uvMesh = new Mesh();

            Vector2[] uvs = GetUVs(mesh);
            var vertices = new Vector3[mesh.vertexCount];
            int ct = 0;
            foreach (var uv in uvs)
            {
                vertices[ct++] = uv;
            }

            uvMesh.vertices = vertices;
            uvMesh.triangles = mesh.triangles;

            uvMesh.subMeshCount = mesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] indices = mesh.GetIndices(i);
                uvMesh.SetIndices(indices, MeshTopology.Triangles, i);
            }
        }

        void CopyTangentsToUV2(Mesh mesh)
        {
            var uv2s = mesh.uv2;
            var tangents = mesh.tangents;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                var tangent = tangents[i];

                var uv2 = new Vector2(tangent.x, tangent.y);
                uv2s[i] = uv2;
            }
            mesh.uv2 = uv2s;
        }

        void CreateStyle()
        {
            // Lazily construct GUI style first time through
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box);
            }
        }

        void DrawLabeledRect(float x, float y, float size, Color color, string label, float scale, TextAnchor alignment)
        {
            Rect pegLabel = new Rect();
            pegLabel.x = x - size;
            pegLabel.y = y - size;
            pegLabel.width = 2 * size;
            pegLabel.height = 2 * size;

            Color guiColor = GUI.color;

            GUI.color = color;
            style.normal.background = EditorGUIUtility.whiteTexture;
            GUI.Box(pegLabel, "", style);

            if (label != null)
            {
                GUI.color = Color.white;
                style.normal.background = null;
                style.normal.textColor = Color.white;
                style.alignment = alignment;

                var textSize = GUI.skin.label.CalcSize(new GUIContent(label));

                float xscale = textSize.x;
                float yscale = textSize.y;
                pegLabel.x = x - xscale;
                pegLabel.y = y - yscale;
                pegLabel.width = 2 * xscale;
                pegLabel.height = 2 * yscale;

                GUI.Box(pegLabel, label, style);
            }

            GUI.color = guiColor;
        }

        public void OnEnable()
        {

        }

        public void OnDisable()
        {

        }

        public void OnDestroy()
        {

        }

        [MenuItem("Window/UV Map Editor")]
        static void ShowUVMapEditor()
        {
            EditorWindow.GetWindow(typeof(rcUVMapEditor));
        }
    }
}