using System;
using System.Collections.Generic;
using UnityEngine;
using MIU;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using SimpleJSON;
using System.Runtime.InteropServices;

// Modified Version
// Version 1.0.0
// Author: TalentedPlatinum (aka god)

public static class TextureHelperClass
{
    public static Texture2D ChangeFormat(this Texture2D oldTexture, TextureFormat newFormat)
    {
        Texture2D newTex = new Texture2D(oldTexture.width, oldTexture.height, newFormat, false);

        newTex.SetPixels(oldTexture.GetPixels());

        newTex.Apply();

        return newTex;
    }
}

public class MIUImporter : EditorWindow
{

    private static GUIStyle bigLabel;
    private static GUIStyle smallLabel;
    private static GUIStyle bigButton;

    public static bool hasResult = false;
    public static bool imHasResult = false;
    public static bool failedLife = false;

    private static string ImportedAuthor;
    private static string ImportedHash;
    private static float ImportedSilver;
    private static float ImportedGold;
    private static float ImportedDiamond;
    private static bool ImportedLegacy;
    private static string ImportedSceneName;

    private static Vector2 scrollPos;

    public static Texture2D DeSerializeMySanity(SerializerHelper sh)
    {
        // One byte flag indicating presence/absence.
        byte oneByteFlag;
        sh.Stream.ReadByte(out oneByteFlag);
        if (oneByteFlag == (byte)0)
            return null;

        UInt16 texHeight;
        sh.Stream.ReadUInt16(out texHeight);
        //tex.height = (int)texHeight;

        UInt16 texWidth;
        sh.Stream.ReadUInt16(out texWidth);
        //tex.width = (int)texWidth;

        Int32 texFormat;
        sh.Stream.ReadInt32(out texFormat);

        ByteStream rawBytes = new ByteStream();
        sh.Stream.ReadBytes(ref rawBytes);

        Texture2D tex;

        if ((TextureFormat)texFormat == TextureFormat.DXT5Crunched)
        {
            tex = new Texture2D(texWidth, texHeight, TextureFormat.DXT1Crunched, true);
        }
        else
        {
            tex = new Texture2D(texWidth, texHeight, (TextureFormat)texFormat, true);
        }


        tex.LoadRawTextureData(rawBytes.Buffer);
        return tex;
    }

    public static Texture2D DeSerializeTexture(SerializerHelper sh)
    {
        // One byte flag indicating presence/absence.
        byte oneByteFlag;
        sh.Stream.ReadByte(out oneByteFlag);
        if (oneByteFlag == (byte)0)
            return null;

        UInt16 texHeight;
        sh.Stream.ReadUInt16(out texHeight);
        //tex.height = (int)texHeight;

        UInt16 texWidth;
        sh.Stream.ReadUInt16(out texWidth);
        //tex.width = (int)texWidth;

        Int32 texFormat;
        sh.Stream.ReadInt32(out texFormat);

        ByteStream rawBytes = new ByteStream();
        sh.Stream.ReadBytes(ref rawBytes);

        Texture2D tex;


        tex = new Texture2D(texWidth, texHeight, (TextureFormat)texFormat, true);

        tex.LoadRawTextureData(rawBytes.Buffer);
        return tex;
    }

    public static void DeSerializeLightmaps(SerializerHelper sh)
    {
        GameObject lightmapStorage = new GameObject();
        lightmapStorage.name = "lightmapStorage";
        lightmapStorage.AddComponent<IgnoreObject>();
    }

