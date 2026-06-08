using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using UnityEngine;


//MADE BY kubinka0505

public static class AtlasDecoder
{
    [Serializable]
    public class DecodedSprite
    {
        public string Name;
        public Sprite Sprite;
        public Texture2D Texture;
    }

    private class FrameData
    {
        public string Name;
        public RectInt Box;
        public Vector2Int RealSize;
        public bool Rotated;
        public RectInt ResultBox;
    }

    public static Dictionary<string, List<Sprite>> Cache = new Dictionary<string, List<Sprite>>();

    public static void ClearCache()
    {
        foreach (var sprites in Cache.Values)
        {
            foreach (var sprite in sprites)
            {
                UnityEngine.Object.Destroy(sprite);
            }
        }

        Cache.Clear();
    }

    // PUBLIC API
    public static List<Sprite> Decode(
        string atlasAssetPath,
        string imageAssetPath, 
        float pivotX = 0, 
        float pivotY = 1
    )
    {
        if (Cache.ContainsKey(atlasAssetPath))
        {
            return Cache[atlasAssetPath];
        }

        List<DecodedSprite> decoded =
            DecodeDetailed(
                atlasAssetPath,
                imageAssetPath,
                pivotX,
                pivotY
            );

        List<Sprite> sprites = new List<Sprite>();

        foreach (var d in decoded)
            sprites.Add(d.Sprite);

        Cache[atlasAssetPath] = sprites;

        return sprites;
    }

    public static List<DecodedSprite> DecodeDetailed(
        string atlasAssetPath,
        string imageAssetPath, 
        float pivotX = 0, 
        float pivotY = 1
    )
    {
        string atlasText =
    ResourcesLoader.LoadText(atlasAssetPath);

        if (string.IsNullOrEmpty(atlasText))
        {
            throw new FileNotFoundException(
                "Atlas file not found or empty",
                atlasAssetPath
            );
        }

            
        Texture2D atlasTexture =
            ResourcesLoader.LoadTexture2D(imageAssetPath);

        if (atlasTexture == null)
        {
            throw new Exception(
                "Failed to load texture: " +
                imageAssetPath
            );
        }

        Dictionary<string, FrameData> frames;

        if (atlasText.TrimStart().StartsWith("{"))
            frames = ParseJson(atlasText);
        else
            frames = ParsePlist(atlasText);

        List<DecodedSprite> result =
            new List<DecodedSprite>();

        foreach (var kv in frames)
        {
            Texture2D tex =
                BuildTexture(
                    atlasTexture,
                    kv.Value
                );

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(
                    0,
                    0,
                    tex.width,
                    tex.height
                ),
                new Vector2(pivotX, pivotY),
                1,
                0,
                SpriteMeshType.FullRect
            );

            sprite.name = kv.Key;

            result.Add(new DecodedSprite
            {
                Name = kv.Key,
                Sprite = sprite,
                Texture = tex
            });
        }

        result.Sort((a, b) =>
        {
            MatchCollection ma = Regex.Matches(a.Name, @"\d+");
            MatchCollection mb = Regex.Matches(b.Name, @"\d+");

            int count = Mathf.Min(ma.Count, mb.Count);

            for (int i = 0; i < count; i++)
            {
                int na = int.Parse(ma[i].Value);
                int nb = int.Parse(mb[i].Value);

                int compare = na.CompareTo(nb);

                if (compare != 0)
                    return compare;
            }

            return a.Name.CompareTo(b.Name);
        });

