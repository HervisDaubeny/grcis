// Text params -> script context.
// Any global pre-processing is allowed here.
using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Threading;

formula.contextCreate = (in Bitmap input, in string param) =>
{
  if (string.IsNullOrEmpty(param))
    return null;

  Dictionary<string, string> p = Util.ParseKeyValueList(param);


  // antialiasing=<int>
  int antialiasing = 0;
  Util.TryParse(p, "antialiasing", ref antialiasing);


  // radius=<int>
  int radius = 75;
  Util.TryParse(p, "radius", ref radius);


  // thickness=<int>
  int thickness = 1;
  Util.TryParse(p, "thickness", ref thickness);

  // 3D=<int>
  int iiiD = 0;
  Util.TryParse(p, "3D", ref iiiD);

  // stripes=<int>
  int stripes = 0;
  Util.TryParse(p, "stripes", ref stripes);

    Dictionary<string, object> sc = new Dictionary<string, object>();
  sc["antialiasing"] = antialiasing;
  sc["radius"] = radius;
  sc["thickness"] = thickness;
  sc["3D"] = iiiD;
  sc["stripes"] = stripes;
  sc["tooltip"] = "antialiasing=<int> .. antialiasing flag (0 - no antialiasing, 1 - use antialiasing, default=0)\r" +
                  "radius=<int> .. radius of the circle in pixels (default=75)\r" +
                  "thickness=<int> .. thickness of the circle in pixels (default=1)\r" +
                  "3D=<int> .. 3d flag (0 - use 2D, 1 - use 3D, default = 0)\r" +
                  "stripes=<int> .. stripes flag (0 - no stripes, 1 - draw stripes, default = 0)";

  return sc;
};