    private static void refresh(SerializerHelper inSh)
    {
        byte[] lightmapData = new byte[inSh.Stream.Buffer.Length - inSh.Stream.Position];
        System.Array.Copy(inSh.Stream.Buffer, inSh.Stream.Position, lightmapData, 0, inSh.Stream.Buffer.Length - inSh.Stream.Position);

        if (!AssetDatabase.IsValidFolder("Assets/MIU-Decompiler/Lightmaps"))
        {
            System.IO.Directory.CreateDirectory("Assets/MIU-Decompiler/Lightmaps");
            AssetDatabase.Refresh();
        }

        File.WriteAllBytes("Assets/MIU-Decompiler/Lightmaps/lightmaps.levelmaps", lightmapData);
        lightmapData = File.ReadAllBytes("Assets/MIU-Decompiler/Lightmaps/lightmaps.levelmaps");
        
        SerializerHelper sh = new SerializerHelper();
        sh.Stream = new ByteStream();
        sh.Stream.Buffer = new byte[lightmapData.Length];
        System.Array.Copy(lightmapData, sh.Stream.Buffer, lightmapData.Length);

        Texture2D lightmapColor = DeSerializeTexture(sh);
        Texture2D lightmapDir = DeSerializeTexture(sh);
        Texture2D shadowDir = DeSerializeMySanity(sh);

        if (lightmapColor == null && shadowDir == null)
        {
            return;
        }

        LightmapData[] newmaps = new LightmapData[1];
        newmaps[0] = new LightmapData();
        newmaps[0].lightmapColor = lightmapColor;
        newmaps[0].lightmapDir = lightmapDir;
        newmaps[0].shadowMask = shadowDir;

        LightmapSettings.lightmaps = newmaps;

        GameObject lightmapKeeper = new GameObject();
        lightmapKeeper.AddComponent<IgnoreObject>();
        lightmapKeeper.AddComponent<BringerOfLightmaps>();
        lightmapKeeper.name = "Lightmap loader";
        lightmapKeeper.transform.SetParent(null);
    }

    public static void fixCloudClusters()
    {
        GameObject[] objectsInScene = GameObject.FindObjectsOfType<GameObject>();

        for (int i = 0; i < objectsInScene.Length; i++)
        {
            var gmObject = objectsInScene[i];

            if (gmObject.name.StartsWith("CloudClusterSky") && gmObject.transform.parent.name == "CLOUDS")
            {
                if (gmObject.name.StartsWith("CloudClusterSky01")) {
                    GameObject prefabTemplate = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/MIU/Prefabs/FX/Clouds/CloudClusterSky01.prefab", typeof(GameObject));
                    GameObject newCloud = Instantiate(prefabTemplate);
                    newCloud.transform.position = gmObject.transform.position;
                    newCloud.transform.rotation = gmObject.transform.rotation;
                    newCloud.transform.localScale = gmObject.transform.localScale;
                    newCloud.name = gmObject.name;
                    newCloud.transform.SetParent(gmObject.transform.parent);
                    DestroyImmediate(gmObject, false);
                } else if (gmObject.name.StartsWith("CloudClusterSky02"))
                {
                    GameObject prefabTemplate = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/MIU/Prefabs/FX/Clouds/CloudClusterSky02.prefab", typeof(GameObject));
                    GameObject newCloud = Instantiate(prefabTemplate);
                    newCloud.transform.position = gmObject.transform.position;
                    newCloud.transform.rotation = gmObject.transform.rotation;
                    newCloud.transform.localScale = gmObject.transform.localScale;
                    newCloud.name = gmObject.name;
                    newCloud.transform.SetParent(gmObject.transform.parent);
                    DestroyImmediate(gmObject, false);
                } else if (gmObject.name.StartsWith("CloudClusterSky03"))
                {
                    GameObject prefabTemplate = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/MIU/Prefabs/FX/Clouds/CloudClusterSky03.prefab", typeof(GameObject));
                    GameObject newCloud = Instantiate(prefabTemplate);
                    newCloud.transform.position = gmObject.transform.position;
                    newCloud.transform.rotation = gmObject.transform.rotation;
                    newCloud.transform.localScale = gmObject.transform.localScale;
                    newCloud.name = gmObject.name;
                    newCloud.transform.SetParent(gmObject.transform.parent);
                    DestroyImmediate(gmObject, false);
                }
            }
        }
    }

