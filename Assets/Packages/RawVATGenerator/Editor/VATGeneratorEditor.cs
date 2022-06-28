using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace VATGenerator
{
    public class VATGeneratorEditor : EditorWindow
    {
        #region Field

        private float _fps = 30f;

        #endregion Field

        #region Method

        [MenuItem("Custom/VATGenerator")]
        private static void Init()
        {
            GetWindow<VATGeneratorEditor>("VATGenerator");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("FPS");
            _fps = EditorGUILayout.FloatField(_fps);
            EditorGUILayout.EndHorizontal();

            if (!GUILayout.Button("Generate"))
            {
                return;
            }

            var selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog
                    ("VATGenerator", "Failed : Select GameObject", "OK");
                return;
            }

            var renderer  = selectedObject.GetComponentInChildren<SkinnedMeshRenderer>();
            var animation = selectedObject.GetComponentInChildren<Animation>();

            if (renderer == null)
            {
                EditorUtility.DisplayDialog
                    ("VATGenerator", "Failed : Select GameObject has no SkinnedMeshRenderer", "OK");
                return;
            }

            if (animation == null)
            {
                EditorUtility.DisplayDialog
                    ("VATGenerator", "Failed : Select GameObject has no Animation", "OK");
                return;
            }

            EditorCoroutineUtility.StartCoroutine
                (GenerateVertexTexture(selectedObject.name, renderer, animation, _fps), this);
        }

        private IEnumerator GenerateVertexTexture(string              name,
                                                  SkinnedMeshRenderer renderer,
                                                  Animation           animation,
                                                  float               fps)
        {
            var directoryPath = GetSaveDirectoryPath(name);

            var vats = new List<VertexAnimationTexture>(animation.GetClipCount());

            foreach (AnimationState animationState in animation)
            {
                vats.Add(new VertexAnimationTexture(renderer, animation, animationState, fps));
            }

            var materials = new List<Material>(vats.Count);
            var bounds    = vats[0].Bounds;

            foreach (var vat in vats)
            {
                // Save Texture

                var vatName    = vats.Count == 0 ? name : name + "_" + vat.Name;
                var posTexPath = Path.Combine(directoryPath, vatName + ".asset");
                var nmlTexPath = Path.Combine(directoryPath, vatName + "_normal.asset");

                AssetDatabase.CreateAsset(vat.PosTex, posTexPath);
                AssetDatabase.CreateAsset(vat.NmlTex, nmlTexPath);

                // Save Material

                var material = new Material(Shader.Find(VATShader.NAME))
                {
                    mainTexture = renderer.sharedMaterial.mainTexture
                };
                material.SetTexture(VATShader.ANIMTEX,        vat.PosTex);
                material.SetTexture(VATShader.ANIMTEX_NORMAL, vat.NmlTex);
                material.SetVector (VATShader.ANIMTEX_LENGTH, new Vector2(vat.LengthSec, vat.FrameCounts));
                material.SetFloat  (VATShader.ANIMTEX_FPS,    fps);

                AssetDatabase.CreateAsset(material, Path.Combine(directoryPath, vatName + ".mat"));
                materials.Add(material);

                // Update Bounds

                bounds = UpdateBounds(bounds, vat.Bounds);
            }

            // Save Mesh

            var mesh = Instantiate(renderer.sharedMesh);
                mesh.bounds = bounds;

            AssetDatabase.CreateAsset(mesh, Path.Combine(directoryPath, name + ".asset"));

            // Save Prefab

            // NOTE:
            // If there are lot of materials, Unity sometimes lost the reference
            // even if AssetDatabase.SaveAssets/Refresh are done. So needs reload.

            var materialName = materials.Count == 0 ?
                               materials[0].name :
                               materials.FirstOrDefault(mat => mat.name.Contains(animation.clip.name)).name;
            materialName += ".mat";

            var defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(directoryPath, materialName));

            var vatObject = new GameObject(name);
                vatObject.AddComponent<MeshRenderer>().sharedMaterial = defaultMaterial;
                vatObject.AddComponent<MeshFilter>().sharedMesh       = mesh;

            PrefabUtility.SaveAsPrefabAsset(vatObject, Path.Combine(directoryPath, name + ".prefab"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            yield return null;
        }

        private static string GetSaveDirectoryPath(string name)
        {
            const string dirAssets = "Assets";
            const string dirRoot   = "VAT";
        
            var directoryPath = Path.Combine(dirAssets, dirRoot);

            if (!Directory.Exists(directoryPath))
            {
                AssetDatabase.CreateFolder(dirAssets, dirRoot);
            }

            var guid = AssetDatabase.CreateFolder(directoryPath, name);

            directoryPath = AssetDatabase.GUIDToAssetPath(guid);

            return directoryPath;
        }

        private static Bounds UpdateBounds(Bounds boundsA, Bounds boundsB)
        {
            var minA = boundsA.min;
            var maxA = boundsA.max;
            var minB = boundsB.min;
            var maxB = boundsB.max;

            var bounds = new Bounds();

            bounds.SetMinMax(new Vector3(Mathf.Min(minA.x, minB.x),
                                         Mathf.Min(minA.y, minB.y),
                                         Mathf.Min(minA.z, minB.z)),
                             new Vector3(Mathf.Max(maxA.x, maxB.x),
                                         Mathf.Max(maxA.y, maxB.y),
                                         Mathf.Max(maxA.z, maxB.z)));
            return bounds;
        }

        #endregion Method
    }
}