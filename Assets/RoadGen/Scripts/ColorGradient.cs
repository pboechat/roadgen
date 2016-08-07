using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RoadGen
{
    public class ColorGradient
    {
        List<Color> colors = new List<Color>();

        public ColorGradient()
        {
        }

        public ColorGradient(IList<Color> colors)
        {
            float step = 1 / (colors.Count - 1.0f);
            for (int i = 0; i < colors.Count; i++)
                AddColor(colors[i], step * i);
        }

        public void AddColor(Color color, float value)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                if (value < colors[i].a)
                {
                    colors.Insert(i, new Color(color.r, color.g, color.b, value));
                    return;
                }
            }
            colors.Add(new Color(color.r, color.g, color.b, value));
        }

        public void ClearGradient()
        {
            colors.Clear();
        }

        public static ColorGradient CreateDefaultHeatMapGradient()
        {
            return new ColorGradient(new Color[] {
        new Color(0, 0, 1, 0),      // Blue.
        new Color(0, 1, 1, 0),     // Cyan.
        new Color(0, 1, 0, 0),      // Green.
        new Color(1, 1, 0, 0),     // Yellow.
        new Color(1, 0, 0, 0)      // Red.
        });
        }

        public void GetColorAtValue(float value, ref Color color)
        {
            if (colors.Count == 0)
                return;

            for (int i = 0; i < colors.Count; i++)
            {
                Color currColor = colors[i];
                if (value < currColor.a)
                {
                    Color prevColor = colors[Mathf.Max(0, i - 1)];
                    float valueDiff = (prevColor.a - currColor.a);
                    float fractBetween = (valueDiff == 0) ? 0 : (value - currColor.a) / valueDiff;
                    color.r = (prevColor.r - currColor.r) * fractBetween + currColor.r;
                    color.g = (prevColor.g - currColor.g) * fractBetween + currColor.g;
                    color.b = (prevColor.b - currColor.b) * fractBetween + currColor.b;
                    return;
                }
            }
            color.r = colors.Last().r;
            color.g = colors.Last().g;
            color.b = colors.Last().b;
            return;
        }

    }

}