    [MenuItem("Window/MIU Decompiler")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MIUImporter));
    }

    MIUImporter()
    {
        titleContent = new GUIContent("MIU Decompiler");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        bigLabel = new GUIStyle(GUI.skin.label);
        bigLabel.fontStyle = FontStyle.Bold;
        bigLabel.richText = true;

        smallLabel = new GUIStyle(GUI.skin.label);
        smallLabel.wordWrap = true;
        smallLabel.fontStyle = FontStyle.Normal;
        smallLabel.richText = true;

        bigButton = new GUIStyle(GUI.skin.button);
        bigButton.fontSize = bigLabel.fontSize;

        GUILayout.Label("Decompiler", bigLabel);

        GUILayout.Space(7);

        if (MIUExists())
        {
            if (GUILayout.Button("Fix CloudClusterSky", bigButton)) fixCloudClusters();

            if (GUILayout.Button("Decompile", bigButton))
            {
                try
                {
                    ImportScene();
                } catch (Exception e)
                {
                    EditorUtility.ClearProgressBar();

                    Debug.Log("<color=red>MIUDecompiler Couldn't successfully decompile level</color>, error: " + e);
                    imHasResult = true;
                    failedLife = true;
                }
                
            }

            if (imHasResult)
            {
                if (failedLife)
                {
                    GUILayout.Label("<color=red>Could not decompile</color>", smallLabel);
                } else
                {
                    GUILayout.Label("<color=green>Successfully decompiled level</color>", smallLabel);
                    GUILayout.Label("Name: " + ImportedSceneName, smallLabel);

                    if (!ImportedLegacy) {
                        GUILayout.Label("Author: " + ImportedAuthor, smallLabel);
                        GUILayout.Label("Level format: Modern");
                    } else { GUILayout.Label("Level format: Legacy"); }

                    GUILayout.Label("Diamond Time: " + ImportedDiamond, smallLabel);
                    GUILayout.Label("Gold Time: " + ImportedGold, smallLabel);
                    GUILayout.Label("Silver Time: " + ImportedSilver, smallLabel);
                }
            }
        } else
        {
            GUILayout.Label("MIU Level Kit Could Not Be Found, Checkout The Github: ");
            GUILayout.Label("https://github.com/MarbleItUp/MIULevelCreationKit/");
        }
        GUILayout.EndScrollView();
    }

    public static void handleObject(LevelObject child, GameObject parent)
    {
        if (("" + child.prefabItem).Length != 0 && "" + child.name != "LevelBounds" && ("" + child.name).StartsWith("CheckPoint") == false)
        {
            if (File.Exists("Assets/MIU/Internal/Resources/" + child.prefabItem + ".prefab"))
            {
                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/MIU/Internal/Resources/" + child.prefabItem + ".prefab", typeof(GameObject));
                GameObject clone = Instantiate(prefab, child.position, child.rotation) as GameObject;
                clone.transform.SetParent(parent.transform);
                clone.name = child.prefabItem;
                return;
            }
            else
            {
                GameObject prefabReplacer = new GameObject();
                prefabReplacer.transform.SetParent(parent.transform);
                prefabReplacer.name = "" + child.name;
                prefabReplacer.transform.position = child.position;
                prefabReplacer.transform.rotation = child.rotation;
                prefabReplacer.transform.localScale = child.scale;

                UnityEngine.Object preVisFab = AssetDatabase.LoadAssetAtPath("Assets/MIU-Decompiler/Prefabs/tempVis.prefab", typeof(GameObject));
                GameObject preVis = Instantiate(preVisFab, prefabReplacer.transform.position, prefabReplacer.transform.rotation) as GameObject;
                preVis.transform.SetParent(prefabReplacer.transform);
                preVis.name = "" + child.name + "_VIS";

                if (child.properties.ContainsKey(LevelObject.TUTORIAL))
                {
                    if (child.properties[LevelObject.TUTORIAL].Equals(true))
                    {
                        if (prefabReplacer.GetComponent<TutorialMessage>() == null)
                        {
                            prefabReplacer.AddComponent<TutorialMessage>();
                        }

                        if (child.properties.ContainsKey(LevelObject.TUTORIAL_MESSAGE))
                        {
                            prefabReplacer.GetComponent<TutorialMessage>().message = (string)child.properties[LevelObject.TUTORIAL_MESSAGE];
                        }

                        if (child.properties.ContainsKey(LevelObject.TUTORIAL_GRAPHIC))
                        {

                        }

                        if (child.properties.ContainsKey(LevelObject.TUTORIAL_SHOWONCE))
                        {
                            prefabReplacer.GetComponent<TutorialMessage>().ShowOnce = (bool)child.properties[LevelObject.TUTORIAL_SHOWONCE];
                        }

                        if (prefabReplacer.GetComponent<BoxCollider>() == null)
                        {
                            prefabReplacer.AddComponent<BoxCollider>();
                        }

                        var box = prefabReplacer.GetComponent<BoxCollider>();

                        box.center = child.GetVector3(LevelObject.BOX_COLLIDER_CENTER);
                        box.size = child.GetVector3(LevelObject.BOX_COLLIDER_SIZE);
                        box.isTrigger = (bool)child.properties[LevelObject.BOX_COLLIDER_ISTRIGGER];

                    }
                }
            }

            return;
        }

        GameObject newObject = new GameObject();
        newObject.name = child.name;
        newObject.transform.position = child.position;
        newObject.transform.rotation = child.rotation;
        newObject.transform.localScale = child.scale;

        MeshFilter meshHandler;
        Mesh newMesh = new Mesh();

        var vertByteSize =
            3 * 4 + // Position
            (child.mesh.HasNormal ? 3 * 4 : 0) + // Normal
            (child.mesh.HasUV1 ? 2 * 4 : 0) + // UV1
            (child.mesh.HasUV2 ? 2 * 4 : 0) + // UV2
            (child.mesh.HasTangents ? 4 * 4 : 0) // Tangent
            ;

        var stagingFloats = new float[child.mesh.vertexCount * 4];
        List<Vector3> verts = new List<Vector3>();

        var offset = 0;
        offset = MemoryMapUtility.MapBytesToVector3(child.mesh.vertexCount, 0, child.mesh.verts.Stream.GetBuffer(), stagingFloats, verts);

        newMesh.vertices = verts.ToArray();

        newMesh.bounds = child.mesh.bounds;

        if (child.mesh.HasNormal)
        {
            List<Vector3> normals = new List<Vector3>();
            offset += MemoryMapUtility.MapBytesToVector3(child.mesh.vertexCount, offset, child.mesh.verts.Stream.GetBuffer(), stagingFloats, normals);

            newMesh.normals = normals.ToArray();
        }

        if (child.mesh.HasUV1)
        {
            List<Vector2> UV1 = new List<Vector2>();
            offset += MemoryMapUtility.MapBytesToVector2(child.mesh.vertexCount, offset, child.mesh.verts.Stream.GetBuffer(), stagingFloats, UV1);

            newMesh.uv = UV1.ToArray();
        }

        if (child.mesh.HasUV2)
        {
            List<Vector2> UV2 = new List<Vector2>();
            offset += MemoryMapUtility.MapBytesToVector2(child.mesh.vertexCount, offset, child.mesh.verts.Stream.GetBuffer(), stagingFloats, UV2);

            newMesh.uv2 = UV2.ToArray();
        }

        if (child.mesh.HasTangents)
        {
            List<Vector4> Tangent = new List<Vector4>();
            offset += MemoryMapUtility.MapBytesToVector4(child.mesh.vertexCount, offset, child.mesh.verts.Stream.GetBuffer(), stagingFloats, Tangent);

            newMesh.tangents = Tangent.ToArray();
        }
        
        var shortIdx = new ushort[child.mesh.indexCount];
        Buffer.BlockCopy(child.mesh.indices.Stream.GetBuffer(), 0, shortIdx, 0, child.mesh.indexCount * 2);

        var intIdx = new int[child.mesh.indexCount];

        for (var i = 0; i < child.mesh.indexCount; i++)
        {
            intIdx[i] = (int)shortIdx[i];
        }

        newMesh.SetIndices(intIdx, MeshTopology.Triangles, 0);

        newMesh.subMeshCount = child.mesh.subMeshes.Count;

        for (var subMeshIndex = 0; subMeshIndex < child.mesh.subMeshes.Count; subMeshIndex++)
        {
            LevelSubMesh currentSubMesh = child.mesh.subMeshes[subMeshIndex];
            newMesh.SetSubMesh(subMeshIndex, new SubMeshDescriptor(currentSubMesh.start, currentSubMesh.count, MeshTopology.Triangles));
        }

        if (child.name != "StartPad" && child.name != "EndPad")
        {

            if (child.properties.ContainsKey(LevelObject.MESH_COLLIDER))
            {
                if (child.properties[LevelObject.MESH_COLLIDER].Equals(true))
                {
                    newObject.AddComponent<MeshCollider>();
                }
            }

            if (child.properties.ContainsKey(LevelObject.BOX_COLLIDER))
            {
                if (child.properties[LevelObject.BOX_COLLIDER].Equals(true))
                {
                    newObject.AddComponent<BoxCollider>();
                    var box = newObject.GetComponent<BoxCollider>();

                    box.center = child.GetVector3(LevelObject.BOX_COLLIDER_CENTER);
                    box.size = child.GetVector3(LevelObject.BOX_COLLIDER_SIZE);
                    box.isTrigger = (bool)child.properties[LevelObject.BOX_COLLIDER_ISTRIGGER];

                }
            }

        }

        if (child.properties.ContainsKey(LevelObject.TUTORIAL))
        {
            if (child.properties[LevelObject.TUTORIAL].Equals(true))
            {
                if (newObject.GetComponent<TutorialMessage>() == null)
                {
                    newObject.AddComponent<TutorialMessage>();
                }

                if (child.properties.ContainsKey(LevelObject.TUTORIAL_MESSAGE))
                {
                    newObject.GetComponent<TutorialMessage>().message = (string)child.properties[LevelObject.TUTORIAL_MESSAGE];
                }

                if (child.properties.ContainsKey(LevelObject.TUTORIAL_GRAPHIC))
                {
                    Debug.Log(child.properties[LevelObject.TUTORIAL_GRAPHIC]);
                }

                if (child.properties.ContainsKey(LevelObject.TUTORIAL_SHOWONCE))
                {
                    newObject.GetComponent<TutorialMessage>().ShowOnce = (Boolean)child.properties[LevelObject.TUTORIAL_SHOWONCE];
                }

            }
        }

        if (child.properties.ContainsKey(LevelObject.CHECKPOINT))
        {
            if (child.properties[LevelObject.CHECKPOINT].Equals(true))
            {
                newObject.name = "CheckPoint";
            }
            
        }

        if (child.properties.ContainsKey(LevelObject.MESH_RENDERER))
        {
            if (child.properties[LevelObject.MESH_RENDERER].Equals(true))
            {
                newObject.AddComponent<MeshRenderer>();

                int amountFound = 0;
                bool[] matFinds = new bool[child.mesh.materials.Count];
                var mats = new Material[child.mesh.materials.Count];
                int matCounter = 0;
                int idCounter = 0;

                foreach (var mat in child.mesh.materials)
                {
                    matFinds[idCounter] = false;
                    List<string> files = new List<string>();
                    files.AddRange(Directory.GetFiles("Assets/MIU/", "*.mat", SearchOption.AllDirectories));
                    files.AddRange(Directory.GetFiles("Assets/MIU-Decompiler/", "*.mat", SearchOption.AllDirectories));
                    foreach (string fileStr in files)
                    {
                        if (fileStr.EndsWith(mat + ".mat"))
                        {
                            mats[matCounter] = (Material)AssetDatabase.LoadAssetAtPath(fileStr, typeof(Material));
                            matCounter += 1;

                            matFinds[idCounter] = true;
                            amountFound += 1;
                            break;
                        }
                    }

                    idCounter += 1;
                }

                if (amountFound != 0)
                {
                    newObject.GetComponent<MeshRenderer>().sharedMaterials = mats;
                }

                idCounter = 0;

                foreach (var matFound in matFinds)
                {
                    if (!matFound)
                    {
                        Material replacer = new Material(Shader.Find("Specular"));
                        replacer.name = child.mesh.materials[idCounter];
                        mats[amountFound + idCounter] = replacer;

                        idCounter += 1;
                    }
                }

                if (idCounter != 0)
                {
                    newObject.GetComponent<MeshRenderer>().sharedMaterials = mats;
                }

                if (child.name.StartsWith("StaticMesh_"))
                {
                    newObject.isStatic = true;
                }

                if (newObject.isStatic)
                {
                    newObject.GetComponent<MeshRenderer>().lightmapIndex = child.mesh.lightmapIndex;
                    newObject.GetComponent<MeshRenderer>().lightmapScaleOffset = child.mesh.lightmapScaleOffset;
                    newObject.AddComponent<StorageL>();
                    newObject.GetComponent<StorageL>().lightmapIndex = child.mesh.lightmapIndex;
                    newObject.GetComponent<StorageL>().lightmapScaleOffset = child.mesh.lightmapScaleOffset;
                }

            }
        }

        if (child.properties.ContainsKey(LevelObject.MOVER))
        {
            if (child.properties[LevelObject.MOVER].Equals(true))
            {
                var p = child.properties;

                newObject.AddComponent<ElevatorMover>();
                var mover = newObject.GetComponent<ElevatorMover>();

                mover.mode = (ElevatorMover.Mode)(int)p[LevelObject.MOVER_MODE];
                mover.WaitForTouched = (bool)p[LevelObject.MOVER_COLLAPSETRIGGERED];
                mover.StartOffsetTime = (float)p[LevelObject.MOVER_STARTOFFSETTIME];
                mover.delta = child.GetVector3(LevelObject.MOVER_DELTA);
                mover.deltaRotation = child.GetVector3(LevelObject.MOVER_DELTAROTATION);
                mover.pauseTime = (float)p[LevelObject.MOVER_PAUSETIME];
                mover.moveTime = (float)p[LevelObject.MOVER_MOVETIME];
                mover.moveFirst = (bool)p[LevelObject.MOVER_MOVEFIRST];
                mover.splineSpeed = (float)p[LevelObject.MOVER_SPLINESPEED];
                mover.KeepOrientation = (bool)p[LevelObject.MOVER_KEEPORIENTATION];

                if (p.ContainsKey(LevelObject.MOVER_ENABLEBOB))
                {
                    mover.EnableBob = (bool)p[LevelObject.MOVER_ENABLEBOB];
                } else
                {
                    mover.EnableBob = false;
                }

                if (mover.EnableBob)
                {
                    mover.BobOffset = (float)p[LevelObject.MOVER_BOBOFFSET];
                    mover.BobPeriod = (float)p[LevelObject.MOVER_BOBPERIOD];
                    mover.BobVector = child.GetVector3(LevelObject.MOVER_BOBVECTOR);
                }

                if (mover.mode == ElevatorMover.Mode.Spline)
                {
                    var splineCount = (int)p[LevelObject.SPLINE_COUNT];
                    GameObject splineDrawer = new GameObject();
                    splineDrawer.name = "SplineDrawer";
                    splineDrawer.AddComponent<SplineDrawer>();

                    mover.splineGo = splineDrawer;

                    for (var i = 0; i < splineCount; i++)
                    {
                        GameObject splinePoint = new GameObject();
                        splinePoint.transform.position = child.GetVector3(LevelObject.SPLINE_POSITION + i);
                        splinePoint.transform.SetParent(mover.splineGo.transform);
                    }
                }
            }
        }

        if (child.properties.ContainsKey(LevelObject.PARTICLE_SYSTEM))
        {
            newObject.AddComponent<ParticleSystem>();
            ParticleSystem MTA = newObject.GetComponent<ParticleSystem>();

            SimpleBuffer sb = (SimpleBuffer)child.properties[LevelObject.PARTICLE_SYSTEM];

            var sh = new SerializerHelper();
            sh.Stream = new ByteStream();
            sh.Stream.Buffer = sb.Stream.GetBuffer();
            sh.Read(ref MTA);

            Material replacer = new Material(Shader.Find("Specular"));
            replacer.name = (string)child.properties[LevelObject.PARTICLE_SYSTEM_MATERIAL];

            if (newObject.GetComponent<ParticleSystemRenderer>() == null)
            {
                newObject.AddComponent<ParticleSystemRenderer>();
            }

            newObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = replacer;
        }

        if (child.properties.ContainsKey(LevelObject.MESH_TUNNEL_ANIMATOR))
        {
            newObject.AddComponent<MeshTunnelAnimator>();
            MeshTunnelAnimator MTA = newObject.GetComponent<MeshTunnelAnimator>();

            SimpleBuffer sb = (SimpleBuffer)child.properties[LevelObject.MESH_TUNNEL_ANIMATOR];

            var sh = new SerializerHelperImporter();
            sh.Stream = new ByteStream();
            sh.Stream.Buffer = sb.Stream.GetBuffer();
            sh.Read(ref MTA);

            Material replacer = new Material(Shader.Find("Specular"));
            replacer.name = (string)child.properties[LevelObject.MESH_TUNNEL_ANIMATOR_MATERIAL];

            MTA.OverrideMaterial = replacer;
            MTA.DisplayMesh = newObject;

            newObject.name = (string)child.properties[LevelObject.MESH_TUNNEL_ANIMATOR_DISPLAYMESH];

            if (!AssetDatabase.IsValidFolder("Assets/MIU-Decompiler/AUTO_MTA_PREFABS"))
            {
                System.IO.Directory.CreateDirectory("Assets/MIU-Decompiler/AUTO_MTA_PREFABS");
                AssetDatabase.Refresh();
            }

            bool found = false;
            string prefabLocation = "";
            string MTAPrefab = (string)child.properties[LevelObject.MESH_TUNNEL_ANIMATOR_DISPLAYMESH];
            
            string[] files = Directory.GetFiles("Assets/MIU-Decompiler/AUTO_MTA_PREFABS", "*.prefab", SearchOption.AllDirectories);
            foreach (string fileStr in files)
            {
                if (fileStr.EndsWith(MTAPrefab + ".prefab"))
                {
                    prefabLocation = fileStr;

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                GameObject prefabNew = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prefabNew.name = MTAPrefab;
                prefabNew.AddComponent<MeshRenderer>();
                prefabNew.GetComponent<MeshRenderer>().material = replacer;
                PrefabUtility.SaveAsPrefabAsset(prefabNew, "Assets/MIU-Decompiler/AUTO_MTA_PREFABS/" + MTAPrefab + ".prefab");
                prefabLocation = "Assets/MIU-Decompiler/AUTO_MTA_PREFABS/" + MTAPrefab + ".prefab";
                GameObject.DestroyImmediate(prefabNew, false);
            }

            GameObject MTAPrefabMesh = (GameObject)AssetDatabase.LoadAssetAtPath(prefabLocation, typeof(GameObject));

            MTA.DisplayMesh = MTAPrefabMesh;

            // Respawn needs special permission, non modified versions do not have this
            // MTA.Respawn();
        }

        if (child.name != "StartPad" && child.name != "EndPad")
        {
            if (newMesh.vertexCount > 0)
            {
                newObject.AddComponent<MeshFilter>();
                meshHandler = newObject.GetComponent<MeshFilter>();
                meshHandler.sharedMesh = newMesh;
            }

        }

        newObject.transform.SetParent(parent.transform);

        foreach (var child2 in child.children)
        {
            handleObject(child2, newObject);
        }
    }

    public static void ImportScene()
    {
        imHasResult = false;
        failedLife = true;

        var bigLabel = new GUIStyle(GUI.skin.label);
        bigLabel.fontStyle = FontStyle.Bold;
        bigLabel.richText = true;

        var smallLabel = new GUIStyle(GUI.skin.label);
        smallLabel.wordWrap = true;
        smallLabel.fontStyle = FontStyle.Normal;
        smallLabel.richText = true;

        var bigButton = new GUIStyle(GUI.skin.button);
        bigButton.fontSize = bigLabel.fontSize;

        Debug.Log("Decompiling level...");
        string filePath = EditorUtility.OpenFilePanel("Level file", "", "level");

        if (filePath.Length != 0)
        {
            Debug.Log("File found...");
        }
        else { failedLife = true; imHasResult = true; Debug.Log("<color=red>Decompiling failed:</color> File not found"); return; }
        
        //Status
        var sh = new SerializerHelper();
        sh.Stream = new ByteStream();

        sh.Stream.Buffer = File.ReadAllBytes(filePath);

        byte[] pog = { (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0 };
        byte outByte2;
        sh.Stream.Position = 0;
        sh.Stream.ReadByte(out outByte2);
        pog[0] = outByte2;
        sh.Stream.ReadByte(out outByte2);
        pog[1] = outByte2;
        sh.Stream.ReadByte(out outByte2);
        pog[2] = outByte2;
        sh.Stream.ReadByte(out outByte2);
        pog[3] = outByte2;
        sh.Stream.ReadByte(out outByte2);
        pog[4] = outByte2;
        sh.Stream.ReadByte(out outByte2);
        pog[5] = outByte2;

        float outSilver;
        float outGold;
        float outDiamond;

        sh.Stream.ReadSingle(out outSilver);
        sh.Stream.ReadSingle(out outGold);
        sh.Stream.ReadSingle(out outDiamond);

        bool legacy = false;
        string outAuthor = "";

        if ((char)pog[3] == '5')
        {
            legacy = true;
        } else if ((char)pog[3] == '7')
        {
            Debug.Log("Unable to load in level timing format, this format is currently not supported");
            throw new Exception("Invalid level format");
        }
        else
        {
            sh.Stream.ReadString(out outAuthor);
        }

        string outPhysParams;

        sh.Stream.ReadString(out outPhysParams);

        // Mayhem Lock
        if (pog[2] != (byte)'l')
        {
            Debug.Log("Mayhem levels are currently not supported, check the github for when that becomes available");
            throw new Exception("Mayhem is not supported"); 
        }

        string outHash;

        sh.Stream.ReadString(out outHash);

        LevelScene outScene = new LevelScene();

        sh.Read(ref outScene);

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject mainLight = GameObject.Find("Directional Light");
        mainLight.transform.forward = (outScene.sunDirection);

        GameObject container = new GameObject();
        container.name = "Container";

        int childIndex = 0;
        int totalChildren = outScene.root.children.Count;

        foreach (LevelObject child in outScene.root.children)
        {
            EditorUtility.DisplayProgressBar("Decompiling scene", "Loading level objects", childIndex / totalChildren);
            handleObject(child, container);
            childIndex += 1;
        }
        EditorUtility.ClearProgressBar();

        RenderSettings.skybox = ((Material)AssetDatabase.LoadAssetAtPath("Assets/MIU-Decompiler/Dependencies/Sky001.mat", typeof(Material)));
        RenderSettings.skybox.name = outScene.skyboxId.ToString();

        Camera.main.transform.position = outScene.previewPosition;
        Camera.main.transform.rotation = outScene.previewOrientation;

        ImportedAuthor = outAuthor;
        ImportedHash = outHash;
        ImportedSilver = outSilver;
        ImportedGold = outGold;
        ImportedDiamond = outDiamond;
        ImportedLegacy = legacy;
        ImportedSceneName = outScene.name;

        GameObject levelTiming = new GameObject();
        levelTiming.name = "LevelTiming";
        levelTiming.AddComponent<LevelTiming>();
        levelTiming.GetComponent<LevelTiming>().GoldTime = outGold;
        levelTiming.GetComponent<LevelTiming>().DiamondTime = outDiamond;
        levelTiming.GetComponent<LevelTiming>().SilverTime = outSilver;

        levelTiming.GetComponent<LevelTiming>().Author = outAuthor;

        if (outPhysParams.Length != 0)
        {
            levelTiming.AddComponent<PlayModifiers>();

            var JSONParsed = JSON.Parse(outPhysParams);
           
            if (JSONParsed != null)
            {
                levelTiming.GetComponent<PlayModifiers>().FrictionMult = JSONParsed["frictionmult"].AsFloat;
                levelTiming.GetComponent<PlayModifiers>().GravityMult = JSONParsed["gravity"].AsFloat;
                levelTiming.GetComponent<PlayModifiers>().RollForceMult = new Vector2(JSONParsed["rollX"].AsFloat, JSONParsed["rollY"].AsFloat);
                levelTiming.GetComponent<PlayModifiers>().ScaleMult = JSONParsed["scalemult"].AsFloat;
                levelTiming.GetComponent<PlayModifiers>().AirForceMult = new Vector2(JSONParsed["airX"].AsFloat, JSONParsed["airY"].AsFloat);
                levelTiming.GetComponent<PlayModifiers>().AirJumps = JSONParsed["airjumps"].AsInt;
                levelTiming.GetComponent<PlayModifiers>().BounceMult = JSONParsed["bouncemult"].AsFloat;
                levelTiming.GetComponent<PlayModifiers>().CanBlast = JSONParsed["canblast"].AsBool;
                levelTiming.GetComponent<PlayModifiers>().JumpForceMult = JSONParsed["jumpmult"].AsFloat;
            }


        }

        refresh(sh);

        imHasResult = true;
        failedLife = false;
        Debug.Log("<color=green>Level imported: </color>" + outScene.name);

    }

    private bool MIUExists()
    {
        if (AssetDatabase.IsValidFolder("Assets/MIU"))
        {
            return true;
        }

        return false;
    }

}