void circle(ref float R, ref float G, ref float B, ImageContext imageContext) {
  int centX = imageContext.width / 2;
  int centY = imageContext.height / 2;
  int curX = imageContext.x;
  int curY = imageContext.y;
  int distX = (centX - imageContext.x);
  int distY = (centY - imageContext.y);
  int antialiasing = 0;
  int thickness = 1;
  int radius = 74;
  int iiiD = 0;
  int stripes = 0;
  double distance = Math.Sqrt(Math.Pow((centX - curX), 2) + Math.Pow((centY - curY), 2));

  Util.TryParse(imageContext.context, "radius", ref radius);
  Util.TryParse(imageContext.context, "thickness", ref thickness);
  Util.TryParse(imageContext.context, "antialiasing", ref antialiasing);
  Util.TryParse(imageContext.context, "3D", ref iiiD);
  Util.TryParse(imageContext.context, "stripes", ref stripes);

  if (radius <= distance && distance <= radius + thickness) {
  /* full saturation pixel */
    R = 0.0f;
    G = 0.0f;
    B = 0.0f;
  }
  else if (
     (antialiasing == 1)
     && distance < radius
     && (radius - Math.Sqrt(2) / 2) < distance
    ) {
    /* inner antialiasing pixel */
    float from_center_distance = (float) (distance - (radius - Math.Sqrt(2) / 2));
    float saturation = (float) (1 - (from_center_distance / (Math.Sqrt(2) / 2)));
    R = saturation;
    G = saturation;
    B = saturation;
  }
  else if (
        (antialiasing == 1)
        && distance > radius + thickness
        && distance < (radius + thickness + Math.Sqrt(2)/2)
        ) {

  /* outer antialiasing pixel */
    float from_pixels_center_distance = (float)((radius + thickness + Math.Sqrt(2) / 2) - distance);
    float saturation = (float)(1 - (from_pixels_center_distance / (Math.Sqrt(2) / 2)));
    R = saturation;
    G = saturation;
    B = saturation;
  }

  /* center dot */
  if (centX == curX && centY == curY) {
    R = 0.0f;
    G = 0.0f;
    B = 0.0f;
  }

  if (stripes == 0)
    return;

  /* stripes */
  int magical_ratio = radius / 7;
  if (distance < radius) {
    for (int i = 0; i < magical_ratio; i++, i++) {
      if (
        12 * i <= Math.Abs(curX - centX)
        && Math.Abs(curX - centX) <= 12 * (i + 1)
        ) {
        R = 0.0f;
        G = 0.0f;
        B = 0.0f;
      }
      if (
        iiiD == 0
        && antialiasing == 1
        && (12 * i == Math.Abs(curX - centX) || Math.Abs(curX - centX) == 12 * (i + 1))
      ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
      }
      else if (iiiD == 1
        && antialiasing == 1
        && (12 * i == Math.Abs(curX - centX) || Math.Abs(curX - centX) == 12 * (i + 1))
        && Math.Abs(curY - centY) >= Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 7 / 8
      ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
      }

      if (iiiD == 0)
        continue;
      #region center
      if (
        Math.Abs(curX - centX) <= 12 * (i + 1) + (12 * 1 / 2)
        &&
        12 * (i + 1) < Math.Abs(curX - centX)
        && Math.Abs(curY - centY) <= Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) / 3
        ) {
        R = 0.0f;
        G = 0.0f;
        B = 0.0f;
      }
      if (
        antialiasing == 1
        && Math.Abs(curX - centX) == 12 * (i + 1) + (12 * 1 / 2)
        && 12 * (i + 1) < Math.Abs(curX - centX)
        && Math.Abs(curY - centY) <= Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) / 3
        ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
      }
      if (
        iiiD == 1
        && 12 * i <= Math.Abs(curX - centX)
        && Math.Abs(curX - centX) < 12 * i + (12 * 1 / 2)
        && Math.Abs(curY - centY) < Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) / 3
        ) {
        R = 1.0f;
        G = 1.0f;
        B = 1.0f;
      }
      if (
        iiiD == 1
        && antialiasing == 1
        && 12 * i <= Math.Abs(curX - centX)
        && Math.Abs(curX - centX) == 12 * i - 1 + (12 * 1 / 2)
        && Math.Abs(curY - centY) < Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) / 3
      ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
      }
      #endregion

      #region 3D center + 1/3
      if (
        Math.Abs(curX - centX) <= 12 * (i + 1) + (12 * 1 / 4)
        && 12 * (i + 1) < Math.Abs(curX - centX)
        && Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) / 3 < Math.Abs(curY - centY)
        && Math.Abs(curY - centY) < (Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2))) * 2 / 3
      ) {
        R = 0.0f;
        G = 0.0f;
        B = 0.0f;
      }
      if (
        antialiasing == 1
        && Math.Abs(curX - centX) == 12 * (i + 1) + (12 * 1 / 4)
        && 12 * (i + 1) < Math.Abs(curX - centX)
        && Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) / 3 < Math.Abs(curY - centY)
        && Math.Abs(curY - centY) < (Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2))) * 2 / 3
      ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
      }
      if (
        12 * i <= Math.Abs(curX - centX)
        && Math.Abs(curX - centX) < 12 * i + (12 * 1 / 4)
        && Math.Abs(curY - centY) <= Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 2 / 3
      ) {
        R = 1.0f;
        G = 1.0f;
        B = 1.0f;
      }
      if (
        antialiasing == 1
        && 12 * i <= Math.Abs(curX - centX)
        && Math.Abs(curX - centX) == 12 * i - 1 + (12 * 1 / 4)
        && Math.Abs(curY - centY) > Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) / 3
        && Math.Abs(curY - centY) < Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 2 / 3
      ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
      }
      #endregion

      #region center + 7/8
      if (
        Math.Abs(curX - centX) <= 12 * (i + 1) + (12 * 1 / 6)
        && 12 * (i + 1) < Math.Abs(curX - centX)
        && Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 2 / 3 < Math.Abs(curY - centY)
        && Math.Abs(curY - centY) < (Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2))) * 7 / 8
      ) {
        R = 0.0f;
        G = 0.0f;
        B = 0.0f;
      }
      if (
        antialiasing == 1
        && Math.Abs(curX - centX) == 12 * (i + 1) + (12 * 1 / 6)
        && 12 * (i + 1) < Math.Abs(curX - centX)
        && Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 2 / 3 < Math.Abs(curY - centY)
        && Math.Abs(curY - centY) < (Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2))) * 7 / 8
      ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
      }
      if (
        12 * i <= Math.Abs(curX - centX)
        && Math.Abs(curX - centX) < 12 * i + (12 * 1 / 6)
        && Math.Abs(curY - centY) <= Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 7 / 8
      ) {
        R = 1.0f;
        G = 1.0f;
        B = 1.0f;
      }
      if (
        antialiasing == 1
        && 12 * i <= Math.Abs(curX - centX)
        && Math.Abs(curX - centX) == 12 * i - 1 + (12 * 1 / 4)
        && Math.Abs(curY - centY) > Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 2 / 3
        && Math.Abs(curY - centY) < Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(curX - centX, 2)) * 7 / 8
      ) {
        R = 0.5f;
        G = 0.5f;
        B = 0.5f;
        #endregion
      }
    }
  }
}

/* Transform image to a drawn circle. */
formula.pixelTransform0 = (in ImageContext ic, ref float R, ref float G, ref float B) => {
  R = 1.0f;
  G = 1.0f;
  B = 1.0f;

  circle(ref R, ref G, ref B, ic);

  // Output color was modified.
  return true;
};

/* Unexpected art */
formula.pixelCreate = (in ImageContext ic, out float R, out float G, out float B) => {
  int centX = ic.width / 2;
  int centY = ic.height / 2;

  if (ic.x == centX && ic.y == centY) {
    R = 0.0f;
    G = 0.0f;
    B = 0.0f;
    return;
  }

  if (Math.Sqrt((centX - ic.x) ^ 2 + (centY - ic.y) ^ 2) >= 4) {
    R = 0.0f;
    G = 0.0f;
    B = 0.0f;
  }
  else {
    R = 1.0f;
    G = 1.0f;
    B = 1.0f;
  }
};
