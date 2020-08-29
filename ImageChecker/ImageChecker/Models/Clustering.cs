using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;



//参考文献  LINQ を使って 8 行で k-means 法を実装してみた - Qiita https://qiita.com/ytakashina/items/ad6f4c05940fa11b02ad
namespace ImageChecker.Models
{
    
    class Clustering
    {
        public static Tuple<Vector3[], int[]> KMeans(Vector3[] data, int k, int loopUpperLimit, Func<IEnumerable<Vector3>, Vector3> average)
        {
            var rand = new Random();
            var means = data.OrderBy(v => rand.Next()).Take(k).ToList();

            var assignments = new int[data.Length];
            var loop = 0;
            while (loop<loopUpperLimit)
            {
                var prevAssignments = assignments.Select(v => v).ToArray();


                assignments = data.Select(v => means.IndexOf(means.MinBy(m => (v - m).LengthSquared()).First())).ToArray();
                

                if (Enumerable.Range(0, assignments.Length).All(i => assignments[i] == prevAssignments[i])) break;
                means = means.Select((m, i) => average(data.Where((v, j) => i == assignments[j]).DefaultIfEmpty(m))).ToList();
                loop++;
            }
            System.Diagnostics.Debug.WriteLine(loop);
            return Tuple.Create(means.ToArray(), assignments);
        }

    }
}