        return result;
    }

    // BUILD TEXTURE
    private static Texture2D BuildTexture(Texture2D atlas, FrameData frame)
    {
        // crop from atlas
        Color[] crop = atlas.GetPixels(
            frame.Box.x,
            atlas.height - frame.Box.y - frame.Box.height,
            frame.Box.width,
            frame.Box.height
        );

        // full canvas
        Texture2D result = new Texture2D(
            frame.RealSize.x,
            frame.RealSize.y,
            TextureFormat.RGBA32,
            false
        );

        Color[] empty = new Color[
            frame.RealSize.x * frame.RealSize.y
        ];

        for (int i = 0; i < empty.Length; i++)
            empty[i] = new Color(0, 0, 0, 0);

        result.SetPixels(empty);

        int startX = frame.ResultBox.x;

        int startY =
            frame.RealSize.y
            - frame.ResultBox.y
            - frame.Box.height;

        if (
            startX < 0 ||
            startY < 0 ||
            startX + frame.Box.width > result.width ||
            startY + frame.Box.height > result.height
        )
        {
            Debug.LogError(
                $"Sprite outside bounds: {frame.Name}\n" +
                $"Result texture: {result.width}x{result.height}\n" +
                $"SetPixels: x={startX}, y={startY}, w={frame.Box.width}, h={frame.Box.height}\n" +
                $"ResultBox: {frame.ResultBox}\n" +
                $"RealSize: {frame.RealSize}\n" +
                $"Rotated: {frame.Rotated}"
            );

            return result;
        }

        result.SetPixels(
            startX,
            startY,
            frame.Box.width,
            frame.Box.height,
            crop
        );

        result.wrapMode = TextureWrapMode.Clamp;
        result.filterMode = FilterMode.Trilinear;

        result.Apply();

        // rotation AFTER placement (matches python behavior)
        if (frame.Rotated)
            result = Rotate90Canvas(result);

        return result;
    }

    // ROTATION
    private static Texture2D Rotate90Canvas(Texture2D source)
    {
        int w = source.width;
        int h = source.height;

        Texture2D result =
            new Texture2D(h, w, TextureFormat.RGBA32, false);

        Color[] src = source.GetPixels();
        Color[] dst = new Color[src.Length];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                dst[x * h + (h - y - 1)] =
                    src[y * w + x];
            }
        }

        result.SetPixels(dst);
        result.Apply();

        return result;
    }

    // JSON
    [Serializable]
    private class JsonRoot
    {
        public JsonFrame[] frames;
    }

    [Serializable]
    private class JsonFrame
    {
        public string filename;
        public bool rotated;
        public JsonRect frame;
        public JsonRect sourceSize;
    }

    [Serializable]
    private class JsonRect
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }

    private static Dictionary<string, FrameData>
        ParseJson(string json)
    {
        JsonRoot data =
            JsonUtility.FromJson<JsonRoot>(json);

        Dictionary<string, FrameData> result =
            new Dictionary<string, FrameData>();

        foreach (var f in data.frames)
        {
            bool rotated = f.rotated;

            int width =
                rotated
                    ? f.frame.h
                    : f.frame.w;

            int height =
                rotated
                    ? f.frame.w
                    : f.frame.h;

            int realWidth =
                rotated
                    ? f.sourceSize.h
                    : f.sourceSize.w;

            int realHeight =
                rotated
                    ? f.sourceSize.w
                    : f.sourceSize.h;

            FrameData frame =
                new FrameData
                {
                    Box = new RectInt(
                        f.frame.x,
                        f.frame.y,
                        width,
                        height
                    ),

                    RealSize =
                        new Vector2Int(
                            realWidth,
                            realHeight
                        ),

                    Rotated = rotated,

                    ResultBox =
                        new RectInt(
                            (realWidth - width) / 2,
                            (realHeight - height) / 2,
                            width,
                            height
                        )
                };

            result[f.filename] = frame;
        }

        return result;
    }

    // PLIST
    private static Dictionary<string, FrameData>
        ParsePlist(string xml)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        XmlNode plist =
            doc.DocumentElement;

        XmlNode dict = plist["dict"];

        Dictionary<string, object> root =
            PlistToDictionary(dict);

        Dictionary<string, object> frames =
            (Dictionary<string, object>)
                root["frames"];

        bool format3 = false;

        if (root.ContainsKey("metadata"))
        {
            var metadata =
                (Dictionary<string, object>)
                    root["metadata"];

            if (metadata.ContainsKey("format"))
            {
                format3 =
                    Convert.ToInt32(
                        metadata["format"]
                    ) == 3;
            }
        }

        Dictionary<string, FrameData> result =
            new Dictionary<string, FrameData>();

        foreach (var kv in frames)
        {
            string name = kv.Key;

            Dictionary<string, object> frame =
                (Dictionary<string, object>)
                    kv.Value;

            if (format3)
            {
                frame =
                    new Dictionary<string, object>()
                    {
                        {
                            "frame",
                            frame["textureRect"]
                        },
                        {
                            "rotated",
                            frame["textureRotated"]
                        },
                        {
                            "sourceSize",
                            frame["spriteSourceSize"]
                        },
                        {
                            "offset",
                            frame["spriteOffset"]
                        }
                    };
            }

            List<float> rect =
                ParseList(
                    (string)frame["frame"]
                );

            bool rotated =
                Convert.ToBoolean(
                    frame["rotated"]
                );

            int width =
                (int)rect[
                    rotated ? 3 : 2
                ];

            int height =
                (int)rect[
                    rotated ? 2 : 3
                ];

            int posX = (int)rect[0];
            int posY = (int)rect[1];

            List<float> real =
                ParseList(
                    (string)frame["sourceSize"]
                );

            int realWidth =
                (int)real[
                    rotated ? 1 : 0
                ];

            int realHeight =
                (int)real[
                    rotated ? 0 : 1
                ];

            List<float> offset =
                ParseList(
                    (string)frame["offset"]
                );

            int offsetX =
                (int)offset[
                    rotated ? 1 : 0
                ];

            int offsetY =
                (int)offset[
                    rotated ? 0 : 1
                ];

            RectInt resultBox =
                CenterBox(
                    width,
                    height,
                    realWidth,
                    realHeight,
                    offsetX,
                    offsetY,
                    rotated
                );

            result[name] =
                new FrameData
                {
                    Box = new RectInt(
                        posX,
                        posY,
                        width,
                        height
                    ),

                    RealSize =
                        new Vector2Int(
                            realWidth,
                            realHeight
                        ),

                    Rotated = rotated,

                    ResultBox = resultBox
                };
        }

        return result;
    }

    // HELPERS
    private static RectInt CenterBox(
        int width,
        int height,
        int realWidth,
        int realHeight,
        int offsetX,
        int offsetY,
        bool rotated
    )
    {
        if (rotated)
            offsetY = -offsetY;

        return new RectInt(
            (realWidth - width) / 2 + offsetX,
            (realHeight - height) / 2 - offsetY,
            width,
            height
        );
    }

    private static List<float> ParseList(
        string s
    )
    {
        List<float> result =
            new List<float>();

        string cleaned =
            s.Replace("{", "")
             .Replace("}", "");

        string[] parts =
            cleaned.Split(',');

        foreach (string p in parts)
        {
            if (
                float.TryParse(
                    p.Trim(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out float val
                )
            )
            {
                result.Add(val);
            }
        }

        return result;
    }

    private static Dictionary<string, object>
        PlistToDictionary(XmlNode node)
    {
        Dictionary<string, object> result =
            new Dictionary<string, object>();

        XmlNodeList children =
            node.ChildNodes;

        for (
            int i = 0;
            i < children.Count;
            i += 2
        )
        {
            XmlNode key = children[i];
            XmlNode val = children[i + 1];

            result[key.InnerText] =
                ParsePlistValue(val);
        }

        return result;
    }

    private static object ParsePlistValue(
        XmlNode node
    )
    {
        switch (node.Name)
        {
            case "string":
                return node.InnerText;

            case "integer":
                return int.Parse(
                    node.InnerText
                );

            case "true":
                return true;

            case "false":
                return false;

            case "dict":
                return PlistToDictionary(node);
        }

        return null;
    }
}