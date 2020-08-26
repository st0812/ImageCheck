using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


//参考文献  LINQ を使って 8 行で k-means 法を実装してみた - Qiita https://qiita.com/ytakashina/items/ad6f4c05940fa11b02ad
namespace ImageChecker.Models
{
    public static class Extensions
    {
        public static Vector3 Average(this IEnumerable<Vector3> self)
        {
            var array = self as Vector3[] ?? self.ToArray();
            return array.Aggregate(Vector3.Zero, (v1, v2) => v1 + v2) / array.Length;
        }

        public static Vector3 Average2(this IEnumerable<Vector3> self)
        {
            var array = self as Vector3[] ?? self.ToArray();
            double Xx=0;
            double Xy=0;
            double Y = 0;
            double Z = 0;
            foreach(var v in array)
            {
                var v1xcos = Math.Cos(Math.PI * v.X / 180);
                var v1xsin = Math.Sin(Math.PI * v.X / 180);
                Xx += v1xcos;
                Xy += v1xsin;
                Y += v.Y;
                Z += v.Z;
                
            }
            var tmp = Math.Atan2(Xy , Xx);
            if (tmp < 0) tmp +=2.0* Math.PI;
            tmp = tmp *180/Math.PI;

            return new Vector3((float)tmp, (float)(Y / array.Length), (float)(Z / array.Length));
        }

    }
    class Clustering
    {
        public static Tuple<Vector3[], int[]> KMeans(Vector3[] data, int k)
        {
            var rand = new Random();
            var means = data.OrderBy(v => rand.Next()).Take(k).ToList();

            var assignments = new int[data.Length];
            while (true)
            {
                var prevAssignments = assignments.Select(v => v).ToArray();


                //assignments = data.Select(v => means.IndexOf(means.MinBy(m => (v - m).LengthSquared()))).ToArray();
                for (var i = 0; i < data.Length; i++)
                {
                    int minindex = 0;
                    double minlength = double.MaxValue;
                    for (int index = 0; index < k; index++)
                    {
                        //var tmp = (means[index] - data[i]).LengthSquared();
                        var tmp = HSVColorRegion.Distance(means[index] , data[i]);

                        if (tmp < minlength)
                        {
                            minlength = tmp;
                            minindex = index;
                        }
                    }
                    assignments[i] = minindex;


                }

                if (Enumerable.Range(0, assignments.Length).All(i => assignments[i] == prevAssignments[i])) break;
                means = means.Select((m, i) => data.Where((v, j) => i == assignments[j]).DefaultIfEmpty(m).Average2()).ToList();
            }
            return Tuple.Create(means.ToArray(), assignments);
        }

    }
}
