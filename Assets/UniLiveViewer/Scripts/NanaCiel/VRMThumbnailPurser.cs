using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace NanaCiel
{
    /// <summary>
    /// バイナリから直接サムネイル抽出、両対応(0.x/1.0)
    /// </summary>
    public static class VRMThumbnailPurser
    {
        public static Texture2D Parse(string path)
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var (json, bin) = ReadGlb(bytes);
                var texBytes = ExtractThumbnailBytes(json, bin);
                if (texBytes == null || texBytes.Length == 0)
                {
                    Debug.LogWarning("Thumbnail not found in VRM.");
                    return null;
                }

                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(texBytes))
                {
                    Debug.LogWarning("Failed to decode image bytes.");
                    return null;
                }
                tex.name = Path.GetFileNameWithoutExtension(path) + "_thumbnail";
                return tex;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error: {ex.Message}");
            }
            return null;
        }

        // --- GLB reader (glTF 2.0) ---
        static (string json, byte[] bin) ReadGlb(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            uint magic = br.ReadUInt32(); // "glTF" = 0x46546C67
            if (magic != 0x46546C67) throw new Exception("Not a GLB (magic mismatch)");
            uint version = br.ReadUInt32();
            if (version != 2) throw new Exception($"Unsupported GLB version: {version}");
            uint length = br.ReadUInt32(); // total length (unused)

            string json = null;
            byte[] bin = null;

            while (ms.Position < ms.Length)
            {
                if (ms.Length - ms.Position < 8) break;
                uint chunkLen = br.ReadUInt32();
                uint chunkType = br.ReadUInt32(); // JSON=0x4E4F534A, BIN=0x004E4942

                var chunkData = br.ReadBytes((int)chunkLen);
                if (chunkType == 0x4E4F534A) // JSON
                {
                    json = Encoding.UTF8.GetString(chunkData);
                }
                else if (chunkType == 0x004E4942) // BIN
                {
                    bin = chunkData;
                }
            }

            if (json == null) throw new Exception("JSON chunk not found");
            return (json, bin);
        }

        // --- Extract VRM thumbnail bytes (VRM0 & VRM1) ---
        static byte[] ExtractThumbnailBytes(string json, byte[] binChunk)
        {
            var root = JObject.Parse(json);

            // 1) figure out image index
            int? imageIndex = null;

            // VRM1.0: extensions.VRMC_vrm.meta.thumbnailImage (image index directly)
            imageIndex = root["extensions"]?["VRMC_vrm"]?["meta"]?["thumbnailImage"]?.Value<int?>();

            // VRM0.x: extensions.VRM.meta.texture (texture index -> textures[n].source -> image index)
            if (imageIndex == null)
            {
                var texIdx = root["extensions"]?["VRM"]?["meta"]?["texture"]?.Value<int?>();
                if (texIdx != null)
                {
                    var textures = root["textures"] as JArray;
                    if (textures != null && texIdx.Value >= 0 && texIdx.Value < textures.Count)
                    {
                        imageIndex = textures[texIdx.Value]?["source"]?.Value<int?>();
                    }
                }
            }

            if (imageIndex == null) return null;

            var images = root["images"] as JArray;
            if (images == null || imageIndex.Value < 0 || imageIndex.Value >= images.Count) return null;

            var img = images[imageIndex.Value] as JObject;

            // Case A: data URI
            var uri = img?["uri"]?.Value<string>();
            if (!string.IsNullOrEmpty(uri))
            {
                // data:[<mediatype>][;base64],<data>
                if (uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    var comma = uri.IndexOf(',');
                    if (comma >= 0)
                    {
                        var meta = uri.Substring(5, comma - 5); // e.g. image/png;base64
                        var payload = uri.Substring(comma + 1);
                        if (meta.Contains("base64"))
                            return Convert.FromBase64String(payload);
                        else
                            return Encoding.UTF8.GetBytes(payload);
                    }
                }
                // (外部URIはVRMでは通常使われない想定)
                return null;
            }

            // Case B: bufferView + mimeType
            var bufferViewIndex = img?["bufferView"]?.Value<int?>();
            if (bufferViewIndex == null) return null;

            var bufferViews = root["bufferViews"] as JArray;
            if (bufferViews == null || bufferViewIndex.Value < 0 || bufferViewIndex.Value >= bufferViews.Count) return null;

            var bv = bufferViews[bufferViewIndex.Value];
            int byteOffset = bv["byteOffset"]?.Value<int?>() ?? 0;
            int byteLength = bv["byteLength"]?.Value<int?>() ?? 0;
            int bufferIdx = bv["buffer"]?.Value<int?>() ?? 0;

            // VRM(GLB)は通常 buffer[0] が BIN チャンク。念のため buffers を確認
            if (bufferIdx != 0)
            {
                // 追加の buffer がある場合は対応外（ほぼ無い想定）
                return null;
            }

            if (binChunk == null || byteOffset + byteLength > binChunk.Length) return null;

            var outBytes = new byte[byteLength];
            Buffer.BlockCopy(binChunk, byteOffset, outBytes, 0, byteLength);
            return outBytes;
        }
    }
}