using MIU;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class BringerOfLightmaps : MonoBehaviour
{
    void Awake()
    {
        updateLightmaps();
    }

    public static void updateLightmaps()
    {
        if (!File.Exists("Assets/MIU-Decompiler/Lightmaps/lightmaps.levelmaps"))
            return;

        byte[] lightmapData = File.ReadAllBytes("Assets/MIU-Decompiler/Lightmaps/lightmaps.levelmaps");

        SerializerHelper sh = new SerializerHelper();
        sh.Stream = new ByteStream();
        sh.Stream.Buffer = new byte[lightmapData.Length];
        System.Array.Copy(lightmapData, sh.Stream.Buffer, lightmapData.Length);

        StorageL[] lmDataList = GameObject.FindObjectsOfType<StorageL>();

        foreach (var lmData in lmDataList)
        {
            MeshRenderer mr = lmData.gameObject.GetComponent<MeshRenderer>();
            mr.lightmapIndex = lmData.lightmapIndex;
            mr.lightmapScaleOffset = lmData.lightmapScaleOffset;
        }
        
        Texture2D lightmapColor = DeSerializeTexture(sh);
        Texture2D lightmapDir = DeSerializeTexture(sh);
        Texture2D shadowDir = DeSerializeMySanity(sh);

        LightmapData[] newmaps = new LightmapData[1];
        newmaps[0] = new LightmapData();
        newmaps[0].lightmapColor = lightmapColor;
        newmaps[0].lightmapDir = lightmapDir;
        newmaps[0].shadowMask = shadowDir;

        LightmapSettings.lightmaps = newmaps;

        lmDataList = GameObject.FindObjectsOfType<StorageL>();

        foreach (var lmData in lmDataList)
        {
            MeshRenderer mr = lmData.gameObject.GetComponent<MeshRenderer>();
            mr.lightmapIndex = lmData.lightmapIndex;
            mr.lightmapScaleOffset = lmData.lightmapScaleOffset;
        }
    }

    public static Texture2D DeSerializeMySanity(SerializerHelper sh)
    {
        // One byte flag indicating presence/absence.
        byte oneByteFlag;
        sh.Stream.ReadByte(out oneByteFlag);
        if (oneByteFlag == (byte)0)
            return new Texture2D(0, 0);

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
            return new Texture2D(0, 0);

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
}
