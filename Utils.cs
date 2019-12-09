using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using Ionic.Zlib;

public class Utils {
    public static void Uncompress(ref byte[] bin, int size_uncompressed, ref byte[] bout)
    {
        using (MemoryStream ms = new MemoryStream(bin))
        using (ZlibStream zs = new ZlibStream(ms, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default))
        {
            bout = new byte[size_uncompressed];
            zs.Read(bout, 0, size_uncompressed);
        }
    }

    public static byte[] Float2Short2Byte(ref float[] f)
    {
        byte[] buf = new byte[f.Length * 2];
        byte[] short_byte = { 0, 0 };
        for (int i = 0; i < f.Length; ++i)
        {
            short s = (short)(short.MaxValue * f[i]);
            short_byte = System.BitConverter.GetBytes(s);
            buf[i * 2 + 0] = short_byte[0];
            buf[i * 2 + 1] = short_byte[1];
        }
        return buf;
    }

    public static short[] Float2Short(ref float[] f)
    {
        short[] buf = new short[f.Length];
        for (int i = 0; i < f.Length; ++i)
        {
            short s = (short)(short.MaxValue * f[i]);
            buf[i] = s;
        }
        return buf;
    }

    public static float[] Short2Float(ref short[] s)
    {
        float[] buf = new float[s.Length];
        for (int i = 0; i < s.Length; ++i)
        {
            float f = (float)s[i] / short.MaxValue;
            buf[i] = f;
        }
        return buf;
    }

    static List<Vector3> ScaleColor;
    public static Color GetColorByScale(float a)
    {
        if (ScaleColor == null)
        {
            ScaleColor = new List<Vector3>();
            /*
            ScaleColor.Add(new Vector3(89, 30, 134));//A
            ScaleColor.Add(new Vector3(155, 13, 130));
            ScaleColor.Add(new Vector3(209, 7, 127));//B
            ScaleColor.Add(new Vector3(229, 24, 31));//C
            ScaleColor.Add(new Vector3(239, 147, 26));
            ScaleColor.Add(new Vector3(249, 226, 15));//D
            ScaleColor.Add(new Vector3(98, 175, 64));
            ScaleColor.Add(new Vector3(2, 157, 100));//E
            ScaleColor.Add(new Vector3(0, 158, 142));//F
            ScaleColor.Add(new Vector3(0, 160, 185));
            ScaleColor.Add(new Vector3(0, 160, 222));//G
            ScaleColor.Add(new Vector3(12, 74, 156));
            ScaleColor.Add(new Vector3(89, 30, 134));//A
            */

            ScaleColor.Add(new Vector3(255, 55, 55));//C
            ScaleColor.Add(new Vector3(255, 100, 48));
            ScaleColor.Add(new Vector3(255, 158, 41));//D
            ScaleColor.Add(new Vector3(255, 200, 20));
            ScaleColor.Add(new Vector3(255, 243, 0));//E
            ScaleColor.Add(new Vector3(134, 255, 0));//F
            ScaleColor.Add(new Vector3(77, 255, 118));
            ScaleColor.Add(new Vector3(0, 254, 236));//G
            ScaleColor.Add(new Vector3(0, 183, 240));
            ScaleColor.Add(new Vector3(0, 113, 255));//A
            ScaleColor.Add(new Vector3(82, 83, 255));
            ScaleColor.Add(new Vector3(165, 53, 255));//B
            ScaleColor.Add(new Vector3(255, 55, 55));//C
        }

        int start = (int)(Mathf.Floor(a)) % 12;
        float offset = a - start;
        Vector3 v0 = ScaleColor[start];
        Vector3 v1 = ScaleColor[start + 1];
        Vector3 v = v0 + offset * (v1 - v0);
        return new Color(v.x / 255, v.y / 255, v.z / 255);
    }

    public static int GetKeyByFrequency(float f)
    {

        float f_base = 13.75f;
        int octave = 1;
        for(int i = 0; i < 20; ++i)
        {
            if(f <= f_base)
            {
                f_base /= 2;
                break;
            }
            f_base *= 2;
        }
        float a = Mathf.Log(f / f_base, 2);
        a *= 12;
        int key = (int)a + octave * 12;
        return key;
    }

