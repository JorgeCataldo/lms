using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Domain.Extensions
{
    public class Variance
    {
        public string Prop { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }

    public static class CustomExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            return source.Where(element => knownKeys.Add(keySelector(element)));
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }
            if (enumerable is ICollection<T> collection)
            {
                return collection.Count < 1;
            }
            return !enumerable.Any();
        }

        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static List<Variance> DetailedCompare<T>(this T oldValue, T newValue)
        {
            List<Variance> variances = new List<Variance>();
            FieldInfo[] fi = oldValue.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | 
                BindingFlags.NonPublic | BindingFlags.Public); 
            foreach (FieldInfo f in fi)
            {
                Variance v = new Variance();
                v.Prop = oldValue.GetType().ToString() + "-" + f.Name;
                v.OldValue = f.GetValue(oldValue);
                v.NewValue = f.GetValue(newValue);
                if (v.OldValue is IList && v.OldValue.GetType().IsGenericType &&
                    v.NewValue is IList && v.NewValue.GetType().IsGenericType)
                {
                    var listValOldValue = (IList)v.OldValue;
                    var listValNewValue = (IList)v.NewValue;
                    var count = Math.Max(listValOldValue.Count, listValNewValue.Count);
                    for (var i = 0; i < count; i++)
                    {
                        if (i < listValOldValue.Count && i < listValNewValue.Count)
                        {
                            variances.AddRange(listValOldValue[i].DetailedCompare(listValNewValue[i]));
                        }
                        else if (i < listValOldValue.Count && i >= listValNewValue.Count)
                        {
                            variances.Add(new Variance
                            {
                                Prop = f.Name + "-index-" + i.ToString(),
                                OldValue = listValOldValue[i],
                                NewValue = null
                            });
                        }
                        else if (i >= listValOldValue.Count && i < listValNewValue.Count)
                        {
                            variances.Add(new Variance
                            {
                                Prop = f.Name + "-index-" + i.ToString(),
                                OldValue = null,
                                NewValue = listValNewValue[i]
                            });
                        }
                    }
                }
                else
                {
                    if (!v.OldValue.Equals(v.NewValue))
                    {
                        variances.Add(v);
                    }
                }

            }
            return variances;
        }
    }
}