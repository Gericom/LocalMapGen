using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalMapGen.Core;

namespace LocalMapGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("LocalMapGen by Gericom");
            Console.WriteLine("Working, this may take a minute...");
            var b = new Bitmap(new MemoryStream(File.ReadAllBytes(args[0])));
            var d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            var originalImageTiles = new DCTTile[b.Height / 8, b.Width / 8];
            var imageTiles = new DCTTile[b.Height / 8, b.Width / 8];
            var uniqueTiles = new List<DCTTile>();
            for (int y = 0; y < b.Height; y += 8)
            {
                for (int x = 0; x < b.Width; x += 8)
                {
                    Color[] tileColors = new Color[64];
                    for (int y2 = 0; y2 < 8; y2++)
                    {
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            unsafe
                            {
                                tileColors[y2 * 8 + x2] =
                                    Color.FromArgb(((int*)(d.Scan0 + d.Stride * (y + y2)))[x + x2]);
                            }
                        }
                    }
                    var t = new DCTTile(tileColors);
                    imageTiles[y / 8, x / 8] = t;
                    originalImageTiles[y / 8, x / 8] = t;
                    uniqueTiles.Add(t);
                }
            }
            b.UnlockBits(d);
            int w = b.Width;
            int h = b.Height;
            int maxDiff = 0;
            const int step = 100;
            int stepSize = step;//25;
            //Random rnd = new Random();
            while (uniqueTiles.Count > 1024)
            {
                Console.WriteLine(uniqueTiles.Count);
                //uniqueTiles = new List<DCTTile>(uniqueTiles.OrderBy(r => rnd.Next()));
                var newUniqueTiles = new List<DCTTile>();
                while (uniqueTiles.Count > 0)
                {
                    /*var tile = uniqueTiles[0];
                    var toMerge = new ConcurrentDictionary<DCTTile, DCTTile>();
                    Parallel.ForEach(uniqueTiles, tile2 =>
                    {
                        if (tile == tile2)
                            return;
                        if(tile.Compare(tile2) <= maxDiff)
                            toMerge.TryAdd(tile2, tile2);
                    });
                    var newTile = tile.Merge(toMerge.Keys);
                    newUniqueTiles.Add(newTile);
                    uniqueTiles.Remove(tile);
                    foreach (var dctTile in toMerge.Keys)
                        uniqueTiles.Remove(dctTile);
                    Parallel.For(0, h / 8, y =>
                    {
                        Parallel.For(0, w / 8, x =>
                        {
                            if (imageTiles[y,x] == tile || toMerge.ContainsKey(imageTiles[y, x]))
                                imageTiles[y, x] = newTile;
                        });
                    });*/
                    var tile = uniqueTiles[0];
                    uniqueTiles.Remove(tile);
                    var toMerge = new ConcurrentDictionary<long, DCTTile>();
                    Parallel.ForEach(uniqueTiles, tile2 =>
                    {
                       // var newTile = tile.Merge(tile2, 0);
                       // long score = tile.Compare(tile2);///*tile.CompareComplete(tile2);//*/tile.Compare(tile2);// + (tile.Distortion + tile2.Distortion) / 2;// + tile.Distortion + tile2.Distortion;//*(3 * */tile.Compare(tile2)/* + tile.CompareComplete(tile2)) / 4*/ + tile.Distortion + tile2.Distortion;// + tile2.Distortion;
                       // long score2 = tile.CompareComplete(tile2) / 16;
                      //  long score3 = tile.CompareNew(tile2);// / 16;

                       // score = (score + score2) / 2;
                       long score = tile.CompareNew(tile2);
                        //if (score2 > maxDiff)
                        //    return;

                        /*long score2 = tile.CompareComplete(tile2);
                        if (score2 < score)
                            score = score2;
                       // score += score2 / 100;
                        if (score > maxDiff / 8)// && score2 > maxDiff / 8)
                            score *= (1 + (tile.MergeFactor + tile2.MergeFactor) / 4);*/
                        //if (score > maxDiff / 4 && tile.MergeFactor + tile2.MergeFactor > 4)
                        //    score *= (1 + (tile.MergeFactor + tile2.MergeFactor) / 4);
                        //if (tile.MergeFactor + tile2.MergeFactor > 4)
                        //    return;
                        // score *= (tile.MergeFactor + tile2.MergeFactor) / 2;
                        // if (score > maxDiff / 16)
                        score *= (1 + (tile.MergeFactor + tile2.MergeFactor) / 2);
                        //    score = (long) (score * (1 + ((tile.MergeFactor + tile2.MergeFactor) * (tile.MergeFactor + tile2.MergeFactor) / 8f)));
                        if (score <= maxDiff)
                            toMerge.TryAdd(score, tile2);
                    });
                    if (toMerge.Count == 0)
                    {
                        uniqueTiles.Remove(tile);
                        newUniqueTiles.Add(tile);
                    }
                    else
                    {
                        var mergeTile = toMerge[toMerge.Keys.Min()];
                        //long score2 = tile.CompareComplete(mergeTile);
                        var newTile = tile.Merge(mergeTile, 0);// score2 + (tile.Distortion +mergeTile.Distortion) /2);//.Keys);
                       // long newScore = newTile.CompareComplete(tile);
                        //newTile.Distortion = (newScore + tile.Distortion) / 2;
                        newUniqueTiles.Add(newTile);
                        uniqueTiles.Remove(mergeTile);
                        //foreach (var dctTile in toMerge.Keys)
                        //     uniqueTiles.Remove(dctTile);
                        Parallel.For(0, h / 8, y =>
                        {
                            Parallel.For(0, w / 8, x =>
                            {
                                if (imageTiles[y, x] == tile || imageTiles[y, x] == mergeTile)//|| toMerge.ContainsKey(imageTiles[y, x]))
                                    imageTiles[y, x] = newTile;
                            });
                        });
                    }
                }
                uniqueTiles = newUniqueTiles;
                maxDiff += stepSize;
                stepSize += step;//25;
            }
            //retiling to improve quality
            //for(int y = 0; y < h / 8; y++)
              /*Parallel.For(0, h / 8, y =>
              {
                  for (int x = 0; x < w / 8; x++)
                  {
                      var tile = originalImageTiles[y, x];
                      DCTTile bestTile = null;
                      long bestScore = long.MaxValue;
                      foreach (var tile2 in uniqueTiles)
                      {
                          long score = tile.CompareComplete(tile2);
                          if (score < bestScore)
                          {
                              bestTile = tile2;
                              bestScore = score;
                          }
                      }
                      imageTiles[y, x] = bestTile;
                  }
              });*/

            var b2 = new Bitmap(w, h);
            var d2 = b2.LockBits(new Rectangle(0, 0, b2.Width, b2.Height), ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            for (int y = 0; y < b.Height; y += 8)
            {
                for (int x = 0; x < b.Width; x += 8)
                {
                    var tileColors = imageTiles[y / 8, x / 8].GetColors();
                    for (int y2 = 0; y2 < 8; y2++)
                    {
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            unsafe
                            {
                                ((int*)(d2.Scan0 + d2.Stride * (y + y2)))[x + x2] = tileColors[y2 * 8 + x2].ToArgb();
                            }
                        }
                    }
                }
            }
            b2.UnlockBits(d2);
            b2.Save(args[1]);
        }
    }
}