    public static float MidiKey2Frequency(int midi_key)
    {
        if (midi_key < 9) return 1f;
        float f_base = 13.75f;
        return f_base * Mathf.Pow(2, (float)(midi_key - 9) / 12);

    }

    public static long GetMilliSec()
    {
        return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "";
    }

    public static List<Vector2> CalcRandomSpacedDistributions(int num_pos, List<Vector2> contour, int num_iteration, int num_wall_loop, float max_wall_space)
    {
        if (contour.Count < 3) return null;
        Vector2 center = Vector2.zero;
        foreach(Vector2 v in contour)
        {
            center += v;
        }
        center /= contour.Count;
        List<Vector2> outer = new List<Vector2>();
        int offset_wall_outer = 5;
        for(int i = offset_wall_outer; i < num_wall_loop + offset_wall_outer; ++i)
        {
            Vector2 previous_v = contour[0] + (contour[0] - center).normalized * max_wall_space * i;
            foreach(Vector2 v in contour)
            {
                Vector2 v0 = v + (v - center).normalized * max_wall_space * i;
                outer.Add(v0);
                Vector2 dir = (v0 - previous_v);
                int poles = (int)(dir.magnitude / max_wall_space);
                for(int j = 0; j < poles; ++j) {
                    float fence_length = dir.magnitude / (poles + 1);
                    outer.Add(previous_v + dir.normalized * fence_length * j);
                }
                
                previous_v = v0;
            }
            //last to first contour.
            {
                Vector2 v = contour[0] + (contour[0] - center).normalized * max_wall_space * i;
                Vector2 v0 = v + (v - center).normalized * max_wall_space * i;
                outer.Add(v0);
                Vector2 dir = (v0 - previous_v);
                int poles = (int)(dir.magnitude / max_wall_space);
                for (int j = 0; j < poles; ++j)
                {
                    float fence_length = dir.magnitude / (poles + 1);
                    outer.Add(previous_v + dir.normalized * fence_length * j);
                }
            }

        }

        float min_len_from_wall = float.MaxValue;
        foreach(Vector2 v in contour)
        {
            float sqr_mag = (v - center).sqrMagnitude;
            if (sqr_mag < min_len_from_wall)
            {
                min_len_from_wall = sqr_mag;
            }
        }
        min_len_from_wall = Mathf.Sqrt(min_len_from_wall);

        List<Vector2> ret = new List<Vector2>();
        for(int i = 0; i < num_pos; ++i)
        {
            Vector3 dir = Random.Range(0f, max_wall_space) * (Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) * Vector3.right);
            ret.Add(center + new Vector2(dir.x, dir.y));
        }

