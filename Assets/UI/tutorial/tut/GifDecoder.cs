using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Pure C# GIF decoder for Unity. Parses a .gif file (from disk) into a list
/// of frames (Texture2D + delay time). No external plugins required.
/// Supports GIF89a with LZW decompression, local/global color tables,
/// transparency, and frame disposal methods (Restore Background / Restore Previous).
/// </summary>
public static class GifDecoder
{
    public class GifFrame
    {
        public Texture2D texture;
        public float delaySeconds;
    }

    public class GifData
    {
        public List<GifFrame> frames = new List<GifFrame>();
        public int width;
        public int height;
    }

    private class LogicalScreenDescriptor
    {
        public int width;
        public int height;
        public bool hasGlobalColorTable;
        public int globalColorTableSize;
        public int backgroundColorIndex;
    }

    private class GraphicControlExtension
    {
        public int disposalMethod;
        public bool transparentColorFlag;
        public int transparentColorIndex;
        public int delayTimeCentiseconds;
    }

    public static GifData Decode(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[GifDecoder] File not found: {filePath}");
            return null;
        }
        byte[] bytes = File.ReadAllBytes(filePath);
        return Decode(bytes);
    }

    public static GifData Decode(byte[] bytes)
    {
        try
        {
            return DecodeInternal(bytes);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GifDecoder] Exception while decoding: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    private static GifData DecodeInternal(byte[] bytes)
    {
        int pos = 0;

        // --- Header ---
        if (bytes.Length < 6)
        {
            Debug.LogError("[GifDecoder] File too short to contain a GIF header.");
            return null;
        }
        string header = System.Text.Encoding.ASCII.GetString(bytes, 0, 6);
        if (header != "GIF87a" && header != "GIF89a")
        {
            Debug.LogError($"[GifDecoder] Not a valid GIF file. Header was: '{header}'");
            return null;
        }
        pos = 6;

        // --- Logical Screen Descriptor ---
        var lsd = new LogicalScreenDescriptor();
        lsd.width = ReadUInt16LE(bytes, pos); pos += 2;
        lsd.height = ReadUInt16LE(bytes, pos); pos += 2;
        byte packed = bytes[pos]; pos += 1;
        lsd.hasGlobalColorTable = (packed & 0x80) != 0;
        int gctSizeExp = packed & 0x07;
        lsd.globalColorTableSize = 2 << gctSizeExp;
        lsd.backgroundColorIndex = bytes[pos]; pos += 1;
        pos += 1; // pixel aspect ratio

        if (lsd.width <= 0 || lsd.height <= 0 || (long)lsd.width * lsd.height > 50_000_000L)
        {
            Debug.LogError($"[GifDecoder] Suspicious/invalid canvas size {lsd.width}x{lsd.height}, aborting to avoid hang/OOM.");
            return null;
        }

        Color32[] globalColorTable = null;
        if (lsd.hasGlobalColorTable)
        {
            globalColorTable = ReadColorTable(bytes, ref pos, lsd.globalColorTableSize);
        }

        var result = new GifData { width = lsd.width, height = lsd.height };

        // Canvas buffer accumulated across frames (GIF frames can be partial + disposal-based)
        Color32[] canvas = new Color32[lsd.width * lsd.height];
        Color32[] previousCanvas = null;

        GraphicControlExtension pendingGce = null;

        while (pos < bytes.Length)
        {
            byte blockType = bytes[pos];

            if (blockType == 0x21) // Extension Introducer
            {
                pos += 1;
                byte label = bytes[pos]; pos += 1;

                if (label == 0xF9) // Graphic Control Extension
                {
                    int blockSize = bytes[pos]; pos += 1; // should be 4
                    byte gcePacked = bytes[pos]; pos += 1;
                    var gce = new GraphicControlExtension();
                    gce.disposalMethod = (gcePacked >> 2) & 0x07;
                    gce.transparentColorFlag = (gcePacked & 0x01) != 0;
                    gce.delayTimeCentiseconds = ReadUInt16LE(bytes, pos); pos += 2;
                    gce.transparentColorIndex = bytes[pos]; pos += 1;
                    pos += 1; // block terminator
                    pendingGce = gce;
                }
                else
                {
                    // Skip other extensions (comment, application, plain text)
                    pos = SkipSubBlocks(bytes, pos);
                }
            }
            else if (blockType == 0x2C) // Image Descriptor
            {
                pos += 1;
                int imgLeft = ReadUInt16LE(bytes, pos); pos += 2;
                int imgTop = ReadUInt16LE(bytes, pos); pos += 2;
                int imgWidth = ReadUInt16LE(bytes, pos); pos += 2;
                int imgHeight = ReadUInt16LE(bytes, pos); pos += 2;
                byte imgPacked = bytes[pos]; pos += 1;

                bool hasLocalColorTable = (imgPacked & 0x80) != 0;
                bool interlaced = (imgPacked & 0x40) != 0;
                int localColorTableSize = 2 << (imgPacked & 0x07);

                Color32[] activeColorTable = globalColorTable;
                if (hasLocalColorTable)
                {
                    activeColorTable = ReadColorTable(bytes, ref pos, localColorTableSize);
                }

                int lzwMinCodeSize = bytes[pos]; pos += 1;
                byte[] indexStream = ReadSubBlocksData(bytes, ref pos);
                byte[] pixelIndices = LzwDecode(indexStream, lzwMinCodeSize, imgWidth * imgHeight);

                // Save previous canvas if disposal method requires restore
                if (pendingGce != null && pendingGce.disposalMethod == 3)
                {
                    previousCanvas = (Color32[])canvas.Clone();
                }

                // Composite this frame's pixels onto canvas
                WriteFrameToCanvas(canvas, lsd.width, lsd.height, pixelIndices, activeColorTable,
                    imgLeft, imgTop, imgWidth, imgHeight, interlaced,
                    pendingGce != null && pendingGce.transparentColorFlag ? pendingGce.transparentColorIndex : -1);

                // Build texture from canvas (Unity textures are bottom-left origin, GIF is top-left)
                Texture2D tex = new Texture2D(lsd.width, lsd.height, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                Color32[] flipped = FlipVertical(canvas, lsd.width, lsd.height);
                tex.SetPixels32(flipped);
                tex.Apply();

                float delay = pendingGce != null ? pendingGce.delayTimeCentiseconds / 100f : 0.1f;
                if (delay <= 0f) delay = 0.1f; // GIF convention: 0 delay -> default to 100ms

                result.frames.Add(new GifFrame { texture = tex, delaySeconds = delay });

                // Handle disposal method for NEXT frame
                if (pendingGce != null)
                {
                    if (pendingGce.disposalMethod == 2) // Restore to background
                    {
                        ClearRegion(canvas, lsd.width, imgLeft, imgTop, imgWidth, imgHeight);
                    }
                    else if (pendingGce.disposalMethod == 3 && previousCanvas != null) // Restore to previous
                    {
                        canvas = (Color32[])previousCanvas.Clone();
                    }
                }

                pendingGce = null;
            }
            else if (blockType == 0x3B) // Trailer
            {
                break;
            }
            else
            {
                // Unknown block, bail safely
                pos += 1;
            }
        }

        return result;
    }

    // ---------- Helpers ----------

    private static int ReadUInt16LE(byte[] bytes, int pos)
    {
        return bytes[pos] | (bytes[pos + 1] << 8);
    }

    private static Color32[] ReadColorTable(byte[] bytes, ref int pos, int size)
    {
        var table = new Color32[size];
        for (int i = 0; i < size; i++)
        {
            byte r = bytes[pos]; byte g = bytes[pos + 1]; byte b = bytes[pos + 2];
            pos += 3;
            table[i] = new Color32(r, g, b, 255);
        }
        return table;
    }

    private static int SkipSubBlocks(byte[] bytes, int pos)
    {
        while (true)
        {
            int size = bytes[pos]; pos += 1;
            if (size == 0) break;
            pos += size;
        }
        return pos;
    }

    private static byte[] ReadSubBlocksData(byte[] bytes, ref int pos)
    {
        using (var ms = new MemoryStream())
        {
            while (true)
            {
                int size = bytes[pos]; pos += 1;
                if (size == 0) break;
                ms.Write(bytes, pos, size);
                pos += size;
            }
            return ms.ToArray();
        }
    }

    /// <summary>Standard GIF LZW decompression into an index stream.</summary>
    private static byte[] LzwDecode(byte[] data, int minCodeSize, int expectedPixelCount)
    {
        int clearCode = 1 << minCodeSize;
        int endCode = clearCode + 1;
        int codeSize = minCodeSize + 1;
        int nextCode = endCode + 1;

        var dict = new Dictionary<int, List<byte>>();
        void ResetDict()
        {
            dict.Clear();
            for (int i = 0; i < clearCode; i++) dict[i] = new List<byte> { (byte)i };
            dict[clearCode] = new List<byte>();
            dict[endCode] = new List<byte>();
            nextCode = endCode + 1;
            codeSize = minCodeSize + 1;
        }
        ResetDict();

        var output = new List<byte>(expectedPixelCount);
        int bitPos = 0;
        int totalBits = data.Length * 8;

        int ReadCode(int size)
        {
            int code = 0;
            for (int i = 0; i < size; i++)
            {
                if (bitPos >= totalBits) return endCode;
                int byteIndex = bitPos / 8;
                int bitIndex = bitPos % 8;
                int bit = (data[byteIndex] >> bitIndex) & 1;
                code |= bit << i;
                bitPos++;
            }
            return code;
        }

        List<byte> previous = null;

        while (bitPos < totalBits && output.Count < expectedPixelCount)
        {
            int code = ReadCode(codeSize);

            if (code == clearCode)
            {
                ResetDict();
                previous = null;
                continue;
            }
            if (code == endCode)
            {
                break;
            }

            List<byte> entry;
            if (dict.ContainsKey(code))
            {
                entry = dict[code];
            }
            else if (code == nextCode && previous != null)
            {
                entry = new List<byte>(previous) { previous[0] };
            }
            else
            {
                break; // corrupt stream, stop safely
            }

            output.AddRange(entry);

            if (previous != null && nextCode < 4096)
            {
                var newEntry = new List<byte>(previous) { entry[0] };
                dict[nextCode] = newEntry;
                nextCode++;
                if (nextCode == (1 << codeSize) && codeSize < 12)
                {
                    codeSize++;
                }
            }

            previous = entry;
        }

        // Pad if short (defensive, avoids index errors on malformed streams)
        while (output.Count < expectedPixelCount) output.Add(0);

        return output.ToArray();
    }

    private static void WriteFrameToCanvas(Color32[] canvas, int canvasWidth, int canvasHeight,
        byte[] pixelIndices, Color32[] colorTable, int left, int top, int w, int h,
        bool interlaced, int transparentIndex)
    {
        int[] rowOrder = BuildRowOrder(h, interlaced);

        int srcIndex = 0;
        for (int rowNum = 0; rowNum < h; rowNum++)
        {
            int actualRow = rowOrder[rowNum];
            int canvasY = top + actualRow;
            if (canvasY < 0 || canvasY >= canvasHeight) { srcIndex += w; continue; }

            for (int x = 0; x < w; x++)
            {
                int canvasX = left + x;
                byte idx = pixelIndices[srcIndex++];
                if (canvasX < 0 || canvasX >= canvasWidth) continue;
                if (transparentIndex >= 0 && idx == transparentIndex) continue; // keep existing canvas pixel

                Color32 c = idx < colorTable.Length ? colorTable[idx] : new Color32(0, 0, 0, 255);
                canvas[canvasY * canvasWidth + canvasX] = c;
            }
        }
    }

    private static int[] BuildRowOrder(int height, bool interlaced)
    {
        var order = new int[height];
        if (!interlaced)
        {
            for (int i = 0; i < height; i++) order[i] = i;
            return order;
        }

        // Interlaced GIF row order: passes at step 8/8/4/2 starting rows 0/4/2/1
        int idx = 0;
        int[][] passes = new int[][] { new int[] { 0, 8 }, new int[] { 4, 8 }, new int[] { 2, 4 }, new int[] { 1, 2 } };
        foreach (var pass in passes)
        {
            for (int row = pass[0]; row < height; row += pass[1])
            {
                order[idx++] = row;
            }
        }
        return order;
    }

    private static void ClearRegion(Color32[] canvas, int canvasWidth, int left, int top, int w, int h)
    {
        for (int y = top; y < top + h; y++)
        {
            for (int x = left; x < left + w; x++)
            {
                canvas[y * canvasWidth + x] = new Color32(0, 0, 0, 0);
            }
        }
    }

    private static Color32[] FlipVertical(Color32[] src, int width, int height)
    {
        var flipped = new Color32[src.Length];
        for (int y = 0; y < height; y++)
        {
            int srcRow = y * width;
            int dstRow = (height - 1 - y) * width;
            Array.Copy(src, srcRow, flipped, dstRow, width);
        }
        return flipped;
    }
}