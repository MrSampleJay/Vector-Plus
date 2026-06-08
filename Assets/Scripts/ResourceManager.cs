using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class ResourceManager
{
    public static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    public static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public static Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();

    public static void ClearTextureCache()
    {
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null)
                UnityEngine.Object.Destroy(sprite);
        }

        spriteCache.Clear();

        foreach (var tex in textureCache.Values)
        {
            if (tex != null)
                UnityEngine.Object.Destroy(tex);
        }

        textureCache.Clear();

        inFlightLoads.Clear();
    }

    public static void LoadAllTextures(string p_path, Vector2 pivot, float pixelsPerUnit)
    {
        foreach (var file in Directory.GetFiles(p_path))
        {
            LoadSpriteFromExternal(file, pivot, pixelsPerUnit);
        }
    }


    public static byte[] GetBinary(string p_path)
    {
        if (p_path.StartsWith(Application.streamingAssetsPath))
        {
            if (!File.Exists(p_path))
            {
                return null;
            }
            FileStream fileStream = new FileStream(p_path, FileMode.Open, FileAccess.Read);
            byte[] array = new byte[fileStream.Length];
            fileStream.Read(array, 0, (int)fileStream.Length);
            fileStream.Close();
            return array;
        }
        char[] trimChars = { '\\', '/', };
        string path = p_path.TrimStart(trimChars);
        path = RemoveExtension(path);
        var obj = Resources.Load<TextAsset>(path);
        if (obj != null)
        {
            return obj.bytes;
        }
        return null;
    }

    public static string GetTextFromResources(string p_path)
    {
        char[] trimChars = { '\\', '/', };
        string path = p_path.TrimStart(trimChars);
        path = RemoveExtension(path);
        var obj = Resources.Load<TextAsset>(path);
        if (obj != null)
        {
            return obj.text;
        }
        return string.Empty;
    }

    private static string RemoveExtension(string p_path)
    {
        if (Path.HasExtension(p_path))
        {
            return Path.ChangeExtension(p_path, null);
        }
        return p_path;
    }

    public static bool FileExists(string p_path, out string existingPath, params string[] exts)
    {
        existingPath = p_path;

        if (p_path.StartsWith(Application.streamingAssetsPath))
        {
            foreach (var ext in exts)
            {
                existingPath = p_path + ext;

                if (File.Exists(existingPath))
                {
                    return true;
                }
            }
        }
        else
        {
            return Resources.Load(p_path) != null;
        }
        return false;
    }

    public static AudioClip GetAudioClipFromExternal(string p_fileName)
    {
        if (audioCache.ContainsKey(p_fileName))
        {
            return audioCache[p_fileName];
        }

        string[] possibleExts = { ".wav", ".mp3", ".ogg" };
        string resolvedPath = p_fileName;

        if (!FileExists(p_fileName, out resolvedPath, possibleExts))
        {
            return null;
        }

        AudioType audioType = AudioType.UNKNOWN;
        string extLower = Path.GetExtension(resolvedPath).ToLower();
        switch (extLower)
        {
            case ".wav": audioType = AudioType.WAV; break;
            case ".mp3": audioType = AudioType.MPEG; break;
            case ".ogg": audioType = AudioType.OGGVORBIS; break;
        }

        using (var www = UnityWebRequestMultimedia.GetAudioClip($"file:///{resolvedPath}", audioType))
        {
            www.SendWebRequest();

            while (!www.isDone && string.IsNullOrEmpty(www.error)) { }

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError($"Error loading audio clip from '{resolvedPath}': {www.error}");
                return null;
            }
            var audioClip = DownloadHandlerAudioClip.GetContent(www);
            audioCache.Add(p_fileName, audioClip);
            return audioClip;
        }
    }

    public static IEnumerator GetAudioClipFromExternalAsync(string p_fileName, Action<AudioClip> onLoaded, bool stream = false)
    {
        if (audioCache.ContainsKey(p_fileName))
        {
            onLoaded?.Invoke(audioCache[p_fileName]);
            yield break;
        }

        string[] possibleExts = { ".wav", ".mp3", ".ogg" };
        string resolvedPath = p_fileName;

        if (!FileExists(p_fileName, out resolvedPath, possibleExts))
        {
            onLoaded?.Invoke(null);
            yield break;
        }

        AudioType audioType = AudioType.UNKNOWN;
        string extLower = Path.GetExtension(resolvedPath).ToLower();
        switch (extLower)
        {
            case ".wav": audioType = AudioType.WAV; break;
            case ".mp3": audioType = AudioType.MPEG; break;
            case ".ogg": audioType = AudioType.OGGVORBIS; break;
        }

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"file:///{resolvedPath}", audioType);

        DownloadHandlerAudioClip handler =
    (DownloadHandlerAudioClip)request.downloadHandler;

        handler.streamAudio = stream;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load audio clip: {resolvedPath}\n{request.error}");
            onLoaded?.Invoke(null);
            yield break;
        }

        var audioClip = DownloadHandlerAudioClip.GetContent(request);
        audioCache.Add(p_fileName, audioClip);
        onLoaded?.Invoke(audioClip);
    }

    public static Texture2D LoadTextureFromExternal(string path)
    {
        if (!FileExists(path, out path, ".png", ".jpg", ".jpeg")) return null;

        if (textureCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        byte[] array;
        using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            array = new byte[fileStream.Length];
            fileStream.Read(array, 0, array.Length);
        }
        Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture2D.LoadImage(array);
        textureCache.Add(path, texture2D);
        return texture2D;
    }

    public static Sprite LoadSpriteFromExternal(string path, Vector2 pivot, float ppu)
    {
        string originalPath = path;
        string spriteKey = $"{originalPath}|{pivot.x}|{pivot.y}|{ppu}";

        if (spriteCache.TryGetValue(spriteKey, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        var tex = LoadTextureFromExternal(path);
        if (tex == null) return null;

        Rect rect = new Rect(0, 0, tex.width, tex.height);
        Sprite sprite = Sprite.Create(
            tex,
            rect,
            pivot,
            ppu,
            0,
            SpriteMeshType.FullRect
        );

        spriteCache[spriteKey] = sprite;

        return sprite;
    }
    private static readonly Dictionary<string, List<System.Action<Texture2D>>> inFlightLoads = new();

    public static IEnumerator LoadTextureFromExternalAsync(
        string path,
        System.Action<Texture2D> onLoaded)
    {
        if (!FileExists(path, out path, ".png", ".jpg", ".jpeg"))
        {
            onLoaded?.Invoke(null);
            yield break;
        }

        if (textureCache.TryGetValue(path, out var cached) && cached != null)
        {
            onLoaded?.Invoke(cached);
            yield break;
        }

        if (inFlightLoads.TryGetValue(path, out var callbacks))
        {
            callbacks.Add(onLoaded);
            yield break;
        }

        inFlightLoads[path] = new List<System.Action<Texture2D>> { onLoaded };

        string uri = path;

        if (!uri.Contains("://"))
            uri = "file://" + uri;

        using UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(uri, nonReadable: true);

        yield return request.SendWebRequest();

        Texture2D tex = null;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load texture: {path}\n{request.error}");
        }
        else
        {
            tex = DownloadHandlerTexture.GetContent(request);

            if (tex != null)
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;

                textureCache[path] = tex;
            }
        }

        var waitingCallbacks = inFlightLoads[path];
        inFlightLoads.Remove(path);

        foreach (var callback in waitingCallbacks)
            callback?.Invoke(tex);
    }

    private static readonly Dictionary<string, List<Action<Sprite>>> inFlightSprites = new();

    public static IEnumerator LoadSpriteFromExternalAsync(
        string path,
        Vector2 pivot,
        float ppu,
        Action<Sprite> onLoaded)
    {
        string spriteKey = $"{path}|{pivot.x}|{pivot.y}|{ppu}";

        if (spriteCache.TryGetValue(spriteKey, out Sprite cachedSprite) && cachedSprite != null)
        {
            onLoaded?.Invoke(cachedSprite);
            yield break;
        }

        if (inFlightSprites.TryGetValue(spriteKey, out var callbacks))
        {
            callbacks.Add(onLoaded);
            yield break;
        }

        inFlightSprites[spriteKey] = new List<Action<Sprite>> { onLoaded };

        Sprite sprite = null;

        yield return LoadTextureFromExternalAsync(path, tex =>
        {
            if (tex == null)
                return;

            Rect rect = new Rect(0, 0, tex.width, tex.height);

            sprite = Sprite.Create(
                tex,
                rect,
                pivot,
                ppu,
                0,
                SpriteMeshType.FullRect
            );

            spriteCache[spriteKey] = sprite;
        });

        var waitingCallbacks = inFlightSprites[spriteKey];
        inFlightSprites.Remove(spriteKey);

        foreach (var callback in waitingCallbacks)
            callback?.Invoke(sprite);
    }
}