        for(int i = 0; i < num_iteration; ++i)
        {
            for(int j = 0; j < ret.Count; ++j)
            {
                Vector2 dir = Vector2.zero;
                for(int k = 0; k < ret.Count; ++k)
                {
                    if (k == j) continue;
                    Vector2 v = ret[k] - ret[j];
                    float weight = Mathf.Exp(-v.sqrMagnitude / (0.2f * max_wall_space * max_wall_space * 16));
                    dir -= weight * v.normalized;
                }
                for(int k = 0; k < outer.Count; ++k)
                {
                    Vector2 v = outer[k] - ret[j];
                    float weight = Mathf.Exp(-v.sqrMagnitude / (0.2f * max_wall_space * max_wall_space * 16));
                    dir -= weight * v.normalized;
                }
                Vector3 random = Random.Range(0f, 0.5f) * (Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) * Vector3.right);
                dir -= new Vector2(random.x, random.y);
                ret[j] += dir.normalized * max_wall_space / 2f;
            }
        }

        return ret;
    }

    //Butterworth
    public static float[] LowPass(float[] indata, float freq_in, float freq_cutoff)
    {
        if (indata == null) return null;
        if (freq_cutoff == 0) return indata;

        long dF2 = indata.Length - 1;        // The data range is set with dF2
        float[] Dat2 = new float[dF2 + 4]; // Array with 4 extra points front and back
        float[] data = indata; // Ptr., changes passed data

        // Copy indata to Dat2
        for (long r = 0; r < dF2; r++)
        {
            Dat2[2 + r] = indata[r];
        }
        Dat2[1] = Dat2[0] = indata[0];
        Dat2[dF2 + 3] = Dat2[dF2 + 2] = indata[dF2];

        float wc = Mathf.Tan(freq_cutoff * Mathf.PI / freq_in);
        float k1 = 1.414213562f * wc; // Sqrt(2) * wc
        float k2 = wc * wc;
        float a = k2 / (1 + k1 + k2);
        float b = 2 * a;
        float c = a;
        float k3 = b / k2;
        float d = -2 * a + k3;
        float e = 1 - (2 * a) - k3;

        // RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
        float[] DatYt = new float[dF2 + 4];
        DatYt[1] = DatYt[0] = indata[0];
        for (long s = 2; s < dF2 + 2; s++)
        {
            DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2]
                       + d * DatYt[s - 1] + e * DatYt[s - 2];
        }
        DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

        // FORWARD filter
        float[] DatZt = new float[dF2 + 2];
        DatZt[dF2] = DatYt[dF2 + 2];
        DatZt[dF2 + 1] = DatYt[dF2 + 3];
        for (long t = -dF2 + 1; t <= 0; t++)
        {
            DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4]
                        + d * DatZt[-t + 1] + e * DatZt[-t + 2];
        }

        // Calculated points copied for return
        for (long p = 0; p < dF2; p++)
        {
            data[p] = DatZt[p];
        }

        return data;
    }

    public static void Speed(ref float[] source, out float[] dst, float speed, int sampling_frequency)
    {
        //適当な値。downsamplingのためにSAMPLING_FREQUENCY / 2を設定したいが、
        //設定するとキーン音が入る（ローパスフィルタの不具合？）
        //なので可聴域をカットしすぎないほどの、適当な値。
        const float freq_cutoff = 3000f;

        float[] buf = new float[(int)(source.Length / speed)];
        float[] s = null;
        if (speed > 1f)
        {
            //down sampling
            s = LowPass(source, sampling_frequency, freq_cutoff);
        }
        else
        {
            s = source;
        }

        for (int i = 0; i < buf.Length; ++i)
        {
            buf[i] = s[(int)(i * speed)];
        }

        if (speed < 1f)
        {
            buf = LowPass(buf, sampling_frequency, freq_cutoff);
        }

        dst = buf;
    }

    public static void TimeStretch(ref float[] source, out float[] dst, float stretch, int length_window)
    {
        float[] buf = new float[(int)(source.Length * stretch)];

        float a_cross = 0.2f;
        int len_cross = (int)(length_window * a_cross);
        int num_unit_dst = buf.Length / length_window;
        for (int i = 0; i < num_unit_dst; ++i)
        {
            int i_src = i * source.Length / buf.Length;
            for (int j = 0; j < length_window + len_cross; ++j)
            {
                int idx_src = (int)(i_src * length_window + j);
                if (idx_src >= source.Length)
                {
                    idx_src = source.Length - 1;
                }
                int idx_dst = i * length_window + j;
                if (idx_dst >= buf.Length)
                {
                    break;
                }

                float a = 1f;
                if (j < len_cross)
                {
                    a = (float)j / len_cross;
                }
                else if (j >= length_window)
                {
                    a = 1f - (j - length_window) / (float)len_cross;
                }
                buf[idx_dst] += a * source[idx_src];
            }
        }
        dst = buf;
    }

    static List<string> CodeTexts = null;
    public static string GetCodeText(int key)
    {
        if (key < 0) return "";
        if(CodeTexts == null)
        {
            CodeTexts = new List<string>();
            CodeTexts.Add("C");
            CodeTexts.Add("C#");
            CodeTexts.Add("D");
            CodeTexts.Add("D#");
            CodeTexts.Add("E");
            CodeTexts.Add("F");
            CodeTexts.Add("F#");
            CodeTexts.Add("G");
            CodeTexts.Add("G#");
            CodeTexts.Add("A");
            CodeTexts.Add("A#");
            CodeTexts.Add("B");
        }
        int octave = key / 12;
        return "" + octave + CodeTexts[key % 12];
    }
}
