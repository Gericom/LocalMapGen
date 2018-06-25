using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LocalMapGen.Core
{
    public class DCTTile// : Tile
    {
        private readonly int[][] _dcts = new int[3][];
        public int MergeFactor { get; set; }
        public long Distortion { get; set; }

        private DCTTile(int[][] dcts)
        //: base(colors)
        {
            _dcts = dcts;
        }

        public DCTTile(Color[] colors)
        //: base(colors)
        {
            var components = new[] { new int[64], new int[64], new int[64] };
            Parallel.For(0, 64, i =>
            {
                components[0][i] = (colors[i].R * 257 + colors[i].G * 504 + colors[i].B * 98 + 500) / 1000 + 16;
                components[1][i] = (colors[i].R * -148 + colors[i].G * -291 + colors[i].B * 439 + 500) / 1000 + 128;
                components[2][i] = (colors[i].R * 439 + colors[i].G * -368 + colors[i].B * -71 + 500) / 1000 + 128;
            });
            Parallel.For(0, 3, i =>
            {
                _dcts[i] = Dct64(components[i]);
            });
            MergeFactor = 1;
        }

        public long Compare(DCTTile b)
        {
            //int simdLength = Vector<int>.Count;
            //todo: weights for each dct coefficient
            var results = new long[3];
            for (int i = 0; i < 3; i++)//Parallel.For(0, 3, i =>
            {
                /*int[] diffs = new int[64];
                for (int j = 0; j < 64; j++)
                {
                    int diff = _dcts[i][j] - b._dcts[i][j];
                    if (diff < 0)
                        diff = -diff;
                    diffs[j] = diff;
                }*/

                //normalization
                //int myMax = _dcts[i].Max(a => a < 0 ? -a : a);
                //int bMax = b._dcts[i].Max(a => a < 0 ? -a : a);
                //int avg = (myMax + bMax) / 2;


                long diff00 = _dcts[i][0] - b._dcts[i][0];
                diff00 *= diff00;
                //if (diff00 < 0)
                //    diff00 = -diff00;
                long diff01 = _dcts[i][1] - b._dcts[i][1];
                diff01 *= diff01;
                // if (diff01 < 0)
                //    diff01 = -diff01;

                // int diff02 = _dcts[i][2] - b._dcts[i][2];
                //  if (diff02 < 0)
                //     diff02 = -diff02;

                long diff10 = _dcts[i][8] - b._dcts[i][8];
                diff10 *= diff10;
                // if (diff10 < 0)
                //   diff10 = -diff10;
                long diff11 = _dcts[i][9] - b._dcts[i][9];
                diff11 *= diff11;
                // if (diff11 < 0)
                //     diff11 = -diff11;

                // int diff20 = _dcts[i][16] - b._dcts[i][16];
                // if (diff20 < 0)
                //    diff20 = -diff20;
                results[i] = (diff00 * 5 + diff01 * 2 + diff10 * 2 + diff11) * 4 / 10;
                // results[i] = (diff00 * 5 + diff01 * 2 + diff10 * 2 + diff11 + diff02 + diff20) * 4 / 12;*/
                //results[i] = (2 * diffs[0] + diffs[1] + diffs[2] + diffs[3] + diffs[8] + diffs[16] + diffs[24]) / 2;
                /*for (int y = 0; y < 2; y++)
                {
                    for(int x = 0; x < 2; x++)
                    {
                        int diff = _dcts[i][y * 8 + x] - b._dcts[i][y * 8 + x];
                        if (diff < 0)
                            diff = -diff;
                        results[i] += diff;
                    }
                }*/
            }//);
            return results[0] * 10 + results[1] * 4 + results[2] * 6;
        }

        /*private static readonly int[] Weights =
        {
            /*8 ,16 ,19 ,22 ,26, 27, 29 ,34,
            16 ,16, 22, 24, 27, 29 ,34 ,37,
            19 ,22 ,26 ,27 ,29 ,34 ,34 ,38,
            22, 22, 26 ,27, 29 ,34 ,37 ,40,
            22, 26 ,27 ,29 ,32, 35, 40, 48,
            26 ,27, 29, 32 ,35 ,40, 48 ,58,
            26, 27, 29 ,34 ,38 ,46, 56, 69,
            27 ,29 ,35, 38 ,46, 56, 69 ,83/
            33, 14, 1, 1, 1, 1, 1, 1,
            14, 7, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1
        };*/

        private static readonly int[] Weights =
        {
            33, 17, 8, 4, 2, 1, 1, 1,
            17, 8, 4, 2, 1, 1, 1, 1,
            8, 4, 2, 1, 1, 1, 1, 1,
            4, 2, 1, 1, 1, 1, 1, 1,
            2, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1
        };

        public long CompareNew(DCTTile b)
        {
            var results = new long[3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    long diff = _dcts[i][j] - b._dcts[i][j];
                    diff *= diff;
                    diff = diff * Weights[j] / 32;
                    results[i] += diff;
                }
            }
            return (results[0] * 10 + results[1] * 4 + results[2] * 6) * 128 / 166;
        }

        public long CompareComplete(DCTTile b)
        {
            var results = new long[3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    long diff = _dcts[i][j] - b._dcts[i][j];
                    diff *= diff;
                    //if (diff < 0)
                    //    diff = -diff;
                    results[i] += diff;
                }
            }
            return results[0] * 10 + results[1] * 4 + results[2] * 6;
        }

        public DCTTile Merge(DCTTile tile, long distortion)//IEnumerable<DCTTile> tiles)
        {
            //int simdLength = Vector<int>.Count;
            var newDct = new[] { new int[64], new int[64], new int[64] };
            // Parallel.For(0, 3, i =>
            for (int i = 0; i < 3; i++)
            {
                //Array.Copy(_dcts[i], newDct[i], 64);
                //int count = 2;
                //foreach (var tile in tiles)
                //{
                    //Parallel.For(0, 64, k =>
                    for (int k = 0; k < 64; k++)
                    {
                        newDct[i][k] = (_dcts[i][k] * MergeFactor + tile._dcts[i][k] * tile.MergeFactor) / (MergeFactor + tile.MergeFactor);
                    }//);
                  //  count++;
                //}
                //Parallel.For(0, 64, k =>
                //{
                //    newDct[i][k] /= 2;//count;
                //});
            }//);
            return new DCTTile(newDct){MergeFactor = MergeFactor + tile.MergeFactor, Distortion = distortion };
        }

        private static int[] Dct64(int[] pixels)
        {
            int[] tmp2 = new int[64];
            int[] tmp = new int[64];
            for (int i = 0; i < 8; i++)
            {
                int p = pixels[i * 8 + 0] * 64;
                int q = pixels[i * 8 + 1] * 64;
                int r = pixels[i * 8 + 2] * 64;
                int s = pixels[i * 8 + 3] * 64;
                int t = pixels[i * 8 + 4] * 64;
                int u = pixels[i * 8 + 5] * 64;
                int v = pixels[i * 8 + 6] * 64;
                int w = pixels[i * 8 + 7] * 64;
                tmp[i * 8 + 0] = v + q + t + s + u + r + w + p;
                tmp[i * 8 + 1] = -40 * v + 40 * q - 12 * t + 12 * s - 24 * u + 24 * r - 48 * w + 48 * p;
                tmp[i * 8 + 2] = v + q - 2 * t - 2 * s - u - r + 2 * w + 2 * p;
                tmp[i * 8 + 3] = 12 * v - 12 * q + 24 * t - 24 * s + 48 * u - 48 * r - 40 * w + 40 * p;
                tmp[i * 8 + 4] = -v - q + t + s - u - r + w + p;
                tmp[i * 8 + 5] = 48 * v - 48 * q - 40 * t + 40 * s - 12 * u + 12 * r - 24 * w + 24 * p;
                tmp[i * 8 + 6] = -2 * v - 2 * q - t - s + 2 * u + 2 * r + w + p;
                tmp[i * 8 + 7] = 24 * v - 24 * q + 48 * t - 48 * s - 40 * u + 40 * r - 12 * w + 12 * p;
            }
            for (int i = 0; i < 8; i++)
            {
                int p = tmp[0 * 8 + i];
                int q = tmp[1 * 8 + i];
                int r = tmp[2 * 8 + i];
                int s = tmp[3 * 8 + i];
                int t = tmp[4 * 8 + i];
                int u = tmp[5 * 8 + i];
                int v = tmp[6 * 8 + i];
                int w = tmp[7 * 8 + i];
                tmp2[i * 8 + 0] = v + q + t + s + u + r + w + p;
                tmp2[i * 8 + 1] = -40 * v + 40 * q - 12 * t + 12 * s - 24 * u + 24 * r - 48 * w + 48 * p;
                tmp2[i * 8 + 2] = v + q - 2 * t - 2 * s - u - r + 2 * w + 2 * p;
                tmp2[i * 8 + 3] = 12 * v - 12 * q + 24 * t - 24 * s + 48 * u - 48 * r - 40 * w + 40 * p;
                tmp2[i * 8 + 4] = -v - q + t + s - u - r + w + p;
                tmp2[i * 8 + 5] = 48 * v - 48 * q - 40 * t + 40 * s - 12 * u + 12 * r - 24 * w + 24 * p;
                tmp2[i * 8 + 6] = -2 * v - 2 * q - t - s + 2 * u + 2 * r + w + p;
                tmp2[i * 8 + 7] = 24 * v - 24 * q + 48 * t - 48 * s - 40 * u + 40 * r - 12 * w + 12 * p;
            }
            tmp2[0] /= 64;
            tmp2[1] /= 2312;
            tmp2[2] /= 80;
            tmp2[3] /= 2312;
            tmp2[4] /= 64;
            tmp2[5] /= 2312;
            tmp2[6] /= 80;
            tmp2[7] /= 2312;

            tmp2[8] /= 2312;
            tmp2[9] /= 83521;
            tmp2[10] /= 2890;
            tmp2[11] /= 83521;
            tmp2[12] /= 2312;
            tmp2[13] /= 83521;
            tmp2[14] /= 2890;
            tmp2[15] /= 83521;

            tmp2[16] /= 80;
            tmp2[17] /= 2890;
            tmp2[18] /= 100;
            tmp2[19] /= 2890;
            tmp2[20] /= 80;
            tmp2[21] /= 2890;
            tmp2[22] /= 100;
            tmp2[23] /= 2890;

            tmp2[24] /= 2312;
            tmp2[25] /= 83521;
            tmp2[26] /= 2890;
            tmp2[27] /= 83521;
            tmp2[28] /= 2312;
            tmp2[29] /= 83521;
            tmp2[30] /= 2890;
            tmp2[31] /= 83521;

            tmp2[32] /= 64;
            tmp2[33] /= 2312;
            tmp2[34] /= 80;
            tmp2[35] /= 2312;
            tmp2[36] /= 64;
            tmp2[37] /= 2312;
            tmp2[38] /= 80;
            tmp2[39] /= 2312;

            tmp2[40] /= 2312;
            tmp2[41] /= 83521;
            tmp2[42] /= 2890;
            tmp2[43] /= 83521;
            tmp2[44] /= 2312;
            tmp2[45] /= 83521;
            tmp2[46] /= 2890;
            tmp2[47] /= 83521;

            tmp2[48] /= 80;
            tmp2[49] /= 2890;
            tmp2[50] /= 100;
            tmp2[51] /= 2890;
            tmp2[52] /= 80;
            tmp2[53] /= 2890;
            tmp2[54] /= 100;
            tmp2[55] /= 2890;

            tmp2[56] /= 2312;
            tmp2[57] /= 83521;
            tmp2[58] /= 2890;
            tmp2[59] /= 83521;
            tmp2[60] /= 2312;
            tmp2[61] /= 83521;
            tmp2[62] /= 2890;
            tmp2[63] /= 83521;
            return tmp2;
        }

        private static byte Clamp(int x, byte min, byte max)
        {
            if (x < min)
                return min;
            if (x > max)
                return max;
            return (byte)x;
        }

        public static byte[] IDct64(int[] dct)
        {
            int lr = 0;
            int r11 = 0;
            int r0 = (int)dct[lr++];
            int r1 = (int)dct[lr++];
            int r2 = (int)dct[lr++];
            int r3 = (int)dct[lr++];
            int r4 = (int)dct[lr++];
            int r5 = (int)dct[lr++];
            int r6 = (int)dct[lr++];
            int r7 = (int)dct[lr++];
            int r8, r9;
            r0 += 0x20;

            int[] DCTtmp = new int[64];

            int r12 = 8;
            while (true)
            {
                r8 = r0 + r4;
                r9 = r0 - r4;
                r0 = r2 + (r6 >> 1);
                r4 = (r2 >> 1) - r6;
                r2 = r9 + r4;
                r4 = r9 - r4;
                r6 = r8 - r0;
                r0 = r8 + r0;
                r8 = r1 + r7;
                r8 -= r3;
                r8 -= (r3 >> 1);
                r9 = r7 - r1;
                r9 += r5;
                r9 += (r5 >> 1);
                r7 += (r7 >> 1);
                r7 = r5 - r7;
                r7 -= r3;
                r3 += r5;
                r3 += r1;
                r3 += (r1 >> 1);
                r1 = r7 + (r3 >> 2);
                r7 = r3 - (r7 >> 2);
                r3 = r8 + (r9 >> 2);
                r5 = (r8 >> 2) - r9;
                r0 += r7;
                r7 = r0 - r7 * 2;
                r8 = r2 + r5;
                r9 = r2 - r5;
                r2 = r4 + r3;
                r5 = r4 - r3;
                r3 = r6 + r1;
                r4 = r6 - r1;
                r1 = r8;
                r6 = r9;
                DCTtmp[r11 + 56] = r7;
                DCTtmp[r11 + 48] = r6;
                DCTtmp[r11 + 40] = r5;
                DCTtmp[r11 + 32] = r4;
                DCTtmp[r11 + 24] = r3;
                DCTtmp[r11 + 16] = r2;
                DCTtmp[r11 + 8] = r1;
                DCTtmp[r11 + 0] = r0;
                r11++;
                r12--;
                if (r12 <= 0) break;
                r0 = (int)dct[lr++];
                r1 = (int)dct[lr++];
                r2 = (int)dct[lr++];
                r3 = (int)dct[lr++];
                r4 = (int)dct[lr++];
                r5 = (int)dct[lr++];
                r6 = (int)dct[lr++];
                r7 = (int)dct[lr++];
            }
            r11 -= 8;
            byte[] result = new byte[64];
            int Offset = 0;
            for (int i = 0; i < 8; i++)
            {
                r0 = (int)DCTtmp[r11++];
                r1 = (int)DCTtmp[r11++];
                r2 = (int)DCTtmp[r11++];
                r3 = (int)DCTtmp[r11++];
                r4 = (int)DCTtmp[r11++];
                r5 = (int)DCTtmp[r11++];
                r6 = (int)DCTtmp[r11++];
                r7 = (int)DCTtmp[r11++];
                r9 = r0 + r4;
                int r10 = r0 - r4;
                r0 = r2 + (r6 >> 1);
                r4 = (r2 >> 1) - r6;
                r2 = r10 + r4;
                r4 = r10 - r4;
                r6 = r9 - r0;
                r0 = r9 + r0;
                r9 = r1 + r7;
                r9 -= r3;
                r9 -= (r3 >> 1);
                r10 = r7 - r1;
                r10 += r5;
                r10 += (r5 >> 1);
                r7 += (r7 >> 1);
                r7 = r5 - r7;
                r7 -= r3;
                r3 += r5;
                r3 += r1;
                r3 += (r1 >> 1);
                r1 = r7 + (r3 >> 2);
                r7 = r3 - (r7 >> 2);
                r3 = r9 + (r10 >> 2);
                r5 = (r9 >> 2) - r10;
                r0 += r7;
                r7 = r0 - r7 * 2;
                r9 = r2 + r5;
                r10 = r2 - r5;
                r2 = r4 + r3;
                r5 = r4 - r3;
                r3 = r6 + r1;
                r4 = r6 - r1;
                r1 = r9;
                r6 = r10;
                result[Offset + 0] = Clamp((r0 >> 6), 0, 255);
                result[Offset + 1] = Clamp((r1 >> 6), 0, 255);
                result[Offset + 2] = Clamp((r2 >> 6), 0, 255);
                result[Offset + 3] = Clamp((r3 >> 6), 0, 255);
                result[Offset + 4] = Clamp((r4 >> 6), 0, 255);
                result[Offset + 5] = Clamp((r5 >> 6), 0, 255);
                result[Offset + 6] = Clamp((r6 >> 6), 0, 255);
                result[Offset + 7] = Clamp((r7 >> 6), 0, 255);
                Offset += 8;
            }
            return result;
        }

        public Color[] GetColors()
        {
            var components = new byte[3][];
            var colors = new Color[64];
            Parallel.For(0, 3, i =>
            {
                components[i] = IDct64(_dcts[i]);
            });
            Parallel.For(0, 64, i =>
            {
                float r, g, b;
                r = 1.164f * (components[0][i] - 16f) + 1.596f * (components[2][i] - 128f);
                g = 1.164f * (components[0][i] - 16f) - 0.392f * (components[1][i] - 128f) - 0.813f * (components[2][i] - 128f);
                b = 1.164f * (components[0][i] - 16f) + 2.017f * (components[1][i] - 128f);
                if (r < 0)
                    r = 0;
                else if (r > 255)
                    r = 255;
                if (g < 0)
                    g = 0;
                else if (g > 255)
                    g = 255;
                if (b < 0)
                    b = 0;
                else if (b > 255)
                    b = 255;
                colors[i] = Color.FromArgb((int)r, (int)g, (int)b);
            });
            return colors;
        }
    }
}
